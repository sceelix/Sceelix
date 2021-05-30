using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Sceelix.Core.Exceptions;
using Sceelix.Core.Graphs;
using Sceelix.Core.Messages;
using Sceelix.Core.Procedures;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.GUI.Execution
{
    public class GraphExecutionManager
    {
        private readonly GameThreadWorker _workItem;
        private readonly Synchronizer _synchronizer;
        private bool _shouldReexecute;
        private ExecutionOptions _pendingRequestOptions;
        private bool _requestPending = false;

        private MessageManager _messageManager;

        private GraphDocumentControl _graphDocumentControl;
        
        private FileItem _fileItem;
        private GraphControl _graphControl;


        public GraphExecutionManager(FileItem fileItem, IServiceLocator serviceLocator, GraphControl graphControl)
        {
            _graphDocumentControl = graphControl.GraphDocumentControl;
            
            _fileItem = fileItem;
            _graphControl = graphControl;

            _messageManager = serviceLocator.Get<MessageManager>();

            _workItem = new GameThreadWorker("Graph " + fileItem.Name, GraphExecution);
            _workItem.Completed += OnThreadedWorkItemFinished;

            _synchronizer = new Synchronizer();

            _messageManager.Register<GraphContentChanged>(OnGraphChanged, val => val.Item == fileItem);
        }


        public void Update(TimeSpan deltaTime)
        {
            _synchronizer.Update();

            _workItem.Update(deltaTime);

            if (_shouldReexecute)
            {
                ExecuteGraphAsync(null);

                _shouldReexecute = false;
            }
        }


        public void ExecuteGraphAsync(ExecutionOptions options)
        {
            //_totalCounter = System.Diagnostics.Stopwatch.StartNew();
            //Console.WriteLine("Let's start!");

            //if an execution is requested, but the system is busy, cancel the previous instance
            //when the cancellation process is finished, try to start again
            if (_workItem.IsRunning)
            {
                _requestPending = true;
                _pendingRequestOptions = options;
                //_workItem.Cancel();
            }
            else
            {
                _requestPending = false;

                //tell the user we are preparing
                _graphDocumentControl.InformProcessStarted("Preparing...", false);

                //wait for the next update and then start the process
                _synchronizer.Enqueue(() => StartGraphExecution(options));
            }
        }


        private void StartGraphExecution(ExecutionOptions options)
        {
            //warn the interface that we are preparing


            if (options == null)
                options = new ExecutionOptions();


            if (_graphDocumentControl.EditorSettings.ClearLogsOnExecution.Value)
                _messageManager.Publish(new LogMessageClear());

            _messageManager.Publish(new MarkNode(null));
            _messageManager.Publish(new MarkEdge(null));
            _messageManager.Publish(new MarkPort(null));

            var graph = VisualGraph.Graph;
            var name = VisualGraph.FileItem.Name;
            var procedureEnvironment = VisualGraph.Environment;

            if (options.Debug)
            {
                procedureEnvironment.Services.Add(new GraphTrailExecutionBinding(_graphDocumentControl.EditorSettings));
                procedureEnvironment.Services.Add(new BenchmarkExecutionBinding());
            }
                

            if (options.Limit != null)
            {
                graph = VisualGraph.Graph.Clone(procedureEnvironment);
                options.Limit = graph.GetStructurallyEqual(options.Limit);

                ApplyGraphLimits(graph, options);
            }

            try
            {
                procedureEnvironment.Messenger.Send(new ClearVisualHandles());

                //creates the procedure, in this thread to avoid problems with concurrent access to the visualGraph
                IndependentGraphProcedure independentGraphProcedure = new IndependentGraphProcedure(graph, name, procedureEnvironment);

                //warn the interface that we are starting
                _graphDocumentControl.InformProcessStarted("Running...", true);

                //start the process!
                _workItem.Run(independentGraphProcedure, _fileItem);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }



        private void ReportException(Exception exception)
        {
            if (exception is GraphException)
            {
                var graphException = (GraphException)exception;

                var nodes = graphException.ThrowingNodes.ToList();

                _messageManager.Publish(new MarkNode(graphException.ThrowingNode, new Color(128, 0, 0)));
                _messageManager.Publish(new ExceptionThrown(exception, new CompoundMessage(new Object[] { new FrameNodes(nodes), new MarkNodes(nodes, new Color(128, 0, 0)) })));
            }
            else
            {
                _messageManager.Publish(new ExceptionThrown(exception));
            }

            DesignerProgram.Log.Error("Graph Execution Error.", exception);


            _graphDocumentControl.InformProcessStopped();
        }



        private void ApplyGraphLimits(Graph graph, ExecutionOptions options)
        {
            if (options.Limit is Node)
            {
                var limitNode = (Node)options.Limit;

                //delete all the nodes that descend from this one
                DeleteSubNodes(graph, limitNode);
            }
            else if (options.Limit is Port)
            {
                var port = (Port)options.Limit;
                var node = port.Node;

                //delete all the nodes that descend from this one
                DeleteSubNodes(graph, node);

                //block all the free ports
                BlockFreeOutputPorts(graph);

                if (port is InputPort)
                {
                    //the hack here is create a dummy sequence node, clear its ports
                    //and then create the same number of input ports, plus one
                    //which will be blocked (so that the node can't execute)
                    var sequenceProcedure = new SequenceProcedure();

                    sequenceProcedure.Parameters["Ports"].Set(null);
                    for (int i = 0; i < node.InputPorts.Count + 1; i++)
                        sequenceProcedure.Parameters["Ports"].Set(i < node.InputPorts.Count && node.InputPorts[i].Nature == "Single" ? "Single Input" : "Collective Input");

                    //sequenceProcedure.Parameters["Ports"].Set("Output");

                    //this new node will have the same guid as the previous node
                    Node newNode = new SystemNode(sequenceProcedure, new Core.Graphs.Point(0, 0));
                    newNode.Guid = node.Guid;
                    newNode.RefreshParameterPorts();
                    newNode.InputPorts.Last().PortState = PortState.Blocked;
                    graph.AddNode(newNode);

                    //now, create a new node, to which we will just flush the data (so that we can view it)
                    Node outputNode = new SystemNode(new SequenceProcedure(), new Core.Graphs.Point(100, 100));
                    outputNode.RefreshParameterPorts();
                    graph.AddNode(outputNode);

                    //in the middle of the two, we need a sequence node to do the copying of the entities without cloning
                    //otherwise we would could lose tracking information
                    var middleSequenceProcedure = new SequenceProcedure();
                    middleSequenceProcedure.Parameters["Ports"].Set(null);
                    middleSequenceProcedure.Parameters["Copy Method"].Set("Reference");

                    for (int i = 0; i < node.InputPorts.Count; i++)
                    {
                        middleSequenceProcedure.Parameters["Ports"].Set("Collective Input");
                        middleSequenceProcedure.Parameters["Ports"].Set("Output");
                        middleSequenceProcedure.Parameters["Ports"].Set("Output");
                    }
                    var middleSequenceNode = new SystemNode(middleSequenceProcedure, new Core.Graphs.Point(50, 50));
                    middleSequenceNode.RefreshParameterPorts();
                    graph.AddNode(middleSequenceNode);

                    //make the connections 
                    for (int i = 0; i < node.InputPorts.Count; i++)
                    {
                        if (!options.StrictLimit || port.Index == i)
                        {
                            foreach (Edge edge in node.InputPorts[i].Edges)
                            {
                                graph.AddEdge(new Edge(edge.FromPort, middleSequenceNode.InputPorts[i]));
                            }
                            
                            graph.AddEdge(new Edge(middleSequenceNode.OutputPorts[i * 2], outputNode.InputPorts[0]));
                            graph.AddEdge(new Edge(middleSequenceNode.OutputPorts[i * 2 + 1], newNode.InputPorts[i]));
                        }
                    }

                    //we can delete the old node, we don't want it to run
                    node.Delete();

                    //graph.SaveXML(@"C:\Users\pedro\Documents\Sceelix\Extras\0.8.2.0\Samples\Examples\01- City\MyGraph.slxg");
                }
                else if (port is OutputPort)
                {
                    foreach (Port outputPort in node.OutputPorts)
                    {
                        if (!options.StrictLimit || outputPort == port)
                        {
                            outputPort.PortState = PortState.Normal;
                        }
                    }
                }
            }
            /*else
			{
				var edge = (Edge)options.Limit;
				var node = edge.FromPort.Node;

				//delete all the nodes that descend from this one
				DeleteSubNodes(graph, node);
				
				//block all the free ports
				BlockFreeOutputPorts(graph);

				//unlock the port from where this edge goes
				edge.FromPort.PortState = PortState.Normal;

				Node blockedNode = new SystemNode(new SequenceProcedure(), new Sceelix.Core.Graphs.Models.Point(100, 100));
				blockedNode.RefreshParameterPorts();
				blockedNode.Guid = edge.ToPort.Node.Guid;
				blockedNode.InputPorts[0].PortState = PortState.Blocked;
				graph.AddNode(blockedNode);

				//now, create a new node, to which we will just flush the data (so that we can view it)
				Node outputNode = new SystemNode(new SequenceProcedure(), new Sceelix.Core.Graphs.Models.Point(100, 200));
				outputNode.RefreshParameterPorts();
				graph.AddNode(outputNode);

				graph.AddEdge(new Edge(edge.FromPort, blockedNode.InputPorts[0]));
				graph.AddEdge(new Edge(edge.FromPort, outputNode.InputPorts[0]));
			}*/
        }

        public static Object GraphExecution(GameThreadWorkerData data)
        {
            //VisualGraph visualGraph = (VisualGraph)objects[0];
            //Console.WriteLine("Until Step 4: " + _totalCounter.Elapsed.TotalMilliseconds);

            DesignerProgram.Log.Debug("Starting Graph Execution.");

            //creates the procedure
            IndependentGraphProcedure independentGraphProcedure = (IndependentGraphProcedure)data.Args[0]; //new IndependentGraphProcedure(visualGraph.Graph, visualGraph.FileItem.Name, visualGraph.Environment);


            //execute the procedure
            independentGraphProcedure.Execute();

            //extract the fileItem
            FileItem fileItem = (FileItem)data.Args[1];

            //IEnumerable<Entity> popAll = independentGraphProcedure.Outputs.PopAll();
            //independentGraphProcedure.ProcessReportTexts();
            var entities = independentGraphProcedure.Outputs.DequeueAll();
            var unprocessedEntities = independentGraphProcedure.UnprocessedEntities.ToList();
            unprocessedEntities.ForEach(x => x.SetIsUnprocessedEntity(true));

            var entitiesAndUnprocessedOnes = entities.Concat(unprocessedEntities);

            var result = new GraphExecutionFinished(independentGraphProcedure, entitiesAndUnprocessedOnes.ToArray(), fileItem);

            DesignerProgram.Log.Debug("Finished Graph Execution.");

            return result;
        }


        private void OnThreadedWorkItemFinished(object source, WorkerResultData result)
        {
            DesignerProgram.Log.Debug("Reached ThreadedWorkItemFinished.");

            if (result.Cancelled)
            {
                _graphDocumentControl.InformProcessStopped();
            }
            else if (result.Exception != null)
            {
                ReportException(result.Exception);
            }
            else
            {
                _graphDocumentControl.InformProcessStopped("Generation took " + (int)(result.ElapsedTime.TotalMilliseconds) + "ms.");

                _synchronizer.Enqueue(delegate () { _messageManager.Publish((GraphExecutionFinished)result.Data); });
            }

            if (_requestPending)
                ExecuteGraphAsync(_pendingRequestOptions);
        }


        private void OnGraphChanged(GraphContentChanged obj)
        {
            //Messenger.MessageManager.Publish(new FileContentChanged(FileItem));

            if (LiveExecution && obj.WorthExecuting)
                _shouldReexecute = true;
        }


        private void BlockFreeOutputPorts(Graph graph)
        {
            foreach (Node node in graph.Nodes)
            {
                foreach (Port outputPort in node.OutputPorts.Where(x => x.Edges.Count == 0))
                {
                    outputPort.PortState = PortState.Blocked;
                }
            }
        }



        private void DeleteSubNodes(Graph graph, Node limitNode)
        {
            foreach (Node node in graph.Nodes.ToList())
            {
                //nodes that do not descend from
                if (node != limitNode && !node.IsParentOf(limitNode))
                    node.Delete();
            }
        }


        public bool LiveExecution
        {
            get { return _graphDocumentControl.EditorSettings.LiveExecution.Value; }
        }


        public GraphEditorSettings EditorSettings
        {
            get
            {
                return _graphDocumentControl.EditorSettings;
            }
        }

        public GameThreadWorker WorkItem
        {
            get { return _workItem; }
        }


        public VisualGraph VisualGraph
        {
            get { return _graphControl.VisualGraph; }
        }


        public void Abort()
        {

            _workItem.Abort();
            
            _graphDocumentControl.StatusText = "Canceled";
            _graphDocumentControl.InformProcessStopped();
        }


        public void GuiExecuteRequest(ExecutionOptions options = null)
        {
            if (EditorSettings.SaveOnExecution.Value)
            {
                _graphControl.Save();

                _graphDocumentControl.AlertFileSave();
            }

            _graphControl.GraphExecutionManager.ExecuteGraphAsync(options);
        }
    }
}
