using System;

namespace Sceelix.Annotations
{
    /// <summary>
    /// Specifies a website of assembly's project/creators.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyWebsiteAttribute : Attribute
    {
        /// <summary>
        /// Specifies the website link of this library's project/creators.
        /// </summary>
        public AssemblyWebsiteAttribute(string link)
        {
            Link = link;
        }



        /// <summary>
        /// Like to the website of this library's project/creators.
        /// </summary>
        public string Link
        {
            get;
        }
    }
}