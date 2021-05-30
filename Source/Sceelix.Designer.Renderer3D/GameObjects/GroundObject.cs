using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Renderer3D.Services;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    public struct VertexPositionNormalTextureTangents : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Binormal;



        public VertexPositionNormalTextureTangents(Vector3 position)
            : this()
        {
            Position = position;
        }



        public VertexPositionNormalTextureTangents(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent, Vector3 binormal)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
            Normal = normal;
            Tangent = tangent;
            Binormal = binormal;
        }



        private readonly static VertexDeclaration StaticVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float)*8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(sizeof(float)*11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
        );



        public VertexDeclaration VertexDeclaration
        {
            get { return StaticVertexDeclaration; }
        }
    }


    // Loads a ground plane model and creates a static rigid body for the ground plane.
    public class GroundObject : GameObject
    {
        private readonly IServiceLocator _services;
        private ModelNode _modelNode;
        private RigidBody _rigidBody;



        public GroundObject(IServiceLocator services)
        {
            _services = services;
            Name = "Ground";
        }



        public Mesh CreateQuadMesh()
        {
            var graphicsService = _services.Get<IGraphicsService>();
            var content = _services.Get<ContentManager>();

            Mesh mesh = new Mesh();
            mesh.Submeshes.Add(CreateQuad(graphicsService.GraphicsDevice));
            mesh.Materials.Add(content.Load<Material>("Materials/POMX"));

            return mesh;
        }



        public Mesh CreateQuadMesh2()
        {
            var graphicsService = _services.Get<IGraphicsService>();
            var content = _services.Get<ContentManager>();

            Mesh mesh = new Mesh();
            mesh.Submeshes.Add(CreateQuad2(graphicsService.GraphicsDevice));
            mesh.Materials.Add(content.Load<Material>("Materials\\POMX"));

            return mesh;
        }



        private Submesh CreateQuad(GraphicsDevice device)
        {
            List<VertexPositionNormalTextureTangents> customVertexList = new List<VertexPositionNormalTextureTangents>();

            //customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, -1000), Vector3.UnitY, new Vector2(0, 250),new Vector3(0,0,-1),new Vector3(-1,0,0)));
            //customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, -1000), Vector3.UnitY, new Vector2(250, 250), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));
            //customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, 1000), Vector3.UnitY, new Vector2(250, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));
            //customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, 1000), Vector3.UnitY, new Vector2(0, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));

            /*customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, -1000), Vector3.UnitY, new Vector2(250, 250), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, -1000), Vector3.UnitY, new Vector2(250, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, 1000), Vector3.UnitY, new Vector2(0, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, 1000), Vector3.UnitY, new Vector2(0, 250), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));
            */

            /*
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, -1000), Vector3.UnitY, new Vector2(250, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, -1000), Vector3.UnitY, new Vector2(250, 250), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, 1000), Vector3.UnitY, new Vector2(0, 250), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, 1000), Vector3.UnitY, new Vector2(0, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            */

            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-10, 10, 0), Vector3.UnitZ, new Vector2(0, 2.5f), new Vector3(1, 0, 0), new Vector3(0, 1, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(10, 10, 0), Vector3.UnitZ, new Vector2(2.5f, 2.5f), new Vector3(1, 0, 0), new Vector3(0, 1, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(10, -10, 0), Vector3.UnitZ, new Vector2(2.5f, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-10, -10, 0), Vector3.UnitZ, new Vector2(0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0)));

            /*customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, -1000), Vector3.UnitY, new Vector2(250, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, -1000), Vector3.UnitY, new Vector2(250, 250), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, 1000), Vector3.UnitY, new Vector2(0, 250), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 0, 1000), Vector3.UnitY, new Vector2(0, 250), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, 1000), Vector3.UnitY, new Vector2(0, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 0, -1000), Vector3.UnitY, new Vector2(250, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            */

            /*customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, -1000,0), Vector3.UnitZ, new Vector2(0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, 1000, 0), Vector3.UnitZ, new Vector2(0, 250), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 1000, 0), Vector3.UnitZ, new Vector2(250, 250), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, 1000, 0), Vector3.UnitZ, new Vector2(250, 250), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(1000, -1000, 0), Vector3.UnitZ, new Vector2(250, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-1000, -1000, 0), Vector3.UnitZ, new Vector2(0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0)));*/

            var indices = new int[] {0, 1, 2, 2, 3, 0};
            //var indices = new int[] { 0, 1, 2, 3, 4, 5 };

            //now create the buffers
            VertexDeclaration declaration = customVertexList[0].VertexDeclaration;

            var submesh = new Submesh
            {
                PrimitiveType = PrimitiveType.TriangleList,
                PrimitiveCount = 2,
                VertexCount = customVertexList.Count,
            };


            submesh.VertexBuffer = new VertexBuffer(device, declaration, customVertexList.Count, BufferUsage.None);
            VertexPositionNormalTextureTangents[] array = customVertexList.ToArray();
            submesh.VertexBuffer.SetData(array);

            submesh.IndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.None);
            submesh.IndexBuffer.SetData(indices);

            return submesh;
        }



        private Submesh CreateQuad2(GraphicsDevice device)
        {
            List<VertexPositionNormalTextureTangents> customVertexList = new List<VertexPositionNormalTextureTangents>();


            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-10, 0, -10), Vector3.UnitY, new Vector2(2.5f, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(10, 0, -10), Vector3.UnitY, new Vector2(2.5f, 2.5f), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(10, 0, 10), Vector3.UnitY, new Vector2(0, 2.5f), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));
            customVertexList.Add(new VertexPositionNormalTextureTangents(new Vector3(-10, 0, 10), Vector3.UnitY, new Vector2(0, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0)));

            var indices = new int[] {0, 1, 2, 2, 3, 0};
            //var indices = new int[] { 0, 1, 2, 3, 4, 5 };

            //now create the buffers
            VertexDeclaration declaration = customVertexList[0].VertexDeclaration;

            var submesh = new Submesh
            {
                PrimitiveType = PrimitiveType.TriangleList,
                PrimitiveCount = 2,
                VertexCount = customVertexList.Count,
            };


            submesh.VertexBuffer = new VertexBuffer(device, declaration, customVertexList.Count, BufferUsage.None);
            VertexPositionNormalTextureTangents[] array = customVertexList.ToArray();
            submesh.VertexBuffer.SetData(array);

            submesh.IndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.None);
            submesh.IndexBuffer.SetData(indices);

            return submesh;
        }



        // OnLoad() is called when the GameObject is added to the IGameObjectService.
        protected override void OnLoad()
        {
            // Load model.
            var contentManager = _services.Get<ContentManager>();
            _modelNode = contentManager.Load<ModelNode>("Materials/Ground").Clone(); //Deferred/Ground/Ground
            _modelNode.ScaleLocal = new Vector3F(0.5f);

            var meshNode = new MeshNode(CreateQuadMesh()) {PoseWorld = Pose.Identity};
            //meshNode.PoseLocal = new Pose(Matrix33F.CreateRotationX(-MathHelper.PiOver2));

            //var meshNode = _modelNode.Children.OfType<MeshNode>().First().Clone();
            //meshNode.Mesh.Materials[0] = contentManager.Load<Material>("Materials\\POMX");
            //meshNode.Mesh = meshNode.Mesh;

            //var meshNode = _modelNode.Children.OfType<MeshNode>().First().Clone();
            meshNode.Mesh.Materials[0] = contentManager.Load<Material>("Materials/POM");
            meshNode.Mesh = meshNode.Mesh;

            //VertexPositionNormalTextureTangents[] tangentses = new VertexPositionNormalTextureTangents[4];

            //meshNode.Mesh.Submeshes[0].VertexBuffer.GetData(tangentses);

            /*foreach (var node in _modelNode.GetSubtree())
            {
                // Disable the CastsShadows flag for ground meshes. No need to render
                // this model into the shadow map. (This also avoids any shadow acne on 
                // the ground model.)
                node.CastsShadows = false;

                // If models will never move, set the IsStatic flag. This gives the engine 
                // more room for optimizations. Additionally, some effects, like certain 
                // decals, may only affect static geometry.
                node.IsStatic = true;
            }*/

            // Add model node to scene graph.
            var scene = _services.Get<IScene>();
            scene.Children.Add(meshNode);

            // Create rigid body.
            _rigidBody = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
            {
                MotionType = MotionType.Static,
            };

            // Add rigid body to the physics simulation.
            var simulation = _services.Get<SimulationManager>().Simulation;
            simulation.RigidBodies.Add(_rigidBody);
        }



        // OnUnload() is called when the GameObject is removed from the IGameObjectService.
        protected override void OnUnload()
        {
            // Remove model and rigid body.
            _modelNode.Parent.Children.Remove(_modelNode);
            _modelNode.Dispose(false);
            _modelNode = null;

            _rigidBody.Simulation.RigidBodies.Remove(_rigidBody);
            _rigidBody = null;
        }
    }
}