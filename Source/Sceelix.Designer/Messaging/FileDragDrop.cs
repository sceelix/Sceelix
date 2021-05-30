using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Messaging
{
    public class FileDragDrop
    {
        private readonly string[] _paths;



        public FileDragDrop(string[] paths)
        {
            _paths = paths;
        }



        public string[] Paths
        {
            get { return _paths; }
        }
    }
}
