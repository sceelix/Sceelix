using System;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Designer.Graphs.Logging
{
    class GraphDesignerLogger : ILogger
    {
        private readonly IServiceLocator _services;



        public GraphDesignerLogger(IServiceLocator services)
        {
            _services = services;
        }



        public void Log(object message, LogType logType = LogType.Auto)
        {
            

            Object actualMessage = message;
            Object responseMessage = null;

            if (message is NodeLogMessage)
            {
                var nodeMessage = (NodeLogMessage) message;
                
                actualMessage = nodeMessage.Message;

                //if the user double clicks it, it zooms on the nodes
                responseMessage = new FrameNodes(nodeMessage.Nodes);
            }

            if (logType == LogType.Auto)
                logType = actualMessage is Exception ? LogType.Error : LogType.Information;

            //either way, send the request to the log window or whoever is concerned
            if (logType == LogType.Error && actualMessage is Exception)
                _services.Get<MessageManager>().Publish(new ExceptionThrown((Exception)actualMessage, responseMessage));
            else
                _services.Get<MessageManager>().Publish(new LogMessageSent(actualMessage.SafeToString(), logType, responseMessage));
        }
    }
}
