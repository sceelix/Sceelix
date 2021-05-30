using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class TransparentMaterial : MeshMaterial
    {
        private static readonly AttributeKey TransparencyKey = new GlobalAttributeKey("Transparency");
        private static readonly AttributeKey TextureKey = new GlobalAttributeKey("Texture");



        public TransparentMaterial(float transparency, string texture)
        {
            Transparency = transparency;
            Texture = texture;
        }



        public string Texture
        {
            get { return this.GetAttribute<TextureSlot>(TextureKey).Path; }
            set { this.SetAttribute(TextureKey, new TextureSlot(value, TextureType.Diffuse)); }
        }



        public float Transparency
        {
            get { return this.GetAttribute<float>(TransparencyKey); }
            set { this.SetAttribute(TransparencyKey, value); }
        }
    }
}