using System;
using System.Linq;
using Sceelix.Core.Graphs;

namespace Sceelix.Core.Resolution
{
    public abstract class NodeResolver
    {
        public void CopyNodeProperties(Node oldNode, Node newNode)
        {
            /*oldNode.HasImpulsePort = newNode.HasImpulsePort

            node.HasImpulsePort = xmlNode.GetAttributeOrDefault<bool>("HasImpulsePort", false);
            node.UseCache = xmlNode.GetAttributeOrDefault<bool>("UseCache", false);

            Port.LoadPorts(xmlNode, "InputPorts", node.InputPorts);
            Port.LoadPorts(xmlNode, "OutputPorts", node.OutputPorts);*/
        }



        public void MoveEdgesAndCopyInfo(Port oldPort, Port newPort)
        {
            if (oldPort.GetType() != newPort.GetType())
                throw new ArgumentException("Edge moving can only occur between input ports or between output ports, not combinations.");

            foreach (Edge edge in oldPort.Edges.ToList())
            {
                newPort.Edges.Add(edge);
                oldPort.Edges.Remove(edge);

                if (oldPort is OutputPort)
                    edge.FromPort = newPort;
                else
                    edge.ToPort = newPort;
            }

            newPort.GateLabel = oldPort.GateLabel;
            newPort.PortState = oldPort.PortState;
        }



        public abstract void Resolve(Graph graph, Node node);
    }
}