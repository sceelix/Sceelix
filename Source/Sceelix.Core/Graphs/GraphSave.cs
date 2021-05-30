using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Graphs
{
    public static class GraphSave
    {
        private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};



        public static string GetXML(this Graph graph)
        {
            StringWriter stringWriter = new StringWriter();

            SaveXML(graph, XmlWriter.Create(stringWriter, _xmlWriterSettings));

            return stringWriter.ToString();
        }



        private static void ProcessPortIndices(List<Port> ports, Dictionary<Port, string> portIndices, string portType, int nodeId)
        {
            for (int index = 0; index < ports.Count; index++)
            {
                string id = nodeId + "#" + portType + "#" + index;
                portIndices.Add(ports[index], id);
            }
        }



        public static void SaveXML(this Graph graph, string path)
        {
            SaveXML(graph, XmlWriter.Create(path, _xmlWriterSettings));
        }



        private static void SaveXML(this Graph graph, XmlWriter writer)
        {
            graph.RefreshNodeIds();

            Dictionary<Port, string> portIndices = new Dictionary<Port, string>();

            writer.WriteStartDocument();
            {
                writer.WriteStartElement("Graph");
                {
                    writer.WriteAttributeString("Guid", graph.Guid.ToString());
                    writer.WriteAttributeString("Author", graph.Author);

                    if (!string.IsNullOrWhiteSpace(graph.Description))
                        writer.WriteAttributeString("Description", graph.Description);

                    writer.WriteAttributeString("Color", graph.Color);

                    //writer.WriteAttributeString("OutputStyle", graph.OutputStyle.ToString());

                    if (!string.IsNullOrWhiteSpace(graph.Tags))
                        writer.WriteAttributeString("Tags", graph.Tags);

                    if (!string.IsNullOrWhiteSpace(graph.Category))
                        writer.WriteAttributeString("Category", graph.Category);

                    //save the version of the assembly when this was created
                    writer.WriteAttributeString("Version", typeof(GraphSave).Assembly.GetName().Version.ToString());

                    writer.WriteStartElement("Parameters");
                    {
                        foreach (ParameterInfo parameterInfo in graph.ParameterInfos)
                        {
                            writer.WriteStartElement("Parameter");

                            parameterInfo.WriteXMLDefinition(writer);

                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();

                    /*writer.WriteStartElement("Variables");
                    {
                        foreach (AttributeInfo variableInfo in graph.VariableInfos)
                            variableInfo.WriteXML(writer);
                    }
                    writer.WriteEndElement();
                    */
                    writer.WriteStartElement("Nodes");
                    {
                        for (int index = 0; index < graph.Nodes.Count; index++)
                            if (graph.Nodes[index] is InvalidNode)
                            {
                                InvalidNode invalidNode = (InvalidNode) graph.Nodes[index];

                                writer.WriteNode(new XmlNodeReader(invalidNode.XMLNode), true);

                                ProcessPortIndices(invalidNode.InputPorts, portIndices, "I", index);
                                ProcessPortIndices(invalidNode.OutputPorts, portIndices, "O", index);
                            }
                            else
                            {
                                WriteNodeXML(graph.Nodes[index], writer, index, portIndices);
                            }
                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("Edges");
                    {
                        foreach (Edge edge in graph.Edges)
                            WriteEdgeXML(edge, writer, portIndices);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();

            writer.Close();
        }



        private static void WriteEdgeXML(Edge edge, XmlWriter writer, Dictionary<Port, string> portIndices)
        {
            writer.WriteStartElement("Edge");
            writer.WriteAttributeString("FromPort", portIndices[edge.FromPort]);
            writer.WriteAttributeString("ToPort", portIndices[edge.ToPort]);

            //if the edge is disabled, 
            if (!edge.Enabled)
                writer.WriteAttributeString("Disabled", "True");

            //if (edge.FromPort.Node is ComponentNode)
            {
                writer.WriteAttributeString("FromNode", edge.FromPort.Node.Id.ToString());
                writer.WriteAttributeString("ToNode", edge.ToPort.Node.Id.ToString());
                writer.WriteAttributeString("FromLabel", edge.FromPort.FullLabel);
                writer.WriteAttributeString("ToLabel", edge.ToPort.FullLabel);
            }

            writer.WriteEndElement();
        }



        private static void WriteNodeXML(Node node, XmlWriter writer, int nodeIndex, Dictionary<Port, string> portIndices)
        {
            writer.WriteStartElement("Node");
            {
                writer.WriteAttributeString("Guid", node.Guid.ToString());
                writer.WriteAttributeString("NodeType", node.NodeTypeName);
                writer.WriteAttributeString("NodeTypeGUID", node.ProcedureAttribute.Guid);
                writer.WriteAttributeString("Position", node.Position.ToString(CultureInfo.InvariantCulture));

                if (node.HasImpulsePort)
                    writer.WriteAttributeString("HasImpulsePort", node.HasImpulsePort.ToString(CultureInfo.InvariantCulture));

                if (node.UseCache)
                    writer.WriteAttributeString("UseCache", node.UseCache.ToString());

                if (node.DisableInSubgraphs)
                    writer.WriteAttributeString("DisableInSubgraphs", node.DisableInSubgraphs.ToString());

                if (node.Label != node.DefaultLabel)
                    writer.WriteAttributeString("Label", node.Label);

                node.WriteSpecificXML(writer);

                writer.WriteStartElement("Arguments");
                {
                    foreach (ParameterInfo argument in node.Parameters)
                    {
                        writer.WriteStartElement("Argument");

                        argument.WriteArgumentXML(writer);

                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                /*writer.WriteStartElement("VariableLinks");
                {
                    foreach (AttributeLink variableLink in node.VariableLinks)
                        variableLink.WriteXML(writer);
                }
                writer.WriteEndElement();*/

                //list the input ports
                writer.WriteStartElement("InputPorts");
                {
                    writer.WriteAttributeString("Count", node.InputPorts.Count.ToString(CultureInfo.InvariantCulture));
                    WritePortsXML(writer, node.InputPorts, portIndices, "I", nodeIndex);
                }
                writer.WriteEndElement();

                //list the output ports
                writer.WriteStartElement("OutputPorts");
                {
                    writer.WriteAttributeString("Count", node.OutputPorts.Count.ToString(CultureInfo.InvariantCulture));
                    WritePortsXML(writer, node.OutputPorts, portIndices, "O", nodeIndex);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }



        private static void WritePortsXML(XmlWriter writer, List<Port> ports, Dictionary<Port, string> portIndices, string portType, int nodeId)
        {
            for (int index = 0; index < ports.Count; index++)
                /*String id = nodeId + "#" + portType + "#" + index;
                    portIndices.Add(ports[index], id);*/

                if (ports[index].PortState != PortState.Normal)
                {
                    writer.WriteStartElement("Port");
                    {
                        writer.WriteAttributeString("id", index.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("state", ports[index].PortState.ToString());

                        if (ports[index].PortState == PortState.Gate)
                            writer.WriteAttributeString("GateLabel", ports[index].GateLabel);
                    }
                    writer.WriteEndElement();
                }

            ProcessPortIndices(ports, portIndices, portType, nodeId);
        }
    }
}