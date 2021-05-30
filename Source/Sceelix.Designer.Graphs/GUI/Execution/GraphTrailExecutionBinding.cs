using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Sceelix.Core.Attributes;
using Sceelix.Core.Bindings;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Execution;
using Sceelix.Core.IO;
using Sceelix.Core.Messages;
using Sceelix.Core.Procedures;
using Sceelix.Designer.Graphs.Logging;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Logging;

namespace Sceelix.Designer.Graphs.GUI.Execution
{
    public class GraphTrailExecutionBinding : IExecutionBinding
    {
        private static readonly SystemKey _nodeSequenceKey = new SystemKey("Node Sequence");
        private static readonly Color _executedColor = new Color(101, 160, 75);
        private static readonly Color _executingColor = Color.YellowGreen;

        private readonly GraphEditorSettings _editorSettings;
        


        public GraphTrailExecutionBinding(GraphEditorSettings editorSettings)
        {
            _editorSettings = editorSettings;
        }


        public void BeforeRoundExecution(ExecutionNode executionNode)
        {
        }


        public void AfterRoundExecution(ExecutionNode executionNode)
        {
            
        }


        public void AfterDataTransfer(IEnumerable<ExecutionNode> destinationNodes)
        {
            var list = destinationNodes.ToList();

            if (_editorSettings.TrackEntityPaths.Value)
            {
                foreach (ExecutionNode destinationNode in list)
                {
                    if (destinationNode.SuperGraphProcedure is IndependentGraphProcedure)
                    {
                        BuildInputGraphTrail(destinationNode);
                    }
                }
            }

            foreach (ExecutionNode executionNode in list)
            {
                var messenger = executionNode.Procedure.Environment.GetService<IMessenger>();

                var inputs = executionNode.Procedure.Inputs;
                var inputPorts = executionNode.Node.InputPorts;

                for (int i = 0; i < inputs.Count; i++)
                {
                    var count = inputs[i].Count;

                    if (count > 0)
                        messenger.Send(new MarkPort(inputPorts[i], _executedColor, count));
                }
            }
        }


        public void OnInitialize(ExecutionNode executionNode)
        {
            /*executionNode.Procedure.Environment =
                new ProcedureEnvironment(
                    executionNode.Procedure.Environment.Resources,
                    new NodeDesignerLogger(executionNode, executionNode.Procedure.Environment.Logger),
                    executionNode.Procedure.Environment.ExecutionBinding,
                    executionNode.Procedure.Environment.Messenger);*/

            
            var subEnvironment = new ProcedureEnvironment(executionNode.Environment.GetServices<Object>());
            subEnvironment.RemoveServices<ILogger>();
            subEnvironment.AddService(new NodeDesignerLogger(executionNode, executionNode.Environment.GetService<ILogger>()));

            executionNode.Environment = subEnvironment;
        }


        public void BeforeExecution(ExecutionNode executionNode)
        {
            if (executionNode != null)
            {
                //Store the information about the visited nodes and ports
                //if (executionNode.SuperGraphProcedure is IndependentGraphProcedure && _editorSettings.TrackEntityPaths.Value)
                //    BuildInputGraphTrail(executionNode);

                //mark the input ports as they receive data
                /*if (executionNode.SuperGraphProcedure is IndependentGraphProcedure)
                    foreach (Port ingoingPort in executionNode.Node.InputPorts.Where(x => x.PortState != PortState.Blocked))
                        executionNode.Procedure.Environment.Send(new MarkPort(ingoingPort, _executedColor));*/

                //mark the node as being executed
                if (executionNode.SuperGraphProcedure is IndependentGraphProcedure)
                    executionNode.Procedure.Environment.GetService<IMessenger>().Send(new MarkNode(executionNode.Node, _executingColor));
            }
        }


        public void AfterExecution(ExecutionNode executionNode)
        {
            //if the procedure is the main one
            //end recording
            if (executionNode != null)
            {
                if (executionNode.SuperGraphProcedure is IndependentGraphProcedure)
                {
                    //Store the information about the visited nodes and ports
                    if (_editorSettings.TrackEntityPaths.Value)
                        BuildOutputGraphTrail(executionNode);

                    var node = executionNode.Node;
                    var procedure = executionNode.Procedure;
                    var messenger = executionNode.Procedure.Environment.GetService<IMessenger>();

                    //match the output ports to outputs
                    //Dictionary<Port, InputReference> inputPortData = node.InputPorts.ToDictionary(x => x, y => procedure.Inputs[y.Index]);
                    Dictionary<Port, OutputReference> outputPortData = node.OutputPorts.ToDictionary(x => x, y => procedure.Outputs[y.Index]);

                    //mark the node as having BEEN executed
                    messenger.Send(new MarkNode(node, _executedColor));

                    //mark the output ports as they get data
                    foreach (Port outgoingPort in executionNode.Node.OutputPorts.Where(x => x.PortState != PortState.Blocked && !outputPortData[x].IsEmpty))
                        messenger.Send(new MarkPort(outgoingPort, _executedColor, outputPortData[outgoingPort].Count));

                    //mark the edges as they pass data to the next nodes
                    foreach (Edge outgoingEdge in executionNode.Node.OutgoingEdges.Where(x => x.Enabled && !outputPortData[x.FromPort].IsEmpty))
                        messenger.Send(new MarkEdge(outgoingEdge, _executedColor));

                    //mark the target input ports as they pass data to the next nodes
                    foreach (Edge outgoingEdge in executionNode.Node.OutgoingEdges.Where(x => x.ToPort.PortState != PortState.Blocked && x.Enabled && !outputPortData[x.FromPort].IsEmpty))
                        messenger.Send(new MarkPort(outgoingEdge.ToPort, _executedColor, 0));
                }
            }
        }


        internal void BuildInputGraphTrail(ExecutionNode executionNode)
        {
            var inputs = executionNode.Procedure.Inputs;
            var inputPorts = executionNode.Node.InputPorts;

            for (int i = 0; i < inputs.Count; i++)
            {
                var data = inputs[i].PeekAll();
                foreach (IEntity entity in data)
                {
                    AddToGraphTrail(inputPorts[i], entity);
                }
            }
        }


        internal void BuildOutputGraphTrail(ExecutionNode executionNode)
        {
            var outputs = executionNode.Procedure.Outputs;
            var outputPorts = executionNode.Node.OutputPorts;

            for (int i = 0; i < outputs.Count; i++)
            {
                var data = outputs[i].PeekAll();
                foreach (IEntity entity in data)
                {
                    AddToGraphTrail(executionNode.Node, entity);
                    AddToGraphTrail(outputPorts[i], entity);
                }
            }
        }


        private void AddToGraphTrail(Object obj, IEntity entity)
        {
            var path = entity.Attributes.TryGet(_nodeSequenceKey);
            if (path != null)
            {
                GraphTrail sequence = (GraphTrail) path;
                sequence.Objects.Add(obj);
            }
            else
            {
                entity.Attributes.TrySet(_nodeSequenceKey, new GraphTrail(obj), true);
            }

            foreach (IEntity subEntity in entity.SubEntityTree)
            {
                AddToGraphTrail(obj, subEntity);
            }
        }
    }
}