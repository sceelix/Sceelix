using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Extensions;

namespace Sceelix.Core.Attributes
{
    public abstract class AttributeKey : ICloneable
    {
        public AttributeKey(string name)
        {
            Name = name;
        }



        public AttributeKey(string name, params object[] meta)
        {
            Name = name;
            Meta = meta.ToList();
        }



        public AttributeKey(string name, IEnumerable<object> meta)
        {
            Name = name;
            Meta = meta.ToList();
        }



        public List<object> Meta
        {
            get;
            set;
        }


        public string Name
        {
            get;
        }



        public virtual object Clone()
        {
            var key = (AttributeKey) MemberwiseClone();
            if (Meta != null)
                key.Meta = Meta.Select(x => x.Clone()).ToList();

            return key;
        }



        protected bool Equals(AttributeKey other)
        {
            return string.Equals(Name, other.Name);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AttributeKey) obj);
        }



        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }



        public T GetMeta<T>()
        {
            if (Meta != null)
                return Meta.OfType<T>().FirstOrDefault();

            return default(T);
        }



        public bool HasMeta<T>()
        {
            if (Meta != null)
                return Meta.OfType<T>().Any();

            return false;
        }



        public static bool operator ==(AttributeKey left, AttributeKey right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(AttributeKey left, AttributeKey right)
        {
            return !Equals(left, right);
        }



        public override string ToString()
        {
            return Name;
        }
    }
}