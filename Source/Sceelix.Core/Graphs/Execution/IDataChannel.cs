using System.Collections.Generic;
using Sceelix.Core.Data;

namespace Sceelix.Core.Graphs.Execution
{
    public interface IDataChannel
    {
        IEnumerable<ExecutionNode> ConnectedNodes
        {
            get;
        }


        IEnumerable<Port> ConnectedPorts
        {
            get;
        }


        void TransferData(IEnumerable<IEntity> entities);

        void TransferData(IEntity entity);
    }
}