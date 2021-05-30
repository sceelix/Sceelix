using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Environments;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resolution;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Core.Graphs
{
    public static class GraphLoad
    {
        private static readonly Dictionary<Type, SystemNode> SystemNodeCache = new Dictionary<Type, SystemNode>();



        public static Guid? GetGuid(string fullPath)
        {
            XmlDocument doc = new XmlDocument(); // { PreserveWhitespace = true}
            doc.Load(fullPath);

            XmlElement root = doc.DocumentElement;

            if (root.Attributes != null && root.Attributes["Guid"] != null)
                return root.GetAttributeOrDefault("Guid", default(Guid));

            return null;
        }



        private static Graph Load(XmlDocument doc, IProcedureEnvironment loadEnvironment, Dictionary<string, Graph> graphCache)
        {
            //get the "Graph" xml node
            XmlElement root = doc.DocumentElement;

            //create the graph from its guid and load its details
            Graph graph = new Graph(); //));
            graph.Guid = root.GetAttributeOrDefault("Guid", Guid.NewGuid());
            graph.Author = root.GetAttributeOrDefault("Author", string.Empty);
            graph.Description = root.GetAttributeOrDefault("Description", string.Empty);
            graph.Color = root.GetAttributeOrDefault("Color", "ffffff");
            //graph.OutputStyle = root.GetAttributeOrDefault("OutputStyle", OutputStyle.Standard);
            graph.Tags = root.GetAttributeOrDefault("Tags", string.Empty);
            graph.Category = root.GetAttributeOrDefault("Category", string.Empty);

            //loads the global parameters of the graph
            LoadParameters(root, graph, loadEnvironment);

            //loads the attributes of the graph
            //LoadVariables(root, graph, procedureEnvironment);

            //loads the nodes
            Dictionary<string, Port> portIndices = LoadNodes(root, graph, loadEnvironment, graphCache);

            //and then the edges
            LoadEdges(root, graph, loadEnvironment, portIndices);

            //now, replace the obsolete nodes 
            ReplaceObsoleteNodes(graph);

            return graph;
        }



        #region Loading Edges

        private static void LoadEdges(XmlElement root, Graph graph, IProcedureEnvironment procedureEnvironment, Dictionary<string, Port> portIndices)
        {
            XmlNodeList edgeList = root["Edges"].GetElementsByTagName("Edge");
            foreach (XmlElement xmlNode in edgeList)
            {
                Port fromPort = null, toPort = null;

                //if labels are defined, use them for identifications
                var fromLabel = xmlNode.GetAttributeOrDefault<string>("FromLabel");
                var toLabel = xmlNode.GetAttributeOrDefault<string>("ToLabel");
                var fromNodeId = xmlNode.GetAttributeOrDefault<int>("FromNode");
                var toNodeId = xmlNode.GetAttributeOrDefault<int>("ToNode");

                //legacy support
                var fromPortCode = xmlNode.GetAttributeOrDefault<string>("FromPort");
                var toPortCode = xmlNode.GetAttributeOrDefault<string>("ToPort");

                var fromNode = graph.Nodes.Find(x => x.Id == fromNodeId);
                var toNode = graph.Nodes.Find(x => x.Id == toNodeId);


                if (fromNode != null && toNode != null)
                {
                    fromPort = fromNode.OutputPorts.FirstOrDefault(x => x.FullLabel == fromLabel);
                    toPort = toNode.InputPorts.FirstOrDefault(x => x.FullLabel == toLabel);

                    if (fromPort == null)
                        fromPort = fromNode.OutputPorts.FirstOrDefault(x => x.Label == fromLabel);
                    if (toPort == null)
                        toPort = toNode.InputPorts.FirstOrDefault(x => x.Label == toLabel);

                    //try to get from the FromPort and ToPort, which 
                    if (fromPort == null && fromPortCode != null)
                        portIndices.TryGetValue(fromPortCode, out fromPort);

                    if (toPort == null && toPortCode != null)
                        portIndices.TryGetValue(toPortCode, out toPort);
                }


                if (fromPort != null && toPort != null)
                {
                    Edge edge = new Edge(fromPort, toPort);

                    if (xmlNode.Attributes["Disabled"] != null)
                        edge.Enabled = !Convert.ToBoolean(xmlNode.Attributes["Disabled"].InnerText);

                    graph.AddEdge(edge);
                }
            }
        }

        #endregion



        public static Graph LoadFromPath(string fullPath, IProcedureEnvironment loadEnvironment)
        {
            //string xml = loadEnvironment.Resources.LoadText(fullPath);

            //return LoadFromXML(xml, loadEnvironment);
            return LoadFromPath(fullPath, loadEnvironment, new Dictionary<string, Graph>());
        }



        private static Graph LoadFromPath(string fullPath, IProcedureEnvironment loadEnvironment, Dictionary<string, Graph> graphCache)
        {
            string xml = loadEnvironment.GetService<IResourceManager>().LoadText(fullPath);

            return LoadFromXML(xml, loadEnvironment, graphCache);
        }



        public static Graph LoadFromXML(string xml, IProcedureEnvironment procedureEnvironment)
        {
            return LoadFromXML(xml, procedureEnvironment, new Dictionary<string, Graph>());
        }



        private static Graph LoadFromXML(string xml, IProcedureEnvironment procedureEnvironment, Dictionary<string, Graph> graphCache)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return Load(doc, procedureEnvironment, graphCache);
        }



        public static ProcedureAttribute LoadHeader(string path, IProcedureEnvironment loadEnvironment)
        {
            string xml = loadEnvironment.GetService<IResourceManager>().LoadText(path);

            return LoadHeaderFromXML(xml, loadEnvironment);
        }



        public static ProcedureAttribute LoadHeaderFromXML(string xml, IProcedureEnvironment loadEnvironment)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            //get the "Graph" xml node
            XmlElement root = doc.DocumentElement;

            //create the graph from its guid and load its details
            var guid = root.GetAttributeOrDefault("Guid", Guid.NewGuid()).ToString();
            var author = root.GetAttributeOrDefault("Author", string.Empty);
            var description = root.GetAttributeOrDefault("Description", string.Empty);
            var color = root.GetAttributeOrDefault("Color", "ffffff");
            var tags = root.GetAttributeOrDefault("Tags", string.Empty);
            var category = root.GetAttributeOrDefault("Category", string.Empty);
            var obsolete = root.GetAttributeOrDefault("Obsolete", string.Empty);


            return new ProcedureAttribute(guid) {Author = author, Description = description, HexColor = color, Tags = tags, Category = category};
        }



        public static IEnumerable<Type> LoadNodeTypes(string fullPath, IProcedureEnvironment loadEnvironment)
        {
            string xml = loadEnvironment.GetService<IResourceManager>().LoadText(fullPath);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);


            XmlElement root = doc.DocumentElement;
            XmlNodeList nodeList = root["Nodes"].GetElementsByTagName("Node");
            foreach (XmlElement xmlNode in nodeList)
            {
                string nodeTypeGuid = xmlNode.GetAttributeOrDefault<string>("NodeTypeGUID");

                //Type nodeType = Type.GetType(xmlNode.GetAttributeOrDefault<String>("NodeType"));
                string nodeType = xmlNode.GetAttributeOrDefault<string>("NodeType");
                if (nodeType == "SystemNode")
                {
                    Type systemNodeType = SystemProcedureManager.TryGetType(nodeTypeGuid);
                    if (systemNodeType != null)
                        yield return systemNodeType;
                }
            }
        }



        #region Load Graph Parameters

        private static void LoadParameters(XmlElement root, Graph graph, IProcedureEnvironment loadEnvironment)
        {
            //loads the parameters of the graph
            XmlNodeList parameterList = root["Parameters"].GetElementsByTagName("Parameter");
            foreach (XmlElement xmlNode in parameterList)
                try
                {
                    Type parameterType = xmlNode.GetAttributeOrDefault<Type>("Type");
                    ParameterInfo parameterInfo = (ParameterInfo) Activator.CreateInstance(parameterType, xmlNode);


                    graph.ParameterInfos.Add(parameterInfo);
                }
                catch (Exception ex)
                {
                    loadEnvironment.GetService<ILogger>().Log(string.Format("Failed to load Graph Parameter '{0}': {1}", xmlNode.GetAttributeOrDefault<string>("Label"), ex.Message), LogType.Warning);
                }
        }



        /*private static void LoadVariables(XmlElement root, Graph graph, Environment procedureEnvironment)
        {
            XmlNodeList variableList = root["Variables"].GetElementsByTagName("Variable");
            foreach (XmlElement xmlNode in variableList)
            {
                try
                {
                    graph.VariableInfos.Add(new AttributeInfo(xmlNode));
                }
                catch (Exception ex)
                {
                    procedureEnvironment.Log("Failed to load Attribute: " + ex.Message);
                }
            }
        }*/

        #endregion



        private static void ReplaceObsoleteNodes(Graph graph)
        {
            foreach (Node node in graph.Nodes.ToList())
                if (node.IsObsolete)
                    NodeResolverManager.Resolve(graph, node);
        }



        #region Load Nodes

        private static Dictionary<string, Port> LoadNodes(XmlElement root, Graph graph, IProcedureEnvironment procedureEnvironment, Dictionary<string, Graph> graphCache)
        {
            //keep a dictionary of port ids, so that we can connect edges later
            Dictionary<string, Port> portIndices = new Dictionary<string, Port>();

            //loads the nodes
            XmlNodeList nodeList = root["Nodes"].GetElementsByTagName("Node");
            foreach (XmlElement xmlNode in nodeList)
                try
                {
                    string nodeTypeGuid = xmlNode.GetAttributeOrDefault<string>("NodeTypeGUID");
                    Point position = xmlNode.GetAttributeOrDefault<Point>("Position");
                    Node node = null;

                    //Type nodeType = Type.GetType(xmlNode.GetAttributeOrDefault<String>("NodeType"));
                    string nodeType = xmlNode.GetAttributeOrDefault<string>("NodeType");
                    if (nodeType == "SystemNode")
                    {
                        Type systemNodeType = SystemProcedureManager.TryGetType(nodeTypeGuid);
                        if (systemNodeType != null)
                            try
                            {
                                SystemNode systemNode;
                                if (!SystemNodeCache.TryGetValue(systemNodeType, out systemNode))
                                {
                                    SystemProcedure systemProcedure = (SystemProcedure) Activator.CreateInstance(systemNodeType);
                                    systemProcedure.Environment = procedureEnvironment;
                                    systemNode = new SystemNode(systemProcedure, position);
                                    SystemNodeCache[systemNodeType] = systemNode;
                                    //node = systemNode;
                                }

                                //always create a clone, don't allow the original to be changed
                                node = systemNode.DeepClone();
                                node.Position = position;
                            }
                            catch (Exception)
                            {
                                node = new InvalidNode(nodeTypeGuid, position, xmlNode, nodeType);
                            }
                    }
                    else if (nodeType == "ComponentNode")
                    {
                        string relativePath = xmlNode.GetAttributeOrDefault<string>("RelativePath");

                        var resourceManager = procedureEnvironment.GetService<IResourceManager>();
                        if (!resourceManager.Exists(relativePath))
                            relativePath = resourceManager.Lookup(nodeTypeGuid);

                        if (relativePath != null)
                        {
                            Graph subgraph;

                            if (!graphCache.TryGetValue(relativePath, out subgraph))
                            {
                                subgraph = LoadFromPath(relativePath, procedureEnvironment, graphCache);
                                graphCache.Add(relativePath, subgraph);
                            }

                            node = new ComponentNode(relativePath, subgraph, position);
                        }
                    }


                    if (node == null)
                    {
                        node = new InvalidNode(nodeTypeGuid, position, xmlNode, nodeType);
                    }
                    else
                    {
                        try
                        {
                            node.Guid = xmlNode.GetAttributeOrDefault("Guid", Guid.NewGuid());

                            LoadArguments(xmlNode, node.Parameters, procedureEnvironment, node);
                        }
                        catch (Exception)
                        {
                            procedureEnvironment.GetService<ILogger>().Log("Could not load arguments of node '" + node.DefaultLabel + "'.", LogType.Warning);
                            //send an indication to the logger
                        }

                        node.RefreshParameterPorts();
                        //LoadVariableLinks(xmlNode, node.VariableLinks);
                    }

                    node.HasImpulsePort = xmlNode.GetAttributeOrDefault("HasImpulsePort", false);
                    node.UseCache = xmlNode.GetAttributeOrDefault("UseCache", false);
                    node.DisableInSubgraphs = xmlNode.GetAttributeOrDefault("DisableInSubgraphs", false);

                    Port.LoadPorts(xmlNode, "InputPorts", node.InputPorts);
                    Port.LoadPorts(xmlNode, "OutputPorts", node.OutputPorts);

                    node.Label = xmlNode.GetAttributeOrDefault("Label", node.DefaultLabel);

                    LoadPortIndices(node.InputPorts, portIndices, "I", graph.Nodes.Count);
                    LoadPortIndices(node.OutputPorts, portIndices, "O", graph.Nodes.Count);

                    graph.AddNode(node);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }

            return portIndices;
        }



        /*private static void LoadOutputFilter(XmlElement xmlElement, AggregativeComponentNode aggregativeComponentNode)
        {
            string filterLabel = xmlElement.InnerText;

            OutputFilter item = aggregativeComponentNode.PossibleOutputFilters.FirstOrDefault(val => val.Label == filterLabel);
            if (item != null)
            {
                aggregativeComponentNode.OutputFilters.Add(item);
                aggregativeComponentNode.OutputPorts.Add(new OutputPort(item.Label,item.Type,aggregativeComponentNode));
            }
                
        }*/



        private static void LoadPortIndices(List<Port> ports, Dictionary<string, Port> portIndices, string portType, int nodeId)
        {
            for (int index = 0; index < ports.Count; index++)
            {
                string id = nodeId + "#" + portType + "#" + index;
                portIndices.Add(id, ports[index]);
            }
        }



        public static void LoadArguments(XmlNode xmlNode, List<ParameterInfo> arguments, IProcedureEnvironment loadEnvironment, Node node)
        {
            XmlNodeList argumentNodeList = xmlNode["Arguments"].GetElementsByTagName("Argument");
            foreach (XmlElement argumentNode in argumentNodeList)
            {
                string argumentLabel = argumentNode.Attributes["Label"].InnerText;
                try
                {
                    ParameterInfo argument = arguments.FirstOrDefault(val => val.Label == argumentLabel);
                    if (argument != null) argument.ReadArgumentXML(argumentNode, loadEnvironment);
                }
                catch (Exception)
                {
                    loadEnvironment.GetService<ILogger>().Log("Could not load argument '" + argumentLabel + "' of node '" + node.DefaultLabel + "'.", LogType.Warning);
                }
            }
        }

        #endregion
    }
}