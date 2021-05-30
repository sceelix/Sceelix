using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Execution;
using Sceelix.Core.Graphs.Functions;
using Sceelix.Core.Resources;

namespace Sceelix.Core.Procedures
{
    public class IndependentGraphProcedure : GraphProcedure
    {
        public IndependentGraphProcedure(Graph graph, string name, IProcedureEnvironment loadEnvironment)
            : base(graph, name, loadEnvironment)
        {
            //this static variable assignment is terrible, we need to fix it...
            Rand.Reset(loadEnvironment, 1000);
            SRand.SetEnvironment(loadEnvironment);
        }



        protected override bool ShouldDeleteVariables => false;


        public override bool UseCache => false;



        protected override List<ExecutionNode> FilterValidExecutionNodes(Dictionary<Node, ExecutionNode> nodeDictionary)
        {
            //_executionNodes = new List<ExecutionNode>(nodeDictionary.Values.Where(val => val.CanExecute()));
            return new List<ExecutionNode>(nodeDictionary.Values);
        }



        public new static IndependentGraphProcedure FromPath(IProcedureEnvironment loadEnvironment, string projectRelativePath)
        {
            string loadedText = loadEnvironment.GetService<IResourceManager>().LoadText(projectRelativePath);

            return FromXML(loadEnvironment, loadedText, Path.GetFileNameWithoutExtension(projectRelativePath));
        }



        public new static IndependentGraphProcedure FromXML(IProcedureEnvironment loadEnvironment, string xml, string name = "Unnamed")
        {
            Graph graph = GraphLoad.LoadFromXML(xml, loadEnvironment);

            return new IndependentGraphProcedure(graph, name, loadEnvironment);
        }



        protected override void SetupEdgeConnections(Graph graph, Dictionary<Node, ExecutionNode> nodeDictionary)
        {
            //time to check for the connections between nodes
            foreach (KeyValuePair<Node, ExecutionNode> pair in nodeDictionary)
            {
                ExecutionNode originExecutionNode = pair.Value;

                //check what's connected to each of the output ports
                //foreach (Port outputPort in pair.Key.OutputPorts)
                for (int i = 0; i < pair.Key.OutputPorts.Count; i++)
                {
                    Port outputPort = pair.Key.OutputPorts[i];

                    if (outputPort.PortState != PortState.Blocked)
                    {
                        List<Edge> edges = outputPort.Edges.Where(val => val.Enabled).ToList();

                        if (edges.Count == 0) //if there are no edges, send it to the alloutput output
                            originExecutionNode.AddDataChannel(outputPort, new OutputDataChannel(AllOutput));
                        else if (edges.Count == 1) //if it has exactly one, just make the connection
                            originExecutionNode.AddDataChannel(outputPort, new DataChannel(edges[0], nodeDictionary));
                        else //if there are multiple connections, make sure the copies will be performed
                            originExecutionNode.AddDataChannel(outputPort, new MultiDataChannel(edges.ToList(), nodeDictionary));
                    }
                }
            }
        }



        protected override Dictionary<Node, ExecutionNode> SetupExecutionNodes(Graph graph)
        {
            Dictionary<Node, ExecutionNode> nodeDictionary = new Dictionary<Node, ExecutionNode>();


            //create an executionNode for each of the graph nodes
            foreach (Node node in graph.Nodes.Where(x => !x.ProcedureAttribute.IsDummy))
            {
                ExecutionNode executionNode = new ExecutionNode(node, this, Environment);
                nodeDictionary.Add(node, executionNode);
            }

            _outputs.Add(AllOutput);

            return nodeDictionary;
        }
    }
}