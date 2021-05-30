using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Materials;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Sceelix.Meshes.Operations;
using Material = Sceelix.Actors.Data.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(ReflectiveMaterial))]
    public class ReflectiveMaterialTranslator : MaterialTranslator, IMeshMaterialTranslator, IServiceable
    {
        private IGraphicsService _graphicsService;
        private ReflectiveMeshMaterialTranslator _materialTranslator;
        

        public void Initialize(IServiceLocator services)
        {
            _graphicsService = services.Get<IGraphicsService>();

            _materialTranslator = new ReflectiveMeshMaterialTranslator();
            _materialTranslator.Initialize(services);

            // ----- Workaround for missing effect parameter semantics in MonoGame.
            // The effect used by the reflecting ground object defines some new effect
            // parameters and sets the EffectParameterHint to "PerInstance", e.g.:
            //   texture ReflectionTexture < string Hint = "PerInstance"; >;
            // "PerInstance" means that each mesh instance which uses the effect can 
            // have an individual parameter value, i.e. if there are two instances
            // each instance needs a different ReflectionTexture.
            // MonoGame does not yet support effect parameter annotations in shader 
            // code. But we can add the necessary effect parameter descriptions here:
            var effectInterpreter = _graphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();
            if (!effectInterpreter.ParameterDescriptions.ContainsKey("ReflectionTexture"))
            {
                effectInterpreter.ParameterDescriptions.Add("ReflectionTexture", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionTexture", index, EffectParameterHint.PerInstance));
                effectInterpreter.ParameterDescriptions.Add("ReflectionTextureSize", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionTextureSize", index, EffectParameterHint.PerInstance));
                effectInterpreter.ParameterDescriptions.Add("ReflectionMatrix", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionMatrix", index, EffectParameterHint.PerInstance));
                effectInterpreter.ParameterDescriptions.Add("ReflectionNormal", (parameter, index) => new EffectParameterDescription(parameter, "ReflectionNormal", index, EffectParameterHint.PerInstance));
            }
        }



        public override SceneNode CreateSceneNode(ContentLoader contentLoader, Material sceelixMaterial, object data)
        {
            SceneNodeCollection collection = new SceneNodeCollection();

            List<Face> faces = (List<Face>) data;

            foreach (Face face in faces)
            {
                //var groundModel = _contentManager.Load<ModelNode>("Materials/Ground");
                //var meshNode = groundModel.GetSubtree().OfType<MeshNode>().First().Clone();

                MeshNode meshNode = (MeshNode) _materialTranslator.CreateSceneNode(contentLoader, sceelixMaterial, new List<Face>(1) {face});

                var renderToTexture0 = new RenderToTexture
                {
                    Texture = new RenderTarget2D(
                        _graphicsService.GraphicsDevice,
                        256, 256,
                        false, // No mipmaps. Mipmaps can reduce reflection quality.
                        SurfaceFormat.HdrBlendable,
                        DepthFormat.Depth24Stencil8),
                };
                var planarReflectionNode0 = new PlanarReflectionNode(renderToTexture0)
                {
                    // The reflection is limited to the bounding MeshEntity of the ground mesh.
                    Shape = CalculateTriangleMesh(face),

                    //MeshEntity = meshNode.MeshEntity,
                    // The normal of the reflection plane.
                    NormalLocal = face.Normal.ToVector3F() //new Vector3F(0, 1, 0)//face.Normal.ToVector3F()//new Vector3F(0, 0, 1)//face.Normal.ToVector3F()//new Vector3F(0, 1, 0)//
                };

                meshNode.Children = new SceneNodeCollection(1) {planarReflectionNode0};
                
                if(BuildPlatform.IsWindows)
                    SetReflectionEffectParameters(meshNode, planarReflectionNode0);

                //return new SceneNode();
                //return meshNode;
                collection.Add(meshNode);
            }

            return new SceneNode() {Children = collection};
        }



        private static void SetReflectionEffectParameters(MeshNode meshNode, PlanarReflectionNode planarReflectionNode)
        {
            // Loop through the materials of the mesh. The material uses the effect 
            // GroundReflective/MaterialReflective.fx.
            foreach (var materialInstance in meshNode.MaterialInstances)
            {
                // Get effect binding for the "Material" render pass. (Not the "GBuffer" or other passes.)
                var effectBinding = materialInstance["Material"];

                // Set reflection texture and size parameters.
                var texture = (Texture2D) planarReflectionNode.RenderToTexture.Texture;
                effectBinding.Set<Texture>("ReflectionTexture", texture);
                effectBinding.Set<Vector2>("ReflectionTextureSize", new Vector2(texture.Width, texture.Height));

                // The reflection texture matrix and the reflection normal may change over
                // time. Therefore, we need to set a delegate that updates the value once
                // per frame.
                effectBinding.Set<Matrix>("ReflectionMatrix", (binding, context) => (Matrix)planarReflectionNode.RenderToTexture.TextureMatrix);//(binding, context) => (Matrix) planarReflectionNode.RenderToTexture.TextureMatrix
                effectBinding.Set<Vector3>("ReflectionNormal", (binding, context) => (Vector3) planarReflectionNode.NormalWorld);
            }
        }

        


        private TriangleMeshShape CalculateTriangleMesh(Face face)
        {
            IEnumerable<FaceTriangle> triangles = face.Triangulate();

            //change winding order
            TriangleMesh triangleMesh = new TriangleMesh();

            foreach (FaceTriangle triangle in triangles)
                triangleMesh.Add(new DigitalRune.Geometry.Shapes.Triangle(triangle.V0.Position.ToVector3F(), triangle.V2.Position.ToVector3F(), triangle.V1.Position.ToVector3F()));

            return new TriangleMeshShape(triangleMesh);
        }
    }
}