using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Layouts;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Color = Microsoft.Xna.Framework.Color;

namespace Sceelix.Designer.Managers
{
    public class MainMenuManager
    {
        public static readonly float BarHeight = 28;

        private readonly BarMenu _barMenu;

        private readonly SceelixGame _game;
        private readonly IServiceLocator _services;
        private readonly UIScreen _uiScreen;
        private readonly MenuChild _helpMenuItem;
        private LightHttpServer _server;
        private ExitManager _exitManager;



        public MainMenuManager(SceelixGame game, ServiceManager services, UIScreen uiScreen, LayoutManager layoutManager)
        {
            _game = game;
            _services = services;
            _uiScreen = uiScreen;
            _exitManager = services.Get<ExitManager>();

            _barMenu = new BarMenu();
            _uiScreen.Children.Add(_barMenu);

            services.Register(new BarMenuService(_barMenu));

            var applicationMenuItem = new MenuChild() {Text = "Application"};
            _barMenu.MenuChildren.Add(applicationMenuItem);

            applicationMenuItem.MenuChildren.Add(new MenuChild(OnSettingsMenuClick) {Text = "Settings"});
            applicationMenuItem.MenuChildren.Add(new MenuChild(OnPluginsMenuClick) { Text = "Plugins" });
            applicationMenuItem.MenuChildren.Add(new MenuChild(OnLogsMenuClick) {Text = "Logs"});
            applicationMenuItem.MenuChildren.Add(new MenuChild(OnExitMenuItemClick) {Text = "Exit", BeginGroup = true});

            var windowMenuItem = new MenuChild() {Text = "Window"};
            _barMenu.MenuChildren.Add(windowMenuItem);

            layoutManager.SetupLayoutMenu(windowMenuItem);

            _helpMenuItem = new MenuChild() {Text = "Help"};
            _barMenu.MenuChildren.Add(_helpMenuItem);

            _helpMenuItem.MenuChildren.Add(new MenuChild(OnDocumentationMenuItemClick) {Text = "Documentation"}); //, BeginGroup = activationButtonAdded
            
            _helpMenuItem.MenuChildren.Add(new MenuChild(OnForumsMenuItemClick) {Text = "Forums"});
            //_helpMenuItem.MenuChildren.Add(new MenuChild(OnOfflineManualMenuItemClick) { Text = "Offline Manual" });


            _helpMenuItem.MenuChildren.Add(new MenuChild(delegate { ExtractZip("API"); }) {Text = "API Samples", BeginGroup = true});
            _helpMenuItem.MenuChildren.Add(new MenuChild(delegate { ExtractZip("Unity Plugin"); }) {Text = "Unity Plugin"});

            
            if (BuildDistribution.IsStandard)
            {
                _helpMenuItem.MenuChildren.Add(new MenuChild(OnCheckForUpdatesMenuItemClick) { Text = "Check for Updates" });
            }


            _helpMenuItem.MenuChildren.Add(new MenuChild(OnAboutMenuItemClick) {Text = "About...", BeginGroup = true});


            if (BuildDistribution.IsStandard)
                CheckForUpdates();

            _services.Get<BarMenuManager>().Initialize(_barMenu);
            //_services.Register(new BarMenuManager(_barMenu));
        }


        private void OnPluginsMenuClick(MenuChild obj)
        {
            PluginViewWindow pluginViewWindow = new PluginViewWindow();

            _services.Get<WindowAnimator>().Show(pluginViewWindow);
        }


        //SimpleHttpServer _server;


        private void OnOfflineManualMenuItemClick(MenuChild obj)
        {
            var port = 4040;

            if (_server != null)
                _server.Stop();

            ExtractZip("Docs",false, () =>
            {
                var docsFolder = Path.Combine(SceelixApplicationInfo.ExtrasFolder, "Docs");
                //_server = new SimpleHttpServer(docsFolder, port);
                _server = new LightHttpServer(docsFolder,"localhost", port);
                _server.Start();


                UrlHelper.OpenUrlInBrowser("http://localhost:" + port);
            });
        }



        public BarMenu BarMenu
        {
            get { return _barMenu; }
        }

        



        


        private void ExtractZip(string folder, bool open = true, Action onFinished = null)
        {
            String finalFolder = Path.Combine(SceelixApplicationInfo.ExtrasFolder, folder);

            if (Directory.Exists(finalFolder))
            {
                if(open)
                    UrlHelper.OpenFolderInExplorer(finalFolder);

                if (onFinished != null)
                    onFinished();
            }   
            else
            {
                var zipFilePath = Path.Combine(SceelixApplicationInfo.ContentFolder, folder + ".zip");

                ExtractFolderWindow filesWindow = new ExtractFolderWindow(zipFilePath, SceelixApplicationInfo.ExtrasFolder);
                
                filesWindow.Accepted += delegate
                {
                    if (Directory.Exists(finalFolder) && open)
                        UrlHelper.OpenFolderInExplorer(finalFolder);

                    if (onFinished != null)
                        onFinished();
                };
                filesWindow.Canceled += delegate { Directory.Delete(finalFolder, true); };
                _services.Get<WindowAnimator>().Show(filesWindow);
            }
        }



        private void OnDocumentationMenuItemClick(MenuChild obj)
        {
            UrlHelper.OpenUrlInBrowser("https://www.sceelix.com/docs");
        }



        private void OnForumsMenuItemClick(MenuChild obj)
        {
            UrlHelper.OpenUrlInBrowser("https://www.sceelix.com/forums/");
        }



        private void OnCheckForUpdatesMenuItemClick(MenuChild obj)
        {
            try
            {
                SceelixWebClient client = new SceelixWebClient();
                var downloadString = client.DownloadString(new Uri("https://version.sceelix.com"));

                Version currentVersion = SceelixApplicationInfo.CurrentVersion;
                Version remoteVersion = new Version(downloadString);
                if (remoteVersion > currentVersion)
                {
                    MessageWindow window = new MessageWindow();
                    window.Title = "Update Available";
                    window.Text = "Your are currently using version " + currentVersion + " of Sceelix.\r\nAn updated version (" + remoteVersion + ") is available. Would you like to go to the download page now?";
                    window.Buttons = new[] {"Yes", "No"};
                    window.MessageIcon = MessageWindowIcon.Information;
                    
                    window.Click += delegate
                    {
                        if (window.Selection == "Yes")
                        {
                            UrlHelper.OpenUrlInBrowser("https://get.sceelix.com");
                        }
                    };
                    _services.Get<WindowAnimator>().Show(window);
                }
                else
                {
                    MessageWindow window = new MessageWindow()
                    {
                        Title = "Update Check",
                        Text = "Your Sceelix version (" + currentVersion + ") is up to date.",
                        Buttons = new[] {"OK"},
                        MessageIcon = MessageWindowIcon.Information
                    };
                    
                    _services.Get<WindowAnimator>().Show(window);
                }
            }
            catch (Exception)
            {
                //if the connection or download is not possible, just don't try to update!
                MessageWindow window = new MessageWindow()
                {
                    Title = "Update Check",
                    Text = "Could not check for updates. Please try again later.",
                    Buttons = new[] {"OK"},
                    MessageIcon = MessageWindowIcon.Error
                };
                //window.Show(_uiScreen);
                _services.Get<WindowAnimator>().Show(window);
            }
        }

        



        private void OnAboutMenuItemClick(MenuChild obj)
        {
            AboutWindow aboutWindow = new AboutWindow();

            _services.Get<WindowAnimator>().Show(aboutWindow);
            //aboutWindow.Show(_uiScreen);
        }



        private void OnUpdateMenuItemClick(MenuChild obj)
        {
            UrlHelper.OpenUrlInBrowser("https://get.sceelix.com");
        }



        private void OnExitMenuItemClick(MenuChild obj)
        {
            MessageWindow messageWindow = new MessageWindow()
            {
                MessageIcon = MessageWindowIcon.Question,
                Text = "Are you sure you want to exit Sceelix?",
                //Text = "Again, we set the axis parameter to specify the scope axis on which\n the handle should lie. The depth attribute of the model behaves differently than the width or height attributes. It scales the depth of the cube around a central point. To accommodate this, we use the parameter skin=diameterArrow. This creates a handle with different drag behavior, and two orange arrows that can both be used to change the depth of the cube model.",
                Title = "Exit?",
                Buttons = new[] {"Yes", "No"}
            };
            messageWindow.Click += delegate
            {
                if (messageWindow.Selection == "Yes")
                {
                    _exitManager.Exit();
                }
            };
            //messageWindow.Show(_uiScreen);
            _services.Get<WindowAnimator>().Show(messageWindow);
        }



        private void OnSettingsMenuClick(MenuChild obj)
        {
            _services.Get<SettingsManager>().OpenSettingsWindow("Designer");
        }



        private void OnLogsMenuClick(MenuChild obj)
        {
            UrlHelper.OpenFolderInExplorer(SceelixApplicationInfo.LogsFolder);
        }



        private void CheckForUpdates()
        {
            try
            {
                using (SceelixWebClient client = new SceelixWebClient())
                {
                    client.DownloadStringCompleted += ClientOnDownloadStringCompleted;

                    client.DownloadStringAsync(new Uri("https://version.sceelix.com"));
                }
            }
            catch (Exception ex)
            {
                //the rest has to go to the main thread
                //_services.Get<Synchronizer>().Enqueue(InitializeLayoutManager);
                DesignerProgram.Log.Error("Could Not Check For Updates.", ex);
            }
        }



        private void ClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs downloadStringCompletedEventArgs)
        {
            try
            {
                Version remoteVersion = new Version(downloadStringCompletedEventArgs.Result);
                if (remoteVersion > SceelixApplicationInfo.CurrentVersion)
                {
                    _barMenu.MenuChildren.Add(new MenuChild(OnUpdateMenuItemClick) {Text = "Update!", ToolTip = new ToolTipControl("A new update is available! Click to download the last version."), Background = Color.Green});
                }
            }
            catch (Exception)
            {
                //ignore the exception
                //if the connection or download is not possible, just don't try to update!
            }

            //the rest has to go to the main thread
            //_services.Get<Synchronizer>().Enqueue(InitializeLayoutManager);
        }
    }
}