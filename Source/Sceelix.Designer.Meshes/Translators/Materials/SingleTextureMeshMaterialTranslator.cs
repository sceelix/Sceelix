using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(SingleTextureMaterial))]
    public class SingleTextureMeshMaterialTranslator : MeshMaterialTranslator<VertexPositionNormalTexture>
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



        protected override VertexPositionNormalTexture PrepareVertex(Material textureMaterial, Face face, Vertex vertex)
        {
            VertexPositionNormalTexture customVertex = new VertexPositionNormalTexture();

            customVertex.Position = vertex.Position.ToVector3();
            customVertex.Normal = vertex[face].Normal.ToVector3();
            customVertex.TextureCoordinate = vertex[face].UV0.ToVector2();

            return customVertex;
        }



        protected override DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial)
        {
            var textureMaterial = (SingleTextureMaterial) sceelixMaterial;


            var defaultTexture = Texture2DExtender.CreateColorTexture(_graphicsService.GraphicsDevice, Color.White);

            Texture2D texture = contentLoader.LoadTexture(textureMaterial.Texture) ?? defaultTexture;

            DigitalRune.Graphics.Material material = new DigitalRune.Graphics.Material();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false};

            //basicEffectBinding.Set("DiffuseColor", new Vector4(1, 0.7f, 0.7f, 1));
            //basicEffectBinding = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffectBinding.Set("Texture", texture);
            material.Add("Default", basicEffectBinding);


            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMapAlphaTest"), null, EffectParameterHint.Material);
                shadowMapEffectBinding.Set("DiffuseTexture", texture);
                shadowMapEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                //shadowMapEffectBinding.Set("ScaleAlphaToCoverage", true);
                material.Add("ShadowMap", shadowMapEffectBinding);

                // EffectBinding for the "GBuffer" pass.
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBufferAlphaTest"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("DiffuseTexture", texture);
                gBufferEffectBinding.Set("SpecularPower", 100f);
                gBufferEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                material.Add("GBuffer", gBufferEffectBinding);

                // EffectBinding for the "Material" pass.
                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/MaterialAlphaTest"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseTexture", texture);
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1)); //  * 2f
                materialEffectBinding.Set("SpecularColor", new Vector3(0.2f, 0.2f, 0.2f));
                materialEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                material.Add("Material", materialEffectBinding);
            }

            return material;
        }
    }
}