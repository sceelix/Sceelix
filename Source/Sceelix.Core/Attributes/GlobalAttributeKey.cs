using System;
using System.Collections.Generic;

namespace Sceelix.Core.Attributes
{
    public class GlobalAttributeKey : AttributeKey, ICloneable
    {
        public GlobalAttributeKey(string name) : base(name)
        {
        }



        public GlobalAttributeKey(string name, params object[] meta) : base(name, meta)
        {
        }



        public GlobalAttributeKey(string name, IEnumerable<object> meta) : base(name, meta)
        {
        }



        protected bool Equals(GlobalAttributeKey other)
        {
            return base.Equals(other);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GlobalAttributeKey) obj);
        }



        public override int GetHashCode()
        {
            return base.GetHashCode();
        }



        public static bool operator ==(GlobalAttributeKey left, GlobalAttributeKey right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(GlobalAttributeKey left, GlobalAttributeKey right)
        {
            return !Equals(left, right);
        }
    }
}