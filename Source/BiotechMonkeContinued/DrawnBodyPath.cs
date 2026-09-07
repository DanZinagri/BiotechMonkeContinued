using UnityEngine;
using Verse;

namespace BiotechMonkeContinued
{
    // Resolves the body texture the pawn is actually drawn with. Several mods replace it by
    // postfixing PawnRenderNode_Body.GraphicFor: FemaleBodyVariants swaps in "<path>_Female",
    // Sized Apparel swaps in "<path>_BaseBodyF"/"_BaseBodyM" (optionally with a body variation
    // and a "/CustomPose/<pose>" segment). Rather than reimplement each mod's naming, read back
    // whatever the Body node resolved - our node is a child of Body, so Body is always
    // initialised first. Same approach as my Roo's Minotaur fur-leg patch.
    public static class DrawnBodyPath
    {
        public static string For(Pawn pawn)
        {
            string drawnBodyPath = pawn.Drawer?.renderer?.renderTree?.BodyGraphic?.path;
            if (!drawnBodyPath.NullOrEmpty()
                && ContentFinder<Texture2D>.Get(drawnBodyPath + "_south", reportFailure: false) != null)
            {
                return drawnBodyPath;
            }

            // Body node not resolved yet, or it is drawing something with no _south texture
            // (e.g. B&S hideBody's UI/EmptyImage). Fall back to resolving the FemaleBodyVariants
            // name by hand, then to the plain body path.
            string bodyPath = pawn.story?.bodyType?.bodyNakedGraphicPath;
            if (bodyPath == null || pawn.gender == Gender.Male || bodyPath.Contains("_Female"))
            {
                return bodyPath;
            }
            if (!bodyPath.Contains("_Thin") && !bodyPath.Contains("_Fat") && !bodyPath.Contains("_Hulk"))
            {
                return bodyPath;
            }

            string femaleBodyPath = bodyPath + "_Female";
            if (ContentFinder<Texture2D>.Get(femaleBodyPath + "_south", reportFailure: false) == null)
            {
                return bodyPath;
            }
            return femaleBodyPath;
        }
    }
}
