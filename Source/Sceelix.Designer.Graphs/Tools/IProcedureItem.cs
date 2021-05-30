using System;
using Sceelix.Core;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Tools
{
    public interface IProcedureItem
    {
        String Label
        {
            get;
        }



        String Tags
        {
            get;
        }



        String Category
        {
            get;
        }



        String Description
        {
            get;
        }



        Node GenerateNode(Point position, IProcedureEnvironment environment);
    }
}