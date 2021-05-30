using System.Collections.Generic;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Logging
{
    public class NodeLogMessage
    {
        private readonly object _message;
        private readonly List<Node> _nodes;



        public NodeLogMessage(object message, Node executionNodeNode)
        {
            _message = message;
            _nodes = new List<Node>() {executionNodeNode};
        }



        public object Message
        {
            get { return _message; }
        }


        public List<Node> Nodes
        {
            get { return _nodes; }
        }
    }
}