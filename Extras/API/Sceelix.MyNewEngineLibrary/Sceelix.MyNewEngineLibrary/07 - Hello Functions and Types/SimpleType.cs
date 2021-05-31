using System;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This class is a simple example of a class definition to be used inside parameter expressions 
    /// and as new attribute types.
    /// 
    /// Nothing in particular is necessary in such a class definition.
    /// If it is to be used inside attributes, the class should implement the ICloneable interface). Cloning
    /// is performed, for instance, when new entities are created from previous ones. Depending on your
    /// strategy, you may choose to implement deep cloning or something more shallow.
    /// 
    /// For structs composed purely of value types, a standard struct value copy will be performed, so implementing
    /// the ICloneable is not necessary.
    /// </summary>
    public class SimpleClass : ICloneable
    {
        private String _name;
        private int _value;
        private bool _exists;


        public SimpleClass(string name, int value, bool exists)
        {
            _name = name;
            _value = value;
            _exists = exists;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Value
        {
            get { return _value; }
        }

        public bool Exists
        {
            get { return _exists; }
        }

        public object Clone()
        {
            return new SimpleClass(_name, _value, _exists);
        }
    }
}
