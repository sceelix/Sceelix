using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Graphs.Inspector.Entities.SubObjects
{
    public interface IInspectionInfo
    {
        bool HasChildren
        {
            get;
        }

        String Label
        {
            get;
        }

        IEnumerable<object> Children
        {
            get;
        }

        bool IsInitiallyExpanded
        {
            get;
        }

        String Description
        {
            get;
        }
    }
}
