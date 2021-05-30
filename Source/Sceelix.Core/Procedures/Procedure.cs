using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sceelix.Core.Attributes;
using Sceelix.Core.Bindings;
using Sceelix.Core.Caching;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs.Execution;
using Sceelix.Core.IO;
using Sceelix.Core.Messages;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Resources;
using Sceelix.Core.Utils;
using Sceelix.Logging;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Base class for all Sceelix procedures/node definitions.
    /// </summary>
    public abstract class Procedure
    {
        internal readonly List<Input> _inputs = new List<Input>();
        internal readonly List<Output> _outputs = new List<Output>();

        //using lists is essential to keep track of the order
        //besides, the number of ports and and aspects will be limited
        internal readonly List<Parameter> _parameters = new List<Parameter>();

        private bool _isFirstExecution = true;
        private bool _isImpulsedSource;


        private IProcedureEnvironment _environment = new ProcedureEnvironment();

        protected List<IEntity> _unprocessedEntities = new List<IEntity>();


        protected ICacheManager CacheManager => _environment.GetService<ICacheManager>();


        /// <summary>
        /// Indicates if a procedure has enough input data to be executed.
        /// Source procedures (that have no inputs) always return true.
        /// </summary>
        /// <returns></returns>
        internal bool CanExecute => !HasInputs || HasInputDataReady;



        /// <summary>
        /// Environment that defines resource location and other environment-specific settings.
        /// Should be set after the procedure is instantiated.
        /// </summary>
        public virtual IProcedureEnvironment Environment
        {
            get { return _environment; }
            set { _environment = value; }
        }



        /// <summary>
        /// Special execution options/behaviors will be indicated here. Can be null.
        /// </summary>
        protected IExecutionBinding ExecutionBinding => _environment.GetService<IExecutionBinding>();


        /// <summary>
        /// When instantiated from a graph procedure, this field will be set.
        /// </summary>
        protected internal ExecutionNode ExecutionNode
        {
            get;
            set;
        }



        internal bool HasAnyInputDataLeft
        {
            get { return Inputs.All(input => input.Input.HasDataReady()); } // || input is CollectiveInput
        }



        internal bool HasInputDataReady
        {
            get { return Inputs.All(input => input.Input.HasDataReady() || input.Input.IsOptional); } // || input is CollectiveInput
        }



        internal bool HasInputs => Inputs.Count > 0;


        internal bool HasLocalAttributes
        {
            get;
            set;
        }


        /// <summary>
        /// Read-only collection of inputs of this procedure.
        /// </summary>
        public InputReferenceCollection Inputs => new InputReferenceCollection(_inputs);



        /// <summary>
        /// Indicates if a node is collective, i.e. if it contains at least one collective input.
        /// </summary>
        protected internal bool IsCollective
        {
            get { return _inputs.Any(val => val is CollectiveInput); }
        }



        /// <summary>
        /// Gets a value indicating whether this procedure is optional. Optional nodes
        /// are those that have inputs, but ALL are optional.
        /// </summary>
        public bool IsOptional
        {
            get { return HasInputs && _inputs.All(val => val.IsOptional); }
        }



        protected ILogger Logger => _environment.GetService<ILogger>();


        protected IMessenger Messenger => _environment.GetService<IMessenger>();


        /// <summary>
        /// Read-only collection of outputs of this procedure.
        /// </summary>
        public OutputReferenceCollection Outputs => new OutputReferenceCollection(_outputs);



        /// <summary>
        /// Read-only collection of parameters of this procedure.
        /// </summary>
        public ParameterReferenceCollection Parameters
        {
            get { return new ParameterReferenceCollection(_parameters.Where(x => x.IsPublic)); }
        }



        /// <summary>
        /// Procedure where this procedure is being called from.
        /// Graph procedures, for instance, set this property before start.
        /// </summary>
        public Procedure Parent
        {
            get;
            internal set;
        }


        /*
        protected bool HasEntityPool
        {
            get { return ExecutionNode != null; }
        }*/


        protected IResourceManager Resources => _environment.GetService<IResourceManager>();


        /// <summary>
        /// Indicates if the attributes of the manipulated objects should be deleted when the datablock finishes its execution.
        /// </summary>
        protected virtual bool ShouldDeleteVariables => true;


        /// <summary>
        /// Read-only collection of inputs of this procedure.
        /// </summary>
        protected internal InputCollection SubInputs => new InputCollection(_inputs);


        /// <summary>
        /// Read-only collection of outputs of this procedure.
        /// </summary>
        protected internal OutputCollection SubOutputs => new OutputCollection(_outputs);


        /// <summary>
        /// Read-only collection of parameters of this procedure.
        /// </summary>
        protected internal ParameterCollection SubParameters => new ParameterCollection(_parameters);


        public virtual IEnumerable<IEntity> UnprocessedEntities => _unprocessedEntities;


        public virtual bool UseCache
        {
            get;
            set;
        }



        /// <summary>
        /// Overridable function called after all execution rounds are performed.
        /// </summary>
        protected virtual void AfterExecutionCycle()
        {
        }



        /// <summary>
        /// Overridable function called before the first set of execution rounds is performed.
        /// Useful to perform a certain set of operations only once, by guaranteeing that the parameters/attributes/augmentations/inputs/outputs have been set.
        /// </summary>
        protected virtual void BeforeFirstExecutionCycle()
        {
        }



        /// <summary>
        /// Checks if the inputs still have data after execution.
        /// If there is, then there is probably a matching problem.
        /// </summary>
        internal void CheckForInputMismatch()
        {
            var indexList = Inputs.IndicesWithDataLeft().ToList();

            if (indexList.Count > 0)
            {
                string message = "Possible entity quantity mismatch. ";

                //make the message plural
                if (indexList.Count > 1)
                    message += "Ports " + string.Join(",", indexList.Select(i => i + "(" + Inputs[i].Label + ")")) + " still have unprocessed data.";
                else
                    message += "Port " + indexList[0] + "(" + Inputs[indexList[0]].Label + ")" + " still has unprocessed data.";

                Logger.Log(message, LogType.Warning);

                foreach (var input in _inputs)
                    _unprocessedEntities.AddRange(input.CurrentObjects);

                foreach (var inputReference in Inputs)
                    inputReference.Input.CurrentObjects.Clear();
            }
        }



        /*internal protected void AddVisualHandle(String parameterName, VisualHandle visualHandle)
        {
            var parameter = this.SubParameters.TryGetByFullName(parameterName);
            if (parameter != null)
            {
                visualHandle.FullName = parameter.FullLabel;
                visualHandle.Value = parameter.Get();

                _environment.Messenger.Send(new AddVisualGuide(ExecutionNode.Node, visualHandle));
            }
        }*/


        /*internal protected void AddVisualHandleToParent(String parameterName, VisualHandle visualHandle)
        {
            //this only works if the parent graph is also within a graph
            if (ExecutionNode.SuperGraphProcedure.ExecutionNode != null)
            {
                var node = ExecutionNode.SuperGraphProcedure.ExecutionNode.Node;
                var parameter = ExecutionNode.SuperGraphProcedure.SubParameters.TryGetByFullName(parameterName);
                if (parameter != null)
                {
                    visualHandle.FullName = parameter.FullLabel;
                    visualHandle.Value = parameter.Get();

                    _environment.Messenger.Send(new AddVisualGuide(node, visualHandle));
                }
            }
        }*/



        internal void DeleteAttributeData()
        {
            if (ShouldDeleteVariables && HasLocalAttributes)
            {
                /*foreach (var localEntity in _localEntities)
                    localEntity.DeleteProcedureAttributes(this);

                _localEntities.Clear();*/

                DeleteAttributeData(Outputs.PeekAll());
            }
        }



        private void DeleteAttributeData(IEnumerable<IEntity> entities)
        {
            //String prefix = "Local-" + GetHashCode() + ":";
            foreach (IEntity entity in entities)
            {
                foreach (var localAttributeKey in entity.Attributes.Keys.OfType<LocalAttributeKey>().Where(x => x.Procedure == this).ToList())
                    entity.Attributes.Remove(localAttributeKey);

                foreach (IEntity subEntity in entity.SubEntityTree)
                {
                    foreach (var localAttributeKey in subEntity.Attributes.Keys.OfType<LocalAttributeKey>().Where(x => x.Procedure == this).ToList())
                        subEntity.Attributes.Remove(localAttributeKey);
                    //subEntity.Attributes.DeleteAttributesWithPrefix(prefix);
                }
            }

            HasLocalAttributes = false;
        }



        internal virtual void EvaluateParameters(InputData inputData)
        {
            //evaluate all parameters again
            foreach (Parameter parameter in Parameters.Select(x => x.Parameter))
                parameter.Evaluate(inputData);
        }



        /// <summary>
        /// Executes the procedure, considering all the set inputs and parameters.
        /// </summary>
        public void Execute()
        {
            foreach (var executionBinding in Environment.GetServices<IExecutionBinding>())
                executionBinding.BeforeExecution(ExecutionNode);

            //update the ports, just in case the function wasn't called before
            UpdateParameterPorts();

            //clear all the outputs
            foreach (var output in _outputs)
                output.Clear();

            //if this is a source procedure
            string cacheKey;
            if (PrepareCache(out cacheKey))
            {
                foreach (var executionBinding in Environment.GetServices<IExecutionBinding>())
                    executionBinding.AfterExecution(ExecutionNode);

                return;
            }


            if (_isFirstExecution)
            {
                BeforeFirstExecutionCycle();
                _isFirstExecution = false;
            }


            //if it is a generator node, execute this only once
            if (!HasInputs || IsOptional) //!HasInputs || (!HasInputDataReady && IsIndependent)  //CanExecute
            {
                ExecuteRound();
            }
            else
            {
                //otherwise, execute various rounds just to 
                while (HasInputDataReady)
                {
                    ExecuteRound();
                }
            }

            DeleteAttributeData();


            //
            AfterExecutionCycle();

            //
            StoreCache(cacheKey);

            foreach (var executionBinding in Environment.GetServices<IExecutionBinding>())
                executionBinding.AfterExecution(ExecutionNode);
        }



        private void ExecuteRound()
        {
            if (ExecutionNode != null)
                foreach (var executionBinding in Environment.GetServices<IExecutionBinding>())
                    executionBinding.BeforeRoundExecution(ExecutionNode);

            //put the input data in a working queue
            foreach (var input in _inputs)
                input.PrepareRoundData();


            InputData inputData = new InputData(SubInputs);


            //executes the actual procedure
            try
            {
                //Determines the actual values of the parameters by evaluating possible expressions
                EvaluateParameters(inputData);

                Run();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

                foreach (var input in _inputs)
                    _unprocessedEntities.AddRange(input.RoundEntities);
            }

            //now, get the entities that were outputted during the procedure execution
            List<IEntity> roundEntities = Outputs.SelectMany(val => val.Output.DequeueRoundEntitites()).ToList();

            //make sure that Attribute references are passed on
            ForwardImpulseData(roundEntities);

            if (ExecutionNode != null)
                foreach (var executionBinding in Environment.GetServices<IExecutionBinding>())
                    executionBinding.AfterRoundExecution(ExecutionNode);

            //release the input data
            foreach (var input in _inputs)
                input.ClearRoundData();
        }



        private void ForwardImpulseData(IEnumerable<IEntity> roundEntities)
        {
            if (Inputs.Count == 1 && _isImpulsedSource)
            {
                IEntity entity = (IEntity) Inputs[0].Input.Read();

                foreach (IEntity roundEntity in roundEntities)
                    entity.Attributes.ComplementAttributesTo(roundEntity.Attributes);
            }
        }



        protected IEnumerable<string> GetAutomaticTags()
        {
            return _inputs.Select(val => val.EntityType).Union(_outputs.Select(val => val.EntityType)).Select(Entity.GetDisplayName).Distinct();
        }



        internal IEnumerable<ParameterInfo> GetDefaultArguments()
        {
            return Parameters.Select(parameter => parameter.ParameterInfo);
        }



        private bool PrepareCache(out string cacheKey)
        {
            cacheKey = null;

            if (!HasInputs && UseCache)
            {
                InputData inputData = new InputData(SubInputs);

                //Determines the actual values of the parameters by evaluating possible expressions
                EvaluateParameters(inputData);

                cacheKey = CacheManager.GetCacheKey(this);

                CacheData cacheData = CacheManager.GetCache(this, cacheKey);
                if (cacheData != null)
                {
                    cacheData = cacheData.DeepClone();
                    foreach (OutputReference outputReference in Outputs)
                    {
                        outputReference.Output.Write(cacheData.Data[outputReference.Label]);
                    }

                    return true;
                }
            }

            return false;
        }



        protected abstract void Run();



        internal void SetupImpulsePorts(string label)
        {
            _inputs.Add(new SingleInput(label, typeof(IEntity)));
            _isImpulsedSource = true;
        }



        private void StoreCache(string cacheKey)
        {
            if (!HasInputs && UseCache)
            {
                CacheManager.SetCache(this, cacheKey);
            }
        }



        public void UpdateParameterPorts()
        {
            _inputs.RemoveAll(x => x.Parent is Parameter);
            _outputs.RemoveAll(x => x.Parent is Parameter);

            foreach (var input in Parameters.SelectMany(val => val.Parameter.SubInputTree))
                _inputs.Add(input);

            foreach (var output in Parameters.SelectMany(val => val.Parameter.SubOutputTree))
                _outputs.Add(output);
        }



        /*internal bool NeedsPortUpdate
        {
            get { return _needsPortUpdate; }
            set { _needsPortUpdate = value; }
        }*/

        /*internal HashSet<AttributeCollection> LocalEntities
        {
            get { return _localEntities; }
        }*/
    }
}