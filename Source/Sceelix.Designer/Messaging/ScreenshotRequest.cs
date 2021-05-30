using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Messaging
{
    public class ScreenshotRequest
    {
        private readonly Action<Texture2D> _callback;



        public ScreenshotRequest(Action<Texture2D> callback)
        {
            _callback = callback;
        }



        public Action<Texture2D> Callback
        {
            get { return _callback; }
        }
    }
}