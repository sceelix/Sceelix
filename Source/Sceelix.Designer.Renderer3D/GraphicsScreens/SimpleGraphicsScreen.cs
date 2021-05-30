using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GraphicsScreens
{
    public class SimpleGraphicsScreen : ICustomGraphicsScreen
    {
        private readonly BillboardRenderer _billboardRenderer; // Handles BillboardNodes and ParticleSystemNodes.
        private readonly DebugRenderer _debugRenderer; // Used for drawing text labels.
        private readonly MeshRenderer _meshRenderer; // Handles MeshNodes.
        //private readonly CameraObject _cameraObject;
        private readonly Scene _scene;
        //private readonly CustomFigureRenderer _customFigureRenderer;

        public SimpleGraphicsScreen(IServiceLocator services)
        {
            var graphicsService = services.Get<IGraphicsService>();

            _meshRenderer = new MeshRenderer();

            // The BillboardRenderer handles BillboardNodes and ParticleSystemNodes.
            _billboardRenderer = new BillboardRenderer(graphicsService, 2048);

            var uiContentManager = services.Get<ContentManager>("UIContent");

            var spriteFont = uiContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
            _debugRenderer = new DebugRenderer(graphicsService, spriteFont);

            //_customFigureRenderer = new CustomFigureRenderer(_debugRenderer);

            // Create a new empty scene.
            _scene = new Scene();

            /*var ambientLight = new AmbientLight
            {
                Color = new Vector3F(0.05333332f, 0.09882354f, 0.1819608f),
                Intensity = 1,
                HemisphericAttenuation = 0,
            };
            _scene.Children.Add(new LightNode(ambientLight));

            var keyLight = new DirectionalLight
            {
                Color = new Vector3F(1, 0.9607844f, 0.8078432f),
                DiffuseIntensity = 1,
                SpecularIntensity = 1,
            };
            var keyLightNode = new LightNode(keyLight)
            {
                Name = "KeyLight",
                Priority = 10,   // This is the most important light.
                PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(-0.5265408f, -0.5735765f, -0.6275069f))),
            };
            _scene.Children.Add(keyLightNode);*/
        }



        public void Update(TimeSpan deltaTime)
        {
            Scene.Update(deltaTime);
        }



        public void Render(RenderContext context)
        {
            // Set render context info.
            context.CameraNode = ActiveCameraNode;
            context.Scene = _scene;

            // Frustum culling: Get all scene nodes which overlap the view frustum.
            //var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);
            var query = Scene.Query<CustomSceneQuery>(context.CameraNode, context);

            // Render meshes.
            var graphicsDevice = context.GraphicsService.GraphicsDevice;
            graphicsDevice.Clear(Color.White);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            context.RenderPass = "Default";
            _meshRenderer.Render(query.RenderableNodes, context);
            //context.RenderPass = null;

            // Render billboards using alpha blending.
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _billboardRenderer.Render(query.RenderableNodes, context, RenderOrder.BackToFront);

            //_customFigureRenderer.Render(query.RenderableNodes);

            _debugRenderer.Render(context);

            // Clean up.
            context.RenderPass = null;
            context.Scene = null;
            context.CameraNode = null;
        }



        public DebugRenderer DebugRenderer
        {
            get { return _debugRenderer; }
        }



        public CameraNode ActiveCameraNode
        {
            get;
            set;
        }



        public Scene Scene
        {
            get { return _scene; }
            //set { throw new NotImplementedException(); }
        }
    }
}