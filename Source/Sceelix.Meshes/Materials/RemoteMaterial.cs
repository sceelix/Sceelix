using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class RemoteMaterial : MeshMaterial
    {
        private static readonly AttributeKey PathKey = new GlobalAttributeKey("Path");



        public RemoteMaterial(string path)
        {
            Path = path;
        }



        public string Path
        {
            get { return this.GetAttribute<string>(PathKey); }
            set { this.SetAttribute(PathKey, value); }
        }



        /*public override String Reference
        {
            get { return Path; }
        }*/
    }
}