using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ReloadGraphRequest
    {
        private readonly FileItem _item;



        public ReloadGraphRequest(FileItem item)
        {
            _item = item;
        }



        public FileItem Item
        {
            get { return _item; }
        }
    }
}