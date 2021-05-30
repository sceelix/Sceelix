using System;
using Sceelix.Annotations;

namespace Sceelix.Unity.Annotations
{
    public class UnityJsonConverterAttribute : TypeKeyAttribute
    {
        public UnityJsonConverterAttribute(Type typeKey)
            : base(typeKey)
        {
        }
    }
}