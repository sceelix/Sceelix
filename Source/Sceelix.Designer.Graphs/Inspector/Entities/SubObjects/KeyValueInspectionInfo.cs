using System;
using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.Inspector.Entities.SubObjects
{
    public class KeyValueInspectionInfo : IInspectionInfo
    {
        private readonly string _name;
        private readonly object _value;
        private readonly string _description;



        public KeyValueInspectionInfo(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public KeyValueInspectionInfo(string name, object value, string description)
        {
            _name = name;
            _value = value;
            _description = description;
        }


        public bool HasChildren
        {
            get
            {
                if (_value is SceeList)
                    return ((SceeList)_value).Count > 0;

                return false;
            }
        }



        public String ValueAsString
        {
            get
            {
                if (!(_value is SceeList))
                    return _value.SafeToString();

                return null;
            }
        }



        public String Label
        {
            get { return _name; }
        }



        public IEnumerable<object> Children
        {
            get
            {
                if (_value is SceeList)
                {
                    var list = (SceeList)_value;

                    foreach (KeyValuePair<String, object> pair in list.KeyValues)
                    {
                        yield return new KeyValueInspectionInfo(pair.Key, pair.Value);
                    }
                }
            }
        }



        public bool IsInitiallyExpanded
        {
            get { return false; }
        }



        public string Description
        {
            get { return _description; }
        }



        public object Value
        {
            get { return _value; }
        }
    }
}
