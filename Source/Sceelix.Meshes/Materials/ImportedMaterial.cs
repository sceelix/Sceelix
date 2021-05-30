using System.IO;
using Assimp;
using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Extensions;

namespace Sceelix.Meshes.Materials
{
    public class ImportedMaterial : MeshMaterial
    {
        private static readonly AttributeKey ColorDiffuseKey = new GlobalAttributeKey("ColorDiffuse");
        private static readonly AttributeKey ColorAmbientKey = new GlobalAttributeKey("ColorAmbient");
        private static readonly AttributeKey ColorEmissiveKey = new GlobalAttributeKey("ColorEmissive");
        private static readonly AttributeKey ColorReflectiveKey = new GlobalAttributeKey("ColorReflective");
        private static readonly AttributeKey ColorSpecularKey = new GlobalAttributeKey("ColorSpecular");
        private static readonly AttributeKey ColorTransparentKey = new GlobalAttributeKey("ColorTransparent");

        private static readonly AttributeKey BumpScalingKey = new GlobalAttributeKey("BumpScaling");
        private static readonly AttributeKey ShininessKey = new GlobalAttributeKey("Shininess");
        private static readonly AttributeKey OpacityKey = new GlobalAttributeKey("Opacity");
        private static readonly AttributeKey ReflectivityKey = new GlobalAttributeKey("Reflectivity");
        private static readonly AttributeKey IsWireFrameEnabledKey = new GlobalAttributeKey("IsWireFrameEnabled");
        private static readonly AttributeKey ShininessStrengthKey = new GlobalAttributeKey("ShininessStrength");
        private static readonly AttributeKey ShadingModeKey = new GlobalAttributeKey("ShadingMode");

        private static readonly AttributeKey IsTwoSidedKey = new GlobalAttributeKey("IsTwoSided");
        private static readonly AttributeKey BlendModeKey = new GlobalAttributeKey("BlendMode");


        private static readonly AttributeKey DiffuseTextureKey = new GlobalAttributeKey("DiffuseTexture");
        private static readonly AttributeKey DisplacementTextureKey = new GlobalAttributeKey("DisplacementTexture");
        private static readonly AttributeKey AmbientTextureKey = new GlobalAttributeKey("AmbientTexture");
        private static readonly AttributeKey EmissiveTextureKey = new GlobalAttributeKey("EmissiveTexture");
        private static readonly AttributeKey HeightTextureKey = new GlobalAttributeKey("HeightTexture");
        private static readonly AttributeKey LightMapTextureKey = new GlobalAttributeKey("LightMapTexture");
        private static readonly AttributeKey NormalTextureKey = new GlobalAttributeKey("NormalTexture");
        private static readonly AttributeKey OpacityTextureKey = new GlobalAttributeKey("OpacityTexture");
        private static readonly AttributeKey SpecularTextureKey = new GlobalAttributeKey("SpecularTexture");
        private static readonly AttributeKey ReflectionTextureKey = new GlobalAttributeKey("ReflectionTexture");



        public ImportedMaterial(Material material, string originalDirectory, bool resetAbsolutePaths)
        {
            if (material.HasColorDiffuse)
                ColorDiffuse = material.ColorDiffuse.ToSceelixColor();

            if (material.HasColorAmbient)
                ColorAmbient = material.ColorAmbient.ToSceelixColor();

            if (material.HasColorEmissive)
                ColorEmissive = material.ColorEmissive.ToSceelixColor();

            if (material.HasColorReflective)
                ColorReflective = material.ColorReflective.ToSceelixColor();

            if (material.HasColorSpecular)
                ColorSpecular = material.ColorSpecular.ToSceelixColor();

            if (material.HasColorTransparent)
                ColorTransparent = material.ColorTransparent.ToSceelixColor();

            if (material.HasBumpScaling)
                BumpScaling = material.BumpScaling;

            if (material.HasShininess)
                Shininess = material.Shininess;

            if (material.HasOpacity)
                Opacity = material.Opacity;

            if (material.HasReflectivity)
                Reflectivity = material.Reflectivity;

            if (material.HasWireFrame)
                IsWireFrameEnabled = material.IsWireFrameEnabled;

            if (material.HasShininessStrength)
                ShininessStrength = material.ShininessStrength;

            if (material.HasShadingMode)
                ShadingMode = material.ShadingMode.ToString();

            if (material.HasTwoSided)
                IsTwoSided = material.IsTwoSided;

            if (material.HasBlendMode)
                BlendMode = material.BlendMode.ToString();


            if (material.HasTextureDiffuse)
                DiffuseTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureDiffuse.FilePath, resetAbsolutePaths));

            if (material.HasTextureDisplacement)
                DisplacementTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureDisplacement.FilePath, resetAbsolutePaths));

            if (material.HasTextureAmbient)
                AmbientTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureAmbient.FilePath, resetAbsolutePaths));

            if (material.HasTextureEmissive)
                EmissiveTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureEmissive.FilePath, resetAbsolutePaths));

            if (material.HasTextureHeight)
                HeightTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureHeight.FilePath, resetAbsolutePaths));

            if (material.HasTextureLightMap)
                LightMapTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureLightMap.FilePath, resetAbsolutePaths));

            if (material.HasTextureNormal)
                NormalTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureNormal.FilePath, resetAbsolutePaths));

            if (material.HasTextureOpacity)
                OpacityTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureOpacity.FilePath, resetAbsolutePaths));

            if (material.HasTextureSpecular)
                SpecularTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureSpecular.FilePath, resetAbsolutePaths));

            if (material.HasTextureReflection)
                ReflectionTexturePath = Path.Combine(originalDirectory, UpdatePath(material.TextureReflection.FilePath, resetAbsolutePaths));
        }



        public string AmbientTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(AmbientTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(AmbientTextureKey, new TextureSlot(value, TextureType.Ambient)); }
        }



        public string BlendMode
        {
            get { return this.GetAttribute<string>(BlendModeKey); }
            set { this.SetAttribute(BlendModeKey, value); }
        }



        public float BumpScaling
        {
            get { return this.GetAttribute<float>(BumpScalingKey); }
            set { this.SetAttribute(BumpScalingKey, value); }
        }



        public Color ColorAmbient
        {
            get { return this.GetAttribute<Color>(ColorAmbientKey); }
            set { this.SetAttribute(ColorAmbientKey, value); }
        }



        public Color ColorDiffuse
        {
            get { return this.GetAttribute<Color>(ColorDiffuseKey); }
            set { this.SetAttribute(ColorDiffuseKey, value); }
        }



        public Color ColorEmissive
        {
            get { return this.GetAttribute<Color>(ColorEmissiveKey); }
            set { this.SetAttribute(ColorEmissiveKey, value); }
        }



        public Color ColorReflective
        {
            get { return this.GetAttribute<Color>(ColorReflectiveKey); }
            set { this.SetAttribute(ColorReflectiveKey, value); }
        }



        public Color ColorSpecular
        {
            get { return this.GetAttribute<Color>(ColorSpecularKey); }
            set { this.SetAttribute(ColorSpecularKey, value); }
        }



        public Color ColorTransparent
        {
            get { return this.GetAttribute<Color>(ColorTransparentKey); }
            set { this.SetAttribute(ColorTransparentKey, value); }
        }



        public string DiffuseTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(DiffuseTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(DiffuseTextureKey, new TextureSlot(value, TextureType.Diffuse)); }
        }



        public string DisplacementTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(DisplacementTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(DisplacementTextureKey, new TextureSlot(value, TextureType.Displacement)); }
        }



        public string EmissiveTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(EmissiveTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(EmissiveTextureKey, new TextureSlot(value, TextureType.Emissive)); }
        }



        public bool HasAmbientTexture => this.HasAttribute(AmbientTextureKey);


        public bool HasDiffuseTexture => this.HasAttribute(DiffuseTextureKey);


        public bool HasDisplacementTexture => this.HasAttribute(DisplacementTextureKey);


        public bool HasEmissiveTexture => this.HasAttribute(EmissiveTextureKey);


        public bool HasHeightTexture => this.HasAttribute(HeightTextureKey);


        public bool HasLightMapTexture => this.HasAttribute(LightMapTextureKey);


        public bool HasNormalTexture => this.HasAttribute(NormalTextureKey);


        public bool HasOpacityTexture => this.HasAttribute(OpacityTextureKey);


        public bool HasReflectionTexture => this.HasAttribute(ReflectionTextureKey);


        public bool HasSpecularTexture => this.HasAttribute(SpecularTextureKey);



        public string HeightTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(HeightTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(HeightTextureKey, new TextureSlot(value, TextureType.Height)); }
        }



        public bool IsTwoSided
        {
            get { return this.GetAttribute<bool>(IsTwoSidedKey); }
            set { this.SetAttribute(IsTwoSidedKey, value); }
        }



        public bool IsWireFrameEnabled
        {
            get { return this.GetAttribute<bool>(IsWireFrameEnabledKey); }
            set { this.SetAttribute(IsWireFrameEnabledKey, value); }
        }



        public string LightMapTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(LightMapTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(LightMapTextureKey, new TextureSlot(value, TextureType.Lightmap)); }
        }



        public string NormalTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(NormalTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(NormalTextureKey, new TextureSlot(value, TextureType.Normal)); }
        }



        public float Opacity
        {
            get { return this.GetAttribute<float>(OpacityKey); }
            set { this.SetAttribute(OpacityKey, value); }
        }



        public string OpacityTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(OpacityTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(OpacityTextureKey, new TextureSlot(value, TextureType.Opacity)); }
        }



        public string ReflectionTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(ReflectionTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(ReflectionTextureKey, new TextureSlot(value, TextureType.Reflection)); }
        }



        public float Reflectivity
        {
            get { return this.GetAttribute<float>(ReflectivityKey); }
            set { this.SetAttribute(ReflectivityKey, value); }
        }



        public string ShadingMode
        {
            get { return this.GetAttribute<string>(ShadingModeKey); }
            set { this.SetAttribute(ShadingModeKey, value); }
        }



        public float Shininess
        {
            get { return this.GetAttribute<float>(ShininessKey); }
            set { this.SetAttribute(ShininessKey, value); }
        }



        public float ShininessStrength
        {
            get { return this.GetAttribute<float>(ShininessStrengthKey); }
            set { this.SetAttribute(ShininessStrengthKey, value); }
        }



        public string SpecularTexturePath
        {
            get
            {
                var texture = this.GetAttribute<TextureSlot>(SpecularTextureKey);
                return texture != null ? texture.Path : null;
            }
            set { this.SetAttribute(SpecularTextureKey, new TextureSlot(value, TextureType.Specular)); }
        }



        private string UpdatePath(string filePath, bool resetAbsolutePaths)
        {
            if (resetAbsolutePaths && Path.IsPathRooted(filePath))
                return Path.GetFileName(filePath);

            return filePath;
        }



        /*protected bool Equals(ImportedMaterial other)
        {
            return base.Equals(other) && Equals(_material, other._material);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImportedMaterial) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (_material != null ? _material.GetHashCode() : 0);
            }
        }*/
    }
}