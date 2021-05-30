using System;

namespace Sceelix.Core.Annotations
{
    public class SectionAttribute : Attribute
    {
        public SectionAttribute(string name)
        {
            Name = name;
        }



        public string Name
        {
            get;
        }
    }
}