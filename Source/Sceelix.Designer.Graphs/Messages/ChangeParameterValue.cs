using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Graphs.Messages
{
    public class ChangeParameterValue
    {
        public ChangeParameterValue(string fullParameterName, object value)
        {
            FullParameterName = fullParameterName;
            Value = value;
        }



        public String FullParameterName
        {
            get;
            set;
        }



        public Object Value
        {
            get;
            set;
        }
    }
}