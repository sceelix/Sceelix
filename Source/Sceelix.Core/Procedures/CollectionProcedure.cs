using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Extensions;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Performs data analysis, filtering and organization 
    /// on collections of entities.
    /// </summary>
    [Procedure("3f477acc-ff16-4ceb-9f54-0b1404dc149b", Label = "Collection", Category = "Basic")]
    public class CollectionProcedure : SystemProcedure
    {
        /// <summary>
        /// The entities that will be operated on.
        /// </summary>
        private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

        /// <summary>
        /// The entities that were operated on.
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        /// <summary>
        /// Type of collection operation to perform.
        /// </summary>
        private readonly SelectListParameter<AbstractCollectionParameter> _parameterCollectionOperation = new SelectListParameter<AbstractCollectionParameter>("Operation", "Count");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterCollectionOperation.SubParameterLabels);



        protected override void Run()
        {
            var entities = _input.Read().ToList();

            foreach (AbstractCollectionParameter abstractListParameter in _parameterCollectionOperation.Items)
            {
                _output.Write(abstractListParameter.Apply(entities));
            }
        }



        #region Abstract Parameter

        public abstract class AbstractCollectionParameter : CompoundParameter
        {
            protected AbstractCollectionParameter(string label)
                : base(label)
            {
            }



            internal abstract IEnumerable<IEntity> Apply(List<IEntity> entities);
        }

        #endregion

        #region Count

        /// <summary>
        /// Performs counting or indexing on the input collection.
        /// </summary>
        public class CountCollectionParameter : AbstractCollectionParameter
        {
            /// <summary>
            /// Writes a sequence number corresponding to the number of each entity in the sequence.
            /// </summary>
            private readonly AttributeParameter<int> _attributeIndex = new AttributeParameter<int>("Index", AttributeAccess.Write);

            /// <summary>
            /// Writes the total number of entities on the collection.
            /// </summary>
            private readonly AttributeParameter<int> _attributeCount = new AttributeParameter<int>("Count", AttributeAccess.Write);



            public CountCollectionParameter()
                : base("Count")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                for (int index = 0; index < entities.Count; index++)
                {
                    IEntity entity = entities[index];

                    _attributeIndex[entity] = index;
                    _attributeCount[entity] = entities.Count;
                }

                return entities;
            }
        }

        #endregion

        #region Distinct

        /// <summary>
        /// Filters repeated entities in the collection. 
        /// Especially useful to remove repeated references to the same entity.
        /// </summary>
        public class DistinctCollectionParameter : AbstractCollectionParameter
        {
            public DistinctCollectionParameter()
                : base("Distinct")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                return entities.Distinct();
            }
        }

        #endregion

        #region Reverse

        /// <summary>
        /// Reverses the order of the input collection.
        /// </summary>
        public class ReverseCollectionParameter : AbstractCollectionParameter
        {
            public ReverseCollectionParameter()
                : base("Reverse")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                entities.Reverse();

                return entities;
            }
        }

        #endregion

        #region Shuffle

        /// <summary>
        /// Randomly orders the elements in the input collection.
        /// </summary>
        public class ShuffleCollectionParameter : AbstractCollectionParameter
        {
            /// <summary>
            /// Seed of the random shuffler. Controls the randomness look the shuffle.
            /// </summary>
            private readonly IntParameter _parameterSeed = new IntParameter("Seed", 0);



            public ShuffleCollectionParameter()
                : base("Shuffle")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                entities.Shuffle(_parameterSeed.Value);

                return entities;
            }
        }

        #endregion

        #region Aggregation

        /// <summary>
        /// Aggregates attribute values and stores the result 
        /// into other attributes.
        /// </summary>
        public class AggregationCollectionParameter : AbstractCollectionParameter
        {
            /// <summary>
            /// Value to be aggregated. 
            /// Subexpression is evaluated on each entity from the collection.
            /// </summary>
            private readonly ObjectParameter _parameterValue = new ObjectParameter("Value") {EntityEvaluation = true};

            /// <summary>
            /// Type of aggregative operation to perform.
            /// </summary>
            private readonly ChoiceParameter _parameterChoice = new ChoiceParameter("Operation", "Sum", "Sum", "Average", "Maximum", "Minimum");

            /// <summary>
            /// Attribute where to store the aggregated value.
            /// </summary>
            private readonly AttributeParameter _parameterAggregatedValue = new AttributeParameter("Aggregated Value", AttributeAccess.Write);



            public AggregationCollectionParameter()
                : base("Aggregation")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                var attributeValues = (List<dynamic>) entities.Select(x => _parameterValue.Get(x)).ToList();

                dynamic finalResult = null;

                switch (_parameterChoice.Value)
                {
                    case "Sum":
                        finalResult = attributeValues.Aggregate((result, val) => result + val);
                        break;
                    case "Average":
                        finalResult = attributeValues.Aggregate((result, val) => result + val) / attributeValues.Count;
                        break;
                    case "Maximum":
                        finalResult = attributeValues.Max(x => x);
                        break;
                    case "Minimum":
                        finalResult = attributeValues.Min(x => x);
                        break;
                }

                foreach (var obj in entities)
                    _parameterAggregatedValue[obj] = finalResult;

                return entities;
            }
        }

        #endregion

        #region Take

        /// <summary>
        /// Takes a specified number of contiguous elements
        /// from a certain position of the input collection.
        /// </summary>
        public class TakeParameter : AbstractCollectionParameter
        {
            /// <summary>
            /// Entities that are not delivered through the other port.
            /// </summary>
            private readonly Output<IEntity> _outputElse = new Output<IEntity>("Else");

            /// <summary>
            /// Index where to start the extraction.
            /// </summary>
            private readonly IntParameter _parameterStartingIndex = new IntParameter("Starting Index", 0);

            /// <summary>
            /// Number of items (starting at 'Starting Index') 
            /// to extract to the first port.
            /// </summary>
            private readonly IntParameter _parameterAmount = new IntParameter("Amount", 5);

            /// <summary>
            /// If checked, a repetitive, alternating pattern will be applied.
            /// Example: 10 elements (A,B,C,D,E,F,G,H,I,J), Starting and Amount = 2 will
            /// return A B,E,F,I,J to the first output and C,D,G,H to the second one.
            /// </summary>
            private readonly BoolParameter _parameterLoop = new BoolParameter("Loop", false);



            public TakeParameter()
                : base("Take")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                //slight optimization: store in local variables to avoid calling the Value field very often
                var startingIndex = _parameterStartingIndex.Value;
                var amount = _parameterAmount.Value;
                var step = _parameterStartingIndex.Value + _parameterAmount.Value;

                if (_parameterLoop.Value)
                {
                    for (int i = 0; i < entities.Count; i++)
                    {
                        //if looping, we use this simple formula
                        int countIndex = (i - startingIndex) % step;

                        if (countIndex >= 0 && countIndex < amount)
                            yield return entities[i];
                        else
                            _outputElse.Write(entities[i]);
                    }
                }
                else
                {
                    //simplest case: if inside the start and end limits, return the value, otherwise send to the other port
                    for (int i = 0; i < entities.Count; i++)
                    {
                        if (i >= startingIndex && i < step)
                            yield return entities[i];
                        else
                            _outputElse.Write(entities[i]);
                    }
                }
            }
        }

        #endregion

        #region OrderBy

        /// <summary>
        /// Attribute-based criteria by which to order.
        /// </summary>
        public class OrderByCriteriumParameter : CompoundParameter
        {
            /// <summary>
            /// Value on which to decide the order.
            /// </summary>
            private readonly ObjectParameter _parameterValue = new ObjectParameter("Value") {EntityEvaluation = true};

            /// <summary>
            /// Indicates if the ordering on this criteria should be from
            /// the greatest to the smallest.
            /// </summary>
            private readonly BoolParameter _parameterDescending = new BoolParameter("Descending", false);



            protected OrderByCriteriumParameter()
                : base("Criterium")
            {
            }



            internal bool OrderDescending => _parameterDescending.Value;



            public object GetAttributeValue(IEntity entity)
            {
                return _parameterValue.Get(entity);
            }
        }

        /// <summary>
        /// Orders elements of the input collection
        /// according to values of their attributes.
        /// </summary>
        public class OrderByCollectionParameter : AbstractCollectionParameter
        {
            /// <summary>
            /// List of criteria on which the ordering should be based.
            /// Several criteria can be defined.
            /// </summary>
            private readonly ListParameter<OrderByCriteriumParameter> _parameterOrderCriteria = new ListParameter<OrderByCriteriumParameter>("Criteria");



            public OrderByCollectionParameter()
                : base("Order By")
            {
            }



            internal override IEnumerable<IEntity> Apply(List<IEntity> entities)
            {
                IOrderedEnumerable<IEntity> orderedEnumerable = null;

                foreach (OrderByCriteriumParameter orderByAugmentation in _parameterOrderCriteria.Items)
                {
                    OrderByCriteriumParameter augmentation = orderByAugmentation;

                    if (orderedEnumerable == null)
                        orderedEnumerable = augmentation.OrderDescending ? entities.OrderByDescending(augmentation.GetAttributeValue) : entities.OrderBy(augmentation.GetAttributeValue);
                    else
                        orderedEnumerable = augmentation.OrderDescending ? orderedEnumerable.ThenByDescending(augmentation.GetAttributeValue) : orderedEnumerable.ThenBy(augmentation.GetAttributeValue);
                }

                if (orderedEnumerable != null)
                    entities = orderedEnumerable.ToList();

                return entities;
            }
        }

        #endregion
    }
}