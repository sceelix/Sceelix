using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Creates copies of the input entities.
    /// </summary>
    [Procedure("50c9a7c0-6f52-470d-8bb7-2c8b663c94b8", Label = "Copy", Category = "Basic")]
    public class CopyProcedure : SystemProcedure
    {
        /// <summary>
        /// Type of copy operation to perform.
        /// </summary>
        private readonly SelectListParameter<CopyParameter> _parametersOption = new SelectListParameter<CopyParameter>("Operation", "Standard");



        protected override void Run()
        {
            _parametersOption.SelectedItem.Run();
        }



        #region Abstract Copy

        public abstract class CopyParameter : CompoundParameter
        {
            protected CopyParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Run();
        }

        #endregion

        #region Standard

        /// <summary>
        /// Standard operation for creating copies of a single stream of entities.
        /// </summary>
        public class StandardCopyParameter : CopyParameter
        {
            /// <summary>
            /// The entity to be copied.
            /// </summary>
            private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

            /// <summary>
            /// The entity and its copies.
            /// </summary>
            private readonly Output<IEntity> _output = new Output<IEntity>("Output");

            /// <summary>
            /// The number of copies to output. If 0, the original will be discarded.
            /// </summary>
            private readonly IntParameter _parameterNumberOfCopies = new IntParameter("Number of Copies", 10);

            /// <summary>
            /// Specifies the data copy method. <br/>
            /// <b>Clone</b> means that independent copies of each entity will be produced. <br/>
            /// <b>Reference</b> means that only the entity <i>references</i> will be copied.
            /// </summary>
            private readonly ChoiceParameter _parameterCopyMethod = new ChoiceParameter("Method", "Clone", "Clone", "Reference");

            /// <summary>
            /// Attribute that will contain the index of the copy (starting at 0).
            /// </summary>
            private readonly AttributeParameter<int> _attributeIndex = new AttributeParameter<int>("Index", AttributeAccess.Write);



            public StandardCopyParameter()
                : base("Standard")
            {
            }



            protected internal override void Run()
            {
                IEntity entity = _input.Read();

                if (_parameterCopyMethod.Value == "Clone")
                {
                    for (int i = 0; i < _parameterNumberOfCopies.Value; i++)
                    {
                        IEntity newEntity = entity.DeepClone();

                        if (_attributeIndex.IsMapped)
                            _attributeIndex[newEntity] = i;

                        _output.Write(newEntity);
                    }
                }
                else
                {
                    for (int i = 0; i < _parameterNumberOfCopies.Value; i++)
                        _output.Write(entity);
                }
            }
        }

        #endregion

        #region Relation

        /// <summary>
        /// Performs entity copy operations on entity collections so as to match the number of entities in each defined channel (input-output set).
        /// </summary>
        public class RelationCopyParameter : CopyParameter
        {
            /// <summary>
            /// Channels (sets of input/output) for performing the relation copy.
            /// </summary>
            private readonly ListParameter<RelationCopyChannel> _parameterChannels = new ListParameter<RelationCopyChannel>("Channels");


            /// <summary>
            /// Specifies the data copy method. 
            /// 'Clone' refer to producing independent copies of each entity, while 
            /// 'Reference' means that only the entity references will be copied.
            /// </summary>
            private readonly ChoiceParameter _parameterCopyMethod = new ChoiceParameter("Method", "Clone", "Clone", "Reference");

            /// <summary>
            /// Type of relation copy. See each option for details.
            /// </summary>
            private readonly SelectListParameter<RelationCopyTypeParameter> _parameterType = new SelectListParameter<RelationCopyTypeParameter>("Type", "Clamp");



            public RelationCopyParameter()
                : base("Relation")
            {
                _parameterChannels.Set(new[] {"Channel", "Channel"});
            }



            protected internal override void Run()
            {
                List<RelationCopyChannel> relationCopyChannels = _parameterChannels.Items.ToList();

                var operation = _parameterType.Items.FirstOrDefault();
                if (operation != null)
                {
                    operation.Run(relationCopyChannels, _parameterCopyMethod.Value == "Reference");
                }
            }



            /// <summary>
            /// Defines a stream of entities (a input-output pair) to be considered in the relation copy operation.  
            /// </summary>
            public class RelationCopyChannel : CompoundParameter
            {
                /// <summary>
                /// The entities to be copied.
                /// </summary>
                internal readonly CollectiveInput<IEntity> Input = new CollectiveInput<IEntity>("Input");

                /// <summary>
                /// The entities and their copies.
                /// </summary>
                internal readonly Output<IEntity> Output = new Output<IEntity>("Output");



                public RelationCopyChannel()
                    : base("Channel")
                {
                }
            }

            public abstract class RelationCopyTypeParameter : CompoundParameter
            {
                protected RelationCopyTypeParameter(string label)
                    : base(label)
                {
                }



                public abstract void Run(List<RelationCopyChannel> relationCopyChannels, bool referenceCopy);
            }

            #region Clamp

            /// <summary>
            /// The last item of the smallest collection will be copied until the collection sizes match.
            /// </summary>
            public class ClampCopyTypeParameter : RelationCopyTypeParameter
            {
                public ClampCopyTypeParameter()
                    : base("Clamp")
                {
                }



                public override void Run(List<RelationCopyChannel> relationCopyChannels, bool referenceCopy)
                {
                    List<List<IEntity>> dataLists = relationCopyChannels.Select(x => new List<IEntity>(x.Input.Read())).ToList();

                    var maxValues = dataLists.Max(x => x.Count);

                    for (int i = 0; i < relationCopyChannels.Count; i++)
                    {
                        for (int j = 0; j < maxValues; j++)
                        {
                            var realJ = Math.Min(dataLists[i].Count - 1, j);

                            relationCopyChannels[i].Output.Write(referenceCopy ? dataLists[i][realJ] : dataLists[i][realJ].DeepClone());
                        }
                    }
                }
            }

            #endregion

            #region Repeat

            /// <summary>
            /// The sequence of items from the smallest collection will be copied until the collection sizes match.
            /// </summary>
            public class RepeatCopyTypeParameter : RelationCopyTypeParameter
            {
                public RepeatCopyTypeParameter()
                    : base("Repeat")
                {
                }



                public override void Run(List<RelationCopyChannel> relationCopyChannels, bool referenceCopy)
                {
                    List<CircularList<IEntity>> dataLists = relationCopyChannels.Select(x => new CircularList<IEntity>(x.Input.Read())).ToList();

                    var maxValues = dataLists.Max(x => x.Count);

                    for (int i = 0; i < relationCopyChannels.Count; i++)
                    {
                        for (int j = 0; j < maxValues; j++)
                        {
                            relationCopyChannels[i].Output.Write(referenceCopy ? dataLists[i][j] : dataLists[i][j].DeepClone());
                            /*if (j >= dataLists[i].Count && !referenceCopy)
                                relationCopyChannels[i].Output.Write(dataLists[i][j].DeepClone());
                            else
                                relationCopyChannels[i].Output.Write(dataLists[i][j]);*/
                        }
                    }
                }
            }

            #endregion

            #region Cross

            /// <summary>
            /// Cross product operation between the collections.
            /// In other words, each item will be copied for every item of
            /// the other collections.
            /// For instance, if one list has the items A, B, C and the other
            /// D,E then the result will be AD, AE, BD, BE, CD, CE.
            /// </summary>
            public class CrossCopyTypeParameter : RelationCopyTypeParameter
            {
                public CrossCopyTypeParameter()
                    : base("Cross")
                {
                }



                public override void Run(List<RelationCopyChannel> relationCopyChannels, bool referenceCopy)
                {
                    RunCross(relationCopyChannels, 0, new IEntity[0], referenceCopy);
                }



                private void RunCross(List<RelationCopyChannel> augs, int index, IEntity[] entities, bool referenceCopy)
                {
                    if (index < augs.Count)
                    {
                        IEnumerable<IEntity> enumerable = augs[index].Input.Read();
                        foreach (IEntity entity in enumerable)
                        {
                            RunCross(augs, index + 1, entities.Concat(new[] {entity}).ToArray(), referenceCopy);
                        }
                    }
                    else
                    {
                        //end here
                        for (int i = 0; i < augs.Count; i++)
                        {
                            augs[i].Output.Write(referenceCopy ? entities[i] : entities[i].DeepClone());
                        }
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}