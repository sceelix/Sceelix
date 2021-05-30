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
using Color = Microsoft.Xna.Framework.Color;
using Material = Sceelix.Actors.Data.Material;
using DrMaterial = DigitalRune.Graphics.Material;

namespace Sceelix.Designer.Surfaces.Translators.Materials
{
    [MaterialTranslator(typeof(BlendColorSurfaceMaterial))]
    public class BlendColorSurfaceMaterialTranslator : SurfaceMaterialTranslator<VertexPositionNormalColor2Texture>
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


        protected override Func<Coordinate, VertexPositionNormalColor2Texture?> GetPrepareVertexFunc(SurfaceEntity surfaceEntity, Material sceelixMaterial)
        {
            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
            var normalsLayer = surfaceEntity.GetLayer<NormalLayer>();

            var blendTextures = surfaceEntity.Layers.OfType<BlendLayer>().ToList();

            var colorArray0To3 = blendTextures.ToColorArray(surfaceEntity.NumColumns, surfaceEntity.NumRows);
            var colorArray4To7 = blendTextures.ToColorArray(surfaceEntity.NumColumns, surfaceEntity.NumRows, 4);

            return (coordinate) =>
            {
                VertexPositionNormalColor2Texture customVertex = new VertexPositionNormalColor2Texture();

                customVertex.Position = (heightLayer != null ? heightLayer.GetPosition(coordinate) : surfaceEntity.CalculateFlatPosition(coordinate)).ToVector3();
                customVertex.Normal = (normalsLayer != null ? normalsLayer.GetGenericValue(coordinate) : heightLayer.CalculateNormal(coordinate)).ToVector3();
                customVertex.TextureCoordinate = surfaceEntity.CalculateUV(coordinate, new Vector2D(1,1), new Vector2D(0, 0), false, true).ToVector2();
                customVertex.Color = colorArray0To3[coordinate.X, coordinate.Y].ToXnaColor(); //colorsLayer != null ? colorsLayer.GetValue(column, row).ToXnaColor() : colorSurfaceMaterial.DefaultColor.ToXnaColor();
                customVertex.Color1 = colorArray4To7[coordinate.X, coordinate.Y].ToXnaColor();

                return customVertex;
            };
        }



        protected override DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial)
        {
            var whiteColorTexture = TextureLoader.CreateColorTexture(_graphicsService.GraphicsDevice, Color.White);
            
            DrMaterial material = new DrMaterial();
            
            var materialEffectBinding = BuildPlatform.IsWindows ? CreateWindowsEffectBinding(whiteColorTexture, material) : CreateMacLinuxEffectBinding();

            materialEffectBinding.Set("Texture1", contentLoader.LoadColorTexture(System.Drawing.Color.Red));
            materialEffectBinding.Set("Texture2", contentLoader.LoadColorTexture(System.Drawing.Color.FromArgb(255, 0, 255, 0)));
            materialEffectBinding.Set("Texture3", contentLoader.LoadColorTexture(System.Drawing.Color.Blue));
            materialEffectBinding.Set("Texture4", contentLoader.LoadColorTexture(System.Drawing.Color.FromArgb(255, 0, 0, 0)));

            materialEffectBinding.Set("Texture5", contentLoader.LoadColorTexture(System.Drawing.Color.Yellow));
            materialEffectBinding.Set("Texture6", contentLoader.LoadColorTexture(System.Drawing.Color.Orange));
            materialEffectBinding.Set("Texture7", contentLoader.LoadColorTexture(System.Drawing.Color.DarkViolet));
            materialEffectBinding.Set("Texture8", contentLoader.LoadColorTexture(System.Drawing.Color.SeaGreen));

            if (BuildPlatform.IsWindows)
            {
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1)); //  * 2f
                materialEffectBinding.Set("SpecularColor", new Vector3(0f, 0f, 0f));

                material.Add("Material", materialEffectBinding);
            }
            else
            { 
                materialEffectBinding.Set("DiffuseColor", new Vector3(3, 3, 3));
                material.Add("Default", materialEffectBinding);
            }


            return material;
        }



        private EffectBinding CreateWindowsEffectBinding(Texture2D whiteColorTexture, DrMaterial material)
        {
            Texture2D basicTexture = whiteColorTexture;

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false,};
            basicEffectBinding.Set("Texture", basicTexture);
            material.Add("Default", basicEffectBinding);


            EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
            material.Add("ShadowMap", shadowMapEffectBinding);

            // EffectBinding for the "GBuffer" pass.
            EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBuffer"), null, EffectParameterHint.Material);
            gBufferEffectBinding.Set("SpecularPower", 100f);
            material.Add("GBuffer", gBufferEffectBinding);


            // EffectBinding for the "Material" pass.
            return new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/Material"), null, EffectParameterHint.Material);
        }



        private EffectBinding CreateMacLinuxEffectBinding()
        {
            return new EffectBinding(_graphicsService, new Effect(_graphicsService.GraphicsDevice, EmbeddedResources.Load<byte[]>("Resources/Material_Terrain.mgfx")), null, EffectParameterHint.Material);
        }
    }
}