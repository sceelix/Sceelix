using System.Collections.Generic;
using Sceelix.Core.Graphs.Execution;

namespace Sceelix.Core.Bindings
{
    public class EmptyBinding : IExecutionBinding
    {
        public void AfterDataTransfer(IEnumerable<ExecutionNode> destinationNodes)
        {
        }



        public void AfterExecution(ExecutionNode executionNode)
        {
        }



        public void AfterRoundExecution(ExecutionNode executionNode)
        {
        }



        public void BeforeExecution(ExecutionNode executionNode)
        {
        }



        public void BeforeRoundExecution(ExecutionNode executionNode)
        {
        }



        public void OnInitialize(ExecutionNode executionNode)
        {
        }
    }
}