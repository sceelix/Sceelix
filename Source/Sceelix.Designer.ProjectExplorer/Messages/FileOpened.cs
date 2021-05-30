using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class FileOpened
    {
        private readonly FileItem _item;



        public FileOpened(FileItem item)
        {
            _item = item;
        }



        public FileItem Item
        {
            get { return _item; }
        }
    }
}