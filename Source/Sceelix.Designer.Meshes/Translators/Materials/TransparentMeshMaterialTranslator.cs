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
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(TransparentMaterial))]
    public class TransparentMeshMaterialTranslator : MeshMaterialTranslator<VertexPositionNormalTexture>
    {
        private ContentManager _content;
        private IGraphicsService _graphicsService;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _content = services.Get<ContentManager>();
            _graphicsService = services.Get<IGraphicsService>();
        }

        protected override VertexPositionNormalTexture PrepareVertex(Material sceelixMaterial, Face face, Vertex vertex)
        {
            VertexPositionNormalTexture customVertex = new VertexPositionNormalTexture();

            customVertex.Position = vertex.Position.ToVector3();

            customVertex.Normal = vertex[face].Normal.ToVector3();
            customVertex.TextureCoordinate = vertex[face].UV0.ToVector2();

            return customVertex;
        }



        protected override DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial)
        {
            TransparentMaterial transparentMaterial = (TransparentMaterial) sceelixMaterial;

            DigitalRune.Graphics.Material material = new DigitalRune.Graphics.Material();
            Texture2D texture = contentLoader.LoadTexture(transparentMaterial.Texture);

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false};
            basicEffectBinding.Set("Texture", texture);
            material.Add("Default", basicEffectBinding);


            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
                material.Add("ShadowMap", shadowMapEffectBinding);

                // EffectBinding for the "GBuffer" pass.
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBufferTransparent"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("SpecularPower", 1000f);
                material.Add("GBuffer", gBufferEffectBinding);

                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ForwardTwoSided"));
                materialEffectBinding.Set("DiffuseTexture", texture);
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1));
                materialEffectBinding.Set("SpecularColor", new Vector3(0.2f, 0.2f, 0.2f));
                materialEffectBinding.Set("Alpha", transparentMaterial.Transparency);
                material.Add("AlphaBlend", materialEffectBinding);
            }
            
            return material;
        }
    }
}