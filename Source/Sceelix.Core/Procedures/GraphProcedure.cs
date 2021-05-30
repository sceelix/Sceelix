using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Execution;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Resources;
using Sceelix.Extensions;

namespace Sceelix.Core.Procedures
{
    public class GraphProcedure : Procedure
    {
        protected readonly Output<IEntity> AllOutput = new Output<IEntity>("All");

        protected readonly Dictionary<Input, List<DataChannel>> InputMappings = new Dictionary<Input, List<DataChannel>>();
        private List<ExecutionNode> _executionNodes;



        public GraphProcedure(Graph graph, string name, IProcedureEnvironment environment)
        {
            base.Environment = environment;

            ProcedureAttribute = graph.ProcedureReference;
            ProcedureAttribute.Label = name;
            ProcedureAttribute.Guid = graph.Guid.ToString();

            Graph = graph;

            Build(graph);
        }



        internal IEnumerable<IEntity> CurrentInternalObjects
        {
            get
            {
                return _executionNodes.SelectMany(node => node.Procedure._inputs.SelectMany(input => input.CurrentObjects)
                    .Union(node.Procedure._outputs.SelectMany(output => output.CurrentObjects))).Union(Inputs.SelectMany(input => input.Input.CurrentObjects).Union(_outputs.SelectMany(output => output.CurrentObjects)));
            }
        }



        /// <summary>
        /// Environment that defines resource location and other environment-specific settings.
        /// Should be set after the procedure is instantiated. Will set the value for all child
        /// procedures.
        /// </summary>
        public override IProcedureEnvironment Environment
        {
            get { return base.Environment; }
            set
            {
                base.Environment = value;

                foreach (ExecutionNode executionNode in _executionNodes)
                    executionNode.Procedure.Environment = value;
            }
        }



        public ReadOnlyCollection<ExecutionNode> ExecutionNodes => new ReadOnlyCollection<ExecutionNode>(_executionNodes);


        public Graph Graph
        {
            get;
        }



        public string Name
        {
            get { return ProcedureAttribute.Label; }
            set { ProcedureAttribute.Label = value; }
        }



        internal ProcedureAttribute ProcedureAttribute
        {
            get;
        }



        public override IEnumerable<IEntity> UnprocessedEntities
        {
            get
            {
                foreach (var executionNode in _executionNodes)
                {
                    foreach (var procedureUnprocessedEntity in executionNode.Procedure.UnprocessedEntities)
                        yield return procedureUnprocessedEntity;
                }

                foreach (var unprocessedEntity in _unprocessedEntities)
                {
                    yield return unprocessedEntity;
                }
            }
        }



        protected void Build(Graph graph)
        {
            SetupParameters(graph);

            Dictionary<Node, ExecutionNode> nodeDictionary = SetupExecutionNodes(graph);

            SetupEdgeConnections(graph, nodeDictionary);

            _executionNodes = FilterValidExecutionNodes(nodeDictionary);

            DetermineExecutionIndices();
        }



        private void DetermineExecutionIndices()
        {
            //first, find the cyclic edges
            foreach (ExecutionNode executionNode in _executionNodes)
                FindNodeCycles(executionNode, new List<ExecutionNode> {executionNode});

            //good, now find the order among nodes
            int index = 0;
            LinkedList<ExecutionNode> queue = new LinkedList<ExecutionNode>(_executionNodes.Where(val => val.IncomingConnections == 0 && val.IsEnabled));
            while (queue.Count > 0)
            {
                ExecutionNode executionNode = queue.Dequeue();
                executionNode.ExecutionIndex = index++;

                foreach (var dataChannel in executionNode.PureDataChannels.Where(val => !val.IsCyclic).Reverse())
                {
                    //First and only node...
                    ExecutionNode connectedNode = dataChannel.ConnectedNodes.First();

                    connectedNode.IncomingConnections--;
                    if (connectedNode.IncomingConnections == 0)
                        queue.AddFirst(connectedNode);
                }
            }

            //foreach (ExecutionNode executionNode in _executionNodes)
            //    executionNode.Node.Label += " [" + executionNode.ExecutionIndex + "]";
        }



        protected virtual List<ExecutionNode> FilterValidExecutionNodes(Dictionary<Node, ExecutionNode> nodeDictionary)
        {
            return new List<ExecutionNode>(nodeDictionary.Values);
        }



        private void FindNodeCycles(ExecutionNode executionNode, List<ExecutionNode> previousNodes)
        {
            foreach (var dataChannel in executionNode.DataChannels)
            {
                if (dataChannel is DataChannel)
                    FindNodeCyclesAux((DataChannel) dataChannel, previousNodes);
                else if (dataChannel is MultiDataChannel)
                {
                    foreach (DataChannel subChannel in ((MultiDataChannel) dataChannel).DataChannels)
                        FindNodeCyclesAux(subChannel, previousNodes);
                }
            }
        }



        private void FindNodeCyclesAux(DataChannel dataChannel, List<ExecutionNode> previousNodes)
        {
            if (!dataChannel.IsCyclic && !dataChannel.IsVisited)
            {
                dataChannel.IsVisited = true;

                ExecutionNode nextNode = dataChannel.ConnectedNodes.First();
                if (previousNodes.Contains(nextNode))
                {
                    dataChannel.IsCyclic = true;
                    nextNode.IncomingConnections--;
                }
                else
                    FindNodeCycles(nextNode, new List<ExecutionNode>(previousNodes) {nextNode});
            }
        }



        public static GraphProcedure FromPath(IProcedureEnvironment procedureEnvironment, string path)
        {
            string loadedText = procedureEnvironment.GetService<IResourceManager>().LoadText(path);

            return FromXML(procedureEnvironment, loadedText, Path.GetFileNameWithoutExtension(path));
        }



        public static GraphProcedure FromXML(IProcedureEnvironment procedureEnvironment, string xml, string name = "Unnamed")
        {
            Graph graph = GraphLoad.LoadFromXML(xml, procedureEnvironment);

            return new GraphProcedure(graph, name, procedureEnvironment);
        }



        protected override void Run()
        {
            TransferGateInputData();

            //add the executable nodes, ordered by their execution index
            LinkedList<ExecutionNode> specialNodes = new LinkedList<ExecutionNode>(_executionNodes.Where(val => val.CanExecute).OrderBy(val => val.ExecutionIndex));

            while (!specialNodes.IsEmpty())
            {
                ExecutionNode executionNode = specialNodes.Dequeue();

                //Evaluate aspect references and execute the procedure
                //For every output port, move it to the destination port of the edge
                //and get the nodes that may not be executed
                List<ExecutionNode> newExecutableNodes = executionNode.Execute();

                foreach (ExecutionNode newExecutableNode in newExecutableNodes)
                {
                    bool inserted = false;

                    for (LinkedListNode<ExecutionNode> iterator = specialNodes.First; iterator != null; iterator = iterator.Next)
                    {
                        if (iterator.Value == newExecutableNode)
                        {
                            inserted = true;
                            break;
                        }

                        if (iterator.Value.ExecutionIndex > newExecutableNode.ExecutionIndex)
                        {
                            iterator.List.AddBefore(iterator, newExecutableNode);
                            inserted = true;
                            break;
                        }
                    }

                    if (!inserted)
                        specialNodes.AddLast(newExecutableNode);
                }
            }

            //check for nodes that may have data in their input ports
            _executionNodes.ForEach(x => x.Procedure.CheckForInputMismatch());
        }



        protected virtual void SetupEdgeConnections(Graph graph, Dictionary<Node, ExecutionNode> nodeDictionary)
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

                    if (outputPort.PortState == PortState.Normal)
                    {
                        //get the list of edges which do not end in an input gate
                        List<Edge> edges = outputPort.Edges.Where(val => val.ToPort.PortState != PortState.Gate && val.Enabled).ToList();

                        if (edges.Count == 0) //if there are no edges, send it to the alloutput output
                            originExecutionNode.AddDataChannel(outputPort, new OutputDataChannel(AllOutput));
                        else if (edges.Count == 1) //if it has exactly one, just make the connection
                            originExecutionNode.AddDataChannel(outputPort, new DataChannel(edges[0], nodeDictionary));
                        else //if there are multiple connections, make sure the copies will be performed
                            originExecutionNode.AddDataChannel(outputPort, new MultiDataChannel(edges, nodeDictionary));
                    }
                }
            }
        }



        protected virtual Dictionary<Node, ExecutionNode> SetupExecutionNodes(Graph graph)
        {
            Dictionary<Node, ExecutionNode> nodeDictionary = new Dictionary<Node, ExecutionNode>();

            //create an executionNode for each of the graph nodes
            var executableNodes = graph.Nodes.Where(x => !x.ProcedureAttribute.IsDummy && !x.ActuallyDisabledInSubgraph).ToList();
            foreach (Node node in executableNodes)
            {
                ExecutionNode executionNode = new ExecutionNode(node, this, base.Environment);
                nodeDictionary.Add(node, executionNode);

                SetupInputPorts(node, executionNode);

                foreach (OutputPort port in node.OutputPorts.OfType<OutputPort>().Where(val => val.PortState == PortState.Gate))
                {
                    //this does not consider equal names if the 
                    Output gateOutput = _outputs.FirstOrDefault(val => val.Label == port.GateLabel);
                    if (gateOutput == null)
                    {
                        gateOutput = port.GenerateOutput();
                        _outputs.Add(gateOutput);
                    }

                    executionNode.AddDataChannel(port, new OutputDataChannel(gateOutput));
                }
            }

            //if there aren't any executable nodes, but 
            if (!nodeDictionary.Any() && graph.Nodes.Where(x => x.ActuallyDisabledInSubgraph).SelectMany(x => x.OutputPorts.Concat(x.InputPorts)).Any(x => x.PortState == PortState.Gate))
                throw new InvalidOperationException("Graph '" + Name + "' has defined gates, but no executable nodes. Did you forget the 'Disable in Subgraphs' option enabled in your source nodes?");

            return nodeDictionary;
        }



        /*private Type FindCommonEntityType(IEnumerable<Type> types)
        {
            Type commonType = null;

            foreach (Type type in types)
            {
                if (commonType == null)
                    commonType = type;
                else
                {
                    if(commonType)

                    commonType = type;
                }
            }

            return commonType; //typeof(Entity);
        }*/



        protected void SetupInputPorts(Node node, ExecutionNode executionNode)
        {
            foreach (InputPort port in node.InputPorts.OfType<InputPort>().Where(val => val.PortState == PortState.Gate))
            {
                Input gateInput = _inputs.FirstOrDefault(val => val.Label == port.GateLabel);
                if (gateInput == null)
                {
                    //create a new input, otherwise we would run into scoping issues
                    gateInput = port.ToGateInput();
                    _inputs.Add(gateInput);
                }

                //create a data channel to forward the data later
                DataChannel dataChannel = new DataChannel(port, executionNode);

                //do not count this as an incomming connection
                executionNode.IncomingConnections--;

                List<DataChannel> dataChannels;
                if (!InputMappings.TryGetValue(gateInput, out dataChannels))
                    InputMappings.Add(gateInput, dataChannels = new List<DataChannel>());

                dataChannels.Add(dataChannel);
            }
        }



        protected void SetupParameters(Graph graph)
        {
            //now, first, set the parameters
            foreach (ParameterInfo parameterInfo in graph.ParameterInfos)
            {
                var parameter = parameterInfo.ToParameter();
                parameter.Parent = this;
                _parameters.Add(parameter);
            }


            //and the attributes!
            /*foreach (AttributeInfo variableInfo in graph.VariableInfos)
                Attributes.Add(new Attribute(variableInfo));*/

            //look for gates and create inputs and outputs from there
            /*foreach (InputPort inputPort in graph.Nodes.SelectMany(val => val.InputPorts).Where(val => val.IsGate))
                InputList.Add(inputPort.GateLabel, inputPort.ToGateInput());

            foreach (OutputPort outputPort in graph.Nodes.SelectMany(val => val.OutputPorts).Where(val => val.IsGate))
                OutputList.Add(outputPort.GateLabel, outputPort.GenerateOutput());*/
        }



        protected virtual void TransferGateInputData()
        {
            foreach (var inputMapping in InputMappings)
            {
                if (inputMapping.Key is CollectiveInput)
                {
                    List<IEntity> entities = ((IEnumerable<IEntity>) inputMapping.Key.Read()).ToList();

                    for (int index = 0; index < inputMapping.Value.Count; index++)
                    {
                        //creates a copy
                        if (index > 0)
                            entities = entities.Select(val => val.DeepClone()).ToList();

                        inputMapping.Value[index].TransferData(entities);
                    }
                }
                else
                {
                    IEntity entity = (IEntity) inputMapping.Key.Read();

                    for (int index = 0; index < inputMapping.Value.Count; index++)
                    {
                        //creates a copy
                        if (index > 0)
                            entity = entity.DeepClone();

                        inputMapping.Value[index].TransferData(entity);
                    }
                }
            }
        }
    }
}