using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Extensions;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Performs attribute manipulation on entities, between entities and within subentities.
    /// This procedure includes different operations that handle attributes. Some, such as the "Set" include subparameters
    /// with further customization and inputs/outputs. 
    /// </summary>
    [Procedure("b3ee6334-f7cb-435c-ab3b-3802fc835e0a", Label = "Attribute", Category = "Basic")]
    public class AttributeProcedure : SystemProcedure
    {
        /// <summary>
        /// Type of attribute operation to perform.
        /// </summary>
        private readonly SelectListParameter<AbstractAttributeParameter> _parameterAttributeOperation = new SelectListParameter<AbstractAttributeParameter>("Operations", "Set");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterAttributeOperation.SubParameterLabels);



        protected override void Run()
        {
            var selectedItem = _parameterAttributeOperation.Items.FirstOrDefault();
            if (selectedItem != null)
                selectedItem.Run();
        }



        #region Abstract Parameter

        public abstract class AbstractAttributeParameter : CompoundParameter
        {
            protected AbstractAttributeParameter(string label)
                : base(label)
            {
            }



            //public abstract void Run(List<List<IEntity>> entitiesLists);

            internal abstract void Run();
        }

        #endregion

        #region Set Attribute

        /// <summary>
        /// Creates/changes attribute values on entities. Source values and destination attributes can be determined and many different ways.
        /// For instance, choosing Origin="Entity" and Destination="Self", an entity can set its own attributes from fixed values or expressions.
        /// Yet by choosing Origin="Entity Collection" and Destination="Others", it is possible to grab a list of values, aggregate them and store
        /// them in attributes of other entities.
        /// </summary>
        public class SetAttributeParameter : AbstractAttributeParameter
        {
            /// <summary>
            /// The entity/entities that define the source values to be set. 
            /// </summary>
            private readonly SelectListParameter<OriginParameter> _parameterOrigin = new SelectListParameter<OriginParameter>("Origin", "Entity");



            protected SetAttributeParameter()
                : base("Set")
            {
            }



            internal override void Run()
            {
                foreach (var setAttributeOriginParameter in _parameterOrigin.Items)
                    setAttributeOriginParameter.Run();
            }



            public abstract class OriginParameter : CompoundParameter
            {
                protected OriginParameter(string label)
                    : base(label)
                {
                }



                protected internal abstract void Run();
            }


            /// <summary>
            /// Entity from which the value will be copied.
            /// </summary>
            public class EntityOriginParameter : OriginParameter
            {
                /// <summary>
                /// Entity that holds attribute values to be read.
                /// </summary>
                private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

                /// <summary>
                /// Entity that holds the attribute values that were read from or written to.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");

                /// <summary>
                /// Value to be set. Can be set as an expression involving fixed values, graph parameters or input entity attributes.
                /// </summary>
                private readonly ObjectParameter _parameterValue = new ObjectParameter("Value");


                /// <summary>
                /// The entity/entities where the attributes are to be set. 
                /// </summary>
                private readonly SelectListParameter _parameterDestination = new SelectListParameter("Destination",
                    () => new SelfParameter("Self") {Description = "The attribute will be set on the source entity."},
                    () => new OtherParameter {Description = "The attribute will be set on a different entity."},
                    () => new OthersParameter {Description = "The attribute will be set on other entities."});



                public EntityOriginParameter()
                    : base("Entity")
                {
                    _parameterDestination.Set("Self");
                }



                protected internal override void Run()
                {
                    var entity = _input.Read();
                    var value = _parameterValue.Get(entity);

                    var item = _parameterDestination.Items.FirstOrDefault();
                    if (item is SelfParameter)
                    {
                        item.CastTo<SelfParameter>().SetAttribute(entity, value);
                    }
                    else if (item is OtherParameter)
                    {
                        item.CastTo<OtherParameter>().SetAttribute(value);
                    }
                    else if (item is OthersParameter)
                    {
                        item.CastTo<OthersParameter>().SetAttribute(value);
                    }

                    _output.Write(entity);
                }
            }


            /// <summary>
            /// Entity collection from which the value will be copied.
            /// </summary>
            public class EntityCollectionOriginParameter : OriginParameter
            {
                /// <summary>
                /// Entities that hold the attribute values to be read.
                /// </summary>
                private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

                /// <summary>
                /// Entities that hold the attribute values that were read from or written to.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");

                /// <summary>
                /// Value to be set. Can be set as an expression involving fixed values, graph parameters or input entity attributes.
                /// </summary>
                private readonly ObjectParameter _parameterValue = new ObjectParameter("Value");

                /// <summary>
                /// The entity/entities where the attributes are to be set. 
                /// </summary>
                private readonly SelectListParameter _parameterDestination = new SelectListParameter("Destination",
                    () => new SelfParameter("Selves") {Description = "The attribute will be set on the source entities."},
                    () => new OtherParameter {Description = "The attribute will be set on a different entity."},
                    () => new OthersParameter {Description = "The attribute will be set on other entities."});



                public EntityCollectionOriginParameter()
                    : base("Entity Collection")
                {
                    _parameterDestination.Set("Selves");
                }



                protected internal override void Run()
                {
                    var entities = _input.Read().ToList();

                    var value = _parameterValue.Value;

                    var item = _parameterDestination.Items.FirstOrDefault();
                    if (item is SelfParameter)
                    {
                        foreach (IEntity entity in entities)
                            item.CastTo<SelfParameter>().SetAttribute(entity, value);
                    }
                    else if (item is OtherParameter)
                    {
                        item.CastTo<OtherParameter>().SetAttribute(value);
                    }
                    else if (item is OthersParameter)
                    {
                        item.CastTo<OthersParameter>().SetAttribute(value);
                    }

                    _output.Write(entities);
                }
            }

            /// <summary>
            /// The entity on which the value will be stored is the same as the origin.
            /// </summary>
            public class SelfParameter : CompoundParameter
            {
                /// <summary>
                /// The attribute to be set.
                /// </summary>
                private readonly AttributeParameter<object> _parameterDestination = new AttributeParameter<object>("Attribute", AttributeAccess.Write);



                public SelfParameter(string label)
                    : base(label)
                {
                }



                public void SetAttribute(IEntity entity, object value)
                {
                    _parameterDestination[entity] = value;
                }
            }

            /// <summary>
            /// Entity on which the value will be stored.
            /// </summary>
            public class OtherParameter : CompoundParameter
            {
                /// <summary>
                /// The entity whose attribute is to be set.
                /// </summary>
                private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

                /// <summary>
                /// The entity whose attribute was set.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");

                /// <summary>
                /// The attribute to be set.
                /// </summary>
                private readonly AttributeParameter<object> _destinationParameter = new AttributeParameter<object>("Attribute", AttributeAccess.Write);



                public OtherParameter()
                    : base("Other")
                {
                }



                public void SetAttribute(object value)
                {
                    var otherEntity = _input.Read();

                    _destinationParameter[otherEntity] = value;

                    _output.Write(otherEntity);
                }
            }


            /// <summary>
            /// Entity collection on which the value will be stored.
            /// </summary>
            public class OthersParameter : CompoundParameter
            {
                /// <summary>
                /// The entities whose attribute is to be set.
                /// </summary>
                private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

                /// <summary>
                /// The entities whose attribute was set.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");

                /// <summary>
                /// The attribute to be set.
                /// </summary>
                private readonly AttributeParameter<object> _parameterDestination = new AttributeParameter<object>("Attribute", AttributeAccess.Write);



                public OthersParameter()
                    : base("Others")
                {
                }



                public void SetAttribute(object value)
                {
                    var otherEntities = _input.Read().ToList();

                    foreach (IEntity otherEntity in otherEntities)
                    {
                        _parameterDestination[otherEntity] = value;
                    }

                    _output.Write(otherEntities);
                }
            }
        }

        #endregion

        #region Copy All Attributes

        /// <summary>
        /// Copies all attributes between a source entity/entities and a target entity/entities.
        /// </summary>
        public class CopyAttributesParameter : AbstractAttributeParameter
        {
            /// <summary>
            /// The entity/entities with the attributes to be copied.
            /// </summary>
            private readonly SelectListParameter<OriginParameter> _parameterOrigin = new SelectListParameter<OriginParameter>("Origin", "Entity");



            protected CopyAttributesParameter()
                : base("Copy All")
            {
            }



            internal override void Run()
            {
                foreach (var setAttributeOriginParameter in _parameterOrigin.Items)
                    setAttributeOriginParameter.Run();
            }



            public abstract class OriginParameter : CompoundParameter
            {
                protected OriginParameter(string label)
                    : base(label)
                {
                }



                protected internal abstract void Run();
            }


            /// <summary>
            /// Single entity from which the attributes will be copied.
            /// </summary>
            public class EntityOriginParameter : OriginParameter
            {
                /// <summary>
                /// Entity from which the attributes will be copied.
                /// </summary>
                private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

                /// <summary>
                /// Entity from which the attributes were copied.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");

                /// <summary>
                /// The entity/entities where the attributes are to be copied to. 
                /// </summary>
                private readonly SelectListParameter _parameterDestination = new SelectListParameter("Destination",
                    () => new OtherParameter {Description = "The attributes will be copied to another entity."},
                    () => new OthersParameter {Description = "The attributes will be copied other entities."});



                public EntityOriginParameter()
                    : base("Entity")
                {
                    _parameterDestination.Set("Other");
                }



                protected internal override void Run()
                {
                    var entity = _input.Read();
                    //var value = _parameterValue.Get(entity);

                    var item = _parameterDestination.Items.FirstOrDefault();
                    if (item is OtherParameter)
                    {
                        item.CastTo<OtherParameter>().CopyAttributes(entity.Attributes);
                    }
                    else if (item is OthersParameter)
                    {
                        item.CastTo<OthersParameter>().CopyAttributes(entity.Attributes);
                    }

                    _output.Write(entity);
                }
            }

            /// <summary>
            /// Entity collection from which the attributes will be copied.
            /// </summary>
            public class EntityCollectionOriginParameter : OriginParameter
            {
                /// <summary>
                /// Entities from which the attributes will be copied.
                /// </summary>
                private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

                /// <summary>
                /// Entities from which the attributes were copied.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");

                /// <summary>
                /// The entity/entities where the attributes are to be copied to. 
                /// </summary>
                private readonly SelectListParameter _parameterDestination = new SelectListParameter("Destination",
                    () => new OtherParameter {Description = "The attributes will be copied to another entity."},
                    () => new OthersParameter {Description = "The attributes will be copied other entities."});



                public EntityCollectionOriginParameter()
                    : base("Entity Collection")
                {
                    _parameterDestination.Set("Other");
                }



                protected internal override void Run()
                {
                    var entities = _input.Read().ToList();

                    var item = _parameterDestination.Items.FirstOrDefault();
                    if (item is OtherParameter)
                    {
                        foreach (IEntity entity in entities)
                            item.CastTo<OtherParameter>().CopyAttributes(entity.Attributes);
                    }
                    else if (item is OthersParameter)
                    {
                        foreach (IEntity entity in entities)
                            item.CastTo<OthersParameter>().CopyAttributes(entity.Attributes);
                    }

                    _output.Write(entities);
                }
            }


            /// <summary>
            /// Entity to which the attributes will be copied.
            /// </summary>
            public class OtherParameter : CompoundParameter
            {
                /// <summary>
                /// Defines how the attributes should be copied. <br/>
                /// <b>Set</b> means that the whole attribute list on the target will be erased and made equal to the source. <br/>
                /// <b>Replace</b> means that items that exist on the target already will have their values replaced, while the others will keep existing. <br/>
                /// <b>Complement</b> means that only the attributes that don't exist on the target will be copied.<br/>
                /// </summary>
                private readonly ChoiceParameter _parameterChoice = new ChoiceParameter("Mode", "Complement", "Set", "Replace", "Complement");

                /// <summary>
                /// Entity to which the attributes will be copied.
                /// </summary>
                private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

                /// <summary>
                /// Entity to which the attributes were copied.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");



                public OtherParameter()
                    : base("Other")
                {
                }



                public void CopyAttributes(AttributeCollection sourceCollection)
                {
                    var otherEntity = _input.Read();

                    if (_parameterChoice.Value == "Set")
                    {
                        sourceCollection.SetAttributesTo(otherEntity.Attributes);
                    }
                    else if (_parameterChoice.Value == "Replace")
                    {
                        sourceCollection.ReplaceAttributesOn(otherEntity.Attributes);
                    }
                    else if (_parameterChoice.Value == "Complement")
                    {
                        sourceCollection.ComplementAttributesTo(otherEntity.Attributes);
                    }

                    _output.Write(otherEntity);
                }
            }


            /// <summary>
            /// Entity collection to which the attributes will be copied.
            /// </summary>
            public class OthersParameter : CompoundParameter
            {
                /// <summary>
                /// Defines how the attributes should be copied. <br/>
                /// <b>Set</b> means that the whole attribute list on the target will be erased and made equal to the source. <br/>
                /// <b>Replace</b> means that items that exist on the target already will have their values replaced, while the others will keep existing. <br/>
                /// <b>Complement</b> means that only the attributes that don't exist on the target will be copied.
                /// </summary>
                private readonly ChoiceParameter _parameterChoice = new ChoiceParameter("Mode", "Complement", "Set", "Replace", "Complement");

                /// <summary>
                /// Entities to which the attributes will be copied.
                /// </summary>
                private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

                /// <summary>
                /// Entities to which the attributes were copied.
                /// </summary>
                private readonly Output<IEntity> _output = new Output<IEntity>("Output");



                public OthersParameter()
                    : base("Others")
                {
                }



                public void CopyAttributes(AttributeCollection sourceCollection)
                {
                    var otherEntities = _input.Read().ToList();

                    if (_parameterChoice.Value == "Set")
                    {
                        foreach (IEntity otherEntity in otherEntities)
                            sourceCollection.SetAttributesTo(otherEntity.Attributes);
                    }
                    else if (_parameterChoice.Value == "Replace")
                    {
                        foreach (IEntity otherEntity in otherEntities)
                            sourceCollection.ReplaceAttributesOn(otherEntity.Attributes);
                    }
                    else if (_parameterChoice.Value == "Complement")
                    {
                        foreach (IEntity otherEntity in otherEntities)
                            sourceCollection.ComplementAttributesTo(otherEntity.Attributes);
                    }

                    _output.Write(otherEntities);
                }
            }
        }

        #endregion

        #region Delete Attribute

        /// <summary>
        /// Deletes a certain attribute from an entity.
        /// </summary>
        public class DeleteAttributeParameter : AbstractAttributeParameter
        {
            /// <summary>
            /// The entity whose attribute is to be deleted.
            /// </summary>
            private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

            /// <summary>
            /// The entity whose attribute was deleted.
            /// </summary>
            private readonly Output<IEntity> _output = new Output<IEntity>("Output");

            /// <summary>
            /// Attribute to delete.
            /// </summary>
            private readonly AttributeParameter<object> _attributeParameter = new AttributeParameter<object>("Attribute", AttributeAccess.Write);



            protected DeleteAttributeParameter()
                : base("Delete")
            {
            }



            internal override void Run()
            {
                var entity = _input.Read();

                if (_attributeParameter.IsMapped)
                {
                    entity.Attributes.Remove(_attributeParameter.AttributeKey);
                }
                /*if (!String.IsNullOrWhiteSpace(_attributeParameter.Value))
                {
                    
                }*/

                _output.Write(entity);
            }
        }

        #endregion

        #region Delete All

        /// <summary>
        /// Deletes all attributes from an entity.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Procedures.AttributeProcedure.AbstractAttributeParameter" />
        public class DeleteAllAttributesParameter : AbstractAttributeParameter
        {
            /// <summary>
            /// The entity whose attributes are to be deleted.
            /// </summary>
            private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

            /// <summary>
            /// The entity whose attributes were deleted.
            /// </summary>
            private readonly Output<IEntity> _output = new Output<IEntity>("Output");



            protected DeleteAllAttributesParameter()
                : base("Delete All")
            {
            }



            internal override void Run()
            {
                var entity = _input.Read();

                foreach (AttributeKey attributeKey in entity.Attributes.Keys.OfType<AttributeKey>().ToList())
                    entity.Attributes.Remove(attributeKey);

                _output.Write(entity);
            }
        }

        #endregion
    }
}