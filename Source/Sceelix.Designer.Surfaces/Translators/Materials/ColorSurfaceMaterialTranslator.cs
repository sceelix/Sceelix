using System;
using System.Linq;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Actors.VertexTypes;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;
using Sceelix.Surfaces.Materials;
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Surfaces.Translators.Materials
{
    [MaterialTranslator(typeof(ColorSurfaceMaterial))]
    public class ColorSurfaceMaterialTranslator : SurfaceMaterialTranslator<VertexPositionNormalColorTexture>
    {
        private const float ReferenceAlpha = 0.5f;

        private ContentManager _content;
        private IGraphicsService _graphicsService;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _content = services.Get<ContentManager>();
            _graphicsService = services.Get<IGraphicsService>();
        }


        protected override Func<Coordinate, VertexPositionNormalColorTexture?> GetPrepareVertexFunc(SurfaceEntity surfaceEntity, Material sceelixMaterial)
        {
            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
            var normalsLayer = surfaceEntity.GetLayer<NormalLayer>();

            var getPositionFunc = heightLayer != null ? (Func<Coordinate, Vector3D>)heightLayer.GetPosition : surfaceEntity.CalculateFlatPosition;
            var getNormalFunc = normalsLayer != null ? (Func<Coordinate, Vector3D>)normalsLayer.GetGenericValue : heightLayer.CalculateNormal;

            var colorSurfaceMaterial = (ColorSurfaceMaterial) sceelixMaterial;
            var color = colorSurfaceMaterial.DefaultColor.ToXnaColor();

            return (coordinate) =>
            {
                VertexPositionNormalColorTexture customVertex = new VertexPositionNormalColorTexture();

                customVertex.Position = getPositionFunc(coordinate).ToVector3();
                customVertex.Normal = getNormalFunc(coordinate).ToVector3();
                customVertex.TextureCoordinate = surfaceEntity.CalculateUV(coordinate, new Vector2D(1, 1), new Vector2D(0, 0), false, true).ToVector2();
                customVertex.Color = color; //colorsLayer != null ? colorsLayer.GetValue(column, row).ToXnaColor() : colorSurfaceMaterial.DefaultColor.ToXnaColor();

                return customVertex;
            };
        }
        


        protected override DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial)
        {
            var colorSurfaceMaterial = (ColorSurfaceMaterial)sceelixMaterial;

            Texture2D texture = TextureLoader.CreateColorTexture(_graphicsService.GraphicsDevice, colorSurfaceMaterial.DefaultColor.ToXnaColor());


            DigitalRune.Graphics.Material material = new DigitalRune.Graphics.Material();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) { LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false };
            basicEffectBinding.Set("Texture", texture);
            material.Add("Default", basicEffectBinding);


            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
                material.Add("ShadowMap", shadowMapEffectBinding);

                // EffectBinding for the "GBuffer" pass.
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBuffer"), null, EffectParameterHint.Material);
                material.Add("GBuffer", gBufferEffectBinding);

                // EffectBinding for the "Material" pass.
                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/Material"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseTexture", texture);
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1));
                materialEffectBinding.Set("SpecularColor", new Vector3(0f, 0f, 0f));
                material.Add("Material", materialEffectBinding);
            }

            return material;
        }
    }
}