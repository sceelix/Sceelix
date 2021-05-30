using Sceelix.Core.Messages;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Messaging
{
    class GraphDesignerMessenger : IMessenger
    {
        private readonly IServiceLocator _services;


        public GraphDesignerMessenger(IServiceLocator services)
        {
            _services = services;
        }


        public void Send(object message)
        {
            _services.Get<MessageManager>().Publish(message);
        }
    }
}
