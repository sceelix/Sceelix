using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;

namespace Sceelix.Actors.Procedures
{
    /// <summary>
    /// Places a given Actor into the location of a second Actor, 
    /// adjusting its translation, rotation and scale.
    /// </summary>
    [Procedure("4c7a38df-e3a0-4a77-abb0-599f275d414c", Label = "Actor Insert", Category = "Actor")]
    public class ActorInsertProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input combination to use.
        /// </summary>
        private readonly SelectListParameter<ActorInsertInputParameter> _inputs = new SelectListParameter<ActorInsertInputParameter>("Inputs", "One to One");

        /// <summary>
        /// Entity/Entities that was/were inserted.
        /// </summary>
        private readonly Output<IActor> _outputSource = new Output<IActor>("Source");

        /// <summary>
        /// Entity/Entities that defined the target location(s) where the source(s) was/were inserted.
        /// </summary>
        private readonly Output<IActor> _outputTarget = new Output<IActor>("Target");

        /// <summary>
        /// Defines how the object should be positioned in the XAxis. <br/>
        /// </summary>
        private readonly SelectListParameter<AxisAlignmentParameter> _parameterXAxisAlignment = new SelectListParameter<AxisAlignmentParameter>("Alignment on X-Axis", "Center");

        /// <summary>
        /// Defines how the object should be positioned in the YAxis. <br/>
        /// </summary>
        private readonly SelectListParameter<AxisAlignmentParameter> _parameterYAxisAlignment = new SelectListParameter<AxisAlignmentParameter>("Alignment on Y-Axis", "Center");

        /// <summary>
        /// Defines how the object should be positioned in the ZAxis. <br/>
        /// </summary>
        private readonly SelectListParameter<AxisAlignmentParameter> _parameterZAxisAlignment = new SelectListParameter<AxisAlignmentParameter>("Alignment on Z-Axis", "Center");



        protected override void Run()
        {
            foreach (KeyValuePair<IActor, IActor> actorsPair in _inputs.SelectedItem.GetSourceTargetPairs().ToList())
            {
                IActor source = actorsPair.Key;
                IActor target = actorsPair.Value;

                var xFraction = _parameterXAxisAlignment.SelectedItem.GetAxisFraction(source.BoxScope.Sizes.X, target.BoxScope.Sizes.X);
                var yFraction = _parameterYAxisAlignment.SelectedItem.GetAxisFraction(source.BoxScope.Sizes.Y, target.BoxScope.Sizes.Y);
                var zFraction = _parameterZAxisAlignment.SelectedItem.GetAxisFraction(source.BoxScope.Sizes.Z, target.BoxScope.Sizes.Z);

                var targetScope = target.BoxScope;
                var newTarget = new BoxScope(targetScope.XAxis, targetScope.YAxis, targetScope.ZAxis,
                    targetScope.Translation * new Vector3D(xFraction.BaseTranslation, yFraction.BaseTranslation, zFraction.BaseTranslation) + targetScope.ToWorldDirection(new Vector3D(xFraction.RelativeTranslation, yFraction.RelativeTranslation, zFraction.RelativeTranslation)),
                    new Vector3D(xFraction.RelativeSize, yFraction.RelativeSize, zFraction.RelativeSize));

                source.InsertInto(newTarget);

                _outputSource.Write(source);
                _outputTarget.Write(target);
            }
        }



        #region Axis Alignment

        public abstract class AxisAlignmentParameter : CompoundParameter
        {
            public AxisAlignmentParameter(string label)
                : base(label)
            {
            }



            public abstract AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize);

            public abstract float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize);

            public struct AxisAlignmentSetup
            {
                public float BaseTranslation;
                public float RelativeTranslation;
                public float RelativeSize;
            }
        }

        /// <summary>
        /// The minimum coordinate in the source will match the minimum coordinate in the target.
        /// </summary>
        public class MinimumAxisAlignmentParameter : AxisAlignmentParameter
        {
            public MinimumAxisAlignmentParameter()
                : base("Minimum")
            {
            }



            public override AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize)
            {
                return new AxisAlignmentSetup {BaseTranslation = 1, RelativeTranslation = 0, RelativeSize = sourceAxisSize};
            }



            public override float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize)
            {
                return new[] {0, sourceAxisSize, 1};
            }
        }

        /// <summary>
        /// The minimum coordinate in the source will match the maximum coordinate in the target.
        /// </summary>
        public class MaximumAxisAlignmentParameter : AxisAlignmentParameter
        {
            public MaximumAxisAlignmentParameter()
                : base("Maximum")
            {
            }



            public override AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize)
            {
                return new AxisAlignmentSetup {BaseTranslation = 1, RelativeTranslation = targetAxisSize - sourceAxisSize, RelativeSize = sourceAxisSize};
            }



            public override float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize)
            {
                return new[] {targetAxisSize - sourceAxisSize, sourceAxisSize, 1};
            }
        }

        /// <summary>
        /// The average coordinate in the source will match the average coordinate in the target.
        /// </summary>
        public class CenterAxisAlignmentParameter : AxisAlignmentParameter
        {
            public CenterAxisAlignmentParameter()
                : base("Center")
            {
            }



            public override AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize)
            {
                return new AxisAlignmentSetup {BaseTranslation = 1, RelativeTranslation = targetAxisSize / 2f - sourceAxisSize / 2f, RelativeSize = sourceAxisSize};
            }



            public override float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize)
            {
                return new[] {targetAxisSize / 2f - sourceAxisSize / 2f, sourceAxisSize, 1};
            }
        }

        /// <summary>
        /// The source object will be stretched to match the size and location of the target.
        /// </summary>
        public class StretchAxisAlignmentParameter : AxisAlignmentParameter
        {
            public StretchAxisAlignmentParameter()
                : base("Stretch")
            {
            }



            public override AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize)
            {
                return new AxisAlignmentSetup {BaseTranslation = 1, RelativeTranslation = 0, RelativeSize = targetAxisSize};
            }



            public override float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize)
            {
                return new[] {0, targetAxisSize, 1};
            }
        }

        /// <summary>
        /// The source object will be stretched to match the size and location of the target.
        /// </summary>
        public class NoneAxisAlignmentParameter : AxisAlignmentParameter
        {
            public NoneAxisAlignmentParameter()
                : base("None")
            {
            }



            public override AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize)
            {
                return new AxisAlignmentSetup {BaseTranslation = 0, RelativeTranslation = 0, RelativeSize = sourceAxisSize};
            }



            public override float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize)
            {
                return new[] {0, sourceAxisSize, 0};
            }
        }

        /// <summary>
        /// Allows the customization of the coordinate alignment based on the relative size both source and target actors. 
        /// Example: Source=-0.5 and Target=0.5 achieves the same effect as the "Center" choice.
        /// </summary>
        public class CustomAxisAlignmentParameter : AxisAlignmentParameter
        {
            /// <summary>
            /// Relative size from the source. Can range from -1 to 1.
            /// </summary>
            private readonly FloatParameter _parameterSourceFraction = new FloatParameter("Source", 0f) {MinValue = -1, MaxValue = 1};

            /// <summary>
            /// Relative size from the target. Can range from -1 to 1.
            /// </summary>
            private readonly FloatParameter _parameterTargetFraction = new FloatParameter("Target", 1f) {MinValue = -1, MaxValue = 1};



            public CustomAxisAlignmentParameter()
                : base("Custom")
            {
            }



            public override AxisAlignmentSetup GetAxisFraction(float sourceAxisSize, float targetAxisSize)
            {
                return new AxisAlignmentSetup {BaseTranslation = 1, RelativeTranslation = sourceAxisSize * _parameterSourceFraction.Value + targetAxisSize * _parameterTargetFraction.Value, RelativeSize = sourceAxisSize};
            }



            public override float[] GetAxisFractionOld(float sourceAxisSize, float targetAxisSize)
            {
                return new[] {sourceAxisSize * _parameterSourceFraction.Value + targetAxisSize * _parameterTargetFraction.Value, sourceAxisSize, 1};
            }
        }

        #endregion

        #region Inputs

        public abstract class ActorInsertInputParameter : CompoundParameter
        {
            protected ActorInsertInputParameter(string label)
                : base(label)
            {
            }



            public abstract IEnumerable<KeyValuePair<IActor, IActor>> GetSourceTargetPairs();
        }

        /// <summary>
        /// Accepts one source and one target destination where the source is to be placed.
        /// </summary>
        /// <seealso cref="Sceelix.Actors.Procedures.ActorInsertProcedure.ActorInsertInputParameter" />
        public class OneToOneInputParameter : ActorInsertInputParameter
        {
            /// <summary>
            /// Entity that is meant to be inserted.
            /// </summary>
            private readonly SingleInput<IActor> _inputSource = new SingleInput<IActor>("Source");

            /// <summary>
            /// Entity that defines the target location where to insert the source.
            /// </summary>
            private readonly SingleInput<IActor> _inputTarget = new SingleInput<IActor>("Target");



            protected OneToOneInputParameter()
                : base("One to One")
            {
            }



            public override IEnumerable<KeyValuePair<IActor, IActor>> GetSourceTargetPairs()
            {
                yield return new KeyValuePair<IActor, IActor>(_inputSource.Read(), _inputTarget.Read());
            }
        }

        /// <summary>
        /// Accepts one source and many target destinations. Copies the source as many times as need to fill
        /// the available target locations.
        /// </summary>
        /// <seealso cref="Sceelix.Actors.Procedures.ActorInsertProcedure.ActorInsertInputParameter" />
        public class OneToManyInputParameter : ActorInsertInputParameter
        {
            /// <summary>
            /// Entity that is meant to be inserted.
            /// </summary>
            private readonly SingleInput<IActor> _inputSource = new SingleInput<IActor>("Source");

            /// <summary>
            /// Entities that defines the target locations where to insert the source.
            /// </summary>
            private readonly CollectiveInput<IActor> _inputTargets = new CollectiveInput<IActor>("Targets");



            protected OneToManyInputParameter()
                : base("One to Many")
            {
            }



            public override IEnumerable<KeyValuePair<IActor, IActor>> GetSourceTargetPairs()
            {
                IActor source = null;

                foreach (IActor targetActor in _inputTargets.Read())
                {
                    source = source == null ? _inputSource.Read() : (IActor) source.DeepClone();

                    yield return new KeyValuePair<IActor, IActor>(source, targetActor);
                }
            }
        }

        /// <summary>
        /// Accepts many sources and many target destinations. <br/>
        /// If the number X of source objects is greater than the number Y of targets, then only the first Y source objects will be placed. <br/>
        /// If the number X of source objects is lesser than the number Y of targets, then the source objects will be copied in a repeat fashion (equivalent to a Copy->Relation->Repeat) until they match the Y amount of locations.
        /// </summary>
        /// <seealso cref="Sceelix.Actors.Procedures.ActorInsertProcedure.ActorInsertInputParameter" />
        public class ManyToManyInputParameter : ActorInsertInputParameter
        {
            /// <summary>
            /// Entities that are meant to be inserted.
            /// </summary>
            private readonly CollectiveInput<IActor> _inputSources = new CollectiveInput<IActor>("Sources");

            /// <summary>
            /// Entities that define the target locations where to insert the source entities.
            /// </summary>
            private readonly CollectiveInput<IActor> _inputTargets = new CollectiveInput<IActor>("Targets");



            protected ManyToManyInputParameter()
                : base("Many to Many")
            {
            }



            public override IEnumerable<KeyValuePair<IActor, IActor>> GetSourceTargetPairs()
            {
                var sourceActors = _inputSources.Read().ToList();
                var targetActors = _inputTargets.Read().ToList();

                for (int i = 0; i < targetActors.Count; i++)
                {
                    //if the index can still be found in the list of sources, use the respective actor
                    //otherwise repeat from the beginning, while making a copy
                    IActor sourceActor = i < sourceActors.Count ? sourceActors[i] : (IActor) sourceActors[i % sourceActors.Count].DeepClone();

                    yield return new KeyValuePair<IActor, IActor>(sourceActor, targetActors[i]);
                }
            }
        }

        #endregion
    }
}