using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Conversion;
using Sceelix.Core.Environments;
using Sceelix.Core.Procedures;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.Graphs.Logging;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Meshes.GameObjects;
using Sceelix.Designer.Meshes.SceneNodes;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Logging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GraphicsScreens;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;
using Sceelix.Meshes.Procedures;
using Console = System.Console;

namespace Sceelix.Designer.Meshes.Controls
{
    public class ModelViewer3DControl : RenderTargetControl
    {
        public EventHandler<EventArgs> MeshViewerLoading = delegate(object sender, EventArgs args) {  };
        public EventHandler<EventArgs> MeshViewerLoadingFinished = delegate (object sender, EventArgs args) { };
        

        private SimpleGraphicsScreen _graphicsScreen;
        private GameObjectManager _gameObjectManager;
        private OrbitCameraObject _cameraGameObject;
        private MeshRenderNodeFactory _meshRenderNodeFactory;
        private ContentLoader _contentLoader;
        private FileItem _fileItem;
        private readonly ModelViewer3DSettings _modelViewer3DSettings;
        private DesignerResourceManager _resourceManager;
        private MessageManager _messageManager;

        private Synchronizer _synchronizer = new Synchronizer();
        private MeshRenderNode _meshRenderNode;

        private Dictionary<string, Object> _currentModelStatistics = new Dictionary<string, object>();



        public ModelViewer3DControl(IServiceLocator services, IGraphicsService graphicsService, FileItem fileItem, ModelViewer3DSettings modelViewer3DSettings)
            : base(graphicsService)
        {
            _fileItem = fileItem;
            _modelViewer3DSettings = modelViewer3DSettings;

            var serviceManager = new ServiceManager(services);
            _graphicsScreen = new SimpleGraphicsScreen(services);

            _gameObjectManager = new GameObjectManager();

            _gameObjectManager.Objects.Add(new AxisCross(_graphicsScreen.DebugRenderer, _graphicsScreen.Scene){Enabled = true});
            _gameObjectManager.Objects.Add(_cameraGameObject = new OrbitCameraObject(services, this, _graphicsScreen.Scene));
            _graphicsScreen.ActiveCameraNode = _cameraGameObject.CameraNode;

            _messageManager = services.Get<MessageManager>();

            serviceManager.Register<IScene>(_graphicsScreen.Scene);
            serviceManager.Register<RenderTargetControl>(this);

            _meshRenderNodeFactory = new MeshRenderNodeFactory();
            _meshRenderNodeFactory.Initialize(serviceManager);

            _resourceManager = new DesignerResourceManager(fileItem.Project, serviceManager);
            _contentLoader = new ContentLoader(serviceManager, _resourceManager);
            
            SetupScene();

            ReloadModel();

            Resized += OnResized;
            _modelViewer3DSettings.ShowAxis.Changed += SettingsChanged;
            _modelViewer3DSettings.RenderWireframe.Changed += SettingsChanged;
        }



        private void SettingsChanged(ApplicationField<bool> field, bool oldvalue, bool newvalue)
        {
            this.ShouldRender = true;
        }



        public void ReloadModel()
        {
            if(_meshRenderNode != null)
                _graphicsScreen.Scene.Children.Remove(_meshRenderNode);

            new Thread(LoadModel).Start();
        }



        private void LoadModel()
        {
            try
            {
                MeshViewerLoading.Invoke(this, EventArgs.Empty);

                var environment = new ProcedureEnvironment(_resourceManager, new LogWindowLogger(_messageManager));


                var meshLoadProcedure = new MeshLoadProcedure();
                meshLoadProcedure.Environment = environment;
                meshLoadProcedure.Parameters["File Name"].Set(_fileItem.ProjectRelativePath);
                meshLoadProcedure.Parameters["Rotate Axis"].Set(_fileItem.Properties.GetAndConvert("Rotate Axis", false));
                meshLoadProcedure.Parameters["Flip Faces"].Set(_fileItem.Properties.GetAndConvert("Flip Faces", false));
                meshLoadProcedure.Execute();

                var meshUVProcedure = new MeshUVMapProcedure();
                meshUVProcedure.Environment = environment;
                meshUVProcedure.Parameters["Operation"].Set("Flip UV");
                meshUVProcedure.Parameters["Operation"].Parameters["Flip UV"].Parameters["Flip U"].Set(_fileItem.Properties.GetAndConvert("Flip Texture U", false));
                meshUVProcedure.Parameters["Operation"].Parameters["Flip UV"].Parameters["Flip V"].Set(_fileItem.Properties.GetAndConvert("Flip Texture V", false));
                meshUVProcedure.Inputs["Input"].Enqueue(meshLoadProcedure.Outputs.DequeueAll());
                meshUVProcedure.Execute();

                var meshEntities = meshUVProcedure.Outputs.DequeueAll().OfType<MeshEntity>().ToList();
                var boundingBox = Sceelix.Mathematics.Spatial.BoundingBox.Union(meshEntities.Select(x => x.BoundingBox));

                _meshRenderNode = _meshRenderNodeFactory.Create(meshEntities, _contentLoader);
                _meshRenderNode.PoseWorld = new Pose(-boundingBox.Center.ToVector3F());

                _currentModelStatistics = new Dictionary<String, Object>()
                {
                    {"Mesh Count", meshEntities.Count},
                    {"Vertex Count", meshEntities.Sum(x => x.FaceVerticesWithHoles.Count())},
                    {"Face Count", meshEntities.Sum(x => x.Faces.Count())}
                };

                _synchronizer.Enqueue(() =>
                {
                    _graphicsScreen.Scene.Children.Add(_meshRenderNode);

                    MeshViewerLoadingFinished.Invoke(this, EventArgs.Empty);

                    _cameraGameObject.Frame(boundingBox.BoundingSphere.Radius * 2);

                    this.ShouldRender = true;
                });
            }
            catch (Exception e)
            {
                _messageManager.Publish(new ExceptionThrown(e));
            }
        }



        private void SetupScene()
        {
            var ambientLight = new AmbientLight
            {
                Intensity = 2,
            };
            _graphicsScreen.Scene.Children.Add(new LightNode(ambientLight));
        }



        private void OnResized(Point point)
        {
            _cameraGameObject.ResetProjection(point);

            ShouldRender = true;
        }


        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            _cameraGameObject.OnHandleInput(context);
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();

            _gameObjectManager.Update(deltaTime);

            if (ShouldRender)
                _graphicsScreen.Update(deltaTime);
        }



        public override void Render(RenderContext context)
        {
            _graphicsScreen.DebugRenderer.Clear();

            if (_modelViewer3DSettings.ShowAxis.Value)
            {
                foreach (var drawableGameObject in _gameObjectManager.Objects.OfType<IDrawableElement>())
                    drawableGameObject.Draw(context);
            }

            if (_modelViewer3DSettings.RenderWireframe.Value)
            {
                if (_meshRenderNode != null)
                {
                    foreach (var meshNode in _meshRenderNode.Children.OfType<MeshNode>())
                        _graphicsScreen.DebugRenderer.DrawMesh(meshNode.Mesh, _meshRenderNode.PoseWorld, Vector3F.One, Color.Black, true, true);
                }
            }

            _graphicsScreen.Render(context);
            
            //set this to false, so that we wont render unless requested
            ShouldRender = false;
        }



        public Dictionary<string, object> CurrentModelStatistics
        {
            get { return _currentModelStatistics; }
        }
    }
}
