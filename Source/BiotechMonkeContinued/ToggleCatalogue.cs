using System.Collections.Generic;
using Verse;

namespace BiotechMonkeContinued
{
    // One entry per settings toggle. Labels and descriptions live here, not in the XML, so
    // there is one source of truth; PatchOperationToggled.ConfigErrors flags any XML key that
    // has no entry.
    public class ToggleEntry
    {
        public string key;
        public string label;
        public string description;
        public bool defaultOn;

        // Package ids of mods whose presence makes the toggle moot: when any is active the toggle
        // reads as on and the settings window shows it locked. Used for the masked-fur default,
        // which a body-mod folder always supersedes with its own node.
        public List<string> forcedOnByMods;

        public ToggleEntry(string key, string label, string description, bool defaultOn, List<string> forcedOnByMods = null)
        {
            this.key = key;
            this.label = label;
            this.description = description;
            this.defaultOn = defaultOn;
            this.forcedOnByMods = forcedOnByMods;
        }

        // Name of the first active forcing mod, or null when the toggle is free.
        public string ForcedOnBy
        {
            get
            {
                if (forcedOnByMods == null)
                {
                    return null;
                }
                for (int i = 0; i < forcedOnByMods.Count; i++)
                {
                    ModMetaData mod = ModLister.GetActiveModWithIdentifier(forcedOnByMods[i], ignorePostfix: true);
                    if (mod != null)
                    {
                        return mod.Name;
                    }
                }
                return null;
            }
        }
    }

    public static class ToggleCatalogue
    {
        public const string MaskedFur = "maskedFur";

        public static readonly List<ToggleEntry> Entries = new List<ToggleEntry>
        {
            new ToggleEntry(
                MaskedFur,
                "Masked fur",
                "Draws the Biotech Furskin body art (or whatever retexture overrides it) clipped to "
                + "Monke's semi-fur pattern, instead of Monke's own semi-fur art.\n\nOff restores the "
                + "stock Monke look. Locked on while a supported body mod is active, because that "
                + "mod's folder supplies its own fur and mask.\n\nRestart required.",
                defaultOn: true,
                forcedOnByMods: new List<string>
                {
                    // Same order as the LoadFolders priority, so the lock names the folder that loads.
                    "ChenDuXiu.Gene.MoreCuteYttakin",
                    "Mainrrow.VRECompatibleBody2",
                    "OTYOTY.SizedApparel",
                    "ScrubDaddy.Bodies",
                }),
        };

        public static ToggleEntry Get(string key)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].key == key)
                {
                    return Entries[i];
                }
            }
            return null;
        }
    }
}
