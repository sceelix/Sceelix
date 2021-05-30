using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Applies rendering materials to meshes.
    /// </summary>
    [Procedure("ab33c263-352e-4917-a7ec-0686aebc078b", Label = "Mesh Material", Category = "Mesh")]
    public class MeshMaterialProcedure : TransferProcedure<MeshEntity>
    {
        /// <summary>
        /// The type of material to apply to the faces of this mesh.
        /// </summary>
        private readonly SelectListParameter<MeshMaterialParameter> _parameterMaterial = new SelectListParameter<MeshMaterialParameter>("Material", "Color");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterMaterial.SubParameterLabels);



        protected override MeshEntity Process(MeshEntity entity)
        {
            foreach (var meshMaterialParameter in _parameterMaterial.Items) meshMaterialParameter.Apply(entity);

            return entity;
        }



        public abstract class MeshMaterialParameter : CompoundParameter
        {
            protected MeshMaterialParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Apply(MeshEntity entity);
        }

        #region Color

        /// <summary>
        /// Renders the faces with a given color.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class ColorMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// Color to render the faces with. 
            /// </summary>
            private readonly ColorParameter _parameterColor = new ColorParameter("Color", new Color(255, 0, 0));

            /// <summary>
            /// The specular power for lighting calculations (deprecated).
            /// </summary>
            private readonly FloatParameter _parameterSpecularPower = new FloatParameter("Specular Power", 1);



            public ColorMaterialParameter()
                : base("Color")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new ColorMaterial(_parameterColor.Value);
            }
        }

        #endregion

        #region Emissive

        /// <summary>
        /// Renders the faces with an emissive color.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class EmissiveMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// Color to render the faces with. 
            /// </summary>
            private readonly ColorParameter _parameterColor = new ColorParameter("Color", new Color(255, 0, 0));



            public EmissiveMaterialParameter()
                : base("Emissive")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new EmissiveMaterial(_parameterColor.Value);
            }
        }

        #endregion

        #region Texture

        /// <summary>
        /// Renders the faces with a diffuse texture.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class TextureMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The diffuse texture.
            /// </summary>
            private readonly FileParameter _parameterTexture = new FileParameter("Texture", string.Empty, BitmapExtension.SupportedFileExtensions);



            public TextureMaterialParameter()
                : base("Texture")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new SingleTextureMaterial(_parameterTexture.Value);
            }
        }

        #endregion

        #region Transparent

        /// <summary>
        /// Renders the faces with a diffuse texture with configurable transparency.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class TransparentMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The diffuse texture.
            /// </summary>
            private readonly FileParameter _parameterDiffuse = new FileParameter("Texture", string.Empty, BitmapExtension.SupportedFileExtensions);

            /// <summary>
            /// The level of transparency.
            /// </summary>
            private readonly FloatParameter _parameterTransparency = new FloatParameter("Transparency", 0.5f) {MinValue = 0, MaxValue = 1};



            public TransparentMaterialParameter()
                : base("Transparent")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new TransparentMaterial(1 - _parameterTransparency.Value, _parameterDiffuse.Value);
            }
        }

        #endregion

        #region Reflective

        /// <summary>
        /// Renders the faces with a diffuse and normal texture with reflective properties (mirror-like rendering).
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class ReflectiveMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The diffuse texture.
            /// </summary>
            private readonly FileParameter _parameterTexture = new FileParameter("Diffuse Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};

            /// <summary>
            /// The normal/bump texture.
            /// </summary>
            private readonly FileParameter _parameterNormal = new FileParameter("Normal Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};



            public ReflectiveMaterialParameter()
                : base("Reflective")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new ReflectiveMaterial(_parameterTexture.Value, _parameterNormal.Value);
            }
        }

        #endregion

        #region Bump

        /// <summary>
        /// Renders the faces with a bump effect, using a diffuse, normal and specular texture.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class BumpMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The diffuse texture.
            /// </summary>
            private readonly FileParameter _parameterTexture = new FileParameter("Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};

            /// <summary>
            /// The normal texture of the material.
            /// </summary>
            private readonly FileParameter _parameterBumpTexture = new FileParameter("Bump Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};

            /// <summary>
            /// Optional: The specular texture of the material.
            /// </summary>
            private readonly FileParameter _parameterSpecularTexture = new FileParameter("Specular Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};



            public BumpMaterialParameter()
                : base("Bump")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                var textureBumpMaterial = new TextureAndBumpMaterial(_parameterTexture.Value, _parameterBumpTexture.Value);

                //if (!String.IsNullOrWhiteSpace(_parameterSpecularTexture.Value))
                {
                    textureBumpMaterial.SpecularTexture = _parameterSpecularTexture.Value;
                }

                entity.Material = textureBumpMaterial;
            }
        }

        #endregion

        #region Fur

        /// <summary>
        /// Renders the faces with a fur-looking effect to the mesh faces. Useful to simulate grass or otherwise furry materials.
        /// </summary>
        public class FurMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The underlying texture to set on the face.
            /// </summary>
            private readonly FileParameter _parameterTexture = new FileParameter("Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};

            /// <summary>
            /// Density of the fur effect of the material.
            /// </summary>
            private readonly FloatParameter _parameterFurDensity = new FloatParameter("Fur Density", 0.8f);

            /// <summary>
            /// The jitter map scale of the material effect.
            /// </summary>
            private readonly FloatParameter _parameterJitterMapScale = new FloatParameter("Jitter Map Scale", 0.5f);

            /// <summary>
            /// The maximum length of each fur hair.
            /// </summary>
            private readonly FloatParameter _parameterMaxFurLength = new FloatParameter("Max. Fur Length", 0.5f);

            /// <summary>
            /// The strength of the self shadowing effect.
            /// </summary>
            private readonly FloatParameter _parameterSelfShadowStrength = new FloatParameter("Self Shadow Strength", 1f);



            public FurMaterialParameter()
                : base("Fur")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new FurTextureMaterial
                {
                    Texture = _parameterTexture.Value,
                    MaxFurLength = _parameterMaxFurLength.Value,
                    FurDensity = _parameterFurDensity.Value,
                    SelfShadowStrength = _parameterSelfShadowStrength.Value,
                    JitterMapScale = _parameterJitterMapScale.Value
                };
            }
        }

        #endregion

        #region Parallax Occlusion

        /// <summary>
        /// Renders the faces with the parallax occlusion effect, using a diffuse, normal, height and specular texture.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class ParallaxOcclusionMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The diffuse texture.
            /// </summary>
            private readonly FileParameter _parameterDiffuse = new FileParameter("Diffuse", "", BitmapExtension.SupportedFileExtensions);

            /// <summary>
            /// The height texture.
            /// </summary>
            private readonly FileParameter _parameterHeight = new FileParameter("Height", "", BitmapExtension.SupportedFileExtensions);

            /// <summary>
            /// The normal texture.
            /// </summary>
            private readonly FileParameter _parameterNormal = new FileParameter("Normal", "", BitmapExtension.SupportedFileExtensions);

            /// <summary>
            /// Optional: The specular texture. 
            /// </summary>
            private readonly FileParameter _parameterSpecular = new FileParameter("Specular", "", BitmapExtension.SupportedFileExtensions);



            public ParallaxOcclusionMaterialParameter()
                : base("Parallax Occlusion")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                var parallaxOcclusionMaterial = new ParallaxOcclusionMaterial
                {
                    DiffuseTexture = _parameterDiffuse.Value,
                    NormalTexture = _parameterNormal.Value,
                    HeightTexture = _parameterHeight.Value
                };
                if (!string.IsNullOrWhiteSpace(_parameterSpecular.Value))
                    parallaxOcclusionMaterial.SpecularTexture = _parameterSpecular.Value;

                entity.Material = parallaxOcclusionMaterial;

                /*String folderPath = FolderParameter.Value;
                String folderName = Path.GetFileName(folderPath);
                String folderprefix = Path.Combine(folderPath, folderName);

                entity.Material = new ParallaxOcclusionMaterial
                {
                    DiffuseTexture = folderprefix + "_COLOR.png",
                    HeightTexture = folderprefix + "_DISP.png",
                    NormalTexture = folderprefix + "_NRM.png",
                    SpecularTexture = folderprefix + "_SPEC.png"
                };*/
            }
        }

        #endregion

        #region Water

        /// <summary>
        ///  Renders the faces with a flowing water effect.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class WaterMaterialParameter : MeshMaterialParameter
        {
            public WaterMaterialParameter()
                : base("Water")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new WaterMaterial();
            }
        }

        #endregion

        #region Remote

        /// <summary>
        /// Renders the faces with a material that exists on the platform/application/engine where the meshes are deployed.
        /// For example, setting a path like "Assets/Path/To/Material.mat" in an engine like Unity would result in having the
        /// material in that path loaded.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshMaterialProcedure.MeshMaterialParameter" />
        public class RemoteMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// Path to the material in the target platform.
            /// </summary>
            private readonly StringParameter _parameterPath = new StringParameter("Path", string.Empty);



            public RemoteMaterialParameter()
                : base("Remote")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                entity.Material = new RemoteMaterial(PathHelper.ToUniversalPath(_parameterPath.Value));
            }
        }

        #endregion

        #region CustomMaterial

        /// <summary>
        /// A material that can have a different combination of properties, as defined by the platform where the material will be applied.
        /// By default, it will use some sort of reflection/meta approach to match the property values with the given property names. 
        /// </summary>
        public class CustomMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// Name/Path of the shader.
            /// </summary>
            private readonly StringParameter _parameterShaderName = new StringParameter("Shader Name", string.Empty);

            /// <summary>
            /// List of custom properties of the material.
            /// </summary>
            private readonly ListParameter<CustomMaterialPropertyParameter> _parameterPropertyList = new ListParameter<CustomMaterialPropertyParameter>("Properties");



            public CustomMaterialParameter()
                : base("Custom")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                var newMaterial = new CustomMaterial {ShaderName = _parameterShaderName.Value};

                foreach (CustomMaterialPropertyParameter parameter in _parameterPropertyList.Items) newMaterial.SetGlobalAttribute(parameter.Name, parameter.Value);

                entity.Material = newMaterial;
            }



            public abstract class CustomMaterialPropertyParameter : CompoundParameter
            {
                /// <summary>
                /// The property name.
                /// </summary>
                private readonly StringParameter _parameterName = new StringParameter("Name", "PropertyX");



                protected CustomMaterialPropertyParameter(string typeName)
                    : base(typeName)
                {
                }



                public string Name => _parameterName.Value;


                public abstract object Value
                {
                    get;
                }
            }

            /// <summary>
            /// Sets a textual property on the material.
            /// </summary>
            public class StringPropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The string value.
                /// </summary>
                private readonly StringParameter _parameterValue = new StringParameter("Value", "");



                public StringPropertyParameter()
                    : base("String")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets an integer property on the material.
            /// </summary>
            public class IntPropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The integer value.
                /// </summary>
                private readonly IntParameter _parameterValue = new IntParameter("Value", 0);



                public IntPropertyParameter()
                    : base("Int")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a boolean property on the material.
            /// </summary>
            public class BoolPropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The boolean value.
                /// </summary>
                private readonly BoolParameter _parameterValue = new BoolParameter("Value", false);



                public BoolPropertyParameter()
                    : base("Bool")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a floating-point property on the material.
            /// </summary>
            public class FloatPropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The floating-point value.
                /// </summary>
                private readonly FloatParameter _parameterValue = new FloatParameter("Value", 0);



                public FloatPropertyParameter()
                    : base("Float")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a vector2 property on the material.
            /// </summary>
            public class Vector2PropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The vector2 value.
                /// </summary>
                private readonly Vector2DParameter _parameterValue = new Vector2DParameter("Value", Vector2D.Zero);



                public Vector2PropertyParameter()
                    : base("Vector2")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a vector3 property on the material.
            /// </summary>
            public class Vector3PropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The vector3 value.
                /// </summary>
                private readonly Vector3DParameter _parameterValue = new Vector3DParameter("Value", Vector3D.Zero);



                public Vector3PropertyParameter()
                    : base("Vector3")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a color property on the material.
            /// </summary>
            public class ColorPropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The color value.
                /// </summary>
                private readonly ColorParameter _parameterValue = new ColorParameter("Value", Color.White);



                public ColorPropertyParameter()
                    : base("Color")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a texture property on the material.
            /// </summary>
            public class TexturePropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The path to the texture.
                /// </summary>
                private readonly FileParameter _parameterValue = new FileParameter("Path", string.Empty, BitmapExtension.SupportedFileExtensions);

                /// <summary>
                /// Type of texture/semantics.
                /// </summary>
                private readonly EnumChoiceParameter<TextureType> _parameterType = new EnumChoiceParameter<TextureType>("Type", TextureType.Diffuse);



                public TexturePropertyParameter()
                    : base("Texture")
                {
                }



                public override object Value => new TextureSlot(_parameterValue.Value, _parameterType.Value);
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// A material that may not exist by default in the target platform, unless explicitly interpreted/decoded/implement by the user.
        /// </summary>
        public class OtherMaterialParameter : MeshMaterialParameter
        {
            /// <summary>
            /// The material type/name.
            /// </summary>
            private readonly StringParameter _parameterType = new StringParameter("Type", string.Empty);

            /// <summary>
            /// List of properties of the material.
            /// </summary>
            private readonly ListParameter<OtherMaterialPropertyParameter> _parameterPropertyList = new ListParameter<OtherMaterialPropertyParameter>("Properties");



            public OtherMaterialParameter()
                : base("Other")
            {
            }



            protected internal override void Apply(MeshEntity entity)
            {
                var newMaterial = new Material(_parameterType.Value);

                foreach (OtherMaterialPropertyParameter parameter in _parameterPropertyList.Items) newMaterial.SetGlobalAttribute(parameter.Name, parameter.Value);

                entity.Material = newMaterial;
            }



            public abstract class OtherMaterialPropertyParameter : CompoundParameter
            {
                /// <summary>
                /// The property name.
                /// </summary>
                private readonly StringParameter _parameterName = new StringParameter("Name", "PropertyX");



                protected OtherMaterialPropertyParameter(string typeName)
                    : base(typeName)
                {
                }



                public string Name => _parameterName.Value;


                public abstract object Value
                {
                    get;
                }
            }

            /// <summary>
            /// Sets a textural property on the material.
            /// </summary>
            public class StringPropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The string value.
                /// </summary>
                private readonly StringParameter _parameterValue = new StringParameter("Value", "");



                public StringPropertyParameter()
                    : base("String")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets an integer property on the material.
            /// </summary>
            public class IntPropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The integer value.
                /// </summary>
                private readonly IntParameter _parameterValue = new IntParameter("Value", 0);



                public IntPropertyParameter()
                    : base("Int")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a boolean property on the material.
            /// </summary>
            public class BoolPropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The boolean value.
                /// </summary>
                private readonly BoolParameter _parameterValue = new BoolParameter("Value", false);



                public BoolPropertyParameter()
                    : base("Bool")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets an floating-point property on the material.
            /// </summary>
            public class FloatPropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The floating-point value.
                /// </summary>
                private readonly FloatParameter _parameterValue = new FloatParameter("Value", 0);



                public FloatPropertyParameter()
                    : base("Float")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a vector2 property on the material.
            /// </summary>
            public class Vector2PropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The vector2 value.
                /// </summary>
                private readonly Vector2DParameter _parameterValue = new Vector2DParameter("Value", Vector2D.Zero);



                public Vector2PropertyParameter()
                    : base("Vector2")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a vector3 property on the material.
            /// </summary>
            public class Vector3PropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The vector3 value.
                /// </summary>
                private readonly Vector3DParameter _parameterValue = new Vector3DParameter("Value", Vector3D.Zero);



                public Vector3PropertyParameter()
                    : base("Vector3")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a color property on the material.
            /// </summary>
            public class ColorPropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The color value.
                /// </summary>
                private readonly ColorParameter _parameterValue = new ColorParameter("Value", Color.White);



                public ColorPropertyParameter()
                    : base("Color")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a texture property on the material.
            /// </summary>
            public class TexturePropertyParameter : OtherMaterialPropertyParameter
            {
                /// <summary>
                /// The path to the texture.
                /// </summary>
                private readonly FileParameter _parameterValue = new FileParameter("Path", string.Empty, BitmapExtension.SupportedFileExtensions);



                public TexturePropertyParameter()
                    : base("Texture")
                {
                }



                public override object Value => new ResourcePath(_parameterValue.Value);
            }
        }

        #endregion
    }
}