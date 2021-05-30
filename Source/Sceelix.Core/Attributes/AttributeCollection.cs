using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sceelix.Collections;
using Sceelix.Extensions;

namespace Sceelix.Core.Attributes
{
    //public delegate Object AttributeFetch<T>(AttributeEntry field, String key);
    //public delegate void AttributeSet<T>(AttributeEntry field, String key, Object value);

    public class AttributeCollection : Dictionary<object, object>, ICloneable
    {
        //Because we need to access the FindEntry function in the Dictionary class, we do so by reflection
        private static readonly Func<Dictionary<object, object>, object, int> _findEntryFunc;

        private static readonly Func<Dictionary<object, object>, int, KeyValuePair<object, object>> _getEntryFunc;



        static AttributeCollection()
        {
            _findEntryFunc = GetFindEntryFunc();
            _getEntryFunc = GetEntryFunc();
        }



        public string Hash
        {
            get
            {
                //joints all the keys and values
                var str = string.Join("", this.Select(x => x.Key.ToString() + x.Value));

                //calculates the hash
                return CalculateHash(str).ToString();
            }
        }



        private static ulong CalculateHash(string read)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }

            return hashedValue;
        }



        /*protected bool Equals(AttributeCollection other)
        {
            return this.Count == other.Count && !this.Except(other).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AttributeCollection) obj);
        }

        public override int GetHashCode()
        {
            return this.Count;
        }*/
        public object Clone()
        {
            var clonedCollection = new AttributeCollection();

            SetAttributesTo(clonedCollection);

            return clonedCollection;
        }



        /// <summary>
        /// Add those attributes that do not exist in the target. The check is performed by attribute name.
        /// It works like a merge operation. For those attributes whose names already exist in the target collection, the value is not added.
        /// </summary>
        /// <param name="targetAttributeCollection"></param>
        public void ComplementAttributesTo(AttributeCollection targetAttributeCollection)
        {
            foreach (var entry in this)
                if (!targetAttributeCollection.ContainsKey(entry.Key))
                    targetAttributeCollection.Add(entry.Key.Clone(), entry.Value.Clone());
        }



        public bool ContentEquals(AttributeCollection other)
        {
            return Count == other.Count && !this.Except(other).Any();
        }



        private static Func<Dictionary<object, object>, int, KeyValuePair<object, object>> GetEntryFunc()
        {
            var dictionaryParameter = Expression.Parameter(typeof(Dictionary<object, object>));
            var indexParameter = Expression.Parameter(typeof(int));

            //there seems to be a difference in the naming in the Mono-based Platforms (and depending on the versions!)
            //so we need to test for both options
            var entriesField = typeof(Dictionary<object, object>).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance);
            if (entriesField == null)
                entriesField = typeof(Dictionary<object, object>).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);

            var entriesFieldExpression = Expression.Field(dictionaryParameter, entriesField);
            var arrayExpression = Expression.ArrayAccess(entriesFieldExpression, indexParameter);

            var keyFieldExpression = Expression.Field(arrayExpression, "key");
            var valueFieldExpression = Expression.Field(arrayExpression, "value");

            var constructor = typeof(KeyValuePair<object, object>).GetConstructor(new[] {typeof(object), typeof(object)});
            var newKeyValueExpression = Expression.New(constructor, keyFieldExpression, valueFieldExpression);

            return Expression.Lambda<Func<Dictionary<object, object>, int, KeyValuePair<object, object>>>(newKeyValueExpression, dictionaryParameter, indexParameter).Compile();
        }



        private static Func<Dictionary<object, object>, object, int> GetFindEntryFunc()
        {
            var findEntryMethod = typeof(Dictionary<object, object>).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "FindEntry");
            var dictionaryParameter = Expression.Parameter(typeof(Dictionary<object, object>));
            var keyParameter = Expression.Parameter(typeof(object));

            var methodCallExpression = Expression.Call(dictionaryParameter, findEntryMethod, keyParameter);
            return Expression.Lambda<Func<Dictionary<object, object>, object, int>>(methodCallExpression, dictionaryParameter, keyParameter).Compile();
        }



        public void IntersectAttributes(AttributeCollection otherAttributes)
        {
            foreach (var key in Keys.ToList())
                if (!otherAttributes.ContainsKey(key))
                    Remove(key);
        }



        /// <summary>
        /// Merge the attributes that do not exist in the target. The check is performed by attribute name.
        /// It works like a merge operation. For those attributes whose names already exist in the target collection, the value is not added.
        /// </summary>
        /// <param name="targetAttributeCollection"></param>
        public void MergeAttributesTo(AttributeCollection targetAttributeCollection)
        {
            foreach (var entry in this)
            {
                var targetValue = targetAttributeCollection.TryGet(entry.Key);
                if (targetValue == null)
                    targetAttributeCollection.Add(entry.Key.Clone(), entry.Value.Clone());
                else
                    targetAttributeCollection.TrySet(entry.Key.Clone(), entry.Value.MergeWith(targetValue), true);
            }
        }



        public IEnumerable<KeyValuePair<T, object>> OfKeyType<T>()
        {
            foreach (KeyValuePair<object, object> keyValuePair in this)
                if (keyValuePair.Key is T)
                    //yield return (KeyValuePair<T, object>) keyValuePair;
                    yield return new KeyValuePair<T, object>((T) keyValuePair.Key, keyValuePair.Value);
        }



        /// <summary>
        /// Adds entries from this collection to the the target collection, replacing them if they already exist.
        /// Both names and values are cloned.
        /// </summary>
        /// <param name="targetAttributeCollection"></param>
        public void ReplaceAttributesOn(AttributeCollection targetAttributeCollection)
        {
            foreach (KeyValuePair<object, object> entry in this)
                if (targetAttributeCollection.ContainsKey(entry.Key))
                    targetAttributeCollection[entry.Key] = entry.Value.Clone();
                else
                    targetAttributeCollection.Add(entry.Key.Clone(), entry.Value.Clone());
        }



        /// <summary>
        /// Deletes all the entries on the target collection and replaces with the ones on this one.
        /// Both names and values are cloned.
        /// </summary>
        /// <param name="targetAttributeCollection"></param>
        public void SetAttributesTo(AttributeCollection targetAttributeCollection)
        {
            targetAttributeCollection.Clear();

            foreach (KeyValuePair<object, object> entry in this) targetAttributeCollection.Add(entry.Key.Clone(), entry.Value.Clone());
        }



        public SceeList ToSceeList()
        {
            SceeList list = new SceeList();

            foreach (var attributeEntry in this) list.Add(attributeEntry.Key.ToString(), attributeEntry.Value);

            return list;
        }



        /*public new Object this[Object key]
        {
            get
            {
                try
                {
                    return this[key];
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException(String.Format("An attribute with the name '{0}' does not exist in one of more input entities. Use the ? operator after the attribute name to bypass this verification and use the default 'null' value.", name));
                }

            }
            set
            {
                base[key] = value;
            }
        }*/



        /// <summary>
        /// Tries to get the value that corresponds to the given key. Returns null if none exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The corresponding value for the given key, or null if none exists.</returns>
        public object TryGet(object key)
        {
            object value;

            if (TryGetValue(key, out value))
                return value;

            return null;
        }



        public KeyValuePair<object, object>? TryGetEntry(object key)
        {
            var index = _findEntryFunc(this, key);
            if (index >= 0) return _getEntryFunc(this, index);

            return null;
        }



        public KeyValuePair<T, object>? TryGetEntry<T>(T key)
        {
            var index = _findEntryFunc(this, key);
            if (index >= 0)
            {
                var keyValuePair = _getEntryFunc(this, index);
                return new KeyValuePair<T, object>((T) keyValuePair.Key, keyValuePair.Value);
            }

            return null;
        }



        public void TrySet(object key, object value, bool replaceIfExists = false)
        {
            if (ContainsKey(key))
            {
                if (replaceIfExists)
                {
                    //the key may be identical for query purposes, but not exactly the same
                    //((i.e. could contain extra meta, for instance)
                    Remove(key);
                    this[key] = value;
                }
                else
                {
                    throw new Exception("Attribute '" + key + "' already exists.");
                }
            }
            else
            {
                Add(key, value);
            }
        }
    }
}