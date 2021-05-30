namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    public class ResourceContent
    {
        public ResourceContent(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }



        public byte[] Content
        {
            get;
            set;
        }


        public string Name
        {
            get;
            set;
        }
    }
}