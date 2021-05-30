using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Allows data flow aggregation, ordering and rerouting through 
    /// the definition of sets of matching inputs and outputs. 
    /// </summary>
    [Procedure("03899cac-fff4-46c1-8b67-fcea66165c8a", Label = "Sequence", Tags = "Entity", Category = "Basic")]
    public class SequenceProcedure : SystemProcedure
    {
        /// <summary>
        /// When several outputs are defined, indicates if the if the entities should be copied by reference or by cloning them.
        /// </summary>
        private readonly ChoiceParameter _parameterCopyMethod = new ChoiceParameter("Copy Method", "Clone", "Clone", "Reference");

        /// <summary>
        /// List of inputs of outputs of this node. The order is important and their pairing, too. <br/>
        /// The list should start with an input, which provides the first source of entities. Inputs listed immediately after will have their entities accumulated, in sequence.
        /// When an output comes after the inputs, it will flush the accumulated entities. If multiple output are defined in a row, the data is copied 
        /// according to the defined "copy method". If an input appears after the output(s), the accumulated entities are reset and the process starts again. 
        /// </summary>
        private readonly ListParameter<SequenceParameter> _parameterSequenceList = new ListParameter<SequenceParameter>("Ports");



        public SequenceProcedure()
        {
            _parameterSequenceList.Set(new[] {"Single Input", "Output"});
        }



        protected override void Run()
        {
            List<IEntity> entities = new List<IEntity>();

            SequenceParameter lastSequenceParameter = null;

            foreach (SequenceParameter parameter in _parameterSequenceList.Items)
            {
                if (parameter is SingleInputSequenceParameter)
                {
                    if (lastSequenceParameter is OutputSequenceParameter)
                        entities.Clear();

                    SingleInputSequenceParameter inputAugmentation = (SingleInputSequenceParameter) parameter;
                    entities.Add(inputAugmentation.Input.Read());
                }
                else if (parameter is CollectiveInputSequenceParameter)
                {
                    if (lastSequenceParameter is OutputSequenceParameter)
                        entities.Clear();

                    CollectiveInputSequenceParameter inputAugmentation = (CollectiveInputSequenceParameter) parameter;
                    entities.AddRange(inputAugmentation.Input.Read());
                }
                else if (parameter is OutputSequenceParameter)
                {
                    OutputSequenceParameter outputAugmentation = (OutputSequenceParameter) parameter;

                    if (lastSequenceParameter is OutputSequenceParameter && _parameterCopyMethod.Value == "Clone")
                        outputAugmentation.Output.Write(entities.Select(x => x.DeepClone()));
                    else
                        outputAugmentation.Output.Write(entities);
                }

                lastSequenceParameter = parameter;
            }
        }



        #region Sequence Abstract

        public abstract class SequenceParameter : CompoundParameter
        {
            protected SequenceParameter(string label)
                : base(label)
            {
            }
        }

        #endregion

        #region Single Input

        /// <summary>
        /// An input port that accepts one entity at the time.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.SequenceProcedure.SequenceParameter" />
        public class SingleInputSequenceParameter : SequenceParameter
        {
            /// <summary>
            /// An input port that accepts one entity at the time.
            /// </summary>
            public SingleInput<IEntity> Input = new SingleInput<IEntity>("Input");



            public SingleInputSequenceParameter()
                : base("Single Input")
            {
            }
        }

        #endregion

        #region Collective Input

        /// <summary>
        /// An input port that accepts a set/collection of entities at the time.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.SequenceProcedure.SequenceParameter" />
        public class CollectiveInputSequenceParameter : SequenceParameter
        {
            /// <summary>
            /// An input port that accepts a set/collection of entities at the time.
            /// </summary>
            public CollectiveInput<IEntity> Input = new CollectiveInput<IEntity>("Input");



            public CollectiveInputSequenceParameter()
                : base("Collective Input")
            {
            }
        }

        #endregion

        #region Output

        /// <summary>
        /// An output port that flushes the input data.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.SequenceProcedure.SequenceParameter" />
        public class OutputSequenceParameter : SequenceParameter
        {
            /// <summary>
            /// An output port that flushes the input data.
            /// </summary>
            public Output<IEntity> Output = new Output<IEntity>("Output");



            public OutputSequenceParameter()
                : base("Output")
            {
            }
        }

        #endregion
    }
}