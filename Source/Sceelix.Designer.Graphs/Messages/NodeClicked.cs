using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Messages
{
    public class NodeClicked
    {
        private readonly Node _node;



        public NodeClicked(Node node)
        {
            _node = node;
        }



        public Node Node
        {
            get { return _node; }
        }
    }
}