using System;
using System.Globalization;
using System.Threading;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.Layouts;
using Sceelix.Designer.Login;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Managers
{
    internal class LoadingManager
    {
        //events
        public event EventHandler<EventArgs> Finished = delegate { };

        private ILoginManager _loginManager;
        private readonly PluginManager _pluginManager;

        private readonly UIScreen _uiScreen;
        private readonly Synchronizer _synchronizer;
        private LayoutManager _layoutManager;



        public LoadingManager(ILoginManager loginManager, PluginManager pluginManager, UIScreen uiScreen, Synchronizer synchronizer)
        {
            _loginManager = loginManager;
            _pluginManager = pluginManager;
            _uiScreen = uiScreen;
            _synchronizer = synchronizer;

            _loginManager.Finished += LoginManagerOnFinished;
        }



        private void LoginManagerOnFinished(object sender, EventArgs e)
        {
            //starting loading the rest in a separate thread
            Thread thread = new Thread(PerformLoading)
            {
                IsBackground = true, Name = "Plugin Loading", CurrentCulture = CultureInfo.InvariantCulture, CurrentUICulture = CultureInfo.InvariantCulture
            };
            thread.Start();
        }



        private void PerformLoading()
        {
            _loginManager.LoadingWindow.Text = "Loading Plugins and Libraries...";

            DesignerProgram.Log.Debug("Loading Plugins.");

            //this function takes a while, so leave it at this thread
            _pluginManager.Initialize();

            DesignerProgram.Log.Debug("Loading Layouts.");

            _pluginManager.SubServices.Register(_layoutManager = new LayoutManager(_pluginManager.SubServices, _uiScreen));

            DesignerProgram.Log.Debug("Loading Process has Finished.");

            //give the indication that we are finished
            _synchronizer.Enqueue(Finish);
        }



        private void Finish()
        {
            _loginManager.LoadingWindow.Close();

            //alert interested parties
            Finished.Invoke(this, EventArgs.Empty);
        }



        public PluginManager PluginManager
        {
            get { return _pluginManager; }
        }



        public LayoutManager LayoutManager
        {
            get { return _layoutManager; }
        }
    }
}
