using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI;
using Sceelix.Designer.Managers;

namespace Sceelix.Designer.Layouts
{
    public class WindowLocation : ICloneable
    {
        private string _windowType;
        
        private RectangleF _bounds;



        public WindowLocation()
        {
        }



        public WindowLocation(Window window)
        {
            //var screenBounds = window.Screen.ActualBounds;

            _windowType = window.GetType().ToString();

            var screenBounds = ScreenManager.GetWindowArea(window.Screen);

            _bounds = new RectangleF((window.X - screenBounds.X)/screenBounds.Width, (window.Y - screenBounds.Y)/screenBounds.Height, window.Width/screenBounds.Width, window.Height/screenBounds.Height);
            //_bounds = new RectangleF((window.ActualX - screenBounds.X) / screenBounds.Width, (window.ActualY - screenBounds.Y) / screenBounds.Height, window.ActualWidth / screenBounds.Width, window.ActualHeight / screenBounds.Height);
        }



        //Type of Bounds
        public RectangleF Bounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }



        //Type of Window
        public String WindowType
        {
            get { return _windowType; }
            set { _windowType = value; }
        }



        public object Clone()
        {
            return new WindowLocation {WindowType = WindowType, Bounds = Bounds};
        }
    }
}