using System.Collections.Generic;
using System.Xml;
using Verse;

namespace BiotechMonkeContinued
{
    public class PatchOperationToggled : PatchOperation
    {
        // Keys whose operation actually ran (applied or skipped) this launch. Empty after
        // startup means the patch pass never happened, i.e. the XML came from a cache.
        public static readonly HashSet<string> RanThisLaunch = new HashSet<string>();

        public string key;
        public List<PatchOperation> operations = new List<PatchOperation>();

        private readonly List<string> failures = new List<string>();
        private bool skipped;

        public override IEnumerable<string> ConfigErrors()
        {
            if (string.IsNullOrEmpty(key))
            {
                yield return "PatchOperationToggled with no key.";
                yield break;
            }
            if (ToggleCatalogue.Get(key) == null)
            {
                yield return "PatchOperationToggled key \"" + key + "\" has no entry in ToggleCatalogue.";
            }
            if (operations.Count == 0)
            {
                yield return "PatchOperationToggled \"" + key + "\" has no operations.";
            }
        }

        protected override bool ApplyWorker(XmlDocument xml)
        {
            RanThisLaunch.Add(key);

            // Returning true for a skip is not optional: Complete() reports any operation that
            // never once succeeded as a failed patch, and a switched-off toggle is not a failure.
            if (!BiotechMonkeContinuedMod.IsEnabled(key))
            {
                skipped = true;
                return true;
            }

            // Every child runs even after one fails. PatchOperationSequence stops at the first
            // failure, which would let a single stale xpath quietly take the rest with it.
            bool allApplied = true;
            for (int i = 0; i < operations.Count; i++)
            {
                PatchOperation op = operations[i];
                if (op == null)
                {
                    continue;
                }
                // Vanilla only fills sourceFile in on top-level operations, so children would
                // otherwise report errors with no file name attached.
                if (string.IsNullOrEmpty(op.sourceFile))
                {
                    op.sourceFile = sourceFile;
                }
                if (!op.Apply(xml))
                {
                    failures.Add(op.GetType().Name + " (#" + (i + 1) + ")");
                    allApplied = false;
                }
            }
            return allApplied;
        }

        public override void Complete(string modIdentifier)
        {
            if (failures.Count > 0)
            {
                Log.Error("[Biotech Monke] Toggle \"" + key + "\": " + failures.Count + " of "
                    + operations.Count + " operations failed (" + string.Join(", ", failures.ToArray()) + ")."
                    + (string.IsNullOrEmpty(sourceFile) ? "" : "\nfile: " + sourceFile));
                return;
            }
            if (!skipped)
            {
                base.Complete(modIdentifier);
            }
        }

        public override string ToString()
        {
            return GetType().Name + "(" + key + ")";
        }
    }
}
