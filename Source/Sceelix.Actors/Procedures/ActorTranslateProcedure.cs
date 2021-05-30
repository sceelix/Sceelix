using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;

namespace Sceelix.Actors.Procedures
{
    /// <summary>
    /// Applies translation transformations to Actors.
    /// </summary>
    [Procedure("930eb356-619d-4db7-b68a-016d9a6d1e97", Label = "Actor Translate", Category = "Actor")]
    public class ActorTranslateProcedure : TransferProcedure<IActor>
    {
        /// <summary>
        /// Transformation to apply.
        /// </summary>
        private readonly SelectListParameter<ActorTranslateParameter> _parameterTransforms = new SelectListParameter<ActorTranslateParameter>("Operation", "Translate");


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

        public abstract class ActorTranslateParameter : CompoundParameter
        {
            protected ActorTranslateParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IActor Transform(IActor actor);
        }

        #endregion

        #region Translation

        /// <summary>
        /// Translates an actor by a certain offset.
        /// </summary>
        public class TranslationParameter : ActorTranslateParameter
        {
            /// <summary>
            /// Amount to be translated.
            /// </summary>
            private readonly Vector3DParameter _parameterAmount = new Vector3DParameter("Amount", new Vector3D());

            /// <summary>
            /// Indicates if the operation should be relative to the scope's direction or to the world.
            /// </summary>
            private readonly ChoiceParameter _parameterRelativeTo = new ChoiceParameter("Relative To", "Scope", "Scope", "World");



            public TranslationParameter()
                : base("Translate")
            {
            }



            protected internal override IActor Transform(IActor actor)
            {
                //calculates the actual offset vector
                var actualTranslation = _parameterRelativeTo.Value == "World" ? _parameterAmount.Value : actor.BoxScope.ToWorldDirection(_parameterAmount.Value);

                //calculate the target boxscope
                var targetBoxScope = actor.BoxScope.Translate(actualTranslation);

                //and then assign it
                actor.InsertInto(targetBoxScope);

                return actor;
            }
        }

        #endregion

        #region Reset Translation

        /// <summary>
        /// Resets the position of the actor.
        /// </summary>
        public class ResetTranslationParameter : ActorTranslateParameter
        {
            /// <summary>
            /// Alignment of the X coordinate.<br/>
            /// <b>None</b> means that the coordinate will not be changed.<br/>
            /// <b>Minimum</b> means that the actor's minimum value for this coordinate will match the origin.<br/>
            /// <b>Maximum</b> means that the actor's maximum  value for this coordinate will match the origin.<br/>
            /// <b>Center</b> means that the actor's average of this coordinate will match the origin.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterX = new ChoiceParameter("X", "Center", "None", "Minimum", "Center", "Maximum");

            /// <summary>
            /// Alignment of the Y coordinate.<br/>
            /// <b>None</b> means that the coordinate will not be changed.<br/>
            /// <b>Minimum</b> means that the actor's minimum value for this coordinate will match the origin.<br/>
            /// <b>Maximum</b> means that the actor's maximum  value for this coordinate will match the origin.<br/>
            /// <b>Center</b> means that the actor's average value of this coordinate will match the origin.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterY = new ChoiceParameter("Y", "Center", "None", "Minimum", "Center", "Maximum");

            /// <summary>
            /// Alignment of the Z coordinate.<br/>
            /// <b>None</b> means that the coordinate will not be changed.<br/>
            /// <b>Minimum</b> means that the actor's minimum value for this coordinate will match the origin.<br/>
            /// <b>Maximum</b> means that the actor's maximum  value for this coordinate will match the origin.<br/>
            /// <b>Center</b> means that the actor's average value of this coordinate will match the origin.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterZ = new ChoiceParameter("Z", "Center", "None", "Minimum", "Center", "Maximum");



            public ResetTranslationParameter()
                : base("Reset")
            {
            }



            private float CalculateAxisTranslation(float size, float value, string desiredLocation)
            {
                switch (desiredLocation)
                {
                    case "Minimum":
                        return -value;
                    case "Maximum":
                        return -value - size;
                    case "Center":
                        return -value - size / 2f;
                    default: //Center
                        return 0;
                }
            }



            protected internal override IActor Transform(IActor actor)
            {
                var boxScope = actor.BoxScope;

                var x = CalculateAxisTranslation(boxScope.Sizes.X, boxScope.Translation.X, _parameterX.Value);
                var y = CalculateAxisTranslation(boxScope.Sizes.Y, boxScope.Translation.Y, _parameterY.Value);
                var z = CalculateAxisTranslation(boxScope.Sizes.Z, boxScope.Translation.Z, _parameterZ.Value);

                actor.InsertInto(boxScope.Translate(new Vector3D(x, y, z)));

                return actor;
            }
        }

        #endregion
    }
}