using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sceelix.Core.Bindings;
using Sceelix.Core.Graphs.Execution;

namespace Sceelix.Designer.Graphs.GUI.Execution
{
    public class BenchmarkRecord
    {
        private readonly List<BenchmarkSubRecord> _children = new List<BenchmarkSubRecord>();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        protected double _percentage;
        //private TimeSpan _averageTime = TimeSpan.Zero;


        public BenchmarkRecord()
        {
            //start measuring the time
            _stopwatch.Start();
        }


        public List<BenchmarkSubRecord> Children
        {
            get { return _children; }
        }


        public Stopwatch Stopwatch
        {
            get { return _stopwatch; }
        }


        public double TotalMilliseconds
        {
            get { return _stopwatch.Elapsed.TotalMilliseconds; }
        }


        public double Percentage
        {
            get { return _percentage; }
        }


        public virtual void Process(double parentTime)
        {
            _percentage = (TotalMilliseconds / parentTime);

            foreach (BenchmarkSubRecord executionNodeReport in _children)
            {
                executionNodeReport.Process(TotalMilliseconds);
            }
        }
    }


    public class BenchmarkSubRecord : BenchmarkRecord
    {
        private readonly ExecutionNode _executionNode;


        public BenchmarkSubRecord(ExecutionNode executionNode)
        {
            ExecutedTurns = 0;
            _executionNode = executionNode;
        }


        public int ExecutedTurns
        {
            get;
            set;
        }


        public int ExecutedRounds
        {
            get;
            set;
        }


        /*public override void Process(double parentTime)
        {
            _percentage = TotalMilliseconds / parentTime;

            foreach (BenchmarkSubRecord executionNodeReport in Children)
            {
                executionNodeReport.Process(TotalMilliseconds);
            }
        }*/


        public TimeSpan AverageTime
        {
            get
            {
                if (ExecutedRounds > 0)
                    return TimeSpan.FromMilliseconds(Stopwatch.Elapsed.TotalMilliseconds / ExecutedRounds);

                return TimeSpan.Zero;
            }
        }


        public ExecutionNode ExecutionNode
        {
            get { return _executionNode; }
        }
    }
    

    public class BenchmarkExecutionBinding : IExecutionBinding
    {
        private readonly Stack<BenchmarkSubRecord> _dataStack = new Stack<BenchmarkSubRecord>();
        private readonly Dictionary<ExecutionNode, BenchmarkSubRecord> _executionNodeRecords = new Dictionary<ExecutionNode, BenchmarkSubRecord>();

        private BenchmarkRecord _graphRecord;

        public void OnInitialize(ExecutionNode executionNode)
        {
            
        }


        public void BeforeExecution(ExecutionNode executionNode)
        {
            //if the procedure is the main one
            //start recording
            if (_graphRecord == null)
                _graphRecord = new BenchmarkRecord();

            else if (executionNode != null)
            {
                BenchmarkRecord parent = _dataStack.Count > 0 ? _dataStack.Peek() : _graphRecord;

                //get or create a new node for this data
                BenchmarkSubRecord record;
                if (!_executionNodeRecords.TryGetValue(executionNode, out record))
                {
                    _executionNodeRecords.Add(executionNode, record = new BenchmarkSubRecord(executionNode));

                    parent.Children.Add(record);
                }

                //start or resumes the stopwatch
                record.Stopwatch.Start();

                _dataStack.Push(record);
            }
        }


        public void AfterExecution(ExecutionNode executionNode)
        {
            //if the procedure is the main one
            //end recording
            if (executionNode == null && _dataStack.Count == 0)
            {
                _graphRecord.Stopwatch.Stop();

                _graphRecord.Process(_graphRecord.TotalMilliseconds);
            }
            else if (executionNode != null)
            {
                //get the data out of the stack
                var executionNodeData = _dataStack.Pop();

                executionNodeData.ExecutedTurns++;

                //stop tracking the time
                executionNodeData.Stopwatch.Stop();
            }
        }


        public void BeforeRoundExecution(ExecutionNode executionNode)
        {
            
        }


        public void AfterRoundExecution(ExecutionNode executionNode)
        {
            _dataStack.Peek().ExecutedRounds++;
        }


        public void AfterDataTransfer(IEnumerable<ExecutionNode> destinationNodes)
        {
            
        }

        public BenchmarkRecord GraphBenchmarkRecord
        {
            get { return _graphRecord; }
        }
    }
}
