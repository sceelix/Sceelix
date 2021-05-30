using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class TextureAndBumpMaterial : MeshMaterial
    {
        private static readonly AttributeKey DiffuseTextureKey = new GlobalAttributeKey("DiffuseTexture");
        private static readonly AttributeKey NormalTextureKey = new GlobalAttributeKey("NormalTexture");
        private static readonly AttributeKey SpecularTextureKey = new GlobalAttributeKey("SpecularTexture");



        public TextureAndBumpMaterial(string diffuseTexture, string normalTexture)
        {
            DiffuseTexture = diffuseTexture;
            NormalTexture = normalTexture;
        }



        public TextureAndBumpMaterial(string diffuseTexture, string normalTexture, string specularTexture)
        {
            DiffuseTexture = diffuseTexture;
            NormalTexture = normalTexture;
            SpecularTexture = specularTexture;
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



        public string SpecularTexture
        {
            get { return this.GetAttribute<TextureSlot>(SpecularTextureKey).Path; }
            set { this.SetAttribute(SpecularTextureKey, new TextureSlot(value, TextureType.Specular)); }
        }
    }
}