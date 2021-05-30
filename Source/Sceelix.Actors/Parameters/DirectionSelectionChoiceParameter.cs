using Sceelix.Core.Parameters;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;

namespace Sceelix.Actors.Parameters
{
    public abstract class DirectionSelectionChoiceParameter : CompoundParameter
    {
        protected DirectionSelectionChoiceParameter(string label)
            : base(label)
        {
        }



        public abstract bool Evaluate(Vector3D vector, double degreesLimit);
    }

    /// <summary>
    /// Facing bottom (+Z direction).
    /// </summary>
    public class TopDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected TopDirectionSelectionChoiceParameter()
            : base("Top")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(Vector3D.ZVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing bottom (-Z direction).
    /// </summary>
    public class BottomDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected BottomDirectionSelectionChoiceParameter()
            : base("Bottom")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(-Vector3D.ZVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing left (-X direction).
    /// </summary>
    public class LeftDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected LeftDirectionSelectionChoiceParameter()
            : base("Left")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(-Vector3D.XVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing right (+X direction).
    /// </summary>
    public class RightDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected RightDirectionSelectionChoiceParameter()
            : base("Right")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(Vector3D.XVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing front (+Y direction).
    /// </summary>
    public class FrontDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected FrontDirectionSelectionChoiceParameter()
            : base("Front")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(Vector3D.YVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing back (-Y direction).
    /// </summary>
    public class BackDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected BackDirectionSelectionChoiceParameter()
            : base("Back")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(-Vector3D.YVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing side (+X,-X,+Y,-Y direction).
    /// </summary>
    public class SideDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected SideDirectionSelectionChoiceParameter()
            : base("Side")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            float dot = vector.Dot(Vector3D.ZVector);

            //if it's between +- 45 degrees
            return dot < degreesLimit && dot > -degreesLimit;
        }
    }

    /// <summary>
    /// Facing horizontal (+Y,-Y direction).
    /// </summary>
    public class HorizontalDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected HorizontalDirectionSelectionChoiceParameter()
            : base("Horizontal")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            //if it's between +- 45 degrees
            return vector.Dot(-Vector3D.YVector) > degreesLimit || vector.Dot(Vector3D.YVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing vertical (+Z,-Z direction).
    /// </summary>
    public class VerticalDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected VerticalDirectionSelectionChoiceParameter()
            : base("Vertical")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            //if it's between +- 45 degrees
            return vector.Dot(-Vector3D.ZVector) > degreesLimit || vector.Dot(Vector3D.ZVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Facing lateral (+X,-X direction).
    /// </summary>
    public class LateralDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        protected LateralDirectionSelectionChoiceParameter()
            : base("Lateral")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            //if it's between +- 45 degrees
            return vector.Dot(-Vector3D.XVector) > degreesLimit || vector.Dot(Vector3D.XVector) > degreesLimit;
        }
    }

    /// <summary>
    /// Any other direction.
    /// </summary>
    public class CustomDirectionSelectionChoiceParameter : DirectionSelectionChoiceParameter
    {
        /// <summary>
        /// The direction to compare to.
        /// </summary>
        private readonly Vector3DParameter _directionParameter = new Vector3DParameter("Vector", Vector3D.ZVector);



        protected CustomDirectionSelectionChoiceParameter()
            : base("Custom")
        {
        }



        public override bool Evaluate(Vector3D vector, double degreesLimit)
        {
            return vector.Dot(_directionParameter.Value) > degreesLimit;
        }
    }
}