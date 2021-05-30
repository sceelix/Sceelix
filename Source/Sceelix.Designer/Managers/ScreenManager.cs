using System;
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using RectangleF = DigitalRune.Game.UI.RectangleF;


namespace Sceelix.Designer.Managers
{
    public class ScreenManager //: GameComponent
    {
        // Services which can be used in derived classes.
        private readonly ServiceManager _services;
        private readonly IUIService _uiService;

        private readonly UIScreen _uiScreen;
        private readonly Theme _theme;

        private readonly DesignerSettings _designerSettings;
        private readonly CursorManager _cursorManager;

        private RenderTarget2D _customRenderTarget;
        private ScreenshotRequest _screenshotRequest;
        private readonly GraphicsWindowManager _graphicsWindowManager;
        

#if MACOS
        //private Size _previousBounds;
        #endif

        public ScreenManager(SceelixGame game, ServiceManager services)
            //: base(game)
        {
            _services = services;

            _graphicsWindowManager = _services.Get<GraphicsWindowManager>();
            _graphicsWindowManager.WindowSizeChanged += GraphicsWindowManagerOnGraphicsWindowSizeChanged;
            
            _designerSettings = _services.Get<SettingsManager>().Get<DesignerSettings>();
            _designerSettings.UIScale.Changed += UIScaleOnChanged;

            DesignerProgram.Log.Debug("Loading Screen Manager.");

            var contentManager = _services.Get<ContentManager>();

            var graphicsService = _services.Get<IGraphicsService>();
            _uiService = _services.Get<IUIService>();

            DesignerProgram.Log.Debug("Loading Delegate Screen.");

            // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
            // service. This lets us do the rendering in the Render method of this class.
            var graphicsScreen = new DelegateGraphicsScreen(graphicsService) {RenderCallback = Render, RenderPreviousScreensToTexture = false};
            graphicsService.Screens.Insert(0, graphicsScreen);

            DesignerProgram.Log.Debug("Loading Theme.");

            //Theme theme = contentManager.Load<Theme>("UI Themes/Sceelix/ThemeDark");
            _theme = contentManager.Load<Theme>("UI Themes/Sceelix/ThemeRed");

            DesignerProgram.Log.Debug("Loading Renderer.");

            // Create a UI renderer, which uses the theme info to renderer UI controls.
            var renderer = new ExtendedUIRenderer(game, _theme);
            _services.Register(typeof(IUIRenderer), renderer);


            DesignerProgram.Log.Debug("Loading Screens.");

            var screenWidth = game.Window.ClientBounds.Width*_designerSettings.UIScaleValue;
            var screenHeight = game.Window.ClientBounds.Height*_designerSettings.UIScaleValue;


            // Create a UIScreen and add it to the UI service. The screen is the root of the 
            // tree of UI controls. Each screen can have its own renderer.
            _uiScreen = new UIScreen("SceelixUIScreen", renderer); //, Margin = new Vector4F(0, 20, 0, 0) leaving a margin would cause a mouse offset! We would need to recalculate things...
            _uiService.Screens.Add(_uiScreen);
            
            _uiScreen.FocusManager = new SceelixFocusManager(_uiScreen);
            _uiScreen.Width = screenWidth;
            _uiScreen.Height = screenHeight;
            _uiScreen.ToolTipDelay = TimeSpan.Zero;
            _uiScreen.IsFocusScope = true;
            _uiScreen.Focus();

            #if MACOS
            _designerSettings.LoadedCache.Value = true;
            #endif

            if (BuildPlatform.IsLinux || BuildPlatform.IsMacOS)
            {
                _cursorManager = new CursorManager(game, graphicsService, _uiService, _theme.Cursors.ToList());
            }

            _customRenderTarget = new RenderTarget2D(graphicsService.GraphicsDevice, (int) screenWidth, (int) screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.PreserveContents);
            

            _services.Get<MessageManager>().Register<ScreenshotRequest>((val) => _screenshotRequest = val);
        }



        private void GraphicsWindowManagerOnGraphicsWindowSizeChanged(int oldwidth, int oldheight, int newwidth, int newheight)
        {
            var renderScale = _designerSettings.ParseRenderScale(_designerSettings.UIScale.Value);
            
            ClientSizeChanged(oldwidth * renderScale, newwidth * renderScale, oldheight * renderScale, newheight * renderScale);
        }
        


        private void UIScaleOnChanged(ApplicationField<string> field, string oldValue, string newValue)
        {
            var oldPercentage = _designerSettings.ParseRenderScale(oldValue);
            var newPercentage = _designerSettings.ParseRenderScale(newValue);

#if MACOS
            /*ClientSizeChanged(_previousBounds.Width * oldPercentage,
                _previousBounds.Width * newPercentage,
                _previousBounds.Height * oldPercentage,
                _previousBounds.Height * newPercentage);*/
            
            
#else
            ClientSizeChanged(_graphicsWindowManager.GraphicsDeviceManager.PreferredBackBufferWidth * oldPercentage,
                _graphicsWindowManager.GraphicsDeviceManager.PreferredBackBufferWidth * newPercentage,
                _graphicsWindowManager.GraphicsDeviceManager.PreferredBackBufferHeight * oldPercentage,
                _graphicsWindowManager.GraphicsDeviceManager.PreferredBackBufferHeight * newPercentage);
#endif
        }


        public void ClientSizeChanged(float oldWidth, float newWidth, float oldHeight, float newHeight)
        {
            _customRenderTarget = new RenderTarget2D(_services.Get<IGraphicsService>().GraphicsDevice, (int)newWidth, (int)newHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.PreserveContents);

            _uiScreen.Width = newWidth;
            _uiScreen.Height = newHeight;

            _services.Get<MessageManager>().Publish(new ClientSizeChanged(oldWidth, oldHeight, newWidth, newHeight));

            foreach (var dialogWindow in _uiScreen.Children.OfType<AnimatedWindow>().Where(x => x.KeepCentered))
                dialogWindow.CenterToScreen();
        }



        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Remove UIScreen from UI service.
                //_uiService.Screens.Remove(_backgroundScreen);
                _uiService.Screens.Remove(_uiScreen);
            }

            //base.Dispose(disposing);
        }



        public void Update(GameTime gameTime)
        {
            if(BuildPlatform.IsLinux || BuildPlatform.IsMacOS)
                _cursorManager.Update(gameTime);
        }



        private void Render(RenderContext context)
        {
            //draw the actual window only when there are changes
            if (!_uiScreen.IsVisualValid)
            {
                context.GraphicsService.GraphicsDevice.SetRenderTarget(_customRenderTarget);

                context.GraphicsService.GraphicsDevice.Clear(Color.White);
                //_backgroundScreen.Draw(context.DeltaTime);
                _uiScreen.Draw(context.DeltaTime);

                context.GraphicsService.GraphicsDevice.SetRenderTarget(null);
            }

            _uiScreen.Renderer.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            _uiScreen.Renderer.SpriteBatch.Draw(_customRenderTarget, new Rectangle(0, 0, context.GraphicsService.GraphicsDevice.PresentationParameters.BackBufferWidth, context.GraphicsService.GraphicsDevice.PresentationParameters.BackBufferHeight), new Rectangle(0, 0, _customRenderTarget.Width, _customRenderTarget.Height), Color.White);


            if (_screenshotRequest != null)
            {
                _screenshotRequest.Callback.Invoke(_customRenderTarget.Clone());
                _screenshotRequest = null;
            }

            if(BuildPlatform.IsMacOS)
                _cursorManager.Draw(_uiScreen.Renderer.SpriteBatch, _uiScreen);

            _uiScreen.Renderer.SpriteBatch.End();
        }



        public static RectangleF GetWindowArea(UIScreen uiScreen)
        {
            return new RectangleF(0, MainMenuManager.BarHeight, uiScreen.ActualWidth, uiScreen.ActualHeight - MainMenuManager.BarHeight);
        }



        public UIScreen UiScreen
        {
            get { return _uiScreen; }
        }



        public Theme Theme
        {
            get { return _theme; }
        }
    }
}