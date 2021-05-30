using System;

namespace Sceelix.Annotations
{
    public class StringKeyAttribute : Attribute
    {
        public StringKeyAttribute(string key)
        {
            Key = key;
        }



        public string Key
        {
            get;
        }


        public int Priority
        {
            get;
            set;
        }


        public Type TargetType
        {
            get;
            set;
        }


        /*public String Context
        {
            get;
            set;
        }*/
    }
}