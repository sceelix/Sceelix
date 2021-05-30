using System;
using Sceelix.Core.Graphs.Execution;
using Sceelix.Logging;

namespace Sceelix.Designer.Graphs.Logging
{
    public class NodeDesignerLogger : ILogger
    {
        private readonly ExecutionNode _executionNode;
        private readonly ILogger _logger;
        

        public NodeDesignerLogger(ExecutionNode executionNode, ILogger logger)
        {
            _executionNode = executionNode;
            _logger = logger;
        }



        public void Log(object message, LogType logType = LogType.Auto)
        {
            if (message is NodeLogMessage)
            {
                ((NodeLogMessage)message).Nodes.Add(_executionNode.Node);
                _logger.Log(message, logType);
            }
            else
            {
                //if (logType == LogType.Auto)
                //    logType = message is Exception ? LogType.Error : LogType.Information;

                _logger.Log(new NodeLogMessage(message,_executionNode.Node), logType);
            }
        }
    }
}