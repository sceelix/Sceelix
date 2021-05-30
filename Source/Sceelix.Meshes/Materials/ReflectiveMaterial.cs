using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class ReflectiveMaterial : MeshMaterial
    {
        private static readonly AttributeKey DiffuseTextureKey = new GlobalAttributeKey("DiffuseTexture");
        private static readonly AttributeKey NormalTextureKey = new GlobalAttributeKey("NormalTexture");



        public ReflectiveMaterial(string texture, string normalTexture)
        {
            DiffuseTexture = texture;
            NormalTexture = normalTexture;
        }



        public string DiffuseTexture
        {
            get { return this.GetAttribute<TextureSlot>(DiffuseTextureKey).Path; }
            set { this.SetAttribute(DiffuseTextureKey, new TextureSlot(value, TextureType.Diffuse)); }
        }



        public string NormalTexture
        {
            get { return this.GetAttribute<TextureSlot>(NormalTextureKey).Path; }
            set { this.SetAttribute(NormalTextureKey, new TextureSlot(value, TextureType.Normal)); }
        }
    }
}