using System;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class GenericMessage
    {
        private Object[] _data;
        private String _name;



        public GenericMessage(string name, params Object[] data)
        {
            _name = name;
            _data = data;
        }



        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }



        public object[] Data
        {
            get { return _data; }
            set { _data = value; }
        }
    }
}