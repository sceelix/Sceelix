using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Applies rendering materials to surfaces.
    /// </summary>
    [Procedure("2ff832c1-bb04-4cc0-9b02-36d32a2c6e8b", Label = "Surface Material", Category = "Surface")]
    public class SurfaceMaterialProcedure : TransferProcedure<SurfaceEntity>
    {
        /// <summary>
        /// The type of material to apply to this surface.
        /// </summary>
        private readonly SelectListParameter<SurfaceMaterialParameter> _parameterSurfaceMaterial = new SelectListParameter<SurfaceMaterialParameter>("Material", "Texture");



        protected override SurfaceEntity Process(SurfaceEntity surface)
        {
            foreach (var surfaceMaterialParameter in _parameterSurfaceMaterial.Items) surfaceMaterialParameter.SetMaterial(surface);

            return surface;
        }



        /*#region CustomSurfaceMaterial

        /// <summary>
        /// A material that can have a different combination of properties, as defined by the platform where the material will be applied.
        /// By default, it will use some sort of reflection/meta approach to match the property values with the given property names. 
        /// </summary>
        public class CustomMaterialParameter : SurfaceMaterialParameter
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



            protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
            {
                var newMaterial = new CustomMaterial { ShaderName = _parameterShaderName.Value };

                foreach (CustomMaterialPropertyParameter parameter in _parameterPropertyList.Items)
                {
                    newMaterial.SetAttribute(parameter.Name, parameter.Value);
                }

                surfaceEntity.Material = newMaterial;
            }



            public abstract class CustomMaterialPropertyParameter : CompoundParameter
            {
                /// <summary>
                /// The property name.
                /// </summary>
                private readonly StringParameter _parameterName = new StringParameter("Name", "PropertyX");



                protected CustomMaterialPropertyParameter(String typeName)
                    : base(typeName)
                {
                }



                public String Name
                {
                    get { return _parameterName.Value; }
                }



                public abstract Object Value
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
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



                public override object Value
                {
                    get { return _parameterValue.Value; }
                }
            }

            /// <summary>
            /// Sets a texture property on the material.
            /// </summary>
            public class TexturePropertyParameter : CustomMaterialPropertyParameter
            {
                /// <summary>
                /// The path to the texture.
                /// </summary>
                private readonly FileParameter _parameterValue = new FileParameter("Path", String.Empty, BitmapExtension.SupportedFileExtensions);

                /// <summary>
                /// Type of texture/semantics.
                /// </summary>
                private readonly EnumChoiceParameter<TextureType> _parameterType = new EnumChoiceParameter<TextureType>("Type", TextureType.Diffuse);




                public TexturePropertyParameter()
                    : base("Texture")
                {
                }



                public override object Value
                {
                    get { return new TextureSlot(_parameterValue.Value, _parameterType.Value); }
                }
            }
        }

        #endregion*/
    }
}