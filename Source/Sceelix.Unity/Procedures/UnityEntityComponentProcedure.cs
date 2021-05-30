using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Procedures
{
    /// <summary>
    /// Adds Physical or Behavioral Unity Components to Unity Entities.
    /// </summary>
    [Procedure("f4ddf1f0-64b2-4846-84fd-b919b480c492", Label = "Unity Entity Component", Category = "Unity")]
    public class UnityEntityComponentProcedure : TransferProcedure<UnityEntity>
    {
        /// <summary>
        /// The components to add to the Unity Entity.
        /// </summary>
        private readonly ListParameter<UnityComponentParameter> _parameterUnityComponents = new ListParameter<UnityComponentParameter>("Components");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterUnityComponents.SubParameterLabels);



        protected override UnityEntity Process(UnityEntity entity)
        {
            foreach (var unityComponentParameter in _parameterUnityComponents.Items)
                unityComponentParameter.AddComponent(entity);

            return entity;
        }



        #region Abstract Parameter

        public abstract class UnityComponentParameter : CompoundParameter
        {
            protected UnityComponentParameter(string label)
                : base(label)
            {
            }



            public abstract void AddComponent(UnityEntity entity);
        }

        #endregion

        #region Light

        /// <summary>
        /// Adds a Unity light component to the game object.
        /// </summary>
        /// <seealso cref="UnityComponentParameter" />
        public class LightComponentParameter : UnityComponentParameter
        {
            /// <summary>
            /// The type of light, as interpreted by the Unity Engine.
            /// 
            /// </summary>
            private readonly ChoiceParameter _parameterType = new ChoiceParameter("Type", "Directional", "Area", "Point", "Spot", "Directional");

            /// <summary>
            /// Color of the light.
            /// </summary>
            private readonly ColorParameter _parameterColor = new ColorParameter("Color", Color.White);

            /// <summary>
            /// The light intensity, which is multiplied with the color.
            /// </summary>
            private readonly FloatParameter _parameterIntensity = new FloatParameter("Intensity", 1f);

            /// <summary>
            /// The multiplier that defines the strength of the bounce lighting.
            /// </summary>
            private readonly FloatParameter _parameterBounceIntensity = new FloatParameter("Bounce Intensity", 1f);


            /// <summary>
            /// The shadow type, as interpreted by the Unity engine.
            /// </summary>
            private readonly ChoiceParameter _parameterShadows = new ChoiceParameter("Shadow Type", "Soft", "None", "Soft", "Hard");

            /// <summary>
            /// The range of the light.
            /// </summary>
            private readonly FloatParameter _parameterRange = new FloatParameter("Range", 10);

            /// <summary>
            /// The render mode, as interpreted by the Unity engine.
            /// </summary>
            private readonly ChoiceParameter _parameterRenderMode = new ChoiceParameter("Render Mode", "Auto", "Auto", "ForcePixel", "ForceVertex");



            protected LightComponentParameter()
                : base("Light")
            {
            }



            public override void AddComponent(UnityEntity entity)
            {
                UnityComponent component = new UnityComponent("Light");

                component.SetGlobalAttribute("LightType", _parameterType.Value);
                component.SetGlobalAttribute("Range", _parameterRange.Value);
                component.SetGlobalAttribute("Color", _parameterColor.Value);
                component.SetGlobalAttribute("Intensity", _parameterIntensity.Value);
                component.SetGlobalAttribute("Bounce Intensity", _parameterBounceIntensity.Value);
                component.SetGlobalAttribute("Render Mode", _parameterRenderMode.Value);
                component.SetGlobalAttribute("Shadow Type", _parameterShadows.Value);

                entity.GameComponents.Add(component);
            }
        }

        #endregion

        #region Camera

        /// <summary>
        /// Adds a Unity Camera component to the game object.
        /// </summary>
        /// <seealso cref="UnityComponentParameter" />
        public class CameraComponentParameter : UnityComponentParameter
        {
            /// <summary>
            /// How the camera clears the background.
            /// </summary>
            private readonly ChoiceParameter _parameterClearFlags = new ChoiceParameter("Clear Flags", "Skybox", "Color", "Depth", "Nothing", "Skybox", "SolidColor");

            /// <summary>
            /// The background color.
            /// </summary>
            private readonly ColorParameter _parameterBackground = new ColorParameter("Background", new Color(49, 77, 121, 255));

            /// <summary>
            /// The parameter field of view
            /// </summary>
            private readonly FloatParameter _parameterFieldOfView = new FloatParameter("Field of View", 60);

            /// <summary>
            /// The camera clipping planes.
            /// </summary>
            private readonly CompoundParameter _parameterClippingPlane = new CompoundParameter("Clipping Plane",
                new FloatParameter("Near", 0.3f) {Description = "The near clipping plane distance."},
                new FloatParameter("Far", 1000) {Description = "The far clipping plane distance."});

            /// <summary>
            /// Camera's depth in the camera rendering order.
            /// </summary>
            private readonly FloatParameter _parameterDepth = new FloatParameter("Depth", 0);

            /// <summary>
            /// The rendering path that should be used, if possible.
            /// </summary>
            private readonly ChoiceParameter _parameterRenderingPath = new ChoiceParameter("Rendering Path", "UsePlayerSettings", "DeferredLighting", "DeferredShading", "Forward", "UsePlayerSettings", "VertexLit");

            /// <summary>
            /// Toggles occlusion culling.
            /// </summary>
            private readonly BoolParameter _parameterOcclusionCulling = new BoolParameter("Occlusion Culling", true);


            /// <summary>
            /// Toggles High dynamic range rendering.
            /// </summary>
            private readonly BoolParameter _parameterHdr = new BoolParameter("HDR", false);



            protected CameraComponentParameter()
                : base("Camera")
            {
            }



            public override void AddComponent(UnityEntity entity)
            {
                UnityComponent component = new UnityComponent("Camera");

                component.SetGlobalAttribute("Clear Flags", _parameterClearFlags.Value);
                component.SetGlobalAttribute("Background", _parameterBackground.Value);

                component.SetGlobalAttribute("Field of View", _parameterFieldOfView.Value);
                component.SetGlobalAttribute("Clipping Plane (Near)", _parameterClippingPlane["Near"].Get());
                component.SetGlobalAttribute("Clipping Plane (Far)", _parameterClippingPlane["Far"].Get());

                component.SetGlobalAttribute("Depth", _parameterDepth.Value);

                component.SetGlobalAttribute("Rendering Path", _parameterRenderingPath.Value);
                component.SetGlobalAttribute("Occlusion Culling", _parameterOcclusionCulling.Value);
                component.SetGlobalAttribute("HDR", _parameterHdr.Value);

                entity.GameComponents.Add(component);
            }
        }

        #endregion

        #region RigidBody

        /// <summary>
        /// Adds a Unity Rigidbody component to the game object.
        /// </summary>
        /// <seealso cref="UnityComponentParameter" />
        public class RigidBodyComponentParameter : UnityComponentParameter
        {
            /// <summary>
            /// The mass of the rigidbody.
            /// </summary>
            private readonly FloatParameter _parameterMass = new FloatParameter("Mass", 1);

            /// <summary>
            /// The drag of the object.
            /// </summary>
            private readonly FloatParameter _parameterDrag = new FloatParameter("Drag", 0);

            /// <summary>
            /// The angular drag of the object.
            /// </summary>
            private readonly FloatParameter _parameterAngularDrag = new FloatParameter("Angular Drag", 0.05f);

            /// <summary>
            /// Controls whether gravity affects this rigidbody.
            /// </summary>
            private readonly BoolParameter _parameterUseGravity = new BoolParameter("Use Gravity", true);

            /// <summary>
            /// Controls whether physics affects the rigidbody.
            /// </summary>
            private readonly BoolParameter _parameterIsKinematic = new BoolParameter("Is Kinematic", false);

            /// <summary>
            /// Allows smoothing out the effect of running physics at a fixed frame rate.
            /// </summary>
            private readonly ChoiceParameter _parameterInterpolation = new ChoiceParameter("Interpolate", "None", "Extrapolate", "Interpolate", "None");

            /// <summary>
            /// The Rigidbody's collision detection mode.
            /// </summary>
            private readonly ChoiceParameter _parameterCollisionDetectionMode = new ChoiceParameter("Collision Detection", "Discrete", "Continuous", "ContinuousDynamic", "Discrete");



            public RigidBodyComponentParameter()
                : base("Rigidbody")
            {
            }



            public override void AddComponent(UnityEntity entity)
            {
                UnityComponent component = new UnityComponent("RigidBody");

                component.SetGlobalAttribute("Mass", _parameterMass.Value);
                component.SetGlobalAttribute("Drag", _parameterDrag.Value);
                component.SetGlobalAttribute("Angular Drag", _parameterAngularDrag.Value);
                component.SetGlobalAttribute("Use Gravity", _parameterUseGravity.Value);
                component.SetGlobalAttribute("Is Kinematic", _parameterIsKinematic.Value);
                component.SetGlobalAttribute("Interpolate", _parameterInterpolation.Value);
                component.SetGlobalAttribute("Collision Detection", _parameterCollisionDetectionMode.Value);

                entity.GameComponents.Add(component);
            }
        }

        #endregion

        #region Mesh Collider

        /// <summary>
        /// Adds a Unity Mesh Collider component to the game object.
        /// </summary>
        /// <seealso cref="UnityComponentParameter" />
        public class MeshColliderComponentParameter : UnityComponentParameter
        {
            /// <summary>
            /// Indicates if a convex collider from the mesh sould be used.
            /// </summary>
            private readonly BoolParameter _parameterIsConvex = new BoolParameter("Is Convex", true);

            /// <summary>
            /// Indicates if this collider is a trigger?
            /// </summary>
            private readonly BoolParameter _parameterIsTrigger = new BoolParameter("Is Trigger", true);



            public MeshColliderComponentParameter()
                : base("Mesh Collider")
            {
            }



            public override void AddComponent(UnityEntity entity)
            {
                UnityComponent component = new UnityComponent("Mesh Collider");

                component.SetGlobalAttribute("IsConvex", _parameterIsConvex.Value);
                component.SetGlobalAttribute("IsTrigger", _parameterIsTrigger.Value);

                entity.GameComponents.Add(component);
            }
        }

        #endregion

        #region Custom

        /// <summary>
        /// A component that can have a different combination of properties, as defined by the platform where the component will be applied.
        /// By default, it will use some sort of reflection/meta approach to match the property values with the given property names. 
        /// </summary>
        public class CustomComponentParameter : UnityComponentParameter
        {
            /// <summary>
            /// Name of component to add to the object.
            /// </summary>
            private readonly StringParameter _parameterComponentName = new StringParameter("Name", "MyComponent");

            /// <summary>
            /// List of properties to assign to the component.
            /// </summary>
            private readonly ListParameter<CustomComponentParameterType> _parameterProperties = new ListParameter<CustomComponentParameterType>("Properties");



            public CustomComponentParameter()
                : base("Custom")
            {
            }



            public override void AddComponent(UnityEntity entity)
            {
                CustomComponent customComponent = new CustomComponent();
                customComponent.ComponentName = _parameterComponentName.Value;

                foreach (CustomComponentParameterType customComponentParameterType in _parameterProperties.Items)
                    customComponent.SetGlobalAttribute(customComponentParameterType.Name, customComponentParameterType.Value);

                entity.GameComponents.Add(customComponent);
            }



            public abstract class CustomComponentParameterType : CompoundParameter
            {
                /// <summary>
                /// The property name.
                /// </summary>
                protected readonly StringParameter _parameterName = new StringParameter("Name", "PropertyX");



                protected CustomComponentParameterType(string typeName)
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
            /// Sets a textual property on the component.
            /// </summary>
            /// <seealso cref="CustomComponentParameterType" />
            public class StringComponentParameterType : CustomComponentParameterType
            {
                /// <summary>
                /// The string value.
                /// </summary>
                private readonly StringParameter _parameterValue = new StringParameter("Value", "");



                public StringComponentParameterType()
                    : base("String")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a boolean property on the component.
            /// </summary>
            /// <seealso cref="CustomComponentParameterType" />
            public class BoolComponentParameterType : CustomComponentParameterType
            {
                /// <summary>
                /// The boolean value.
                /// </summary>
                private readonly BoolParameter _parameterValue = new BoolParameter("Value", false);



                public BoolComponentParameterType()
                    : base("Boolean")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets an integer property on the component.
            /// </summary>
            /// <seealso cref="CustomComponentParameterType" />
            public class IntComponentParameterType : CustomComponentParameterType
            {
                /// <summary>
                /// The integer value.
                /// </summary>
                private readonly IntParameter _parameterValue = new IntParameter("Value", 0);



                public IntComponentParameterType()
                    : base("Int")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a floating-point property on the component.
            /// </summary>
            /// <seealso cref="CustomComponentParameterType" />
            public class FloatComponentParameterType : CustomComponentParameterType
            {
                /// <summary>
                /// The single-precision value.
                /// </summary>
                private readonly FloatParameter _parameterValue = new FloatParameter("Value", 0);



                public FloatComponentParameterType()
                    : base("Float")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a double-precision property on the component.
            /// </summary>
            /// <seealso cref="CustomComponentParameterType" />
            public class DoubleComponentParameterType : CustomComponentParameterType
            {
                /// <summary>
                /// The double-precision value.
                /// </summary>
                private readonly DoubleParameter _parameterValue = new DoubleParameter("Value", 0);



                public DoubleComponentParameterType()
                    : base("Double")
                {
                }



                public override object Value => _parameterValue.Value;
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// A component that may not exist by default in the target platform, unless explicity interpreted/decoded/implement by the user.
        /// </summary>
        public class OtherComponentParameter : UnityComponentParameter
        {
            /// <summary>
            /// Type of component to add to the object.
            /// </summary>
            private readonly StringParameter _parameterComponentType = new StringParameter("Type", "MyComponent");

            /// <summary>
            /// List of properties to assign to the component.
            /// </summary>
            private readonly ListParameter<OtherComponentParameterType> _parameterProperties = new ListParameter<OtherComponentParameterType>("Properties");



            public OtherComponentParameter()
                : base("Other")
            {
            }



            public override void AddComponent(UnityEntity entity)
            {
                UnityComponent customComponent = new UnityComponent(_parameterComponentType.Value);

                foreach (OtherComponentParameterType customComponentParameterType in _parameterProperties.Items)
                    customComponent.SetGlobalAttribute(customComponentParameterType.Name, customComponentParameterType.Value);

                entity.GameComponents.Add(customComponent);
            }



            public abstract class OtherComponentParameterType : CompoundParameter
            {
                /// <summary>
                /// The property name.
                /// </summary>
                private readonly StringParameter _parameterName = new StringParameter("Name", "PropertyX");



                protected OtherComponentParameterType(string typeName)
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
            ///  Sets a textual property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class StringComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The string value.
                /// </summary>
                private readonly StringParameter _parameterValue = new StringParameter("Value", "");



                public StringComponentParameterType()
                    : base("String")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            ///  Sets a boolean property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class BoolComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The boolean value.
                /// </summary>
                private readonly BoolParameter _parameterValue = new BoolParameter("Value", false);



                public BoolComponentParameterType()
                    : base("Boolean")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            ///  Sets a integer property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class IntComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The integer value.
                /// </summary>
                private readonly IntParameter _parameterValue = new IntParameter("Value", 0);



                public IntComponentParameterType()
                    : base("Int")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            ///  Sets a floating-point property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class FloatComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The floating-point value.
                /// </summary>
                private readonly FloatParameter _parameterValue = new FloatParameter("Value", 0);



                public FloatComponentParameterType()
                    : base("Float")
                {
                }



                public override object Value => _parameterValue.Value;
            }


            /// <summary>
            /// Sets a double-precision property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class DoubleComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The double-precision value.
                /// </summary>
                private readonly DoubleParameter _parameterValue = new DoubleParameter("Value", 0);



                public DoubleComponentParameterType()
                    : base("Double")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a color property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class ColorComponentParameterType : OtherComponentParameterType
            {
                private readonly ColorParameter _parameterValue = new ColorParameter("Value", Color.White);



                public ColorComponentParameterType()
                    : base("Color")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a vector2D property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class Vector2ComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The vector2D value.
                /// </summary>
                private readonly Vector2DParameter _parameterValue = new Vector2DParameter("Value", Vector2D.Zero);



                public Vector2ComponentParameterType()
                    : base("Vector2")
                {
                }



                public override object Value => _parameterValue.Value;
            }

            /// <summary>
            /// Sets a vector2D property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class Vector3ComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The vector3D value.
                /// </summary>
                private readonly Vector3DParameter _parameterValue = new Vector3DParameter("Value", Vector3D.Zero);



                public Vector3ComponentParameterType()
                    : base("Vector3")
                {
                }



                public override object Value => _parameterValue.Value;
            }


            /// <summary>
            /// Sets an object property on the component.
            /// </summary>
            /// <seealso cref="OtherComponentParameterType" />
            public class ObjectComponentParameterType : OtherComponentParameterType
            {
                /// <summary>
                /// The object value.
                /// </summary>
                private readonly ObjectParameter _parameterValue = new ObjectParameter("Value");



                public ObjectComponentParameterType()
                    : base("Other")
                {
                }



                public override object Value => _parameterValue.Value;
            }
        }

        #endregion
    }
}