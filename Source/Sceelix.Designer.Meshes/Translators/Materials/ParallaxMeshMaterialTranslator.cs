using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Actors.VertexTypes;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using ScMaterial = Sceelix.Actors.Data.Material;
using DrMaterial = DigitalRune.Graphics.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(ParallaxOcclusionMaterial))]
    public class ParallaxMeshMaterialTranslator : MeshMaterialTranslator<VertexPositionNormalTextureTangents>
    {
        private ContentManager _content;
        private IGraphicsService _graphicsService;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _content = services.Get<ContentManager>();
            _graphicsService = services.Get<IGraphicsService>();
        }



        protected override VertexPositionNormalTextureTangents PrepareVertex(ScMaterial textureMaterial, Face face, Vertex vertex)
        {
            VertexPositionNormalTextureTangents customVertex = new VertexPositionNormalTextureTangents();

            customVertex.Position = vertex.Position.ToVector3();

            customVertex.Normal = vertex[face].Normal.ToVector3();
            customVertex.TextureCoordinate = vertex[face].UV0.ToVector2();
            customVertex.Tangent = vertex[face].Tangent.ToVector3();
            customVertex.Binormal = vertex[face].Binormal.ToVector3();

            return customVertex;
        }



        protected override DrMaterial CreateMaterial(ContentLoader contentLoader, ScMaterial sceelixMaterial)
        {
            var parallaxOcclusionMaterial = (ParallaxOcclusionMaterial) sceelixMaterial;
            var diffuseTexture = contentLoader.LoadTexture(parallaxOcclusionMaterial.DiffuseTexture);
            var normalTexture = contentLoader.LoadTexture(parallaxOcclusionMaterial.NormalTexture).ToNormalTexture();
            var displacementTexture = contentLoader.LoadTexture(parallaxOcclusionMaterial.HeightTexture);
            var specularTexture = string.IsNullOrWhiteSpace(parallaxOcclusionMaterial.SpecularTexture) ? TextureLoader.CreateColorTexture(_graphicsService.GraphicsDevice, Color.White) : contentLoader.LoadTexture(parallaxOcclusionMaterial.SpecularTexture);
            var heightTextureSize = displacementTexture != null ? new Vector2(displacementTexture.Width, displacementTexture.Height) : Vector2.One;


            Material material = new Material();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false,};
            basicEffectBinding.Set("SpecularPower", 100f);
            basicEffectBinding.Set("Texture", diffuseTexture);
            material.Add("Default", basicEffectBinding);
            
            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
                material.Add("ShadowMap", shadowMapEffectBinding);

                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/GBufferPom_0"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("SpecularPower", 100f);
                gBufferEffectBinding.Set("NormalTexture", normalTexture);
                gBufferEffectBinding.Set("HeightScale", 0.05f);
                gBufferEffectBinding.Set("HeightBias", 0.0f);
                gBufferEffectBinding.Set("HeightTexture", displacementTexture);
                gBufferEffectBinding.Set("HeightTextureSize", heightTextureSize);
                gBufferEffectBinding.Set("LodThreshold", 6);
                gBufferEffectBinding.Set("MinSamples", 4);
                gBufferEffectBinding.Set("MaxSamples", 9);
                material.Add("GBuffer", gBufferEffectBinding);

                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/MaterialPom_0"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1));
                materialEffectBinding.Set("SpecularColor", new Vector3(1, 1, 1));
                materialEffectBinding.Set("DiffuseTexture", diffuseTexture);
                materialEffectBinding.Set("SpecularTexture", specularTexture);
                materialEffectBinding.Set("HeightScale", 0.05f);
                materialEffectBinding.Set("HeightBias", 0.0f);
                materialEffectBinding.Set("HeightTexture", displacementTexture);
                materialEffectBinding.Set("HeightTextureSize", heightTextureSize);
                materialEffectBinding.Set("LodThreshold", 6);
                materialEffectBinding.Set("MinSamples", 4);
                materialEffectBinding.Set("MaxSamples", 9);
                materialEffectBinding.Set("ShadowScale", 0.03f);
                materialEffectBinding.Set("ShadowSamples", 4);
                materialEffectBinding.Set("ShadowFalloff", 0.5f);
                materialEffectBinding.Set("ShadowStrength", 100f);
                material.Add("Material", materialEffectBinding);
            }

            return material;
        }



        private Texture2D GetDisplacement(Texture2D displacementTexture)
        {
            var editableTexture2D = new MipEditableTexture2D(displacementTexture);
            for (int k = 0; k < editableTexture2D.LevelCount; k++)
            {
                var editableSubTexture = editableTexture2D[k];
                for (int i = 0; i < editableSubTexture.Width; i++)
                {
                    for (int j = 0; j < editableSubTexture.Height; j++)
                    {
                        editableSubTexture[i, j] = new Color(255, 255, 255, editableSubTexture[i, j].R);
                    }
                }
            }
            return editableTexture2D.ToTexture2D();
        }
    }
}