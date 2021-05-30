using System;
using System.Collections.Generic;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Extensions;

namespace Sceelix.Core.Data
{
    /// <summary>
    /// The Sceelix entity is the abstract superclass of all objects that intend to be manipulated by Sceelix procedures.
    /// </summary>
    public class Entity : IEntity
    {
        /// <summary>
        /// Gets the attributes.
        /// </summary>
        public AttributeCollection Attributes
        {
            get;
            private set;
        } = new AttributeCollection();



        /// <summary>
        /// Gets the SubEntities that can be reached from this entity.
        /// </summary>
        /// <value>The sub entity tree.</value>
        public virtual IEnumerable<IEntity> SubEntityTree
        {
            get { yield break; }
        }



        /// <summary>
        /// Performs a deep clone, starting with a MemberwiseClone and then deep cloning the AttributeCollection.
        /// </summary>
        /// <returns>The cloned Entity.</returns>
        public virtual IEntity DeepClone()
        {
            Entity clonedObject = (Entity) MemberwiseClone();

            clonedObject.Attributes = new AttributeCollection();

            Attributes.SetAttributesTo(clonedObject.Attributes);

            return clonedObject;
        }



        /// <summary>
        /// Gets a prettier display name for the given entity type. Does so by getting the EntityAttribute defined on the class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>A string with a prettier display name for the given entity type.</returns>
        public static string GetDisplayName(Type entityType)
        {
            //check if the type itself has any attributes
            var attribute = entityType.GetCustomAttribute<EntityAttribute>();
            if (attribute != null)
                return attribute.Name;

            //if not, check its supertype, but only if it is an entity, too
            if (typeof(IEntity).IsAssignableFrom(entityType.BaseType))
                return GetDisplayName(entityType.BaseType);

            //if it's still not the case, check the interfaces
            foreach (var interfaceType in entityType.GetInterfaces())
                if (typeof(IEntity).IsAssignableFrom(interfaceType))
                    return GetDisplayName(interfaceType);

            //otherwise, just return the class name
            return entityType.Name;
        }



        /// <summary>
        /// Returns the value of the attribute with the given name.
        /// 
        /// Because of its generic implementation, this entity superclass fetches the value from the Attribute Collection.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>Value corresponding to the given attribute name or a SceeList with all the key-value pairs, if the name is empty.</returns>
        /*public virtual Object this[String attributeName]
        {
            get
            {
                return Attributes[AttributeKeyManager.Parse(attributeName)];
                //
                //if (String.IsNullOrWhiteSpace(attributeName))
                //    return new SceeList(Attributes.Select(x => new KeyValuePair<String,Object>(x.Name,x.Value)));

                //return _attributes.Get(attributeName);
            }
            set { _attributes.Set(attributeName,value); }
        }*/
    }
}