using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BiotechMonkeContinued
{
    // Properties for a render node that draws one texture clipped to the "drawn vs not drawn" shape of another.
    public class PawnRenderNodeProperties_MaskedOverlay : PawnRenderNodeProperties
    {
        public FurDef texFurDef;
        public bool texFromBody;

        public FurDef maskFurDef;
        public bool maskFromBody;
        public List<BodyTypeGraphicData> bodyTypeMaskPaths;
        public string maskPath;

        // FemaleBodyVariants convention: a non-male pawn uses "<path>_Female" when that texture exists (e.g. Masks/Thin_Female_south.png). FBV itself only does this inside
        // FurDef.GetFurBodyGraphicPath, so explicit path lists need it done here. Applies to both the texture and the mask.
        public bool femaleVariants = true;

        public PawnRenderNodeProperties_MaskedOverlay()
        {
            nodeClass = typeof(PawnRenderNode_MaskedOverlay);
            // Same worker as the Body and Fur nodes, so this moves/flips exactly with them.
            workerClass = typeof(PawnRenderNodeWorker_Body);
            colorType = AttachmentColorType.Hair;
            rotDrawMode = RotDrawMode.Fresh | RotDrawMode.Rotting;
            // pawn draw tree: body 0, tattoos 2, fur 5, pre-apparel wounds 8, apparel 20.
            baseLayer = 5f;
            debugLabel = "Masked overlay";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (parentTagDef == null)
            {
                parentTagDef = PawnRenderNodeTagDefOf.Body;
            }
        }

        public bool HasTextureSource =>
            texFurDef != null || texFromBody
            || !texPath.NullOrEmpty() || !texPaths.NullOrEmpty() || !bodyTypeGraphicPaths.NullOrEmpty()
            || !texPathFemale.NullOrEmpty() || !texPathsFemale.NullOrEmpty();

        public bool HasMaskSource =>
            maskFurDef != null || maskFromBody || !bodyTypeMaskPaths.NullOrEmpty() || !maskPath.NullOrEmpty();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (!HasTextureSource)
            {
                yield return "Node " + debugLabel + ": no texture source (texFurDef, texFromBody, texPath, texPaths or bodyTypeGraphicPaths).";
            }
            if (!HasMaskSource)
            {
                yield return "Node " + debugLabel + ": no mask source (maskFurDef, maskFromBody, bodyTypeMaskPaths or maskPath).";
            }
        }
    }
}
