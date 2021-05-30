using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Renderer3D.Messages
{
    public class RenderDataReady
    {
        private readonly Bitmap _bitmap;



        public RenderDataReady(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }



        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }
    }
}