using System;

namespace Sceelix.Core.Annotations
{
    public class SubEntityAttribute : Attribute
    {
        public SubEntityAttribute(string label)
        {
            Label = label;
        }



        public string Label
        {
            get;
        }
    }
}