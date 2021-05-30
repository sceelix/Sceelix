using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using ScMaterial = Sceelix.Actors.Data.Material;
using DrMaterial = DigitalRune.Graphics.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(FurTextureMaterial))]
    public class FurMeshMaterialTranslator : MeshMaterialTranslator<VertexPositionNormalTexture>
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



        protected override VertexPositionNormalTexture PrepareVertex(ScMaterial sceelixMaterial, Face face, Vertex vertex)
        {
            VertexPositionNormalTexture customVertex = new VertexPositionNormalTexture();

            customVertex.Position = vertex.Position.ToVector3();

            customVertex.Normal = vertex[face].Normal.ToVector3();
            customVertex.TextureCoordinate = vertex[face].UV0.ToVector2();

            return customVertex;
        }



        protected override DrMaterial CreateMaterial(ContentLoader contentLoader, ScMaterial sceelixMaterial)
        {
            FurTextureMaterial furTextureMaterial = (FurTextureMaterial) sceelixMaterial;
            Texture2D texture = contentLoader.LoadTexture(furTextureMaterial.Texture);


            DrMaterial material = new DrMaterial();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false,};
            basicEffectBinding.Set("Texture", texture);
            material.Add("Default", basicEffectBinding);

            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
                material.Add("ShadowMap", shadowMapEffectBinding);

                // EffectBinding for the "GBuffer" pass.
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBuffer"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("SpecularPower", 100f);
                material.Add("GBuffer", gBufferEffectBinding);

                // EffectBinding for the "Material" pass.
                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/Material"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseTexture", texture);
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1)); //  * 2f
                materialEffectBinding.Set("SpecularColor", new Vector3(0.0f, 0.0f, 0.0f));
                material.Add("Material", materialEffectBinding);
                
                EffectBinding alphaEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/Fur_2"), null, EffectParameterHint.Material);
                alphaEffectBinding.Set("MaxFurLength", furTextureMaterial.MaxFurLength);
                alphaEffectBinding.Set("FurDensity", furTextureMaterial.FurDensity); //  * 2f
                alphaEffectBinding.Set("SelfShadowStrength", furTextureMaterial.SelfShadowStrength);
                alphaEffectBinding.Set("JitterMapScale", furTextureMaterial.JitterMapScale);
                alphaEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1));
                alphaEffectBinding.Set("SpecularColor", new Vector3(0.0f, 0.0f, 0.0f));
                alphaEffectBinding.Set("DiffuseTexture", texture);
                alphaEffectBinding.Set("Alpha", 1f);
                

                material.Add("AlphaBlend", alphaEffectBinding);
            }


            return material;
        }
    }
}