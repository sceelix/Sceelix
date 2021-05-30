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
    /// Groups or ungroups entities.
    /// </summary>
    [Procedure("44d56290-9f2d-4add-b0a6-dbfaaaed1e18", Label = "Group", Category = "Basic")]
    public class GroupProcedure : SystemProcedure
    {
        /// <summary>
        /// Type of grouping operation to perform.
        /// </summary>
        private readonly SelectListParameter<GroupParameter> _parameterGroupList = new SelectListParameter<GroupParameter>("Operation", "Group");



        protected override void Run()
        {
            _parameterGroupList.SelectedItem.Run();
        }



        #region Abstract Parameter

        public abstract class GroupParameter : CompoundParameter
        {
            protected GroupParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Run();
        }

        #endregion

        #region Entity Group

        /// <summary>
        /// Groups actors, creating a group entity.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.GroupProcedure.GroupParameter" />
        public class EntityGroupParameter : GroupParameter
        {
            /// <summary>
            /// Set of entities to be grouped.
            /// </summary>
            private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

            /// <summary>
            /// One or more group entities, grouped according to the specified criteria.
            /// </summary>
            private readonly Output<EntityGroup> _output = new Output<EntityGroup>("Output");

            /// <summary>
            /// Criteria for grouping entities (based on the properties of each entity).
            /// If non is indicated, all the entities will be sent into the same group.
            /// </summary>
            private readonly ListParameter _parameterCriteria = new ListParameter("Criteria", () => new ObjectParameter("Criterium")
            {
                EntityEvaluation = true,
                Description = "Criterium for grouping entities. The @@attributeName refers to the attributes of each entity."
            });

            /// <summary>
            /// Indicates if the resulting groups should carry the common attributes of their children.
            /// </summary>
            private readonly BoolParameter _parameterMergeAttributes = new BoolParameter("Merge Attributes", true);



            public EntityGroupParameter()
                : base("Group")
            {
            }



            private IEnumerable<object> GetData(IEntity entity)
            {
                return _parameterCriteria.Items.Select(x => ((ObjectParameter) x).Get(entity));
            }



            protected internal override void Run()
            {
                foreach (IGrouping<object[], IEntity> entities in _input.Read().GroupBy(x => GetData(x).ToArray(), new ObjectArrayEqualityComparer()))
                {
                    var entityGroup = new EntityGroup(entities);

                    if (_parameterMergeAttributes.Value)
                        entityGroup.MergeAttributes();
                    /*for (int i = 0; i < entities.Key.Length; i++)
                    {
                        var attributeParameter = (AttributeParameter) _parameterCriteria.Items[i];
                        attributeParameter[entityGroup] = entities.Key[i].Clone();
                    }*/

                    _output.Write(entityGroup);
                }
            }
        }

        #endregion

        #region Ungroup

        /// <summary>
        /// Ungroups actor group entities into their subentities.
        /// </summary>
        public class Ungroup : GroupParameter
        {
            /// <summary>
            /// Group to be ungrouped.
            /// </summary>
            private readonly SingleInput<IEntityGroup> _input = new SingleInput<IEntityGroup>("Input");

            /// <summary>
            /// Entities that were contained in the group.
            /// </summary>
            private readonly Output<IEntity> _output = new Output<IEntity>("Output");

            /// <summary>
            /// Indicates what kind of attributes the newly ungrouped entities should possess.<br/>
            /// 
            /// <b>Parent and Entity</b> means that the attributes defined for the group will be added to the ungrouped items (only complementing them, not overwriting). <br/>
            /// <b>Parent</b> means that the attributes of the ungrouped items will be the ones from the parent.<br/>
            /// <b>Entity</b> means that the ungrouped items will only maintain their own attributes.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterAttributes = new ChoiceParameter("Attributes", "Parent and Entity", "Parent", "Entity", "Parent and Entity");



            public Ungroup()
                : base("Ungroup")
            {
            }



            protected internal override void Run()
            {
                var entityGroup = _input.Read();

                foreach (var subEntity in entityGroup.SubEntities)
                {
                    if (_parameterAttributes.Value == "Parent")
                        entityGroup.Attributes.SetAttributesTo(subEntity.Attributes);

                    if (_parameterAttributes.Value == "Parent And Entity")
                        entityGroup.Attributes.ComplementAttributesTo(subEntity.Attributes);

                    _output.Write(subEntity);
                }
            }
        }

        #endregion
    }
}