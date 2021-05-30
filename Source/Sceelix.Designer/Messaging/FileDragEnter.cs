using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sceelix.Designer.Messaging
{
    public class FileDragEnter
    {
        private readonly DragEventArgs _dragEventArgs;

        public FileDragEnter(DragEventArgs dragEventArgs)
        {
            _dragEventArgs = dragEventArgs;
        }



        public DragEventArgs DragEventArgs
        {
            get { return _dragEventArgs; }
        }
    }
}
