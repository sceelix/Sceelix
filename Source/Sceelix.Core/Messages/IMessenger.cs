using System.ComponentModel;

namespace Sceelix.Core.Messages
{
    [DefaultValue(typeof(EmptyMessenger))]
    public interface IMessenger
    {
        void Send(object message);
    }
}