using System;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class DelayedMessage
    {
        private readonly long _delay;
        private readonly Object _message;



        public DelayedMessage(Object message, long delay)
        {
            _message = message;
            _delay = delay;
        }



        public Object Message
        {
            get { return _message; }
        }



        public long Delay
        {
            get { return _delay; }
        }
    }
}