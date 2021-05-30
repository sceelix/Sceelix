using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Settings;
using System.Drawing;
using Sceelix.Designer.Utils;
#if MACOS
using Sceelix.Designer.GUI.Dialogs;
using Sceelix.Designer.GUI.Windows;
#else
using Point = Microsoft.Xna.Framework.Point;
#endif

namespace Sceelix.Designer.Managers
{
    public class GraphicsWindowManager
    {
        public delegate void SizeChanged(int oldWidth, int oldHeight, int newWidth, int newHeight);

        private Size _previousBounds;
        
        #if WINDOWS
        private Form _form;
        #endif
        
        
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        
        private readonly Game _game;
        private readonly GameWindow _window;
        
        public event SizeChanged WindowSizeChanged = delegate { };


        public GraphicsWindowManager(Game game, DesignerSettings designerSettings)
        {
            _game = game;
            _window = game.Window;
            
            
            #if DEBUG
            GraphicsAdapter.UseDebugDevice = true;
            #endif


            #if MACOS
            if (!designerSettings.LoadedCache.Value)
                OXDialogs.ShowCacheLoadingMessage();
            #endif

            _graphicsDeviceManager = new GraphicsDeviceManager(game)
            {
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = designerSettings.Use60FpsLimit.Value,
                IsFullScreen = false,
                PreferredBackBufferWidth = Screen.PrimaryScreen.WorkingArea.Size.Width,
                PreferredBackBufferHeight = Screen.PrimaryScreen.WorkingArea.Size.Height
            };

            _graphicsDeviceManager.DeviceCreated += (sender, args) => _graphicsDeviceManager.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            // HiDef support for these platforms is coming. Currently you have to use Reach.
            if (!BuildPlatform.IsWindows)
                _graphicsDeviceManager.GraphicsProfile = GraphicsProfile.Reach;

            game.Window.ClientSizeChanged += Window_ClientSizeChanged;

            game.IsMouseVisible = true;
            game.IsFixedTimeStep = designerSettings.Use60FpsLimit.Value;
            game.Window.AllowUserResizing = true;
            game.Window.Title = "Sceelix Designer";

            #if MACOS
            Window.SetBoundsOrigin(new PointF(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top));
            _previousBounds = new Size(Screen.PrimaryScreen.WorkingArea.Size.Width, Screen.PrimaryScreen.WorkingArea.Size.Height);
            #else
            game.Window.Position = new Point(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top);
#endif

#if LINUX
            Bitmap bm = EmbeddedResources.Load<Bitmap>("Resources.LogoWhite_64x64.png");
            game.Window.Icon = Icon.FromHandle(bm.GetHicon());
#endif

#if WINDOWS
            _form = (Form) Control.FromHandle(Window.Handle);
            _form.Shown += (sender, args) =>
            {
                _form.WindowState = FormWindowState.Maximized;
                _form.Size = new Size(Screen.PrimaryScreen.WorkingArea.Size.Width, Screen.PrimaryScreen.WorkingArea.Size.Height);
            };
#endif
        }
        

        
        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            //make this check because maximizing the window returns some strange zero values to these properties
            if (_game.Window.ClientBounds.Width == 0 || _game.Window.ClientBounds.Height == 0)
                return;

            //make this check otherwise the ApplyChanges function will enter an endless loop for some reason
            if (_graphicsDeviceManager.PreferredBackBufferWidth != _game.Window.ClientBounds.Width
                || _graphicsDeviceManager.PreferredBackBufferHeight != _game.Window.ClientBounds.Height)
            {
                SetResolution(_game.Window.ClientBounds.Width, _game.Window.ClientBounds.Height);
            }
        }
        
        
        private void SetResolution(int width, int height)
        {
            if (BuildPlatform.IsMacOS)
            {
                WindowSizeChanged.Invoke(_previousBounds.Width, _previousBounds.Height, width, height);

                _previousBounds = new Size(width, height);
            }
            else
            {
                var oldWidth = _graphicsDeviceManager.PreferredBackBufferWidth;
                var oldHeight = _graphicsDeviceManager.PreferredBackBufferHeight;


                _graphicsDeviceManager.PreferredBackBufferWidth = width;
                _graphicsDeviceManager.PreferredBackBufferHeight = height;
                _graphicsDeviceManager.ApplyChanges();

                WindowSizeChanged.Invoke(oldWidth, oldHeight, width, height);
            }
        }
        
        



        public GameWindow Window
        {
            get { return _window; }
        }


        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get { return _graphicsDeviceManager; }
        }


        public bool IsMinimized
        {
            get
            {
#if WINDOWS
                    return _form.WindowState == FormWindowState.Minimized;
#else
                    return false;
#endif
            }
        }
        

        /// <summary>
        /// We need this extra function because windows does not properly evaluate the IsActive flag when the user clicks on the taskbar.
        /// In this situation the window becomes minimized but it is still considered active!
        /// </summary>
        public bool IsReallyActive
        {
            get
            {
                return _game.IsActive && !IsMinimized;
            }
        }



        public Game Game
        {
            get { return _game; }
        }
    }
}