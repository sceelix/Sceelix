using System;
using System.ComponentModel;
using Sceelix.Annotations;

namespace Sceelix.Designer.Annotations
{
    [DisplayName("Designer")]
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DesignerLibraryAttribute : SceelixLibraryAttribute
    {
        public DesignerLibraryAttribute()
        {
        }
    }
}