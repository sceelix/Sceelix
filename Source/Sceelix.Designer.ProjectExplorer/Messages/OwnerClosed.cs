using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class OwnerClosed
    {
        private readonly Object _owner;



        public OwnerClosed(Object owner)
        {
            _owner = owner;
        }



        public object Owner
        {
            get { return _owner; }
        }
    }
}