using System;

namespace Sceelix.Designer.Messaging
{
    public class ExceptionThrown
    {
        private readonly Exception _exception;
        private readonly Object _responseMessage;
        private readonly DateTime _time;



        public ExceptionThrown(Exception exception, Object responseMessage = null)
        {
            _exception = exception;
            _responseMessage = responseMessage;
            _time = DateTime.Now;
        }



        public Exception Exception
        {
            get { return _exception; }
        }



        public DateTime Time
        {
            get { return _time; }
        }



        public Object ResponseMessage
        {
            get { return _responseMessage; }
        }
    }
}