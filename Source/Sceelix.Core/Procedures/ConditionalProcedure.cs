using System;
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
    /// Allows data flow control and redirection by evaluating conditions on input entities.
    /// </summary>
    [Procedure("8c5f1758-7567-41ef-9089-2e033169567d", Label = "Conditional", Category = "Basic")]
    public class ConditionalProcedure : SystemProcedure
    {
        /// <summary>
        /// The entity which is meant to be redirected based the evaluated condition/value/type.
        /// </summary>
        private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

        /// <summary>
        /// Check operation for condition evaluation.
        /// </summary>
        private readonly SelectListParameter<ConditionalParameter> _parametersCheck = new SelectListParameter<ConditionalParameter>("Check", "If...Else");


        public override IEnumerable<string> Tags => base.Tags.Union(_parametersCheck.SubParameterLabels);



        protected override void Run()
        {
            var entity = _input.Read();

            foreach (var checkParameter in _parametersCheck.Items)
            {
                checkParameter.Handle(entity);
            }
        }



        #region Abstract Conditional

        public abstract class ConditionalParameter : CompoundParameter
        {
            protected ConditionalParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Handle(IEntity entity);
        }

        #endregion

        #region If...Else

        /// <summary>
        /// Performs entity flow control based on conditions that are evaluated for each entity.
        /// </summary>
        public class IfElseConditionalParameter : ConditionalParameter
        {
            /// <summary>
            /// List of conditions to be evaluated. The first to evaluate to true will return the entity to its corresponding output. 
            /// Otherwise, the entity will pass on to the next condition.
            /// </summary>
            private readonly ListParameter _parameterConditions = new ListParameter("Conditions",
                () => new BoolParameter("If", false) {IsExpression = true, SubOutputs = new OutputCollection(new Output<IEntity>("If") {Description = "Output through which the entity will come if the condition is true."})});

            /// <summary>
            /// If none of the conditions are true, the entity will be sent to this "Else" output.
            /// </summary>
            private readonly CompoundParameter _parameterElse = new CompoundParameter("Else") {SubOutputs = new OutputCollection(new Output<IEntity>("Else") {Description = "Output through which the entity will come if the condition is false for all the others."})};



            public IfElseConditionalParameter()
                : base("If...Else")
            {
                //add a default "If"
                _parameterConditions.Set("If");
            }



            protected internal override void Handle(IEntity entity)
            {
                foreach (var boolParameter in _parameterConditions.Items.OfType<BoolParameter>())
                {
                    //if the boolean expression checks out, send the entity to its output 
                    //and finish here, otherwise, pass on to the next boolean expression
                    if (boolParameter.Value)
                    {
                        boolParameter.SubOutputs[0].Write(entity);
                        return;
                    }
                }

                //if it reached this point, it means no other boolean expressions were true
                //so return on this one
                _parameterElse.SubOutputs[0].Write(entity);
            }
        }

        #endregion

        #region Switch...Case

        /// <summary>
        /// Performs entity flow control based on equality comparisons that are evaluated for each entity.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.ConditionalProcedure.ConditionalParameter" />
        public class SwitchCaseConditionalParameter : ConditionalParameter
        {
            /// <summary>
            /// Value that will be compared.
            /// </summary>
            private readonly ObjectParameter _parameterSwitchValue = new ObjectParameter("Switch");

            /// <summary>
            /// The list of values to compare to.
            /// </summary>
            private readonly ListParameter _parameterConditions = new ListParameter("Cases",
                () => new ObjectParameter("Case")
                {
                    SubOutputs = new OutputCollection(new Output<IEntity>("Case") {Description = "Output through which the entity will come if the value matches."}),
                    Description = "Value to compare to. If the value in \"Switch\" matches this one, the entity will be outputted through the port associated to this parameter."
                });


            /// <summary>
            /// If none of the value matches, the entity will be sent to this "Default" output.
            /// </summary>
            private readonly CompoundParameter _parameterDefault = new CompoundParameter("Default") {SubOutputs = new OutputCollection(new Output<IEntity>("Else") {Description = "Output through which the entity will come if the value does not match any of the others."})};



            public SwitchCaseConditionalParameter()
                : base("Switch...Case")
            {
                //add a default "If"
                _parameterConditions.Set("Case");
            }



            protected internal override void Handle(IEntity entity)
            {
                var switchValue = _parameterSwitchValue.Value;

                foreach (var objectParameter in _parameterConditions.Items.OfType<ObjectParameter>())
                {
                    //if the equality checks out, send the entity to its output 
                    //and finish here, otherwise, pass on to the next boolean expression
                    if (objectParameter.Value.Equals(switchValue))
                    {
                        objectParameter.SubOutputs[0].Write(entity);
                        return;
                    }
                }

                //if it reached this point, it means no other equality expressions worked out
                //so return on this one
                _parameterDefault.SubOutputs[0].Write(entity);
            }
        }

        #endregion

        #region Type

        /// <summary>
        /// Performs entity flow control based on the type of entity that is being handled.
        /// </summary>
        public class TypeConditionalParameter : ConditionalParameter
        {
            /// <summary>
            /// Dictionary of type display names -> type
            /// </summary>
            private static Dictionary<Type, string> _nameTypeIndex;


            /// <summary>
            /// List of types to check. The first to evaluate to true will return to its corresponding output. 
            /// </summary>
            private readonly ListParameter _parameterTypes = new ListParameter("Types", () => new ChoiceParameter("Type", "")
            {
                Choices = InitializeTypes(), SubOutputs = new OutputCollection(new Output<IEntity>("Type") {Description = "Output through which the entity will come if the type matches."}),
                Description = "Type to verify against. If the entity matches this type, it will be outputted through the port associated to this parameter."
            });


            /// <summary>
            /// If none of the types match, the entity will be sent to this "Else" output.
            /// </summary>
            private readonly CompoundParameter _parameterElse = new CompoundParameter("Else") {SubOutputs = new OutputCollection(new Output<IEntity>("Else") {Description = "Output through which the entity will come if the type does not match any of the others."})};



            public TypeConditionalParameter()
                : base("Type")
            {
                //add a default "If"
                _parameterTypes.Set("Type");
            }



            protected internal override void Handle(IEntity entity)
            {
                foreach (var choiceParameter in _parameterTypes.Items.OfType<ChoiceParameter>())
                {
                    if (!string.IsNullOrWhiteSpace(choiceParameter.Value))
                    {
                        var name = _nameTypeIndex[entity.GetType()];

                        //if the equality checks out, send the entity to its output 
                        //and finish here, otherwise, pass on to the next type
                        if (choiceParameter.Value == name)
                        {
                            choiceParameter.SubOutputs[0].Write(entity);
                            return;
                        }
                    }
                }

                //if it reached this point, it means no other types were matched
                //so return on this one
                _parameterElse.SubOutputs[0].Write(entity);
            }



            public static string[] InitializeTypes()
            {
                if (_nameTypeIndex == null)
                {
                    var browsableTypes = EntityManager.Types.Where(type => type.HasCustomAttribute<EntityAttribute>() && type.GetCustomAttribute<EntityAttribute>().TypeBrowsable);

                    _nameTypeIndex = browsableTypes.ToDictionary(x => x, y => y.GetCustomAttribute<EntityAttribute>().Name);
                }

                return _nameTypeIndex.Values.OrderBy(x => x).ToArray();
            }
        }

        #endregion
    }
}