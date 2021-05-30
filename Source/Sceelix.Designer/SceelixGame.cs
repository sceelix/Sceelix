using System.IO;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using DigitalRune.Animation;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Graphics;
using DigitalRune.Storages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Conversion;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Layouts;
using Sceelix.Designer.Logging;
using Sceelix.Designer.Login;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;


namespace Sceelix.Designer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SceelixGame : Game
    {
        // The IoC service container providing access to all services.
        private ServiceManager _services;

        // Services of the game:
        private ExtendedInputManager _inputManager; // Input
        private GraphicsManager _graphicsManager; // Graphics
        private UIManager _uiManager; // GUI
        private AnimationManager _animationManager; // Animation
        private MessageManager _messageManager = new MessageManager();

        private Synchronizer _mainSynchronizer;

        private DesignerSettings _designerSettings;
        private GraphicsWindowManager _graphicsWindowManager;
        private WindowAnimator _windowAnimator;
        private PluginManager _pluginManager;
        private BarMenuManager _barMenuManager;
        private FpsCounter _fpsCounter;
        private ScreenManager _screenManager;
        private ContentManager _graphicsContentManager;
        private FpsSettingsManager _fpsSettingsManager;
        private SettingsManager _settingsManager;
        private WindowChangeManager _windowChangeManager;
        private MainMenuManager _mainMenuManager;
        private LayoutManager _layoutManager;
        private ILoginManager _loginManager;
        private LoadingManager _loadingManager;
        private WindowDragHandler _windowDragHandler;



        public SceelixGame()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            DigitalRune.ResourcePool.Enabled = false;
            
            GraphicsAdapter.UseDebugDevice = true;

            DesignerProgram.Log.Debug("Loading Window and Graphics.");

            RegisterDefaultConverters();

            _designerSettings = new DesignerSettings();
            _graphicsWindowManager = new GraphicsWindowManager(this, _designerSettings);
        }



        private void RegisterDefaultConverters()
        {
            //Initialize the Assembly Resource Manager
            DesignerProgram.Log.Debug("Loading Resource Managers.");
            
            //_defaultConversionManager = new DefaultConversionManager();
            //add these manually, as the plugin ones will not be loaded by the time we need them
            ConvertHelper.Register<Stream, String>(delegate (Stream stream) {
                return new StreamReader(stream).ReadToEnd();
            });

            ConvertHelper.Register<Stream, Bitmap>(delegate (Stream stream) {
                return (Bitmap)Bitmap.FromStream(stream);
            });
        }



        // Initializes services and adds game components.
        protected override void Initialize()
        {
            DesignerProgram.Log.Debug("Loading Services.");

            _services = new ServiceManager();
            _inputManager = new ExtendedInputManager(_graphicsWindowManager, _designerSettings);
            
            InitializeContent();
            

            DesignerProgram.Log.Debug("Loading Messaging and Input.");

            _messageManager = new MessageManager();
            _messageManager.Register<CompoundMessage>(message =>
            {
                foreach (var subMessage in message.Messages)
                    _messageManager.Publish(subMessage);
            });


            DesignerProgram.Log.Debug("Loading Graphics and UI.");

            // Graphics
            _graphicsManager = new GraphicsManager(GraphicsDevice, Window, _graphicsContentManager);
            

            // GUI
            _uiManager = new UIManager(this, _inputManager);

            //we can only add this after the textures are loaded
            ConvertHelper.Register<Stream, Texture2D>(delegate(Stream stream)
            {
                return Texture2D.FromStream(_graphicsManager.GraphicsDevice, stream);
            });
            
            _uiManager.KeyMap = new KeyMap();
            _inputManager.UIManager = _uiManager;
            
            _barMenuManager = new BarMenuManager();

            DesignerProgram.Log.Debug("Loading Animator and Synchronizer.");

            // Animation
            _animationManager = new AnimationManager();
            _windowAnimator = new WindowAnimator(_animationManager, _uiManager);
            _mainSynchronizer = new Synchronizer();
            _fpsSettingsManager = new FpsSettingsManager(_designerSettings, _windowAnimator, _graphicsWindowManager);

            DesignerProgram.Log.Debug("Loading Settings.");
            _settingsManager = new SettingsManager(_windowAnimator);

            //add the settings from the designer
            _settingsManager.Register("Designer", _designerSettings);

            _fpsCounter = new FpsCounter(Window, _designerSettings);

            _pluginManager = new PluginManager(_services);

            _windowDragHandler = new WindowDragHandler(Window, _messageManager);

            RegisterServices();

            _screenManager = new ScreenManager(this, _services);

            _services.Register(new UIZoomManager(_screenManager.UiScreen, _designerSettings, this.Window));

            _services.Register(new ExitManager(this, _pluginManager, _messageManager));
            
            _loginManager = BuildDistribution.IsSteam ? 
                            (ILoginManager)new SteamLoginManager(_services, _screenManager.UiScreen) 
                            : new StandardLoginManager(_services);


            _loadingManager = new LoadingManager(_loginManager, _pluginManager, _screenManager.UiScreen, _mainSynchronizer);
            _loadingManager.Finished += OnLoadingFinished;

            _loginManager.Initialize();

            base.Initialize();

            //DesignerProgram.Log.Debug("Finished Initialize.");
        }



        private void OnLoadingFinished(object sender, EventArgs e)
        {
            var loadingManager = (LoadingManager) sender;
            _pluginManager = loadingManager.PluginManager;
            _layoutManager = loadingManager.LayoutManager;

            //let's add the top menu now
            _mainMenuManager = new MainMenuManager(this, _services, _screenManager.UiScreen, _layoutManager);
            
            _windowChangeManager = new WindowChangeManager(_messageManager, _layoutManager, _mainMenuManager);

            _layoutManager.LoadDefaultLayout();

            _messageManager.Publish(new DesignerLoaded());
        }



        private void InitializeContent()
        {

            DesignerProgram.Log.Debug("Loading Storage.");

            // The VfsStorage creates a virtual file system.
            var vfsStorage = new VfsStorage();

            // A ZipStorage can be used to access files inside a ZIP archive.
            var assetsStorage = new ZipStreamStorage(vfsStorage, "Content", EmbeddedResources.Load<Stream>("Resources/Content.zip"));
            vfsStorage.MountInfos.Add(new VfsMountInfo(assetsStorage, null));

            // Register the virtual file system as a service.
            _services.Register<IStorage>(assetsStorage);

            // ----- Content Managers
            // The GraphicsDeviceManager needs to be registered in the service container.
            // (This is required by the XNA content managers.)

            //_services.Register(typeof(GraphicsDeviceManager), null, _graphicsDeviceManager);

            // Register a default, shared content manager.
            // The new StorageContentManager can be used to read assets from the virtual
            // file system. (Replaces the content manager stored in Game.Content.)
            DesignerProgram.Log.Debug("Loading Content Managers.");

            Content = new StorageContentManager(_services, vfsStorage);


            // Create a sprite batch.
            //_spriteBatch = new SpriteBatch(_graphicsDeviceManager.GraphicsDevice);

            // Create and register content manager that will be used to load the GUI.
            var uiContentManager = new StorageContentManager(_services, assetsStorage);

            _services.Register<ContentManager>(uiContentManager, "UIContent");

            // Create content manager that will be used exclusively by the graphics service
            // to load the pre-built effects and resources of DigitalRune.Graphics. (We
            // could use Game.Content, but it is recommended to separate the content. This 
            // allows to unload the content of the samples without unloading the other 
            // content.)
            _graphicsContentManager = new StorageContentManager(_services, vfsStorage);
        }

        
        
        private void RegisterServices()
        {

            _services.Register(_messageManager);

            _services.Register(_graphicsWindowManager);
            _services.Register(typeof(IGraphicsDeviceService), _graphicsWindowManager.GraphicsDeviceManager);

            _services.Register<IInputService>(_inputManager);

            _services.Register(new LoggingManager(_designerSettings));

            _services.Register(typeof(PluginManager), _pluginManager);

            _services.Register<ContentManager>(Content);

            _services.Register(Content);

            _services.Register<IGraphicsService>(_graphicsManager);

            _services.Register(typeof(IUIService), _uiManager);

            _services.Register(_barMenuManager);

            _services.Register(typeof(IAnimationService), _animationManager);

            _services.Register(_mainSynchronizer);

            _services.Register(_fpsSettingsManager);

            //start the settings manager
            _services.Register(_settingsManager);

            _services.Register(_windowAnimator);

            _services.Register(_windowDragHandler);
        }







        // Updates the different sub-systems (input, physics, game logic, ...).
        protected override void Update(GameTime gameTime)
        {
            try
            {
                //update the window location and size, if it changed
                //_layoutManager.WindowBounds = Window.ClientBounds;

                var deltaTime = gameTime.ElapsedGameTime;


                //if (IsReallyActive)

                {
                    _messageManager.Update(deltaTime);

                    //this is a main synchronization method
                    _mainSynchronizer.Update();

                    // Update input manager. The input manager gets the device states and performs other work.
                    // (Note: XNA requires that the input service is run on the main thread!)
                    _inputManager.Update(deltaTime);

                    // Update animations.
                    // (The animation results are stored internally but not yet applied).
                    _animationManager.Update(deltaTime);

                    // Apply animations.
                    // (The animation results are written to the objects and properties that 
                    // are being animated. ApplyAnimations() must be called at a point where 
                    // it is thread-safe to change the animated objects and properties.)
                    _animationManager.ApplyAnimations();
                }

                
                _screenManager.Update(gameTime);
                _pluginManager.Update(deltaTime);

                base.Update(gameTime);

                // Update UI manager. The UI manager updates all registered UIScreens.
                //if (IsReallyActive)
                _uiManager.Update(deltaTime);
            }
            catch (TargetInvocationException ex)
            {
                Exception realException = ex.GetRealException();

                _services.Get<MessageManager>().Publish(new ExceptionThrown(realException));

                DesignerProgram.Log.Error("Unhandled Exception.", realException);
            }
            catch (Exception ex)
            {
                _services.Get<MessageManager>().Publish(new ExceptionThrown(ex));

                DesignerProgram.Log.Error("Unhandled Exception.", ex);
            }
        }



        // Draws the game content.
        protected override void Draw(GameTime gameTime)
        {
            // Manually clear background. 
            // (This is not really necessary because the individual samples are 
            // responsible for rendering. However, if we skip this Clear() on Android 
            // then we can see trash in the back buffer when switching between samples.)
            //GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
            
            _fpsCounter.Draw(gameTime);

            // Update the graphics (including graphics screens).
            // Important, if symbol EnableParallelGameLoop is true: Currently 
            // animation, physics and particles are running in parallel. Therefore, 
            // the GraphicsScreen.OnUpdate() methods must not influence the animation,
            // physics or particle state!
            _graphicsManager.Update(gameTime.ElapsedGameTime);


            // Render graphics screens to the back buffer.
            _graphicsManager.Render(false);
        }
        
        
        
        public MessageManager MessageManager
        {
            get { return _messageManager; }
        }
        
        
        public IServiceLocator ServiceLocator
        {
            get { return _services; }
        }
    }
}