using System;

namespace Sceelix.Annotations
{
    /// <summary>
    /// Specifies the author (if an individual, instead of company) of the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyAuthorAttribute : Attribute
    {
        /// <summary>
        /// Specifies the author for this assembly.
        /// </summary>
        /// <param name="author">Author name.</param>
        public AssemblyAuthorAttribute(string author)
        {
            Author = author;
        }



        /// <summary>
        /// Author of the assembly.
        /// </summary>
        public string Author
        {
            get;
        }
    }
}