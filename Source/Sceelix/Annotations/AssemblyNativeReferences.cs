using System;

namespace Sceelix.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyNativeReferences : Attribute
    {
        public AssemblyNativeReferences(params string[] relativePaths)
        {
            RelativePaths = relativePaths;
        }



        public string[] RelativePaths
        {
            get;
        }
    }
}