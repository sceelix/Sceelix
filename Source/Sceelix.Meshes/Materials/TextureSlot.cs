namespace Sceelix.Meshes.Materials
{
    public enum TextureType
    {
        Ambient,
        Diffuse,
        Displacement,
        Emissive,
        Height,
        Lightmap,
        None,
        Normal,
        Opacity,
        Reflection,
        Shininess,
        Specular,
        Unknown
    }

    public class TextureSlot
    {
        public TextureSlot(string path, TextureType type)
        {
            Path = path;
            Type = type;
        }



        public string Path
        {
            get;
            set;
        }


        public TextureType Type
        {
            get;
            set;
        }



        protected bool Equals(TextureSlot other)
        {
            return string.Equals(Path, other.Path) && Type == other.Type;
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TextureSlot) obj);
        }



        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ (int) Type;
            }
        }



        public static bool operator ==(TextureSlot left, TextureSlot right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(TextureSlot left, TextureSlot right)
        {
            return !Equals(left, right);
        }



        public override string ToString()
        {
            return string.Format("Path: {0}, Type: {1}", Path, Type);
        }
    }
}