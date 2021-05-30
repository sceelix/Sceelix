using Sceelix.Core.Graphs;
using Sceelix.Core.Handles;

namespace Sceelix.Core.Messages
{
    public class AddVisualHandle
    {
        public AddVisualHandle(Node node, VisualHandle visualHandle)
        {
            Node = node;
            VisualHandle = visualHandle;
        }



        public Node Node
        {
            get;
            set;
        }


        public VisualHandle VisualHandle
        {
            get;
            set;
        }
    }
}