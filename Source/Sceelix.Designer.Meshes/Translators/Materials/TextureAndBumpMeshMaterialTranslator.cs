using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Actors.VertexTypes;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(TextureAndBumpMaterial))]
    public class TextureAndBumpMeshMaterialTranslator : MeshMaterialTranslator<VertexPositionNormalTextureTangents>
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


        protected override VertexPositionNormalTextureTangents PrepareVertex(Material textureMaterial, Face face, Vertex vertex)
        {
            VertexPositionNormalTextureTangents customVertex = new VertexPositionNormalTextureTangents();

            customVertex.Position = vertex.Position.ToVector3();

            customVertex.Normal = vertex[face].Normal.ToVector3();
            customVertex.TextureCoordinate = vertex[face].UV0.ToVector2();
            customVertex.Tangent = vertex[face].Tangent.ToVector3();
            customVertex.Binormal = vertex[face].Binormal.ToVector3();

            return customVertex;
        }



        protected override DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial)
        {
            TextureAndBumpMaterial textureAndBumpMaterial = (TextureAndBumpMaterial) sceelixMaterial;

            var defaultTexture = Texture2DExtender.CreateColorTexture(_graphicsService.GraphicsDevice, Color.White);

            Texture2D texture = contentLoader.LoadTexture(textureAndBumpMaterial.DiffuseTexture) ?? defaultTexture;
            Texture2D bumptexture = contentLoader.LoadTexture(textureAndBumpMaterial.NormalTexture).ToNormalTexture();
            Texture2D specularTexture = !String.IsNullOrWhiteSpace(textureAndBumpMaterial.SpecularTexture) ? contentLoader.LoadTexture(textureAndBumpMaterial.SpecularTexture) : null;


            //bumptexture = contentLoader.TextureLoader.BuildMipMapGPUSync(bumptexture);
            //bumptexture = content.Load<Texture2D>("Materials/Pavement/Pavement_NRM");

            DigitalRune.Graphics.Material material = new DigitalRune.Graphics.Material();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false};
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
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBufferAlphaTestNormal"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("DiffuseTexture", texture);
                gBufferEffectBinding.Set("NormalTexture", bumptexture);
                gBufferEffectBinding.Set("SpecularPower", 100f);
                gBufferEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                material.Add("GBuffer", gBufferEffectBinding);

                // EffectBinding for the "Material" pass.
                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/MaterialAlphaTest"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseTexture", texture);

                if (specularTexture != null)
                    materialEffectBinding.Set("SpecularTexture", specularTexture);

                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1)); //  * 2f
                materialEffectBinding.Set("SpecularColor", new Vector3(0.2f, 0.2f, 0.2f));
                materialEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                material.Add("Material", materialEffectBinding);
            }
            
            return material;
        }
    }
}