using System;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;

namespace Sceelix.Actors.Procedures
{
    /// <summary>
    /// Applies scaling transformations to Actors.
    /// </summary>
    [Procedure("bda91758-2aea-4aad-a607-cc4e76adbeae", Label = "Actor Scale", Category = "Actor")]
    public class ActorScaleProcedure : TransferProcedure<IActor>
    {
        /// <summary>
        /// The amount on each axis to scale.
        /// </summary>
        private readonly Vector3DParameter _parameterAmount = new Vector3DParameter("Amount", new Vector3D(1, 1, 1));

        /// <summary>
        /// Indicates how the scaling should be performed.<br/>
        /// <b>Relative</b> means that the indicated amount is a factor, which will be multiplied by the dimensions of the actor.<br/>
        /// <b>Absolute</b> means that the indicated amount is the absolute size that the actor will assume after the operation. If a value is negative, it will be set to a value proportional to the other dimensions.<br/>
        /// <b>Additive</b> means that the indicated absolute amount is added to the current size of the actor.<br/>
        /// </summary>
        private readonly ChoiceParameter _parameterScalingMode = new ChoiceParameter("Mode", "Relative", "Absolute", "Additive", "Relative");

        /// <summary>
        /// Indicates if the scaling for the given axes is relative to the scope or to the world.
        /// </summary>
        private readonly ChoiceParameter _parameterRelativeTo = new ChoiceParameter("Relative To", "Scope", "Scope", "World");


        /// <summary>
        /// The pivot that defines around which point the actor will stretch.
        /// </summary>
        private readonly CompoundParameter _parameterPivot = new CompoundParameter("Pivot",
            new Vector3DParameter("Position", new Vector3D(0.5f, 0.5f, 0.5f)) {Description = "Position of the pivot."},
            new ChoiceParameter("Offset", "Relative", "Absolute", "Relative") {Description = "Indicates if the position is measured as absolute units or scope-size relative value (between 0 and 1)"},
            new ChoiceParameter("Relative To", "Scope", "Scope", "World") {Description = "Indicates if the operation should be relative to the scope's direction or to the world."}  );



        protected override IActor Process(IActor actor)
        {
            //first of all, let's determine the actual scaling
            var scaling = Vector3D.Zero;
            var amount = _parameterAmount.Value;

            switch (_parameterScalingMode.Value)
            {
                case "Absolute":

                    if (amount.X < 0 && amount.Y < 0 && amount.Z < 0)
                        throw new ArgumentException("Only two XYZ components can be negative for absolute scaling.");

                    scaling = (amount / actor.BoxScope.Sizes).MakeValid();

                    var scalingValues = scaling.ToArray();
                    var minScaling = scaling.ToArray().Where(val => val > 0).Min();

                    for (int i = 0; i < scalingValues.Length; i++)
                    {
                        if (scalingValues[i] < 0)
                            scalingValues[i] = minScaling;
                        if (Math.Abs(scaling[i]) < float.Epsilon)
                            scalingValues[i] = 1;
                    }

                    scaling = new Vector3D(scalingValues[0], scalingValues[1], scalingValues[2]);

                    break;
                case "Additive":
                    scaling = new Vector3D(1, 1, 1) + (amount / actor.BoxScope.Sizes).MakeValid();
                    break;
                case "Relative":
                    scaling = amount;
                    break;
            }

            //now, let's focus on calculating the pivot
            var pivot = _parameterPivot["Position"].CastTo<Vector3DParameter>().Value;

            if (_parameterPivot["Offset"].CastTo<ChoiceParameter>().Value == "Relative")
                pivot *= actor.BoxScope.Sizes;

            if (_parameterPivot["Relative To"].CastTo<ChoiceParameter>().Value == "Scope")
                pivot = actor.BoxScope.ToWorldPosition(pivot);

            Matrix transformation = _parameterRelativeTo.Value == "Scope"
                ? Matrix.CreateTranslation(pivot) * actor.BoxScope.ToWorldDirectionMatrix() * Matrix.CreateScale(scaling) * actor.BoxScope.ToScopeDirectionMatrix() * Matrix.CreateTranslation(-pivot)
                : Matrix.CreateTranslation(pivot) * Matrix.CreateScale(scaling) * Matrix.CreateTranslation(-pivot);

            //calculate the target boxscope
            var oldScope = actor.BoxScope;
            var targetBoxScope = actor.BoxScope.Transform(transformation);

            //and send the 
            actor.InsertInto(targetBoxScope);

            if (targetBoxScope.IsSkewed)
                actor.BoxScope = oldScope;

            return actor;
        }
    }
}