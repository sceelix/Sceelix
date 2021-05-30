using DigitalRune.Game.UI;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector
{
    [DesignerWindow("Inspector")]
    public class InspectorWindow : AnimatedWindow, IServiceable
    {
        private object _currentOwner;
        private MessageManager _messageManager;


        public void Initialize(IServiceLocator services)
        {
            _messageManager = services.Get<MessageManager>();

            _messageManager.Register<ShowPropertiesRequest>(OnShowPropertiesRequest);
            _messageManager.Register<OwnerClosed>(OnOwnerClosed);
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "Inspector";
            Width = 300;
            Height = 500;
            CanResize = true;
        }


        private void OnOwnerClosed(OwnerClosed obj)
        {
            if (_currentOwner == obj.Owner)
            {
                Content = null;
            }
        }



        private void OnShowPropertiesRequest(ShowPropertiesRequest obj)
        {
            Content = null;

            Content = obj.ControlToShow;

            if (Content != null)
            {
                _currentOwner = obj.Owner;

                Content.HorizontalAlignment = HorizontalAlignment.Stretch;
                Content.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }
    }
}