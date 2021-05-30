using System;
using System.IO;
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
using ScMaterial = Sceelix.Actors.Data.Material;
using DrMaterial = DigitalRune.Graphics.Material;


namespace Sceelix.Designer.Surfaces.Translators.Materials
{
    [MaterialTranslator(typeof(MultiTextureSurfaceMaterial))]
    public class MultiTextureSurfaceMaterialTranslator : SurfaceMaterialTranslator<VertexPositionNormalColor2Texture>
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


        protected override Func<Coordinate, VertexPositionNormalColor2Texture?> GetPrepareVertexFunc(SurfaceEntity surfaceEntity, ScMaterial sceelixMaterial)
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
                customVertex.TextureCoordinate = surfaceEntity.CalculateBaseUV(coordinate, false, true).ToVector2();
                customVertex.Color = colorArray0To3[coordinate.X, coordinate.Y].ToXnaColor();
                customVertex.Color1 = colorArray4To7[coordinate.X, coordinate.Y].ToXnaColor();

                return customVertex;
            };
        }
        

        protected override DrMaterial CreateMaterial(ContentLoader contentLoader, ScMaterial sceelixMaterial)
        {
            var textureMaterial = (MultiTextureSurfaceMaterial) sceelixMaterial;
            var whiteColorTexture = TextureLoader.CreateColorTexture(_graphicsService.GraphicsDevice, Color.White);
            var material = new DrMaterial();
            var maxTexturesCount = Math.Min(textureMaterial.TextureSetups.Length, 8);

            var materialEffectBinding = BuildPlatform.IsWindows ? 
                CreateWindowsEffectBinding(contentLoader, whiteColorTexture, textureMaterial, material)
                : CreateMacLinuxEffectBinding();

            for (int i = 0; i < maxTexturesCount; i++)
            {
                var textureName = "Texture" + (i + 1);
                var textureSetup = textureMaterial.TextureSetups[i];

                materialEffectBinding.Set(textureName, !String.IsNullOrWhiteSpace(textureSetup.DiffuseMapPath) ? 
                        contentLoader.LoadTexture(textureSetup.DiffuseMapPath) 
                        : whiteColorTexture);

                if(BuildPlatform.IsWindows)
                    materialEffectBinding.Set(textureName + "Tiling", textureSetup.UVTiling.ToVector2F());
            }

            var materialPass = BuildPlatform.IsWindows ? "Material" : "Default";
            material.Add(materialPass, materialEffectBinding);

            return material;
        }



        private EffectBinding CreateMacLinuxEffectBinding()
        {
            var materialEffectBinding = new EffectBinding(_graphicsService, new Effect(_graphicsService.GraphicsDevice, EmbeddedResources.Load<byte[]>("Resources/Material_Terrain.mgfx")), null, EffectParameterHint.Material);
            materialEffectBinding.Set("DiffuseColor", new Vector3(3, 3, 3));

            return materialEffectBinding;
        }



        private EffectBinding CreateWindowsEffectBinding(ContentLoader contentLoader, Texture2D whiteColorTexture, MultiTextureSurfaceMaterial textureMaterial, Material material)
        {
            Texture2D basicTexture = whiteColorTexture;
            if (textureMaterial.TextureSetups.Any() && File.Exists(textureMaterial.TextureSetups[0].DiffuseMapPath))
                basicTexture = contentLoader.LoadTexture(textureMaterial.TextureSetups[0].DiffuseMapPath);


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
            EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/Material"), null, EffectParameterHint.Material);
            materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1));
            materialEffectBinding.Set("SpecularColor", new Vector3(0f, 0f, 0f));


            return materialEffectBinding;
        }
    }
}