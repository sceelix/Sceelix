using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Extensions
{
    public static class EntityExtension
    {
        public static IEntity CreateImpulse(this IEntity sourceEntity)
        {
            var entity = new Entity();

            sourceEntity.Attributes.SetAttributesTo(entity.Attributes);

            return entity;
        }



        public static object GetAttribute(this IEntity sourceEntity, AttributeKey key)
        {
            return sourceEntity.Attributes.TryGet(key);
        }



        public static T GetAttribute<T>(this IEntity sourceEntity, AttributeKey key)
        {
            var value = sourceEntity.Attributes.TryGet(key);
            if (value == null)
                return default(T);

            return (T) value;
        }



        public static object GetAttributeWithMeta<T>(this IEntity sourceEntity, AttributeKey key)
        {
            var entry = sourceEntity.Attributes.TryGetEntry(key);
            if (entry != null && entry.Value.Key.HasMeta<T>())
                return entry.Value.Value;

            return null;
        }



        public static object GetGlobalAttribute(this IEntity sourceEntity, string name)
        {
            return sourceEntity.Attributes.TryGet(new GlobalAttributeKey(name));
        }



        public static T GetGlobalAttribute<T>(this IEntity sourceEntity, string name)
        {
            var value = sourceEntity.Attributes.TryGet(new GlobalAttributeKey(name));
            if (value == null)
                return default(T);

            return (T) value;
        }



        public static object GetLocalAttribute(this IEntity sourceEntity, string name, Procedure procedure)
        {
            return sourceEntity.Attributes.TryGet(new LocalAttributeKey(name, procedure));
        }



        public static T GetLocalAttribute<T>(this IEntity sourceEntity, string name, Procedure procedure)
        {
            var value = sourceEntity.Attributes.TryGet(new LocalAttributeKey(name, procedure));
            if (value == null)
                return default(T);

            return (T) value;
        }



        public static bool HasAttribute(this IEntity sourceEntity, AttributeKey key)
        {
            return sourceEntity.Attributes.TryGet(key) != null;
        }



        public static void SetAttribute(this IEntity sourceEntity, AttributeKey key, object value)
        {
            sourceEntity.Attributes.TrySet(key, value, true);

            //if (key is LocalAttributeKey)
            //    key.CastTo<LocalAttributeKey>().Procedure.HasLocalAttributes = true;
        }



        public static void SetGlobalAttribute(this IEntity sourceEntity, string name, object value)
        {
            sourceEntity.Attributes.TrySet(new GlobalAttributeKey(name), value, true);
        }



        public static void SetLocalAttribute(this IEntity sourceEntity, string name, Procedure procedure, object value)
        {
            sourceEntity.Attributes.TrySet(new LocalAttributeKey(name, procedure), value, true);

            procedure.HasLocalAttributes = true;
        }



        /*public static void SetAttributesTo(this IEntity sourceEntity, IEntity targetEntity)
        {
            if (sourceEntity.Attributes == null)
                return;

            if(targetEntity.Attributes == null)
                targetEntity.Attributes = new AttributeCollection();
            else
                targetEntity.Attributes.Clear();
            

            foreach (KeyValuePair<object, object> entry in sourceEntity.Attributes)
            {
                targetEntity.Attributes.Add(entry.Key.Clone(), entry.Value.Clone());
            }
        }



        /// <summary>
        /// Adds entries from this collection to the the target collection, replacing them if they already exist.
        /// Both names and values are cloned.
        /// </summary>
        /// <param name="targetAttributeCollection"></param>
        public void ReplaceAttributesOn(AttributeCollection targetAttributeCollection)
        {
            foreach (KeyValuePair<object, object> entry in this)
            {
                if (targetAttributeCollection.ContainsKey(entry.Key))
                    targetAttributeCollection[entry.Key] = entry.Value.Clone();
                else
                    targetAttributeCollection.Add(entry.Key.Clone(), entry.Value.Clone());
            }
        }


        /// <summary>
        /// Add those attributes that do not exist in the target. The check is performed by attribute name.
        /// It works like a merge operation. For those attributes whose names already exist in the target collection, the value is not added.
        /// </summary>
        /// <param name="targetAttributeCollection"></param>
        public void ComplementAttributesTo(AttributeCollection targetAttributeCollection)
        {
            foreach (var entry in this)
            {
                if (!targetAttributeCollection.ContainsKey(entry.Key))
                    targetAttributeCollection.Add(entry.Key.Clone(), entry.Value.Clone());
            }
        }

        public void IntersectAttributes(AttributeCollection otherAttributes)
        {
            foreach (var key in this.Keys.ToList())
            {
                if (!otherAttributes.ContainsKey(key))
                    this.Remove(key);
            }
        }

        public Object TryGet(Object key)
        {
            Object value;

            if (this.TryGetValue(key, out value))
                return value;

            return null;
        }

        public void TrySet(Object key, Object value, bool replaceIfExists = false)
        {
            if (this.ContainsKey(key))
            {
                if (replaceIfExists)
                    this[key] = value;
                else
                    throw new Exception("Attribute '" + key + "' already exists.");
            }
            else
                this.Add(key, value);
        }

        public SceeList ToSceeList()
        {
            SceeList list = new SceeList();

            foreach (var attributeEntry in this)
            {
                list.Add(attributeEntry.Key.ToString(), attributeEntry.Value);
            }

            return list;
        }*/
        /// <param name="targetAttributeCollection"></param>
        /// </summary>
        /// Both names and values are cloned.
        /// Deletes all the entries on the target collection and replaces with the ones on this one.

        /// <summary>

        /*public static IEnumerable<KeyValuePair<object, object>> GetAttributesWithMeta(this IEntity sourceEntity)
        {
            sourceEntity.Attributes.Where(x => x.Key)
        }*/
    }
}