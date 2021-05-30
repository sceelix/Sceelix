using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Storages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Annotations;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GraphicsScreens;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Services;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Renderer3D.GUI
{
    public class Renderer3DControl : RenderTargetControl
    {
        //these are the services that come from the main window
        private CameraObject _cameraGameObject;
        
        private GameObjectManager _gameObjectManager;
        private ICustomGraphicsScreen _graphicsScreen;
        
        public Synchronizer _rendererSynchronizer = new Synchronizer();

        //these are the services started at this class
        private ServiceManager _services;

        private List<Object> _render3DServices;


        public Renderer3DControl(IServiceLocator systemServices, BarMenuService barMenuService, Renderer3DWindow window)
            : base(systemServices.Get<IGraphicsService>())
        {
            Initialize(systemServices, window, barMenuService);
        }


        private void Initialize(IServiceLocator systemServices, Renderer3DWindow window, BarMenuService barMenuService)
        {
            _services = new ServiceManager();

            _services.Register(this);
            
            _services.Register<RenderTargetControl>(this);
            _services.Register(window);

            _services.Register(systemServices.Get<PluginManager>());
            _services.Register(systemServices.Get<MessageManager>());
            _services.Register(systemServices.Get<Synchronizer>());

            //register the bar menu of the window and self
            _services.Register(barMenuService);
            _services.Register(systemServices.Get<BarMenuService>(),"Main");

            //register the settings
            _services.Register(systemServices.Get<SettingsManager>());

            _services.Register(typeof(Synchronizer), _rendererSynchronizer, "Renderer");

            //we follow the same order as the main game class, just to be sure we're not messing up
            _services.Register(typeof(IStorage), systemServices.Get<IStorage>());

            _services.Register(typeof(IGraphicsDeviceService), systemServices.Get<IGraphicsDeviceService>());
            //_services.Register(typeof(GraphicsDeviceManager), systemServices.Get<IGraphicsDeviceService>());

            //Content
            _services.Register(typeof(ContentManager), systemServices.Get<ContentManager>());
            _services.Register(typeof(ContentManager), systemServices.Get<ContentManager>("UIContent"), "UIContent");

            //Input
            _services.Register(typeof(IInputService), systemServices.Get<IInputService>());

            // Graphics
            _services.Register(typeof(IGraphicsService), systemServices.Get<IGraphicsService>());

            // GUI
            _services.Register(typeof(IUIService), systemServices.Get<IUIService>());

            //Game logic
            _gameObjectManager = new GameObjectManager();
            _services.Register(typeof(IGameObjectService), _gameObjectManager);
            
            _graphicsScreen = BuildPlatform.IsWindows ? (ICustomGraphicsScreen)new DeferredGraphicsScreen(_services) : new SimpleGraphicsScreen(_services);

            _services.Register(typeof(ICustomGraphicsScreen), _graphicsScreen);
            
            _services.Register(typeof(DebugRenderer), _graphicsScreen.DebugRenderer);
            _services.Register(typeof(IScene), _graphicsScreen.Scene);
            _services.Register(typeof(SceneManager), new SceneManager(_graphicsScreen.Scene));

            _cameraGameObject = new CameraObject(_services, this);
            _gameObjectManager.Objects.Add(_cameraGameObject);

            _services.Register(typeof(CameraObject), _cameraGameObject);
            _graphicsScreen.ActiveCameraNode = _cameraGameObject.CameraNode;



            _render3DServices = AttributeReader.OfAttribute<Renderer3DServiceAttribute>().GetInstancesOfType<Object>();
            foreach (var render3DService in _render3DServices)
                _services.Register(render3DService.GetType(), render3DService);


            foreach (var render3DService in _render3DServices.OfType<IServiceable>())
                render3DService.Initialize(_services);

            Resized += OnResized;
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

            foreach (var render3DService in _render3DServices.OfType<IInputHandlerElement>())
                render3DService.HandleInput(context);
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            foreach (var updateable in _render3DServices.OfType<IUpdateableElement>())
                updateable.Update(deltaTime);

            _gameObjectManager.Update(deltaTime);

            if (ShouldRender)
                _graphicsScreen.Update(deltaTime);
        }



        public override void Render(RenderContext context)
        {
            _graphicsScreen.DebugRenderer.Clear();

            foreach (var drawable in _render3DServices.OfType<IDrawableElement>())
                drawable.Draw(context);

            foreach (var drawableGameObject in _gameObjectManager.Objects.OfType<IDrawableElement>())
                drawableGameObject.Draw(context);

            //foreach (var sceelixGameObject in _gameObjectManager.Objects.OfType<SceelixGameObject>())
            //    sceelixGameObject.Draw(context);

            _graphicsScreen.Render(context);

            _rendererSynchronizer.Update(1);

            //set this to false, so that we wont render unless requested
            ShouldRender = false;
        }



        protected override void OnUnload()
        {
            base.OnUnload();

            foreach (var render3DService in _render3DServices.OfType<IDisposable>())
                render3DService.Dispose();

            _gameObjectManager.Objects.Clear();
        }
    }
}