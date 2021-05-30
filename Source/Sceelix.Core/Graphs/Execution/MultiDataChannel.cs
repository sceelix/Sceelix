using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Core.Extensions;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Execution
{
    public class MultiDataChannel : IDataChannel
    {
        public MultiDataChannel(IEnumerable<Edge> edges, Dictionary<Node, ExecutionNode> nodeDictionary)
        {
            //if there are, create datachannels between the execution nodes
            foreach (Edge edge in edges)
            {
                ExecutionNode destinationExecutionNode = nodeDictionary[edge.ToPort.Node];
                DataChannels.Add(new DataChannel(edge.ToPort, destinationExecutionNode));
                //destinationExecutionNode.IncomingConnections++;
            }
        }



        public IEnumerable<ExecutionNode> ConnectedNodes
        {
            get { return DataChannels.SelectMany(val => val.ConnectedNodes); }
        }



        public IEnumerable<Port> ConnectedPorts
        {
            get { return DataChannels.SelectMany(val => val.ConnectedPorts); }
        }



        public List<IDataChannel> DataChannels
        {
            get;
        } = new List<IDataChannel>();



        public void TransferData(IEnumerable<IEntity> entities)
        {
            List<IEntity> sceelixObjectList = entities.ToList();
            foreach (DataChannel dataChannel in DataChannels.OfType<DataChannel>())
                if (dataChannel.DestinationPort.CastTo<InputPort>().IsImpulse)
                    dataChannel.TransferData(sceelixObjectList.Select(val => val.CreateImpulse()));
                else
                    dataChannel.TransferData(sceelixObjectList.Select(val => val.DeepClone()));
            /*if (dataChannel.DestinationNode.CanExecute())
                    nextExecutableNodes.Add(dataChannel.DestinationNode);*/
        }



        public void TransferData(IEntity entity)
        {
        }
    }
}