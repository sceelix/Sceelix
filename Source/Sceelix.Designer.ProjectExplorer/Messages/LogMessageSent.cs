using System;
using Sceelix.Core.Environments;
using Sceelix.Logging;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class LogMessageSent
    {
        private readonly String _message;
        private readonly LogType _logType;
        private readonly Object _responseMessage;
        



        public LogMessageSent(string message, LogType logType = LogType.Information, Object responseMessage = null)
        {
            _message = message;
            _logType = logType;
            _responseMessage = responseMessage;
        }



        public string Message
        {
            get { return _message; }
        }



        public LogType LogType
        {
            get { return _logType; }
        }


        public Object ResponseMessage
        {
            get { return _responseMessage; }
        }
    }
}