using System;

namespace Sceelix.Annotations
{
    public class StandardJsonConverterAttribute : TypeKeyAttribute
    {
        public StandardJsonConverterAttribute(Type typeKey)
            : base(typeKey)
        {
        }
    }
}