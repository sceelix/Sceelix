using System;

namespace Sceelix.Annotations
{
    public class TypeKeyAttribute : Attribute
    {
        public TypeKeyAttribute(Type typeKey)
        {
            TypeKey = typeKey;
            IncludeSubclasses = true;
        }



        /// <summary>
        /// Indicates if this serialization handler applies to the derived types
        /// of the indicated types. Default is true, meaning that it applies
        /// to all derived types, unless overridden.
        /// </summary>
        public bool IncludeSubclasses
        {
            get;
            set;
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


        public Type TypeKey
        {
            get;
            set;
        }


        /*public String Context
        {
            get;
            set;
        }*/


        /*public Object Data
        {
            get; set;
        }*/
    }
}