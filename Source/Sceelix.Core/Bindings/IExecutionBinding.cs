using System.Collections.Generic;
using System.ComponentModel;
using Sceelix.Core.Graphs.Execution;

namespace Sceelix.Core.Bindings
{
    /// <summary>
    /// Execution Bindings constitute a way to listen and interact with the execution process
    /// at certain graph execution moments.
    /// </summary>
    [DefaultValue(typeof(EmptyBinding))]
    public interface IExecutionBinding
    {
        void AfterDataTransfer(IEnumerable<ExecutionNode> destinationNodes);


        void AfterExecution(ExecutionNode executionNode);


        void AfterRoundExecution(ExecutionNode executionNode);


        void BeforeExecution(ExecutionNode executionNode);


        void BeforeRoundExecution(ExecutionNode executionNode);
        void OnInitialize(ExecutionNode executionNode);
    }
}