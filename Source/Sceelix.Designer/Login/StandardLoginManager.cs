using System;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Login
{
    internal class StandardLoginManager : ILoginManager
    {
        //events
        public event EventHandler<EventArgs> Finished = delegate { };

        private IServiceLocator _services;
        private LoadingWindow _loadingWindow;



        public StandardLoginManager(IServiceLocator services)
        {
            _services = services;
        }



        public void Initialize()
        {
            //show the loading window with a message
            _loadingWindow = new LoadingWindow();
            _services.Get<WindowAnimator>().Show(_loadingWindow);
            
            Finished.Invoke(this,EventArgs.Empty);
        }

        

        public LoadingWindow LoadingWindow
        {
            get { return _loadingWindow; }
        }
    }
}
