using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Allows combination and permutation of data by either creating clones or reference 
    /// copies of entities.<br/>
    /// </summary>
    [Procedure("19fbd59a-ed19-483c-bd0d-e0c7773de563", Label = "Combinatorial", Category = "Basic")]
    public class CombinatorialProcedure : SystemProcedure
    {
        /// <summary>
        /// The entities to be combined.
        /// </summary>
        private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

        /// <summary>
        /// The type of operation to perform.<br/>
        /// <b>Combination</b> means that the items will be paired, without caring for the order.<br/>
        /// <b>Permutation</b> means that the items will be paired, caring for the order.<br/>
        /// </summary>
        private readonly ChoiceParameter _parameterType = new ChoiceParameter("Type", "Combination", "Combination", "Permutation");

        /// <summary>
        /// Indicates if the if the entities should be copied by reference or by cloning them.
        /// </summary>
        private readonly ChoiceParameter _parameterCopyMethod = new ChoiceParameter("Copy Method", "Reference", "Clone", "Reference");


        /// <summary>
        /// The output streams, which indicate how the combinations/permutations are performed.
        /// </summary>
        private readonly ListParameter<CombinatorialParameter> _parameterCombinatorialOutputs = new ListParameter<CombinatorialParameter>("Outputs");



        private void DoCombination(List<IEntity> entityList, List<CombinatorialParameter> augs, int startingIndex, int cicleCount, IEntity[] entities, bool referenceCopy)
        {
            if (cicleCount > 0)
            {
                for (int i = startingIndex; i < entityList.Count; i++)
                {
                    DoCombination(entityList, augs, i + 1, cicleCount - 1, entities.Concat(new[] {entityList[i]}).ToArray(), referenceCopy);
                }
            }
            else if (cicleCount == 0)
            {
                for (int i = 0; i < augs.Count; i++)
                {
                    if (referenceCopy)
                        augs[i].Output.Write(entities[i]);
                    else
                        augs[i].Output.Write(entities[i].DeepClone());
                }
            }
        }



        private void DoPermutation(List<IEntity> entityList, List<CombinatorialParameter> augs, int startingIndex, int cicleCount, int[] entityIndices, bool referenceCopy)
        {
            if (cicleCount > 0)
            {
                for (int i = 0; i < entityList.Count; i++)
                {
                    if (!entityIndices.Contains(i))
                        DoPermutation(entityList, augs, i + 1, cicleCount - 1, entityIndices.Concat(new[] {i}).ToArray(), referenceCopy);
                }
            }
            else if (cicleCount == 0)
            {
                for (int i = 0; i < augs.Count; i++)
                {
                    if (referenceCopy)
                        augs[i].Output.Write(entityList[entityIndices[i]]);
                    else
                        augs[i].Output.Write(entityList[entityIndices[i]].DeepClone());
                }
            }
        }



        protected override void Run()
        {
            List<IEntity> list = _input.Read().ToList();

            List<CombinatorialParameter> augs = _parameterCombinatorialOutputs.Items.ToList();

            if (_parameterType.Value == "Combination")
                DoCombination(list, augs, 0, augs.Count, new IEntity[0], _parameterCopyMethod.Value == "Reference");
            else if (_parameterType.Value == "Permutation")
                DoPermutation(list, augs, 0, augs.Count, new int[0], _parameterCopyMethod.Value == "Reference");
        }



        #region Outputs

        /// <summary>
        /// An output stream.
        /// </summary>
        public class CombinatorialParameter : CompoundParameter
        {
            /// <summary>
            /// The output with a clone or reference copy of the entity.
            /// </summary>
            internal readonly Output<IEntity> Output = new Output<IEntity>("Output");



            public CombinatorialParameter()
                : base("Outputs")
            {
            }
        }

        #endregion
    }
}