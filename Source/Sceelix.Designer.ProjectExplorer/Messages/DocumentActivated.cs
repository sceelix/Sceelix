using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class DocumentActivated
    {
        private FileItem _fileItem;



        public DocumentActivated(FileItem fileItem)
        {
            _fileItem = fileItem;
        }



        public FileItem FileItem
        {
            get { return _fileItem; }
        }
    }
}
