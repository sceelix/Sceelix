using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Messaging
{
    public class ClientSizeChanged
    {

        public ClientSizeChanged(float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            NewWidth = newWidth;
            NewHeight = newHeight;
        }



        public float NewWidth
        {
            get;
            private set;
        }

        public float NewHeight
        {
            get;
            private set;
        }

        public float OldWidth
        {
            get;
            private set;
        }

        public float OldHeight
        {
            get;
            private set;
        }
    }
}
