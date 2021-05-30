using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class ParallaxOcclusionMaterial : MeshMaterial
    {
        private static readonly AttributeKey DiffuseTextureKey = new GlobalAttributeKey("DiffuseTexture");
        private static readonly AttributeKey NormalTextureKey = new GlobalAttributeKey("NormalTexture");
        private static readonly AttributeKey SpecularTextureKey = new GlobalAttributeKey("SpecularTexture");
        private static readonly AttributeKey HeightTextureKey = new GlobalAttributeKey("HeightTexture");



        public string DiffuseTexture
        {
            get { return this.GetAttribute<TextureSlot>(DiffuseTextureKey).Path; }
            set { this.SetAttribute(DiffuseTextureKey, new TextureSlot(value, TextureType.Diffuse)); }
        }



        public string HeightTexture
        {
            get { return this.GetAttribute<TextureSlot>(HeightTextureKey).Path; }
            set { this.SetAttribute(HeightTextureKey, new TextureSlot(value, TextureType.Height)); }
        }



        public string NormalTexture
        {
            get { return this.GetAttribute<TextureSlot>(NormalTextureKey).Path; }
            set { this.SetAttribute(NormalTextureKey, new TextureSlot(value, TextureType.Normal)); }
        }



        public string SpecularTexture
        {
            get
            {
                var specularTexture = this.GetAttribute<TextureSlot>(SpecularTextureKey);
                return specularTexture != null ? specularTexture.Path : null;
            }
            set { this.SetAttribute(SpecularTextureKey, new TextureSlot(value, TextureType.Specular)); }
        }
    }
}