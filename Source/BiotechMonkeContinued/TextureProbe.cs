using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BiotechMonkeContinued
{
    // Remembers whether a texture path exists. A ContentFinder miss walks every running mod,
    // then Unity's Resources folder, then every mod asset bundle, which with a large mod list
    // costs milliseconds per probe. The set of textures is fixed after startup, so the answer
    // is kept for the session. Main thread only, same as ContentFinder itself.
    public static class TextureProbe
    {
        private static readonly Dictionary<string, bool> known = new Dictionary<string, bool>();

        public static bool Exists(string path)
        {
            if (known.TryGetValue(path, out bool exists))
            {
                return exists;
            }
            exists = ContentFinder<Texture2D>.Get(path, reportFailure: false) != null;
            known[path] = exists;
            return exists;
        }
    }
}
