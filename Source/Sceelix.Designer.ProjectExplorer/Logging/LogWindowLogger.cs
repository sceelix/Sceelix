using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Designer.ProjectExplorer.Logging
{
    public class LogWindowLogger : ILogger
    {
        private readonly MessageManager _messageManager;



        public LogWindowLogger(MessageManager messageManager)
        {
            _messageManager = messageManager;
        }

        public void Log(object message, LogType logType = LogType.Auto)
        {
            if (logType == LogType.Error && message is Exception)
                _messageManager.Publish(new ExceptionThrown((Exception)message));
            else
                _messageManager.Publish(new LogMessageSent(message.SafeToString(), logType));
        }
    }
}
