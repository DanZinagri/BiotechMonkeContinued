using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BiotechMonkeContinued
{
    // Draws the configured texture through the configured mask. See the properties class for
    // the source options.
    // GraphicDatabase caches per (path, shader, size, colour, maskPath), the render
    // node caches the resulting Graphic, and the render tree only rebuilds draw requests on
    // recache events. So this runs once per pawn setup and never per frame - the same stack
    // the Roo's Minotaur fur-leg patch relies on.
    public class PawnRenderNode_MaskedOverlay : PawnRenderNode
    {
        // The only vanilla skin-family shader that clips to _MaskTex alpha.
        protected override Shader DefaultShader => ShaderDatabase.CutoutSkinOverlay;

        public PawnRenderNodeProperties_MaskedOverlay OverlayProps => (PawnRenderNodeProperties_MaskedOverlay)props;

        public PawnRenderNode_MaskedOverlay(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.story?.bodyType == null)
            {
                return null;
            }

            string texPath = TexPathFor(pawn);
            if (texPath.NullOrEmpty())
            {
                return null;
            }

            string maskPath = MaskPathFor(pawn);
            if (maskPath.NullOrEmpty())
            {
                return null;
            }

            texPath = WithFemaleVariant(pawn, texPath);
            maskPath = WithFemaleVariant(pawn, maskPath);

            Shader shader = ShaderFor(pawn);
            if (shader == null)
            {
                return null;
            }
            if (!shader.SupportsMaskTex())
            {
                Log.WarningOnce(
                    $"[Biotech Monke] Shader '{shader.name}' on node '{props.debugLabel}' has no _MaskTex, falling back to CutoutSkinOverlay.",
                    shader.name.GetHashCode() ^ 0x3A5D);
                shader = ShaderDatabase.CutoutSkinOverlay;
            }
            // Graphic_Multi only looks for suffixed mask files. With none found it would draw the
            // texture unmasked, so refuse instead of silently drawing the whole thing.
            if (ContentFinder<Texture2D>.Get(maskPath + "_south", reportFailure: false) == null
                && ContentFinder<Texture2D>.Get(maskPath + "_north", reportFailure: false) == null
                && ContentFinder<Texture2D>.Get(maskPath + "_east", reportFailure: false) == null)
            {
                Log.WarningOnce(
                    $"[Biotech Monke] No mask textures found at '{maskPath}' (expected {maskPath}_south.png, _north.png, _east.png). Node '{props.debugLabel}' will not draw.",
                    maskPath.GetHashCode() ^ 0x3A5C);
                return null;
            }

            return GraphicDatabase.Get<Graphic_Multi>(texPath, shader, Vector2.one, ColorFor(pawn), Color.white, null, maskPath);
        }

        protected override string TexPathFor(Pawn pawn)
        {
            var p = OverlayProps;
            if (p.texFurDef != null)
            {
                return p.texFurDef.GetFurBodyGraphicPath(pawn);
            }
            if (p.texFromBody)
            {
                return DrawnBodyPath.For(pawn);
            }
            return base.TexPathFor(pawn); // texPath / texPaths / bodyTypeGraphicPaths / female variants
        }

        // Mirrors FemaleBodyVariants: non-male pawns get "<path>_Female" if that texture exists.
        // Paths that came through a FurDef may already carry the suffix (FBV postfixes
        // FurDef.GetFurBodyGraphicPath), hence the EndsWith guard. One ContentFinder lookup at
        // graphic creation; GraphicDatabase caches the result per final path.
        private string WithFemaleVariant(Pawn pawn, string path)
        {
            if (!OverlayProps.femaleVariants || path.NullOrEmpty() || pawn.gender == Gender.Male || path.EndsWith("_Female"))
            {
                return path;
            }
            string femalePath = path + "_Female";
            return ContentFinder<Texture2D>.Get(femalePath + "_south", reportFailure: false) != null ? femalePath : path;
        }

        private string MaskPathFor(Pawn pawn)
        {
            var p = OverlayProps;
            if (p.maskFurDef != null)
            {
                return p.maskFurDef.GetFurBodyGraphicPath(pawn);
            }
            if (p.maskFromBody)
            {
                return DrawnBodyPath.For(pawn);
            }
            List<BodyTypeGraphicData> byBodyType = p.bodyTypeMaskPaths;
            if (byBodyType != null)
            {
                for (int i = 0; i < byBodyType.Count; i++)
                {
                    if (byBodyType[i].bodyType == pawn.story.bodyType && !byBodyType[i].texturePath.NullOrEmpty())
                    {
                        return byBodyType[i].texturePath;
                    }
                }
            }
            return p.maskPath;
        }
    }
}
