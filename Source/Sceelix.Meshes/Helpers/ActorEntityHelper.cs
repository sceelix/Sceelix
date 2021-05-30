using Sceelix.Actors.Data;
using Sceelix.Actors.Procedures;
using Sceelix.Mathematics.Data;

namespace Sceelix.Meshes.Helpers
{
    public enum TranslationReset
    {
        None,
        Minimum,
        Center,
        Maximum
    }

    public static class ActorEntityHelper
    {
        private static readonly ActorScopeProcedure ActorScopeProcedure = new ActorScopeProcedure();
        private static readonly ActorTranslateProcedure ActorTranslateProcedure = new ActorTranslateProcedure();
        private static readonly ActorRotateProcedure ActorRotateProcedure = new ActorRotateProcedure();
        private static readonly ActorScaleProcedure ActorScaleProcedure = new ActorScaleProcedure();



        public static T ResetScope<T>(this T actor) where T : IActor
        {
            lock (ActorScopeProcedure)
            {
                ActorScopeProcedure.Inputs["Input"].Enqueue(actor);
                ActorScopeProcedure.Parameters["Operation"].Set("Reset Scope");
                ActorScopeProcedure.Execute();
                return ActorScopeProcedure.Outputs["Output"].Dequeue<T>();
            }
        }



        /// <summary>
        /// Centers the on origin.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actor">The actor.</param>
        /// <returns></returns>
        public static T ResetTranslation<T>(this T actor, TranslationReset xReset, TranslationReset yReset, TranslationReset zReset) where T : IActor
        {
            lock (ActorTranslateProcedure)
            {
                ActorTranslateProcedure.Inputs["Input"].Enqueue(actor);
                ActorTranslateProcedure.Parameters["Operation"].Set("Reset");
                ActorTranslateProcedure.Parameters["Operation"].Parameters["Reset"].Parameters["X"].Set(xReset);
                ActorTranslateProcedure.Parameters["Operation"].Parameters["Reset"].Parameters["Y"].Set(yReset);
                ActorTranslateProcedure.Parameters["Operation"].Parameters["Reset"].Parameters["Z"].Set(zReset);
                ActorTranslateProcedure.Execute();
                return ActorTranslateProcedure.Outputs["Output"].Dequeue<T>();
            }
        }



        /// <summary>
        /// Rotates an actor entity in the 3D World.
        /// </summary>
        /// <typeparam name="T">Type of entity, which should be a subtype of IActor.</typeparam>
        /// <param name="actor">Actor to be rotated.</param>
        /// <param name="angleDegrees">Angle (in degrees) to rotate the actor.</param>
        /// <param name="axis">The axis around which the actor should be rotated.</param>
        /// <param name="pivot">The pivot offset of the axis around which the actor should be rotated.</param>
        /// <param name="axisRelativeToScope">If true, the axis direction is relative to the scope orientation.</param>
        /// <param name="pivotRelativeToScope">If true, the pivot offset is relative to the scope origin.</param>
        /// <param name="pivotOffsetRelative">If true, the pivot offset has a scope-size relative value (between 0 and 1).</param>
        /// <returns></returns>
        public static T Rotate<T>(this T actor, float angleDegrees, Vector3D axis, Vector3D pivot, bool axisRelativeToScope = false, bool pivotRelativeToScope = false, bool pivotOffsetRelative = false) where T : IActor
        {
            lock (ActorRotateProcedure)
            {
                ActorRotateProcedure.Inputs["Input"].Enqueue(actor);
                ActorRotateProcedure.Parameters["Operation"].Set("Rotate");
                ActorRotateProcedure.Parameters["Operation"].Parameters["Rotate"].Parameters["Angle"].Set(angleDegrees);
                ActorRotateProcedure.Parameters["Operation"].Parameters["Rotate"].Parameters["Axis"].Parameters["Direction"].Set(axis);
                ActorRotateProcedure.Parameters["Operation"].Parameters["Rotate"].Parameters["Axis"].Parameters["Relative To"].Set(axisRelativeToScope ? "Scope" : "World");
                ActorRotateProcedure.Parameters["Operation"].Parameters["Rotate"].Parameters["Pivot"].Parameters["Position"].Set(pivot);
                ActorRotateProcedure.Parameters["Operation"].Parameters["Rotate"].Parameters["Pivot"].Parameters["Relative To"].Set(pivotRelativeToScope ? "Scope" : "World");
                ActorRotateProcedure.Parameters["Operation"].Parameters["Rotate"].Parameters["Pivot"].Parameters["Offset"].Set(pivotOffsetRelative ? "Relative" : "Absolute");
                ActorRotateProcedure.Execute();
                return ActorRotateProcedure.Outputs["Output"].Dequeue<T>();
            }
        }



        /// <summary>
        /// Translates an actor entity in the 3D World.
        /// </summary>
        /// <typeparam name="T">Type of entity, which should be a subtype of IActor.</typeparam>
        /// <param name="actor">Actor to be translated.</param>
        /// <param name="amount">Sized vector with the direction and amount to translate.</param>
        /// <param name="relativeToScope">If true, the translation is relative to the scope orientation.</param>
        /// <returns></returns>
        public static T Translate<T>(this T actor, Vector3D amount, bool relativeToScope = false) where T : IActor
        {
            lock (ActorTranslateProcedure)
            {
                ActorTranslateProcedure.Inputs["Input"].Enqueue(actor);
                ActorTranslateProcedure.Parameters["Operation"].Set("Translate");
                ActorTranslateProcedure.Parameters["Operation"].Parameters["Translate"].Parameters["Amount"].Set(amount);
                ActorTranslateProcedure.Parameters["Operation"].Parameters["Translate"].Parameters["Relative To"].Set(relativeToScope ? "Scope" : "World");
                ActorTranslateProcedure.Execute();
                return ActorTranslateProcedure.Outputs["Output"].Dequeue<T>();
            }
        }
    }
}