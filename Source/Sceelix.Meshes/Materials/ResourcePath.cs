namespace Sceelix.Meshes.Materials
{
    public class ResourcePath
    {
        public ResourcePath(string path)
        {
            Path = path;
        }



        public string Path
        {
            get;
            set;
        }



        protected bool Equals(ResourcePath other)
        {
            return string.Equals(Path, other.Path);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ResourcePath) obj);
        }



        public override int GetHashCode()
        {
            return Path != null ? Path.GetHashCode() : 0;
        }



        public static bool operator ==(ResourcePath left, ResourcePath right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(ResourcePath left, ResourcePath right)
        {
            return !Equals(left, right);
        }



        public override string ToString()
        {
            return string.Format("Path: {0}", Path);
        }
    }
}