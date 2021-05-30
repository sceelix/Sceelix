using System;

namespace Sceelix.Core.Annotations
{
    /// <summary>
    /// Identifies this class as entity that can be reflected upon.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        /// Indicates name as it appears in ports, documentation, selection boxes, etc.
        /// </summary>
        /// <param name="readableName"></param>
        public EntityAttribute(string readableName)
        {
            Name = readableName;
        }



        /// <summary>
        /// A more human-readable name, as it appears in ports, documentation, 
        /// selection boxes, etc.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if this entity should be listed as
        /// a manipulable type, for example in the Type
        /// conditional parameter. Default is true.
        /// </summary>
        public bool TypeBrowsable
        {
            get;
            set;
        } = true;
    }
}