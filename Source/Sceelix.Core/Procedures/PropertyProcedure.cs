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
    /// Gets entity-specific properties and loads them into attributes.
    /// </summary>
    [Procedure("c29f89c9-5215-49cb-8270-663b2cbd4cd3", Label = "Property", Category = "Basic")]
    public class PropertyProcedure : SystemProcedure
    {
        /// <summary>
        /// Type of entity from which to read the properties. The properties are specific to the particular type chosen, disregarding inheritance.
        /// This means that, for instance, in order to get the "Scope" of a mesh or path entity, the option "Actor" should be chosen, since it
        /// is a property from the parent type.
        /// </summary>
        private readonly SelectListParameter<PropertyParameter> _parameterEntity = new SelectListParameter<PropertyParameter>("Entity", "Entity");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterEntity.SubParameterLabels);



        protected override void Run()
        {
            _parameterEntity.SelectedItem.Run();
        }



        #region Property Parameter

        public abstract class PropertyParameter : CompoundParameter
        {
            protected PropertyParameter(string label)
                : base(label)
            {
            }



            public abstract void Run();
        }

        #endregion

        #region Entity Property

        /// <summary>
        /// Reads/calculates properties from entities.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.PropertyProcedure.PropertyParameter" />
        public class EntityPropertyParameter : PropertyParameter
        {
            /// <summary>
            /// A list with all the attributes of this entity.
            /// </summary>
            private readonly AttributeParameter<SceeList> _attributeAttributeList = new AttributeParameter<SceeList>("Attributes", AttributeAccess.Write);

            /// <summary>
            /// The hashcode of this entity. Useful to define unique ids in the scope of the execution.
            /// </summary>
            private readonly AttributeParameter<int> _attributeHashCode = new AttributeParameter<int>("Hash Code", AttributeAccess.Write);

            /// <summary>
            /// The name of the actual type of this entity.
            /// </summary>
            private readonly AttributeParameter<string> _attributeType = new AttributeParameter<string>("Type", AttributeAccess.Write);

            /// <summary>
            /// Entity from which to read the properties.
            /// </summary>
            private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

            /// <summary>
            /// Entity from which the properties were read.
            /// </summary>
            private readonly Output<IEntity> _output = new Output<IEntity>("Output");



            protected EntityPropertyParameter()
                : base("Entity")
            {
            }



            public override void Run()
            {
                var entity = _input.Read();

                if (_attributeType.IsMapped)
                    _attributeType[entity] = entity.GetType().FullName;

                if (_attributeHashCode.IsMapped)
                    _attributeHashCode[entity] = entity.GetHashCode();

                if (_attributeAttributeList.IsMapped)
                    _attributeAttributeList[entity] = entity.Attributes.ToSceeList();

                _output.Write(entity);
            }
        }

        #endregion

        #region Group Property

        /// <summary>
        /// Reads/calculates properties from group entities.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.PropertyProcedure.PropertyParameter" />
        public class GroupPropertyParameter : PropertyParameter
        {
            /// <summary>
            /// Number of items in this group.
            /// </summary>
            private readonly AttributeParameter<int> _attributeGroupSize = new AttributeParameter<int>("Group Size", AttributeAccess.Write);

            /// <summary>
            /// Group entity from which to read the properties.
            /// </summary>
            private readonly SingleInput<IEntityGroup> _input = new SingleInput<IEntityGroup>("Input");

            /// <summary>
            /// Group entity from which the properties were read.
            /// </summary>
            private readonly Output<IEntityGroup> _output = new Output<IEntityGroup>("Output");



            public GroupPropertyParameter()
                : base("Group")
            {
            }



            public override void Run()
            {
                var group = _input.Read();

                if (_attributeGroupSize.IsMapped)
                    _attributeGroupSize[group] = group.SubEntities.Count();

                _output.Write(group);
            }
        }

        #endregion
    }
}