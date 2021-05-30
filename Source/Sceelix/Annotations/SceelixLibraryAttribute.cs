using System;
using System.ComponentModel;

namespace Sceelix.Annotations
{
    [DisplayName("All")]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SceelixLibraryAttribute : Attribute
    {
    }
}