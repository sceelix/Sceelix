using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Materials;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Renderer3D.SceneNodes;
using Sceelix.Designer.Services;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Sceelix.Meshes.Operations;
using Material = Sceelix.Actors.Data.Material;
using Triangle = DigitalRune.Geometry.Shapes.Triangle;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    [MaterialTranslator(typeof(WaterMaterial))]
    public class WaterMaterialTranslator : MaterialTranslator, IMeshMaterialTranslator, IServiceable
    {
        private ContentManager _contentManager;
        private IScene _scene;
        private RenderTargetControl _render3DControl;


        public void Initialize(IServiceLocator services)
        {
            _contentManager = services.Get<ContentManager>();
            _scene = services.Get<IScene>();
            _render3DControl = services.Get<RenderTargetControl>();
        }



        public override SceneNode CreateSceneNode(ContentLoader contentLoader, Material sceelixMaterial, object data)
        {
            List<Face> faces = (List<Face>) data;

            var water = new Water
            {
                SpecularColor = new Vector3F(10f),

                // Small water ripples/waves are created using scrolling normal maps.
                NormalMap0 = _contentManager.Load<Texture2D>("Materials/Wave0"),
                NormalMap1 = _contentManager.Load<Texture2D>("Materials/Wave1"),
                NormalMap0Scale = 1.8f,
                NormalMap1Scale = 2.2f,
                NormalMap0Velocity = new Vector3F(-0.02f, 0, 0.03f),
                NormalMap1Velocity = new Vector3F(0.02f, 0, -0.03f),
                NormalMap0Strength = 0.5f,
                NormalMap1Strength = 0.5f,
                ReflectionDistortion = 0.2f,
                ReflectionColor = new Vector3F(0.7f),
                RefractionDistortion = 0.05f,
            };

            var waterNode = new WaterNode(water, CalculateTriangleMesh(faces)) //CalculateTriangleMesh(waterShape.MeshEntity)
            {
                //PoseWorld = new Pose(new Vector3F(10, 1.5f, 0), Matrix33F.CreateRotationY(0.1f)),
                EnableUnderwaterEffect = false,
                SkyboxReflection = _scene.Children.OfType<SkyboxNode>().FirstOrDefault(),
                Flow = new WaterFlow
                {
                    SurfaceSlopeSpeed = 0.5f,
                    CycleDuration = 2f,
                    NoiseMapStrength = 0.1f,
                    NoiseMapScale = 1,
                },
                DepthBufferWriteEnable = true,
                Children = new SceneNodeCollection()
            };


            //force 
            var dynamicNode = new DynamicNode();
            dynamicNode.Updated += delegate { _render3DControl.ShouldRender = true; };
            waterNode.Children.Add(dynamicNode);

            return waterNode;
        }



        private TriangleMeshShape CalculateTriangleMesh(IEnumerable<Face> faces)
        {
            IEnumerable<FaceTriangle> triangles = faces.SelectMany(val => val.Triangulate());

            //change winding order
            TriangleMesh triangleMesh = new TriangleMesh();

            foreach (FaceTriangle triangle in triangles)
                triangleMesh.Add(new Triangle(triangle.V0.Position.ToVector3F(), triangle.V2.Position.ToVector3F(), triangle.V1.Position.ToVector3F()));

            return new TriangleMeshShape(triangleMesh);
        }
    }
}