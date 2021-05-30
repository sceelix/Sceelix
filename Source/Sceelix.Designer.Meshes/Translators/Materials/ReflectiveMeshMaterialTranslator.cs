using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Actors.VertexTypes;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    public class ReflectiveMeshMaterialTranslator : MeshMaterialTranslator<VertexPositionNormalTextureTangents>
    {
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
            var reflectiveMaterial = (ReflectiveMaterial) sceelixMaterial;

            Texture2D texture = contentLoader.LoadTexture(reflectiveMaterial.DiffuseTexture);
            Texture2D normalTexture = contentLoader.LoadTexture(reflectiveMaterial.NormalTexture);

            normalTexture = GetNormal(normalTexture);

            //Texture2D texture = content.Load<Texture2D>("Materials/Marble_Diffuse_0");
            //Texture2D normalTexture = content.Load<Texture2D>("Materials/Marble_Normal_0");
            //Color[] color = new Color[100];
            //normalTexture = GetNormal(normalTexture);
            //new Texture2D(SurfaceFormat.Dxt5)
            
            //normalTexture.GetData(color,0,100);
            DigitalRune.Graphics.Material material = new DigitalRune.Graphics.Material();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false,};
            basicEffectBinding.Set("Texture", texture);
            material.Add("Default", basicEffectBinding);

            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
                //shadowMapEffectBinding.Set("DiffuseTexture", texture);
                //shadowMapEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                //shadowMapEffectBinding.Set("ScaleAlphaToCoverage", true);
                //shadowMapEffectBinding.Set("SpecularPower", 1000f);
                material.Add("ShadowMap", shadowMapEffectBinding);

                // EffectBinding for the "GBuffer" pass.
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBufferNormal"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("NormalTexture", normalTexture);
                gBufferEffectBinding.Set("SpecularPower", 1000f);
                //gBufferEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                material.Add("GBuffer", gBufferEffectBinding);

                // EffectBinding for the "Material" pass.
                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/MaterialReflective_0"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseTexture", texture);
                materialEffectBinding.Set("DiffuseColor", new Vector3(0.7f, 0.7f, 0.7f)); //  * 2f
                materialEffectBinding.Set("SpecularColor", new Vector3(0.4f, 0.4f, 0.4f));
                //materialEffectBinding.Set("ReferenceAlpha", ReferenceAlpha);
                material.Add("Material", materialEffectBinding);
            }

            return material;
        }



        private Texture2D GetNormal(Texture2D normalTexture)
        {
            var editableTexture = new MipEditableTexture2D(normalTexture);
            for (int k = 0; k < editableTexture.LevelCount; k++)
            {
                var editableSubTexture = editableTexture[k];
                for (int i = 0; i < editableSubTexture.Width; i++)
                {
                    for (int j = 0; j < editableSubTexture.Height; j++)
                    {
                        editableSubTexture[i, j] = new Color(0, editableSubTexture[i, j].G, 0, editableSubTexture[i, j].R);
                    }
                }
            }

            return editableTexture.ToTexture2D();
        }
    }
}