using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sceelix.Core.Bindings;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Exceptions;
using Sceelix.Core.Procedures;
using Sceelix.Logging;

namespace Sceelix.Core.Graphs.Execution
{
    public class ExecutionNode
    {
        private readonly Dictionary<Port, IDataChannel> _dataChannels = new Dictionary<Port, IDataChannel>();

        //private readonly Output<Entity> AllOutput;

        //private Dictionary<String, Object> _userData = new Dictionary<String, Object>();



        public ExecutionNode(Node node, GraphProcedure superGraphProcedure, IProcedureEnvironment environment)
        {
            Node = node;
            SuperGraphProcedure = superGraphProcedure;
            Environment = environment;
            IsEnabled = node.IsEnabled;
            //AllOutput = allOutput;

            try
            {
                foreach (var executionBinding in Environment.GetServices<IExecutionBinding>())
                    executionBinding.OnInitialize(this);

                Procedure = node.CreateParameterizedProcedure(Environment, superGraphProcedure);
                Procedure.Parent = superGraphProcedure;
                Procedure.ExecutionNode = this;

                CheckForObsoletion(node);
            }
            catch (GraphException gpex)
            {
                throw new GraphException(Node, gpex);
            }
            catch (Exception ex)
            {
                //ex.Data["Source"] = this;
                //environment.ReportException(this);
                //throw ex;
                throw new GraphException(Node, ex);
            }


            //if (_procedure is IReflective)
            //    ((IReflective)_procedure).SuperProcedure = superGraphProcedure;

            //_procedure.Environment = environment;
            IncomingConnections = 0;
        }



        public bool CanExecute => Procedure.CanExecute;


        public IEnumerable<IDataChannel> DataChannels => _dataChannels.Values;


        public IProcedureEnvironment Environment
        {
            get;
            set;
        }


        public int ExecutionIndex
        {
            get;
            set;
        }


        public int IncomingConnections
        {
            get;
            set;
        }


        public bool IsEnabled
        {
            get;
        }


        /*public Dictionary<string, object> UserData
        {
            get { return _userData; }
            set { _userData = value; }
        }*/


        public Node Node
        {
            get;
        }


        public Procedure Procedure
        {
            get;
        }



        public IEnumerable<DataChannel> PureDataChannels
        {
            get
            {
                foreach (var dataChannel in DataChannels)
                    if (dataChannel is DataChannel)
                        yield return (DataChannel) dataChannel;
                    else if (dataChannel is MultiDataChannel)
                        foreach (DataChannel subChannel in ((MultiDataChannel) dataChannel).DataChannels)
                            yield return subChannel;
            }
        }



        public GraphProcedure SuperGraphProcedure
        {
            get;
        }



        public void AddDataChannel(Port port, IDataChannel nodeDataChannel)
        {
            _dataChannels.Add(port, nodeDataChannel);
        }



        private void CheckForObsoletion(Node node)
        {
            if (node.IsObsolete)
            {
                var obsoleteAttribute = node.ProcedureAttribute.ObsoleteAttribute;

                var message = string.Format("Node '{0}' is obsolete ({1}).", node.DefaultLabel, obsoleteAttribute.Message);

                if (obsoleteAttribute.IsError)
                    throw new InvalidOperationException(message);

                Environment.GetService<ILogger>().Log(message, LogType.Warning);
            }
        }



        /*public void EmulateRemoval()
        {
            foreach (List<IDataChannel> dataChannel in _dataChannels.Values)
                foreach (DataChannel channel in dataChannel)
                {
                    channel.DestinationNode.IncomingConnections--;
                }
        }*/



        public List<ExecutionNode> Execute()
        {
            List<ExecutionNode> connectedExecutionNodes = new List<ExecutionNode>();

            try
            {
                Procedure.Execute();

                foreach (KeyValuePair<Port, IDataChannel> keyValuePair in _dataChannels)
                {
                    IEnumerable<IEntity> entities = Procedure.Outputs[keyValuePair.Key.Index].DequeueAll();
                    keyValuePair.Value.TransferData(entities);

                    //add the new executable nodes to the list, which will be handled later
                    connectedExecutionNodes.AddRange(keyValuePair.Value.ConnectedNodes.Where(val => val.CanExecute));
                }

                foreach (var executionBinding in SuperGraphProcedure.Environment.GetServices<IExecutionBinding>())
                    executionBinding.AfterDataTransfer(DataChannels.SelectMany(x => x.ConnectedNodes));
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (GraphException gpex)
            {
                throw new GraphException(Node, gpex);
            }
            catch (Exception ex)
            {
                //ex.Data["Source"] = this;
                //Procedure.Environment.ReportException(this);
                //throw ex;
                throw new GraphException(Node, ex);
            }

            return connectedExecutionNodes;
        }



        /*public int Level
        {
            get { return _procedure.ExecutionLevel; }
        }*/



        public override string ToString()
        {
            return string.Format("Label: {0}, ExecutionIndex: {1}", Node.Label, ExecutionIndex);
        }
    }
}