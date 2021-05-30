using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    /// <summary>
    /// Why material is generic:
    /// - Easier to create derivatives, because
    ///    - No need to implement equality comparer
    ///    - No need to implement serializer
    /// </summary>
    public class SingleTextureMaterial : MeshMaterial
    {
        private static readonly AttributeKey TextureKey = new GlobalAttributeKey("Texture");



        public SingleTextureMaterial(string texture)
        {
            Texture = texture;
        }



        /*public String Texture
        {
            get { return Get<ResourcePath>("Texture").Path; }
            set { Set("Texture", new ResourcePath(value)); }
        }*/



        public string Texture
        {
            get { return this.GetAttribute<TextureSlot>(TextureKey).Path; }
            set { this.SetAttribute(TextureKey, new TextureSlot(value, TextureType.Diffuse)); }
        }
    }
}