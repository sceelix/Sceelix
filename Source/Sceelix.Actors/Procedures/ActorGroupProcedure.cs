using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.Actors.Procedures
{
    /// <summary>
    /// Groups or ungroups actor entities.
    /// </summary>
    [Procedure("c1722ebb-8bc2-45a1-a146-b3767d611cd1", Label = "Actor Group", Category = "Actor", Tags = "Actors, Group, Ungroup")]
    public class ActorGroupProcedure : SystemProcedure
    {
        /// <summary>
        /// Type of operation to perform.
        /// </summary>
        private readonly SelectListParameter<ActorGroupParameter> _parameterGroupList = new SelectListParameter<ActorGroupParameter>("Operation", "Group");



        protected override void Run()
        {
            var selectedGroup = _parameterGroupList.Items.FirstOrDefault();
            if (selectedGroup != null) selectedGroup.Run();
        }



        #region Abstract Parameter

        public abstract class ActorGroupParameter : CompoundParameter
        {
            protected ActorGroupParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Run();
        }

        #endregion

        #region Entity Group

        /// <summary>
        /// Groups actors, creating an actor group entity.
        /// </summary>
        /// <seealso cref="Sceelix.Actors.Procedures.ActorGroupProcedure.ActorGroupParameter" />
        public class EntityActorGroupParameter : ActorGroupParameter
        {
            /// <summary>
            /// Set of actors to be grouped.
            /// </summary>
            private readonly CollectiveInput<IActor> _input = new CollectiveInput<IActor>("Input");

            /// <summary>
            /// One or more actor group entities, grouped according to the specified criteria.
            /// </summary>
            private readonly Output<ActorGroup> _output = new Output<ActorGroup>("Output");

            /// <summary>
            /// Indicates if the resulting actor groups should carry the common attributes of their children.
            /// </summary>
            private readonly BoolParameter _parameterMergeAttributes = new BoolParameter("Merge Attributes", true);

            /// <summary>
            /// Criteria for grouping actors (based on the properties of each actor).
            /// If none are indicated, all the entities will be sent into the same actor group.
            /// </summary>
            private readonly ListParameter _parameterCriteria = new ListParameter("Criteria", () => new ObjectParameter("Criterium")
            {
                EntityEvaluation = true,
                Description = "Criterium for grouping actors. The @@attributeName refers to the attributes of each actor."
            });



            public EntityActorGroupParameter()
                : base("Group")
            {
            }



            private IEnumerable<object> GetData(IEntity entity)
            {
                return _parameterCriteria.Items.Select(x => ((ObjectParameter) x).Get(entity));
            }



            protected internal override void Run()
            {
                foreach (IGrouping<object[], IActor> actors in _input.Read().GroupBy(x => GetData(x).ToArray(), new ObjectArrayEqualityComparer()))
                {
                    var actorGroup = new ActorGroup(actors);

                    if (_parameterMergeAttributes.Value)
                        actorGroup.MergeAttributes();

                    _output.Write(actorGroup);
                }
            }
        }

        #endregion

        #region Ungroup

        /// <summary>
        /// Ungroups actor group entities into their subactors.
        /// </summary>
        /// <seealso cref="Sceelix.Actors.Procedures.ActorGroupProcedure.ActorGroupParameter" />
        public class Ungroup : ActorGroupParameter
        {
            /// <summary>
            /// Actor group to be ungrouped.
            /// </summary>
            private readonly SingleInput<ActorGroup> _input = new SingleInput<ActorGroup>("Input");

            /// <summary>
            /// Actors that were contained in the group.
            /// </summary>
            private readonly Output<IActor> _output = new Output<IActor>("Output");

            /// <summary>
            /// Indicates what kind of attributes the newly ungrouped actors should possess.<br/>
            /// 
            /// <b>Parent and Actor</b> means that the attributes defined for the group will be added to the ungrouped items (only complementing them, not overwriting). <br/>
            /// <b>Parent</b> means that the attributes of the ungrouped items will be the ones from the parent.<br/>
            /// <b>Actor</b> means that the ungrouped items will only maintain their own attributes.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterAttributes = new ChoiceParameter("Attributes", "Parent and Actor", "Parent", "Actor", "Parent and Actor");



            public Ungroup()
                : base("Ungroup")
            {
            }



            protected internal override void Run()
            {
                var actorGroup = _input.Read();

                foreach (var subActor in actorGroup.SubEntityTree.OfType<IActor>())
                {
                    if (_parameterAttributes.Value == "Parent")
                        actorGroup.Attributes.SetAttributesTo(subActor.Attributes);

                    if (_parameterAttributes.Value == "Parent And Actor")
                        actorGroup.Attributes.ComplementAttributesTo(subActor.Attributes);

                    _output.Write(subActor);
                }
            }
        }

        #endregion
    }
}