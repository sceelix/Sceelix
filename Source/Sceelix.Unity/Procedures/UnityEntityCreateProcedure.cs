using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Procedures
{
    /// <summary>
    /// Creates Unity Entities (i.e. Game Objects) with given characteristics. 
    /// Can load existing prefabs and carry mesh or Actor data.
    /// </summary>
    [Procedure("eba3dca6-953c-4e64-9e1d-cbc934eea38c", Label = "Unity Entity Create", Category = "Unity")]
    public class UnityEntityCreateProcedure : SystemProcedure
    {
        private readonly Output<UnityEntity> _output = new Output<UnityEntity>("Output");

        /// <summary>
        /// Name of the Game Object, as it appears in the "Hierarchy" panel.
        /// </summary>
        private readonly StringParameter _parameterName = new StringParameter("Name", "Sceelix Game Object");

        /// <summary>
        /// Enabled flag that allows gameobject state to be toggled.
        /// </summary>
        private readonly BoolParameter _parameterEnabled = new BoolParameter("Enabled", true);

        /// <summary>
        /// IsStatic flag that allows gameobject drawing to be optimized.
        /// </summary>
        private readonly BoolParameter _parameterIsStatic = new BoolParameter("Static", true);

        /// <summary>
        /// Layer of the Game Object.
        /// </summary>
        private readonly StringParameter _parameterLayer = new StringParameter("Layer", "");

        /// <summary>
        /// Path to the prefab, relative to the "Assets" folder.e.g. 
        /// For example Assets/MyFolder/myprefab
        /// </summary>
        private readonly StringParameter _parameterPrefab = new StringParameter("Prefab", "");

        /// <summary>
        /// Defines how the prefab positioning should be placed within the Game Object scope.<br/>
        /// <b>Minimum</b> means that prefab will be translated so that its minimum point will match the scope minimum.
        /// <b>Pivot</b> means that the prefab will be translated so that its pivot point will match the scope minimum.<br/>
        /// </summary>
        private readonly ChoiceParameter _parameterPositioning = new ChoiceParameter("Positioning", "Minimum", "Minimum", "Pivot");

        /// <summary>
        /// Defines how the prefab dimensions should be placed within the Game Object scope.<br/>
        /// <b>None</b> means that prefabs will be left with its original scaling.
        /// <b>Stretch To Fill</b> means that the prefab will stretched to fill the scope volume.<br/>
        /// <b>Scale To Fit</b> means that the prefab will be scaled, maintaining aspect ratio, so it completely fits withing the scope volume.
        /// </summary>
        private readonly ChoiceParameter _parameterScaleMode = new ChoiceParameter("Scale Mode", "Stretch To Fill", "None", "Stretch To Fill", "Scale To Fit");

        /// <summary>
        /// Tag of the Game Object.
        /// </summary>
        private readonly StringParameter _parameterTag = new StringParameter("Tag", "");

        /// <summary>
        /// Other options for the creation of the Game Object.
        /// </summary>
        private readonly SelectListParameter<UnityEntityCreateParameter> _parameterOptions = new SelectListParameter<UnityEntityCreateParameter>("Options", "Simple");



        protected override void Run()
        {
            var unityEntity = new UnityEntity
            {
                Name = _parameterName.Value,
                Tag = _parameterTag.Value,
                Layer = _parameterLayer.Value,
                Prefab = _parameterPrefab.Value,
                ScaleMode = _parameterScaleMode.Value,
                Static = _parameterIsStatic.Value,
                Enabled = _parameterEnabled.Value,
                Positioning = _parameterPositioning.Value
            };

            UnityEntityCreateParameter options = _parameterOptions.Items.FirstOrDefault();
            if (options != null)
                options.Run(unityEntity);

            _output.Write(unityEntity);
        }



        #region Abstract Parameter

        public abstract class UnityEntityCreateParameter : CompoundParameter
        {
            protected UnityEntityCreateParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Run(UnityEntity unityEntity);
        }

        #endregion

        #region Simple Transformation

        /// <summary>
        /// Defines the transformation of the unity object from the same Translation/Rotation/Scale vectors that Unity provides by default.
        /// </summary>
        /// <seealso cref="UnityEntityCreateParameter" />
        public class SimpleParameter : UnityEntityCreateParameter
        {
            /// <summary>
            /// Translation (in Unity Coordinates) or the Unity Entity.
            /// </summary>
            private readonly Vector3DParameter _parameterTranslation = new Vector3DParameter("Translation");

            /// <summary>
            /// Rotation (in Unity Coordinates) or the Unity Entity.
            /// </summary>
            private readonly Vector3DParameter _parameterRotation = new Vector3DParameter("Rotation");

            /// <summary>
            /// Scale (in Unity Coordinates) or the Unity Entity.
            /// </summary>
            private readonly Vector3DParameter _parameterScale = new Vector3DParameter("Scale", Vector3D.One);



            protected SimpleParameter()
                : base("Simple")
            {
            }



            protected internal override void Run(UnityEntity unityEntity)
            {
                var rotation = _parameterRotation.Value;

                //this is kinda strange, but this is how it works
                //Unity applies rotations in the Z,X,Y order
                //meaning we need to do the same, plus consider
                //the fact that Y and Z are flipped in this Sceelix->Unity conversion
                var matrix = Matrix.CreateRotateZ(MathHelper.ToRadians(-rotation.Y))
                             * Matrix.CreateRotateX(MathHelper.ToRadians(-rotation.X))
                             * Matrix.CreateRotateY(MathHelper.ToRadians(-rotation.Z));


                var rotatedScope = BoxScope.Identity.Transform(matrix);

                unityEntity.BoxScope = new BoxScope(rotatedScope, translation: _parameterTranslation.Value.FlipYZ(), sizes: _parameterScale.Value.FlipYZ());
            }
        }

        #endregion

        #region From Mesh

        /// <summary>
        /// Creates a unity entity from a given mesh, making use of its transform (translation/rotation/scale) and creating a mesh component (filter/renderer).
        /// </summary>
        public class FromMeshParameter : UnityEntityCreateParameter
        {
            /// <summary>
            /// The mesh entity from which to create the unity object.
            /// </summary>
            private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");



            protected FromMeshParameter()
                : base("From Mesh")
            {
            }



            protected internal override void Run(UnityEntity unityEntity)
            {
                MeshEntity meshEntity = _input.Read();

                var originalScope = meshEntity.BoxScope;
                unityEntity.BoxScope = originalScope;
                unityEntity.RelativeScale = new Vector3D(1 / originalScope.Sizes.X, 1 / originalScope.Sizes.Y, 1 / originalScope.Sizes.Z).MakeValid();

                //reset the mesh
                meshEntity.InsertInto(new BoxScope(sizes: originalScope.Sizes));

                meshEntity.Attributes.ComplementAttributesTo(unityEntity.Attributes);

                unityEntity.GameComponents.Add(new MeshComponent(meshEntity));
            }
        }

        #endregion

        #region From Billboard

        /// <summary>
        /// Creates a unity entity from a given billboard, making use of its transform (translation/rotation/scale) and creating a billboard component.
        /// </summary>
        public class FromBillboardParameter : UnityEntityCreateParameter
        {
            /// <summary>
            /// The billboard entity from which to create the unity game object.
            /// </summary>
            private readonly SingleInput<BillboardEntity> _input = new SingleInput<BillboardEntity>("Input");



            protected FromBillboardParameter()
                : base("From Billboard")
            {
            }



            protected internal override void Run(UnityEntity unityEntity)
            {
                BillboardEntity billboardEntity = _input.Read();

                var originalScope = billboardEntity.BoxScope;

                billboardEntity.Attributes.ComplementAttributesTo(unityEntity.Attributes);

                unityEntity.BoxScope = originalScope;
                unityEntity.GameComponents.Add(new BillboardComponent(billboardEntity));
            }
        }

        #endregion

        #region From Mesh Instance

        /// <summary>
        /// Creates a unity entity from a given mesh instance, making use of its transform (translation/rotation/scale) and creating a mesh component  (filter/renderer).
        /// </summary>
        /// <seealso cref="UnityEntityCreateParameter" />
        public class FromMeshInstanceParameter : UnityEntityCreateParameter
        {
            /// <summary>
            /// The mesh instance entity from which to create the unity game object.
            /// </summary>
            private readonly SingleInput<MeshInstanceEntity> _input = new SingleInput<MeshInstanceEntity>("Input");



            protected FromMeshInstanceParameter()
                : base("From Mesh Instance")
            {
            }



            protected internal override void Run(UnityEntity unityEntity)
            {
                MeshInstanceEntity meshInstanceEntity = _input.Read();

                var originalScope = meshInstanceEntity.BoxScope;

                meshInstanceEntity.Attributes.ComplementAttributesTo(unityEntity.Attributes);

                unityEntity.BoxScope = originalScope;
                unityEntity.RelativeScale = meshInstanceEntity.RelativeScale;
                unityEntity.GameComponents.Add(new MeshInstanceComponent(meshInstanceEntity));
            }
        }

        #endregion

        #region From Surface

        /// <summary>
        /// Creates a unity entity from a given surface, making use of its transform (translation/rotation/scale) and creating a terrain component.
        /// </summary>
        public class FromSurfaceParameter : UnityEntityCreateParameter
        {
            /// <summary>
            /// The surface entity from which to create the unity game object.
            /// </summary>
            private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");



            protected FromSurfaceParameter()
                : base("From Surface")
            {
            }



            protected internal override void Run(UnityEntity unityEntity)
            {
                SurfaceEntity surfaceEntity = _input.Read();

                var originalScope = surfaceEntity.BoxScope;

                surfaceEntity.Attributes.ComplementAttributesTo(unityEntity.Attributes);

                //reset the mesh
                //surfaceEntity.InsertInto(BoxScope.Identity);
                unityEntity.RelativeScale = new Vector3D(1 / originalScope.Sizes.X, 1 / originalScope.Sizes.Y, 1 / originalScope.Sizes.Z).MakeValid(); //1 / (Math.Max(1,originalScope.Sizes.Z))
                unityEntity.BoxScope = originalScope;
                surfaceEntity.InsertInto(new BoxScope(sizes: originalScope.Sizes));

                unityEntity.GameComponents.Add(new SurfaceComponent(surfaceEntity));
            }
        }

        #endregion

        #region From Actor

        /// <summary>
        /// Creates a unity entity from a given actor, making use of its transform (translation/rotation/scale) only.
        /// </summary>
        /// <seealso cref="UnityEntityCreateParameter" />
        public class FromActorParameter : UnityEntityCreateParameter
        {
            /// <summary>
            /// The actor entity from which to create the unity game object.
            /// </summary>
            private readonly SingleInput<IActor> _input = new SingleInput<IActor>("Input");



            protected FromActorParameter()
                : base("From Actor")
            {
            }



            protected internal override void Run(UnityEntity unityEntity)
            {
                IActor actor = _input.Read();

                actor.Attributes.ComplementAttributesTo(unityEntity.Attributes);

                unityEntity.BoxScope = actor.BoxScope;
            }
        }

        #endregion

        #region From Group

        #endregion
    }
}