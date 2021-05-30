using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Services;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    // Loads a ground plane model and creates a static rigid body for the ground plane.
    public class WhiteGroundObject : GameObject
    {
        private readonly IServiceLocator _services;
        //private TextureLoader _loader;
        private readonly CameraObject _cameraObject;
        private SceneNode _groundNode;
        private readonly int _numTilesX = 3;
        private readonly int _numTilesY = 3;
        //private readonly Renderer3DControl _render3DControl;
        //private MeshNode _meshNode;
        private RigidBody _rigidBody;
        private readonly float _tileSize = 10000;
        private int _previousIndexX, _previousIndexY;
        private MeshNode[,] _groundTiles;
        private Vector3F _lastMousePosition;

        private Renderer3DControl _render3DControl;
        private Renderer3DWindow _window;

        private IInputService _inputService;
        //private Vector3F _globalOffset;



        public WhiteGroundObject(IServiceLocator services)
        {
            _services = services;
            Name = "WhiteGround";
            
            _cameraObject = _services.Get<CameraObject>();
            _render3DControl = _services.Get<Renderer3DControl>();
            _window = _services.Get<Renderer3DWindow>();
            _inputService = _services.Get<IInputService>();
        }
        


        public Mesh CreateQuadMesh()
        {
            var graphicsService = _services.Get<IGraphicsService>();
            var content = _services.Get<ContentManager>();
            
            Texture2D texture = content.Load<Texture2D>("Textures/GridPatternGround");

            Material material = BuildPlatform.IsWindows ? 
                    LoadWindowsMaterial(graphicsService, content, texture)
                        : LoadSimpleMaterial(graphicsService, texture);

            Mesh mesh = new Mesh();
            mesh.Submeshes.Add(CreateQuad(graphicsService.GraphicsDevice));
            mesh.Materials.Add(material);

            return mesh;
        }



        private Material LoadSimpleMaterial(IGraphicsService graphicsService, Texture2D texture)
        {
            Material material = new Material();

            BasicEffectBinding defaultEffectBinding = new BasicEffectBinding(graphicsService, null)
            {
                LightingEnabled = false,
                TextureEnabled = true,
                VertexColorEnabled = false,
                //PreferPerPixelLighting = true
            };

            defaultEffectBinding.Set("Texture", texture);
            defaultEffectBinding.Set("DiffuseColor", new Vector4(new Vector3(1f, 1f, 1f), 1));
            material.Add("Default", defaultEffectBinding);

            return material;
        }



        private Material LoadWindowsMaterial(IGraphicsService graphicsService, ContentManager content, Texture2D texture)
        {
            Material material = new Material();

            EffectBinding shadowMapEffectBinding = new EffectBinding(graphicsService, content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
            //shadowMapEffectBinding.Set("DiffuseTexture", texture);
            //shadowMapEffectBinding.Set("ReferenceAlpha", 0.5f);
            //shadowMapEffectBinding.Set("ScaleAlphaToCoverage", true);
            material.Add("ShadowMap", shadowMapEffectBinding);


            // EffectBinding for the "GBuffer" pass.
            EffectBinding gBufferEffectBinding = new EffectBinding(graphicsService, content.Load<Effect>("DigitalRune/Materials/GBuffer"), null, EffectParameterHint.Material);
            //gBufferEffectBinding.Set("DiffuseTexture", texture);
            gBufferEffectBinding.Set("SpecularPower", 10f);
            //gBufferEffectBinding.Set("ReferenceAlpha", 0.5f);
            material.Add("GBuffer", gBufferEffectBinding);


            // EffectBinding for the "Material" pass.
            EffectBinding materialEffectBinding = new EffectBinding(graphicsService, content.Load<Effect>("DigitalRune/Materials/Material"), null, EffectParameterHint.Material);
            materialEffectBinding.Set("DiffuseTexture", texture);
            materialEffectBinding.Set("DiffuseColor", new Vector3(1f, 1f, 1f)); //  * 2f
            materialEffectBinding.Set("SpecularColor", new Vector3(0.1f, 0.1f, 0.1f));
            // materialEffectBinding.Set("ReferenceAlpha", 0.5f);
            material.Add("Material", materialEffectBinding);

            return material;
        }



        private Submesh CreateQuad(GraphicsDevice device)
        {
            float texSize = _tileSize/8;
            float y = -0.1f;
            List<VertexPositionNormalTexture> customVertexList = new List<VertexPositionNormalTexture>();

            /*customVertexList.Add(new VertexPositionNormalTexture(new Vector3(-_tileSize, y, -_tileSize), Vector3.UnitY, new Vector2(0, texSize)));
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(_tileSize, y, -_tileSize), Vector3.UnitY, new Vector2(texSize, texSize)));
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(_tileSize, y, _tileSize), Vector3.UnitY, new Vector2(texSize, 0)));
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(-_tileSize, y, _tileSize), Vector3.UnitY, new Vector2(0, 0)));*/
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(0, y, 0), Vector3.UnitY, new Vector2(0, texSize)));
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(_tileSize, y, 0), Vector3.UnitY, new Vector2(texSize, texSize)));
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(_tileSize, y, _tileSize), Vector3.UnitY, new Vector2(texSize, 0)));
            customVertexList.Add(new VertexPositionNormalTexture(new Vector3(0, y, _tileSize), Vector3.UnitY, new Vector2(0, 0)));
            
            //now create the buffers
            VertexDeclaration declaration = VertexPositionNormalTexture.VertexDeclaration;

            var submesh = new Submesh
            {
                PrimitiveType = PrimitiveType.TriangleList,
                PrimitiveCount = 2,
                VertexCount = customVertexList.Count,
            };


            submesh.VertexBuffer = new VertexBuffer(device, declaration, customVertexList.Count, BufferUsage.WriteOnly);
            VertexPositionNormalTexture[] array = customVertexList.ToArray();
            submesh.VertexBuffer.SetData(array);

            if (BuildPlatform.IsWindows)
            {
                var indices = new int[] { 0, 1, 2, 2, 3, 0 };
                submesh.IndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
                submesh.IndexBuffer.SetData(indices);
            }
            else
            {
                var indices = new short[] { 0, 1, 2, 2, 3, 0 };
                submesh.IndexBuffer = new IndexBuffer(device, typeof(short), indices.Length, BufferUsage.WriteOnly);
                submesh.IndexBuffer.SetData(indices);
            }

            return submesh;
        }



        // OnLoad() is called when the GameObject is added to the IGameObjectService.
        protected override void OnLoad()
        {
            _groundNode = new SceneNode() {Children = new SceneNodeCollection()};

            // Load model.
            //var contentManager = _services.Get<ContentManager>();
            var meshNode = new MeshNode(CreateQuadMesh()) {PoseWorld = Pose.Identity, CastsShadows = false, IsStatic = true};

            //var baseX = _numTilesX*_tileSize/2f;
            //var baseY = _numTilesY*_tileSize/2f;
            //_globalOffset = new Vector3F(-baseX, 0, -baseY);
            //_globalOffset = new Vector3F();

            _groundTiles = new MeshNode[_numTilesX, _numTilesY];
            for (int i = 0; i < _numTilesX; i++)
            {
                for (int j = 0; j < _numTilesY; j++)
                {
                    var newTile = _groundTiles[i,j] = meshNode.Clone();

                    newTile.PoseWorld = new Pose(new Vector3F(i*_tileSize, 0, j*_tileSize));

                    _groundNode.Children.Add(newTile);
                }
            }

            //_meshNode.ScaleLocal = new Vector3F(0.5f);

            /*foreach (var node in _meshNode.GetSubtree())
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
            scene.Children.Add(_groundNode);

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
            _groundNode.Parent.Children.Remove(_groundNode);
            _groundNode.Dispose(false);
            _groundNode = null;

            _rigidBody.Simulation.RigidBodies.Remove(_rigidBody);
            _rigidBody = null;
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_render3DControl.IsMouseOver && !_inputService.MousePositionDelta.IsNumericallyZero)
            {
                UpdateMousePosition();
            }

            UpdateMousePositionBarText();

            UpdateInfiniteTilePositioning();

        }



        private void UpdateInfiniteTilePositioning()
        {
            var cameraPosition = _cameraObject.CameraNode.PoseWorld.Position;
            var indexX = (int)Math.Floor((cameraPosition.X) / _tileSize);
            var indexY = (int)Math.Floor((cameraPosition.Z) / _tileSize);

            if (indexX != _previousIndexX || indexY != _previousIndexY)
            {
                var localOffset = new Vector3F((indexX-1) * _tileSize, 0, (indexY-1) * _tileSize);

                for (int i = 0; i < _numTilesX; i++)
                {
                    for (int j = 0; j < _numTilesY; j++)
                    {
                        _groundTiles[i, j].PoseWorld = new Pose(localOffset + new Vector3F(i * (_tileSize), 0, j * (_tileSize)));
                    }
                }

                _previousIndexX = indexX;
                _previousIndexY = indexY;
            }

        }



        public void UpdateMousePositionBarText()
        {
            var cameraPosition = DigitalRuneUtils.YUpToZUpRotationMatrix * _cameraObject.CameraNode.PoseWorld.Position;

            _window.SetCornerCoordinateText(cameraPosition.X.ToString("F2") + "," + cameraPosition.Y.ToString("F2") + "," + cameraPosition.Z.ToString("F2") 
                                                            + "  |  " +  _lastMousePosition.X.ToString("F2") + "," + _lastMousePosition.Y.ToString("F2"));
        }


        private void UpdateMousePosition()
        {
            var ray = _cameraObject.GetRay();

            if (ray != null)
            {
                var collisionDetection = new CollisionDetection();
                var rayCollisionObject = new CollisionObject(new GeometricObject(ray, Pose.Identity));

                var contactSet = collisionDetection.GetContacts(rayCollisionObject, _rigidBody.CollisionObject);

                if (contactSet != null)
                {
                    var contact = contactSet.FirstOrDefault();
                    if (contact != null)
                    {
                        _lastMousePosition = DigitalRuneUtils.YUpToZUpRotationMatrix*contact.Position;
                    }
                }
            }
        }
    }
}