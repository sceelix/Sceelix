using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.IO;

namespace Sceelix.Core.Graphs.Execution
{
    public class OutputDataChannel : IDataChannel
    {
        private readonly Output _output;



        public OutputDataChannel(Output output)
        {
            _output = output;
        }



        public IEnumerable<ExecutionNode> ConnectedNodes
        {
            get { yield break; }
        }



        public IEnumerable<Port> ConnectedPorts
        {
            get { yield break; }
        }



        public void TransferData(IEnumerable<IEntity> entities)
        {
            _output.Write(entities);
        }



        public void TransferData(IEntity entity)
        {
            _output.Write(entity);
        }
    }
}