using System;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Caching;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.Graphs.GUI.Execution;
using Sceelix.Designer.Graphs.GUI.Interactions;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.GUI.Navigation;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Point = Microsoft.Xna.Framework.Point;

namespace Sceelix.Designer.Graphs.GUI
{
    public class GraphControl : RenderTargetControl
    {
        /// <summary> 
        /// The ID of the <see cref="EdgeColor"/> game object property.
        /// </summary>
        public static readonly int EdgeColorPropertyId = CreateProperty(typeof(GraphControl), "EdgeColor", GamePropertyCategories.Appearance, null, new Color(51, 153, 255, 168), UIPropertyOptions.None);

        private readonly IServiceLocator _services;


        private readonly FileItem _fileItem;
        private readonly GraphDocumentControl _graphDocumentControl;
        private AmplifiedRenderHandler _amplifiedRenderHandler;
        private Camera2D _camera;
        private readonly string _debugText = String.Empty;
        private bool _firstTimeResize = true;
        private SpriteFont _font;

        private GraphDropHandler _graphDropHandler;
        private readonly GraphicsDevice _graphicsDevice;
        private InteractionHandler _interactionHandler;
        private VisualGraph _visualGraph;
        private readonly SpriteBatch _spritebatch;

        private readonly GraphExecutionManager _graphExecutionManager;

        private ShadowHandler _shadowHandler;

        private ICacheManager _cacheManager;

        public GraphControl(IServiceLocator services, FileItem fileItem, GraphDocumentControl graphDocumentControl)
            : base(services.Get<IGraphicsService>())
        {
            _services = services;
            _fileItem = fileItem;
            _graphDocumentControl = graphDocumentControl;

            Style = "GraphControl";

            _graphicsDevice = _services.Get<IGraphicsService>().GraphicsDevice;

            _spritebatch = new SpriteBatch(_graphicsDevice);

            _graphExecutionManager = new GraphExecutionManager(fileItem, _services, this);

            _cacheManager = new MemoryCacheManager(new DesignerResourceManager(fileItem.Project, services));

            Resized += OnResized;
        }


        private void OnResized(Point point)
        {
            if (BuildPlatform.IsWindows)
                _shadowHandler.Resize(point);

            _amplifiedRenderHandler.Resize(point);
            _camera.Resize(point);


            //FrameAll(false);
            if (_firstTimeResize)
            {
                FrameSelection(false);
                _firstTimeResize = false;
            }
        }


        public void FrameSelection(bool animate)
        {
            if (_camera != null)
                _camera.FrameSelectionOrDefault(animate);
        }


        protected void LoadContent(ContentManager manager)
        {
            _font = manager.Load<SpriteFont>("Fonts/Arial18");

            //_background = manager.Load<Texture2D>("Graphs/GraphBackground"); //EmbeddedResources.Load<Texture2D>("Resources/GraphBackground.jpg");
            //_background = EmbeddedResources.Load<Texture2D>("Resources/GridPatternBackground.png");

            _visualGraph = new VisualGraph(_services, this, manager, _cacheManager);

            _camera = new Camera2D(this);

            _interactionHandler = new InteractionHandler(_services, this);
            _interactionHandler.LoadContent(manager);

            //context menu handle initialization HAS to come after the interaction handler
            //_contextMenuHandler = new ContextMenuHandler(this);

            _graphDropHandler = new GraphDropHandler(_services, this);

            if (BuildPlatform.IsWindows)
            {
                _shadowHandler = new ShadowHandler(this);
                _shadowHandler.LoadContent(manager);
            }

            _amplifiedRenderHandler = new AmplifiedRenderHandler(this);
        }


        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _camera.Update(deltaTime);

            _visualGraph.Update(deltaTime);

            _interactionHandler.Update(deltaTime);

            _graphExecutionManager.Update(deltaTime);
        }


        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            context.IsMouseOver = IsMouseOver;
            context.MousePosition = context.ScreenMousePosition - new Vector2F(this.ActualX, this.ActualY);

            _interactionHandler.OnHandleInput(InputService, context);
        }


        private bool _firstLoad = true;

        protected override void OnLoad()
        {
            base.OnLoad();

            if (_firstLoad)
            {
                LoadContent(_services.Get<ContentManager>());
                _firstLoad = false;
            }
                

            _interactionHandler.OnLoad();
        }
        


        public override void Render(RenderContext context)
        {
            //first, create the shadow texture
            if (BuildPlatform.IsWindows)
                _shadowHandler.ProcessShadowTexture(_spritebatch, context);

            _amplifiedRenderHandler.ProcessAmplifiedTexture(_spritebatch, context);

            //draw the background
            _graphicsDevice.Clear(Color.Transparent);

            /*var position = _camera.ToScreenPosition(Vector2.Zero);
            _spritebatch.Begin(samplerState: SamplerState.LinearWrap);
            _spritebatch.Draw(_background, new Rectangle(0, 0, (int)ActualWidth, (int)ActualHeight), new Rectangle(-(int)position.X, -(int)position.Y, (int)ActualWidth, (int)ActualHeight), Color.White);
            _spritebatch.End();*/

            //draw the shadow
            if(BuildPlatform.IsWindows)
                _shadowHandler.DrawShadowTexture(_spritebatch);

            _amplifiedRenderHandler.DrawAmplifiedTexture(_spritebatch);

            //Draw now in screen space
            _spritebatch.Begin();
            _spritebatch.DrawString(_font, _debugText, new Vector2(0, 0), Color.White);
            _spritebatch.End();

            //avoid having to render all the type
            ShouldRender = false;
        }


        public void ShowGraphProperties()
        {
            Services.Get<MessageManager>().Publish(new ShowPropertiesRequest(new GraphInspectorControl(Services, Graph, _fileItem), GraphDocumentControl));
        }


        public void Save()
        {
            Graph.SaveXML(_fileItem.FullPath);
        }


        public void ReloadGraph(bool firstTime)
        {
            if (!firstTime)
            {
                GraphDocumentControl.InformProcessStarted("Updating...", false);

                _services.Get<Synchronizer>().Enqueue(delegate()
                {
                    string xml = VisualGraph.Graph.GetXML();

                    //GraphLoad.ClearCache();
                    Graph graph = GraphLoad.LoadFromXML(xml, VisualGraph.Environment);

                    VisualGraph.Graph = graph;
                    VisualGraph.RefreshVisualGraph(false);

                    GraphDocumentControl.InformProcessStopped();
                });
            }
            else
            {
                VisualGraph.RefreshVisualGraph(false);
            }
        }


        public void Dispose()
        {
            _services.Get<MessageManager>().Unregister(this);

            _visualGraph.Dispose();
        }


        public IServiceLocator Services
        {
            get { return _services; }
        }


        public GraphEditorSettings Settings
        {
            get { return _graphDocumentControl.EditorSettings; }
        }


        public GraphExecutionManager GraphExecutionManager
        {
            get { return _graphExecutionManager; }
        }


        public FileItem FileItem
        {
            get { return _fileItem; }
        }


        public VisualGraph VisualGraph
        {
            get { return _visualGraph; }
        }


        public Graph Graph
        {
            get { return _visualGraph.Graph; }
        }


        public Camera2D Camera
        {
            get { return _camera; }
        }


        public InteractionHandler InteractionHandler
        {
            get { return _interactionHandler; }
        }


        public GraphDocumentControl GraphDocumentControl
        {
            get { return _graphDocumentControl; }
        }


        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
        }


        public Color EdgeColor
        {
            get { return GetValue<Color>(EdgeColorPropertyId); }
            set { SetValue(EdgeColorPropertyId, value); }
        }



        public void ClearCache()
        {
            _cacheManager.Clear();
        }
    }
}