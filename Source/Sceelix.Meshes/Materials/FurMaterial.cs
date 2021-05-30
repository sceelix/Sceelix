using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class FurTextureMaterial : MeshMaterial
    {
        private static readonly AttributeKey TextureKey = new GlobalAttributeKey("Texture");
        private static readonly AttributeKey MaxFurLengthKey = new GlobalAttributeKey("MaxFurLength");
        private static readonly AttributeKey FurDensityKey = new GlobalAttributeKey("FurDensity");
        private static readonly AttributeKey SelfShadowStrengthKey = new GlobalAttributeKey("SelfShadowStrength");
        private static readonly AttributeKey JitterMapScaleKey = new GlobalAttributeKey("JitterMapScale");



        public float FurDensity
        {
            get { return this.GetAttribute<float>(FurDensityKey); }
            set { this.SetAttribute(FurDensityKey, value); }
        }



        public float JitterMapScale
        {
            get { return this.GetAttribute<float>(JitterMapScaleKey); }
            set { this.SetAttribute(JitterMapScaleKey, value); }
        }



        public float MaxFurLength
        {
            get { return this.GetAttribute<float>(MaxFurLengthKey); }
            set { this.SetAttribute(MaxFurLengthKey, value); }
        }



        public float SelfShadowStrength
        {
            get { return this.GetAttribute<float>(SelfShadowStrengthKey); }
            set { this.SetAttribute(SelfShadowStrengthKey, value); }
        }



        public string Texture
        {
            get { return this.GetAttribute<TextureSlot>(TextureKey).Path; }
            set { this.SetAttribute(TextureKey, new TextureSlot(value, TextureType.Diffuse)); }
        }
    }
}