using System;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Attributes
{
    public class LocalAttributeKey : AttributeKey, ICloneable
    {
        public LocalAttributeKey(string name, Procedure procedure)
            : base(name)
        {
            Procedure = procedure;
        }



        public Procedure Procedure
        {
            get;
        }



        protected bool Equals(LocalAttributeKey other)
        {
            return base.Equals(other) && Equals(Procedure, other.Procedure);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LocalAttributeKey) obj);
        }



        /*public override object Clone()
        {
            var localAttributeKey = new LocalAttributeKey(Name, _procedure);

            if (Meta != null)
                localAttributeKey.Meta = Meta.Select(x => x.Clone()).ToList();

            return localAttributeKey;
        }*/



        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Procedure.GetHashCode();
            }
        }



        public static bool operator ==(LocalAttributeKey left, LocalAttributeKey right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(LocalAttributeKey left, LocalAttributeKey right)
        {
            return !Equals(left, right);
        }
    }
}