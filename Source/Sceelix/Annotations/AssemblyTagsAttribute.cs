using System;
using System.Linq;

namespace Sceelix.Annotations
{
    /// <summary>
    /// Specifies tags for assemblies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyTagsAttribute : Attribute
    {
        /// <summary>
        /// Specifies tags for this assembly.
        /// </summary>
        /// <param name="tags">Tags for this assembly (e.g. Database, 3D)</param>
        public AssemblyTagsAttribute(params string[] tags)
        {
            Tags = tags.Select(x => x.Trim()).ToArray();
        }



        /// <summary>
        /// Tags for this project
        /// </summary>
        public string[] Tags
        {
            get;
        }
    }
}