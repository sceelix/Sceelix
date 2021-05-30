using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.Messages
{
    public class GraphContentChanged
    {
        private readonly FileItem _item;
        private readonly bool _worthExecuting;



        public GraphContentChanged(FileItem item, bool worthExecuting = false)
        {
            _item = item;
            _worthExecuting = worthExecuting;
        }



        public FileItem Item
        {
            get { return _item; }
        }



        public bool WorthExecuting
        {
            get { return _worthExecuting; }
        }
    }
}