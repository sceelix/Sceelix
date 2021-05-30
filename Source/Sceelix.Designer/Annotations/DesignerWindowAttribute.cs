using System;

namespace Sceelix.Designer.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DesignerWindowAttribute : Attribute
    {
        private String _windowName;
        private Type _windowType;



        public DesignerWindowAttribute(string windowName)
        {
            _windowName = windowName;
        }



        internal Type WindowType
        {
            get { return _windowType; }
            set { _windowType = value; }
        }



        internal string WindowName
        {
            get { return _windowName; }
            set { _windowName = value; }
        }
    }
}