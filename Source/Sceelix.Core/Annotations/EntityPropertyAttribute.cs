using System;

namespace Sceelix.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityPropertyAttribute : Attribute
    {
        public EntityPropertyAttribute()
        {
        }



        public EntityPropertyAttribute(string readableName)
        {
            ReadableName = readableName;
        }



        public Type HandleType
        {
            get;
            set;
        }


        public string ReadableName
        {
            get;
            set;
        }
    }
}