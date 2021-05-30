using System.Collections.Generic;
using Sceelix.Core.Data;

namespace Sceelix.Core.Graphs.Execution
{
    public class DataChannel : IDataChannel
    {
        private readonly bool _transfer;


        /*public DataChannel(int labelDestination, ExecutionNode destinationNode, bool transfer)
        {
            _labelDestination = labelDestination;
            _destinationNode = destinationNode;
            _transfer = transfer;
            _destinationNode.IncomingConnections++;
        }*/



        public DataChannel(Edge edge, Dictionary<Node, ExecutionNode> nodeDictionary)
        {
            Edge = edge;
            DestinationPort = edge.ToPort;
            LabelDestination = edge.ToPort.Index;
            DestinationNode = nodeDictionary[edge.ToPort.Node];
            DestinationNode.IncomingConnections++;
            _transfer = edge.ToPort.PortState != PortState.Blocked;
        }



        public DataChannel(Port port, ExecutionNode destinationExecutionNode)
        {
            DestinationPort = port;
            LabelDestination = port.Index;
            DestinationNode = destinationExecutionNode;
            _transfer = port.PortState != PortState.Blocked;
            DestinationNode.IncomingConnections++;
        }



        public IEnumerable<ExecutionNode> ConnectedNodes
        {
            get { yield return DestinationNode; }
        }



        public IEnumerable<Port> ConnectedPorts
        {
            get { yield return DestinationPort; }
        }



        public ExecutionNode DestinationNode
        {
            get;
        }


        public Port DestinationPort
        {
            get;
        }


        public Edge Edge
        {
            get;
        }


        public bool IsCyclic
        {
            get;
            set;
        }


        public bool IsVisited
        {
            get;
            set;
        }


        public int LabelDestination
        {
            get;
        }



        public void TransferData(IEnumerable<IEntity> entities)
        {
            if (_transfer)
                DestinationNode.Procedure.Inputs[LabelDestination].Enqueue(entities);
        }



        public void TransferData(IEntity entity)
        {
            if (_transfer)
                DestinationNode.Procedure.Inputs[LabelDestination].Enqueue(entity);
        }
    }


    /*public class AggregativeDataChannel : IDataChannel
    {
        private String _gateLabel;
        private Output _output;

        public AggregativeDataChannel(string gateLabel)
        {
            _gateLabel = gateLabel;
        }

        public void TransferData(IEnumerable<IEntity> entities)
        {
            if(_output != null)
                _output.WriteAll(entities);
        }

        public void TransferData(IEntity entity)
        {
            if (_output != null)
                _output.Write(entity);
        }

        public IEnumerable<ExecutionNode> ConnectedNodes
        {
            get { yield break; }
        }

        public string GateLabel
        {
            get { return _gateLabel; }
            set { _gateLabel = value; }
        }

        public Output Output
        {
            get { return _output; }
            set { _output = value; }
        }
    }*/
}