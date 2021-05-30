using System.ComponentModel;
using Sceelix.Core.Messages;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.Handles
{
    [DefaultValue(typeof(EmptyVisualHandleManager))]
    public interface IVisualHandleManager
    {
        bool CreateVisualHandles
        {
            get;
        }

        void AddVisualHandle(Parameter parameter, VisualHandle visualHandle);

        void AddVisualHandleToParent(Parameter parameter, VisualHandle visualHandle);
    }


    public class EmptyVisualHandleManager : IVisualHandleManager
    {
        public bool CreateVisualHandles => false;



        public void AddVisualHandle(Parameter parameter, VisualHandle visualHandle)
        {
            //do nothing, if called
        }



        public void AddVisualHandleToParent(Parameter parameter, VisualHandle visualHandle)
        {
            //do nothing, if called
        }
    }


    public class SimpleVisualHandleManager : IVisualHandleManager
    {
        public bool CreateVisualHandles => true;



        public void AddVisualHandle(Parameter parameter, VisualHandle visualHandle)
        {
            visualHandle.FullName = parameter.FullLabel;
            visualHandle.Value = parameter.Get();

            var procedureEnvironment = parameter.Procedure.Environment;
            procedureEnvironment.GetService<IMessenger>().Send(new AddVisualHandle(parameter.Procedure.ExecutionNode.Node, visualHandle));
        }



        public void AddVisualHandleToParent(Parameter parameter, VisualHandle visualHandle)
        {
            var executionNode = parameter.Procedure.ExecutionNode;
            var environment = parameter.Procedure.Environment;

            //this only works if the parent graph is also within a graph
            if (executionNode.SuperGraphProcedure.ExecutionNode != null)
            {
                var node = executionNode.SuperGraphProcedure.ExecutionNode.Node;
                var superParameter = executionNode.SuperGraphProcedure.SubParameters.TryGetByFullName(parameter.FullLabel);
                if (superParameter != null)
                {
                    visualHandle.FullName = parameter.FullLabel;
                    visualHandle.Value = parameter.Get();

                    environment.GetService<IMessenger>().Send(new AddVisualHandle(node, visualHandle));
                }
            }
        }
    }
}