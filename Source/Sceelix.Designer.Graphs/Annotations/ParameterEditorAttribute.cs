using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sceelix.Annotations;

namespace Sceelix.Designer.Graphs.Annotations
{
    public class ParameterEditorAttribute : TypeKeyAttribute
    {
        public ParameterEditorAttribute(Type typeKey) 
            : base(typeKey)
        {
        }
    }
}
