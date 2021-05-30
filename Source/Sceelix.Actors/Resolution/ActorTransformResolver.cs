using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resolution;

namespace Sceelix.Actors.Resolution
{
    [NodeResolver("80c671f8-e12b-4a98-b13d-2c7e5b186f31")]
    internal class ActorTransformResolver : NodeResolver
    {
        public override void Resolve(Graph graph, Node node)
        {
            var oldOperationSelector = (SelectListParameterInfo) node.Parameters[0];

            //if this is an expression, we cannot know for sure, so give up
            if (oldOperationSelector.IsExpression)
                return;

            Node newNode = null;
            CompoundParameterInfo newCompoundParameter = null;

            if (oldOperationSelector.SelectedItem.Label == "Translation")
            {
                newNode = graph.AddNode(new SystemNode(SystemProcedureManager.FromGuid("930eb356-619d-4db7-b68a-016d9a6d1e97"), node.Position));

                //select the first argument and set it to reset
                var operationArgument = (SelectListParameterInfo) newNode.Parameters[0];
                newCompoundParameter = (CompoundParameterInfo) operationArgument.Select("Translate");
            }
            else if (oldOperationSelector.SelectedItem.Label == "Reset Translation")
            {
                newNode = graph.AddNode(new SystemNode(SystemProcedureManager.FromGuid("930eb356-619d-4db7-b68a-016d9a6d1e97"), node.Position));

                //select the first argument and set it to reset
                var operationArgument = (SelectListParameterInfo) newNode.Parameters[0];
                newCompoundParameter = (CompoundParameterInfo) operationArgument.Select("Reset");
            }
            else if (oldOperationSelector.SelectedItem.Label == "Rotate")
            {
                newNode = graph.AddNode(new SystemNode(SystemProcedureManager.FromGuid("8906cb79-297a-40a5-8e36-b5f12ddbb847"), node.Position));

                //select the first argument and set it to reset
                var operationArgument = (SelectListParameterInfo) newNode.Parameters[0];
                newCompoundParameter = (CompoundParameterInfo) operationArgument.Select("Rotate");
            }
            else if (oldOperationSelector.SelectedItem.Label == "Orient")
            {
                newNode = graph.AddNode(new SystemNode(SystemProcedureManager.FromGuid("8906cb79-297a-40a5-8e36-b5f12ddbb847"), node.Position));

                //select the first argument and set it to reset
                var operationArgument = (SelectListParameterInfo) newNode.Parameters[0];
                newCompoundParameter = (CompoundParameterInfo) operationArgument.Select("Orient");
            }

            if (newNode != null && newCompoundParameter != null)
            {
                newCompoundParameter.Fields.Clear();

                foreach (ParameterInfo field in ((CompoundParameterInfo) oldOperationSelector.SelectedItem).Fields)
                {
                    var subParameter = (ParameterInfo) field.Clone();
                    subParameter.Parent = field;
                    newCompoundParameter.Fields.Add(subParameter);
                }


                MoveEdgesAndCopyInfo(node.InputPorts[0], newNode.InputPorts[0]);
                MoveEdgesAndCopyInfo(node.OutputPorts[0], newNode.OutputPorts[0]);

                //node.Ports
                graph.Nodes.Remove(node);
            }


            //the scale node is different
            if (oldOperationSelector.SelectedItem.Label == "Scale")
            {
                newNode = graph.AddNode(new SystemNode(SystemProcedureManager.FromGuid("bda91758-2aea-4aad-a607-cc4e76adbeae"), node.Position));

                newNode.Parameters.Clear();

                foreach (ParameterInfo field in ((CompoundParameterInfo) oldOperationSelector.SelectedItem).Fields)
                {
                    var subParameter = (ParameterInfo) field.Clone();
                    subParameter.Parent = null;
                    newNode.Parameters.Add(subParameter);
                }

                MoveEdgesAndCopyInfo(node.InputPorts[0], newNode.InputPorts[0]);
                MoveEdgesAndCopyInfo(node.OutputPorts[0], newNode.OutputPorts[0]);

                //node.Ports
                graph.Nodes.Remove(node);
            }
        }
    }
}