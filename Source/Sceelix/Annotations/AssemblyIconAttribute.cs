using System;

namespace Sceelix.Annotations
{
    /// <summary>
    /// Specifies the icon associated to the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyIconAttribute : Attribute
    {
        /// <summary>
        /// Specifies the icon for this assembly. Should be marked as embedded resource.
        /// </summary>
        /// <param name="iconPath">Path to the icon (for example, "Resources/MyIcon.png"). </param>
        public AssemblyIconAttribute(string iconPath)
        {
            IconPath = iconPath;
        }



        /// <summary>
        /// Path to the assembly icon.
        /// </summary>
        public string IconPath
        {
            get;
        }
    }
}