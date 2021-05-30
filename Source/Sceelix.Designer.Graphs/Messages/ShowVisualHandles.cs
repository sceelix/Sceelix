using Sceelix.Designer.Graphs.GUI.Model;

namespace Sceelix.Designer.Graphs.Messages
{
    public class ShowVisualHandles
    {
        public ShowVisualHandles(VisualNode visualNode)
        {
            VisualNode = visualNode;
        }



        public VisualNode VisualNode
        {
            get;
            set;
        }
    }
}