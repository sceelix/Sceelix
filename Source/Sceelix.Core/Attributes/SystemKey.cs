namespace Sceelix.Core.Attributes
{
    public class SystemKey
    {
        public SystemKey(string name)
        {
            Name = name;
        }



        public string Name
        {
            get;
        }



        protected bool Equals(SystemKey other)
        {
            return string.Equals(Name, other.Name);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SystemKey) obj);
        }



        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }



        public static bool operator ==(SystemKey left, SystemKey right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(SystemKey left, SystemKey right)
        {
            return !Equals(left, right);
        }



        public override string ToString()
        {
            return Name;
        }
    }
}