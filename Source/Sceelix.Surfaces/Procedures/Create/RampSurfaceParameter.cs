using System;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    public class RampSurfaceParameter : SurfaceCreateParameter
    {
        /// <summary>
        /// Width (size in X) of the surface.
        /// </summary>
        private readonly IntParameter _parameterWidth = new IntParameter("Width", 100) {MinValue = 1};

        /// <summary>
        /// Length (size in Y) of the surface.
        /// </summary>
        private readonly IntParameter _parameterLength = new IntParameter("Length", 100) {MinValue = 1};

        /// <summary>
        /// The scale of the surface height values (in Z).
        /// </summary>
        private readonly IntParameter _parameterHeightScale = new IntParameter("Height Scale", 50) {MinValue = 1};


        /// <summary>
        /// Square size of each terrain cell. Should be a multiple of both Width and Length.
        /// </summary>
        private readonly FloatParameter _parameterCellSize = new FloatParameter("Cell Size", 1);

        /// <summary>
        /// The type of interpolation.
        /// </summary>
        private readonly EnumChoiceParameter<SurfaceInterpolation> _parameterSurfaceInterpolation = new EnumChoiceParameter<SurfaceInterpolation>("Interpolation", SurfaceInterpolation.TopLeft);

        /// <summary>
        /// Shape of the surface ramp.
        /// </summary>
        [Section("Pattern")] private readonly ChoiceParameter _parameterShape = new ChoiceParameter("Shape", "Gradient X", "Gradient X", "Gradient Y", "Pyramid", "Circular");

        /// <summary>
        /// Method used for defining the ramp curve.
        /// </summary>
        private readonly ChoiceParameter _parameterMethod = new ChoiceParameter("Method", "Linear", "Linear", "Quadratic", "Sinusoidal", "Exponential");

        /// <summary>
        /// Indicates if the shape should be vertically inverted. 
        /// </summary>
        private readonly BoolParameter _parameterInvert = new BoolParameter("Invert", false);


        /// <summary>
        /// The offset of the ramp, relative to the surface size. 0.5 would mean an offset equal to the half the size of the surface.
        /// </summary>
        private readonly Vector2DParameter _parameterOffset = new Vector2DParameter("Offset", Vector2D.Zero);

        /// <summary>
        /// The size of the ramp, relative to the surface size. 0.5 would mean half the size of the surface, 2 would mean twice the size.
        /// </summary>
        private readonly Vector2DParameter _parameterSize = new Vector2DParameter("Size", Vector2D.One);

        /// <summary>
        /// How the pattern should behave after the ramp boundaries (repeating, mirroring, etc.) 
        /// </summary>
        private readonly EnumChoiceParameter<SampleMethod> _parameterContinuity = new EnumChoiceParameter<SampleMethod>("Continuity", SampleMethod.Mirror);



        public RampSurfaceParameter()
            : base("Ramp")
        {
        }



        private float Circular(float x, float y)
        {
            var inverseX = 1 - x;
            var inverseY = 1 - y;

            return Math.Max(0, 1 - (float) Math.Sqrt(inverseX * inverseX + inverseY * inverseY));
        }



        protected internal override SurfaceEntity Create()
        {
            int columns = (int) (_parameterWidth.Value / _parameterCellSize.Value) + 1;
            int rows = (int) (_parameterLength.Value / _parameterCellSize.Value) + 1;

            var shapeFunction = GetShapeFunction();
            var methodFunction = GetMethodFunction();
            var continuityFunction = GetContinuityFunction();

            var cellSize = _parameterCellSize.Value;
            var size = new Vector2D(_parameterWidth.Value, _parameterLength.Value);
            size *= _parameterSize.Value;

            var offsetValue = _parameterOffset.Value;
            offsetValue *= new Vector2D(_parameterWidth.Value, _parameterLength.Value);


            float[,] heights = new float[columns, rows];
            ParallelHelper.For(0, columns, x =>
            {
                for (int y = 0; y < rows; y++)
                {
                    var fractionX = continuityFunction((x * cellSize + offsetValue.X) / size.X, 0, 1);
                    var fractionY = continuityFunction((_parameterLength.Value - y * cellSize + offsetValue.Y) / size.Y, 0, 1);

                    var mergedFraction = shapeFunction(fractionX, fractionY);
                    var alteredFraction = (float) methodFunction(mergedFraction);

                    heights[x, y] = alteredFraction * _parameterHeightScale.Value;
                }
            });


            var surfaceEntity = new SurfaceEntity(heights.GetLength(0), heights.GetLength(1), _parameterCellSize.Value);
            surfaceEntity.AddLayer(new HeightLayer(heights) {Interpolation = _parameterSurfaceInterpolation.Value});

            return surfaceEntity;
        }



        private Func<float, float, float, float> GetContinuityFunction()
        {
            Func<float, float, float, float> continuityFunction = (value, min, max) => value < min || value > max ? 0 : value;
            switch (_parameterContinuity.Value)
            {
                case SampleMethod.Repeat:
                    continuityFunction = MathHelper.Repeat;
                    break;
                case SampleMethod.Clamp:
                    continuityFunction = MathHelper.Clamp;
                    break;
                case SampleMethod.Mirror:
                    continuityFunction = MathHelper.Mirror;
                    break;
            }

            return continuityFunction;
        }



        private Func<double, double> GetMethodFunction()
        {
            Func<double, double> methodFunction = CurvesHelper.Linear;
            switch (_parameterMethod.Value)
            {
                case "Quadratic":
                    methodFunction = CurvesHelper.Quadratic.InOut;
                    break;
                case "Sinusoidal":
                    methodFunction = CurvesHelper.Sinusoidal.InOut;
                    break;
                case "Exponential":
                    methodFunction = CurvesHelper.Exponential.InOut;
                    break;
            }

            if (_parameterInvert.Value)
            {
                var function = methodFunction;
                methodFunction = d => 1 - function(d);
            }

            return methodFunction;
        }



        private Func<float, float, float> GetShapeFunction()
        {
            Func<float, float, float> shapeFunction = GradientX;
            switch (_parameterShape.Value)
            {
                case "Gradient Y":
                    shapeFunction = GradientY;
                    break;
                case "Pyramid":
                    shapeFunction = Pyramid;
                    break;
                case "Circular":
                    shapeFunction = Circular;
                    break;
            }

            return shapeFunction;
        }



        private float GradientX(float x, float y)
        {
            return x;
        }



        private float GradientY(float x, float y)
        {
            return y;
        }



        private float Pyramid(float x, float y)
        {
            return Math.Min(x, y);
        }
    }
}