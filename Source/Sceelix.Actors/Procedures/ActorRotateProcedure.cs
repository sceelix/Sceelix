using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Parameters;

namespace Sceelix.Actors.Procedures
{
    /// <summary>
    /// Applies rotation transformations to Actors.
    /// </summary>
    [Procedure("8906cb79-297a-40a5-8e36-b5f12ddbb847", Label = "Actor Rotate", Category = "Actor")]
    public class ActorRotateProcedure : TransferProcedure<IActor>
    {
        /// <summary>
        /// Transformation to apply.
        /// </summary>
        private readonly SelectListParameter<ActorRotateParameter> _parameterTransforms = new SelectListParameter<ActorRotateParameter>("Operation", "Rotate");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterTransforms.SubParameterLabels);



        protected override IActor Process(IActor actor)
        {
            //apply all the transforms sequentially
            foreach (var parameterTransform in _parameterTransforms.Items)
                actor = parameterTransform.Transform(actor);

            return actor;
        }



        #region Transformation Pivot

        public class TransformationPivot : CompoundParameter
        {
            private readonly ChoiceParameter _parameterRelativeTo = new ChoiceParameter("Relative To", "Scope", "Scope", "World");

            /// <summary>
            /// Position of the Pivot.
            /// </summary>
            private readonly Vector3DParameter _positionParameter = new Vector3DParameter("Position", new Vector3D(0.5f, 0.5f, 0.5f));

            /// <summary>
            /// Indicates if the position is measured as absolute units or scope-size relative value (between 0 and 1).
            /// </summary>
            private readonly ChoiceParameter _referentialParameter = new ChoiceParameter("Referential", "Relative", "Absolute", "Relative");



            public TransformationPivot(string label)
                : base(label)
            {
            }
        }

        #endregion

        #region Abstract Parameter

        public abstract class ActorRotateParameter : CompoundParameter
        {
            protected ActorRotateParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IActor Transform(IActor actor);
        }

        #endregion

        #region Rotate

        /// <summary>
        /// Rotates an actor around a given axis.
        /// </summary>
        public class RotateParameter : ActorRotateParameter
        {
            /// <summary>
            /// Angle (in degrees) to rotate the entity.
            /// </summary>
            private readonly FloatParameter _parameterAngle = new FloatParameter("Angle", 0);

            /// <summary>
            /// Axis around which the rotation will be performed.
            /// </summary>
            private readonly CompoundParameter _parameterAxis = new CompoundParameter("Axis",
                new Vector3DParameter("Direction", new Vector3D(1f, 1f, 1f)) {Description = "Direction of the axis."},
                new ChoiceParameter("Relative To", "Scope", "Scope", "World") {Description = "Indicates if the direction is relative to the scope or the world."});

            /// <summary>
            /// Pivot that defines the location of the axis.
            /// </summary>
            private readonly CompoundParameter _parameterPivot = new CompoundParameter("Pivot",
                new Vector3DParameter("Position", new Vector3D(0.5f, 0.5f, 0.5f)) {Description = "Position of the pivot."},
                new ChoiceParameter("Offset", "Relative", "Absolute", "Relative") {Description = "Indicates if the position is measured as absolute units or scope-size relative value (between 0 and 1)"},
                new ChoiceParameter("Relative To", "Scope", "Scope", "World") {Description = "Indicates if the position is relative to the scope or the world."}) {IsExpandedAsDefault = false};



            public RotateParameter()
                : base("Rotate")
            {
            }



            protected internal override IActor Transform(IActor actor)
            {
                var angle = MathHelper.ToRadians(_parameterAngle.Value);

                //first let's focus on calculating the pivot
                var pivot = _parameterPivot["Position"].CastTo<Vector3DParameter>().Value;

                if (_parameterPivot["Offset"].CastTo<ChoiceParameter>().Value == "Relative")
                    pivot *= actor.BoxScope.Sizes;

                if (_parameterPivot["Relative To"].CastTo<ChoiceParameter>().Value == "Scope")
                    pivot = actor.BoxScope.ToWorldPosition(pivot);

                var axis = _parameterAxis["Direction"].CastTo<Vector3DParameter>().Value.Normalize();
                if (_parameterAxis["Relative To"].CastTo<ChoiceParameter>().Value == "Scope")
                    axis = actor.BoxScope.ToWorldDirection(axis);

                var targetBoxScope = actor.BoxScope.Transform(Matrix.CreateTranslation(pivot) * Matrix.CreateAxisAngle(axis, angle) * Matrix.CreateTranslation(-pivot));

                actor.InsertInto(targetBoxScope);

                return actor;
            }
        }

        #endregion

        #region Orient

        /// <summary>
        /// Orients the actor so that a given scope axis direction faces a certain direction.
        /// For instance, setting the axis direction "X" to face (0,0,1) will rotate the actor so that its X-axis arrow faces the Z-Up direction.
        /// </summary>
        public class OrientParameter : ActorRotateParameter
        {
            /// <summary>
            /// The scope axis that is meant to be oriented.
            /// </summary>
            private readonly ChoiceParameter _parameterAxisParameter = new ChoiceParameter("Axis", "X", "X", "Y", "Z");

            /// <summary>
            /// The direction to which the mentioned axis should be facing.
            /// </summary>
            private readonly Vector3DParameter _parameterDirectionParameter = new Vector3DParameter("Direction", Vector3D.One);

            /// <summary>
            /// Pivot that defines the location of the axis.
            /// </summary>
            private readonly CompoundParameter _parameterPivot = new CompoundParameter("Pivot",
                new Vector3DParameter("Position", new Vector3D(0.5f, 0.5f, 0.5f)) {Description = "Position of the pivot."},
                new ChoiceParameter("Offset", "Relative", "Absolute", "Relative") {Description = "Indicates if the position is measured as absolute units or scope-size relative value (between 0 and 1)"},
                new ChoiceParameter("Relative To", "Scope", "Scope", "World") {Description = "Indicates if the position is relative to the scope or the world."}) {IsExpandedAsDefault = false};



            public OrientParameter()
                : base("Orient")
            {
            }



            protected internal override IActor Transform(IActor actor)
            {
                //first let's focus on calculating the pivot
                var pivot = _parameterPivot["Position"].CastTo<Vector3DParameter>().Value;

                if (_parameterPivot["Offset"].CastTo<ChoiceParameter>().Value == "Relative")
                    pivot *= actor.BoxScope.Sizes;

                if (_parameterPivot["Relative To"].CastTo<ChoiceParameter>().Value == "Scope")
                    pivot = actor.BoxScope.ToWorldPosition(pivot);

                Vector3D axis = actor.BoxScope.XAxis;

                if (_parameterAxisParameter.Value == "Y")
                    axis = actor.BoxScope.YAxis;
                else if (_parameterAxisParameter.Value == "Z")
                    axis = actor.BoxScope.ZAxis;

                var newScope = actor.BoxScope.OrientTo(axis, _parameterDirectionParameter.Value, pivot);

                actor.InsertInto(newScope);

                return actor;
            }
        }

        #endregion
    }
}