using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class FileImported
    {
        private readonly FileItem _item;



        public FileImported(FileItem item)
        {
            _item = item;
        }



        public FileItem Item
        {
            get { return _item; }
        }
    }
}