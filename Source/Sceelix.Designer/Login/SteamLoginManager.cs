using System;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Login
{
    internal class SteamLoginManager : ILoginManager
    {
        //events
        public event EventHandler<EventArgs> Finished = delegate { };
        
        private readonly IServiceLocator _services;
        private readonly UIScreen _uiScreen;
        private readonly ExitManager _exitManager;

        //windows that we are going to show
        private LoadingWindow _loadingWindow;
        



        public SteamLoginManager(IServiceLocator services, UIScreen uiScreen)
        {
            _services = services;
            _uiScreen = uiScreen;
            _exitManager = services.Get<ExitManager>();
        }



        public void Initialize()
        {
            //show the loading window with a message
            _loadingWindow = new LoadingWindow();
            _services.Get<WindowAnimator>().Show(_loadingWindow);

            SteamCheck();
        }



        public LoadingWindow LoadingWindow
        {
            get { return _loadingWindow; }
        }




        /// <summary>
        /// Tips from Steam Forums:
        /// 1) if SteamAPI_RestartAppIfNecessary(): exit
        /// 2) if not SteamAPI_IsSteamRunning(): exit with message box "please start Steam client"
        /// 3) if not SteamAPI_Init(): exit with message box "init failed" (it does happen)
        /// 4) if not SteamApps()->BIsSubscribedApp(APPID): exit with message box "game not owned"
        /// </summary>
        private void SteamCheck()
        {
            _loadingWindow.Text = "Connecting to Steam Account...";

#if STEAM

            var appId = new Steamworks.AppId_t(509020);

            if (Steamworks.SteamAPI.RestartAppIfNecessary(appId))
            {
                ShowSteamNotConnectedDialog("An error has occurred. Please restart the application.");
                return;
            }

            if (!Steamworks.SteamAPI.IsSteamRunning())
            {
                ShowSteamNotConnectedDialog("Could not connect to your Steam account. Please make sure that Steam is running.");
                return;
            }

            if (!Steamworks.SteamAPI.Init())
            {
                ShowSteamNotConnectedDialog("Could not initialize Steam data. You may need to restart the application.");
                return;
            }

            if (!Steamworks.SteamApps.BIsSubscribedApp(appId))
            {
                ShowSteamNotConnectedDialog("Could not verify application ownership. Please make sure that you have a valid product license.");
                return;
            }

#endif

            Finished.Invoke(this, EventArgs.Empty);
        }



        /// <summary>
        /// Error message when Steam is not able to connect/verify.
        /// </summary>
        /// <param name="message"></param>
        private void ShowSteamNotConnectedDialog(String message)
        {
            MessageWindow window = new MessageWindow();

            window.Title = "Steam Error";
            window.Text = message + "\n\nIf the problem persists, please contact us at Sceelix (www.sceelix.com).";
            window.Buttons = new[] {"Exit"};
            window.MessageIcon = MessageWindowIcon.Error;

            window.Click += delegate { _exitManager.Exit(); };
            window.Show(_uiScreen);
        }
    }
}