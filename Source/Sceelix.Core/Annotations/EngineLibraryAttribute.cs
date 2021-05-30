using System;
using System.ComponentModel;
using Sceelix.Annotations;

namespace Sceelix.Core.Annotations
{
    [DisplayName("Engine")]
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class EngineLibraryAttribute : SceelixLibraryAttribute
    {
    }
}