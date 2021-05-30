using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class FileCreated
    {
        private readonly FileItem _item;



        public FileCreated(FileItem item)
        {
            _item = item;
        }



        public FileItem Item
        {
            get { return _item; }
        }
    }
}