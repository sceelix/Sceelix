using System;

namespace Sceelix.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExpressionFunctionsAttribute : Attribute
    {
        public ExpressionFunctionsAttribute(string name)
        {
            Name = name;
        }



        public string Name
        {
            get;
        }
    }
}