using System;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Extensions;

namespace Sceelix.Surfaces.Data
{
    public abstract class GenericSurfaceLayer<T> : SurfaceLayer
    {
        private SurfaceEntity _surface;

        protected T[,] _values;


        private SurfaceInterpolation _interpolation = SurfaceInterpolation.Bilinear;
        private Func<float, float, object[], object> _interpolationFunction;



        protected GenericSurfaceLayer(T[,] values)
        {
            _values = values;
            _interpolationFunction = _interpolation.ToObjectFunction(this);
        }



        public virtual T DefaultValue => default(T);



        [EntityProperty]
        public SurfaceInterpolation Interpolation
        {
            get { return _interpolation; }
            set
            {
                _interpolation = value;
                _interpolationFunction = _interpolation.ToObjectFunction(this);
            }
        }



        /// <summary>
        /// Number of columns of the internal array. May not be coincident with the NumColumns property from surface.
        /// </summary>
        public override int NumColumns => _values.GetLength(0);


        /// <summary>
        /// Number of rows of the internal array. May not be coincident with the NumRows property from surface.
        /// </summary>
        public override int NumRows => _values.GetLength(1);


        public T[,] RawValues => _values;



        public override SurfaceEntity Surface
        {
            get { return _surface; }
            protected internal set { _surface = value; }
        }



        protected abstract T Add(T valueA, T valueB);



        public override object Add(object valueA, object valueB)
        {
            return Add((T) valueA, (T) valueB);
        }



        /// <summary>
        /// Performs a deep clone, following the memberwiseClone of the base class.
        ///
        /// Should be overriden in derived layers if either the array values are of reference type,
        /// or if it contains other fields that would be otherwise be shallow cloned.
        /// </summary>
        /// <returns></returns>
        public override IEntity DeepClone()
        {
            var clone = (GenericSurfaceLayer<T>) base.DeepClone();

            //clone the items using the clone extension
            clone._values = (T[,]) _values.Clone();

            return clone;
        }



        public T GetGenericValue(Coordinate layerCoordinate)
        {
            return (T) GetValue(layerCoordinate);
        }



        public T GetGenericValue(Coordinate layerCoordinate, SampleMethod sampleMethod)
        {
            return (T) GetValue(layerCoordinate, sampleMethod);
        }



        public T GetGenericValue(Vector2D worldPosition, SampleMethod sampleMethod = SampleMethod.Clamp)
        {
            return (T) GetValue(worldPosition, sampleMethod);
        }



        public override object GetValue(Coordinate surfaceCoordinate)
        {
            return _values[surfaceCoordinate.X, surfaceCoordinate.Y];
        }



        public override object GetValue(Coordinate surfaceCoordinate, SampleMethod sampleMethod)
        {
            var sampleMethodFunc = sampleMethod.ToIntFunction();

            var layerColumn = sampleMethodFunc(surfaceCoordinate.X, 0, NumColumns - 1);
            var layerRow = sampleMethodFunc(surfaceCoordinate.Y, 0, NumRows - 1);
            var newCoordinate = new Coordinate(layerColumn, layerRow);
            if (!Contains(newCoordinate)) return DefaultValue;

            return GetValue(newCoordinate);
        }



        public override object GetValue(Vector2D worldPosition, SampleMethod sampleMethod = SampleMethod.Clamp)
        {
            var c0 = ToLayerCoordinates(worldPosition);
            var c1 = ToLayerCoordinatesUpper(worldPosition);

            Vector2D topLeftPosition = _surface.ToWorldPosition(c0);
            Vector2D bottomRightPosition = _surface.ToWorldPosition(c1);

            var fractionX = c0.X == c1.X ? 0 : (worldPosition.X - topLeftPosition.X) / (bottomRightPosition.X - topLeftPosition.X);
            var fractionY = c0.Y == c1.Y ? 0 : (topLeftPosition.Y - worldPosition.Y) / (topLeftPosition.Y - bottomRightPosition.Y);

            var val = _interpolationFunction(fractionX, fractionY, new[]
            {
                GetValue(new Coordinate(c0.X, c0.Y), sampleMethod),
                GetValue(new Coordinate(c1.X, c0.Y), sampleMethod),
                GetValue(new Coordinate(c0.X, c1.Y), sampleMethod),
                GetValue(new Coordinate(c1.X, c1.Y), sampleMethod)
            });
            return val;
        }



        public T Interpolate(float fractionX, float fractionY, object[] values)
        {
            return (T) _interpolationFunction(fractionX, fractionY, values);
        }



        public sealed override object Invert(object value)
        {
            return InvertValue((T) value);
        }



        protected abstract T InvertValue(T value);


        protected abstract T Minus(T valueA, T valueB);



        public override object Minus(object valueA, object valueB)
        {
            return Minus((T) valueA, (T) valueB);
        }



        protected abstract T Multiply(T value1, T value2);



        public override object Multiply(object valueA, object valueB)
        {
            return Multiply((T) valueA, (T) valueB);
        }



        protected abstract T MultiplyScalar(T value, float scalar);



        public override object MultiplyScalar(object value, float scalar)
        {
            return MultiplyScalar((T) value, scalar);
        }



        public override void SetValue(Coordinate layerCoordinate, object value)
        {
            //changes the getter to the default one
            _values[layerCoordinate.X, layerCoordinate.Y] = (T) value;
        }



        //TODO:Review if the surface version can be used instead
        public Coordinate ToLayerCoordinates(Vector2D worldPosition)
        {
            var relativePosition = worldPosition - _surface.Origin;

            var coordX = (int) (relativePosition.X / _surface.CellSize);
            var coordY = NumRows - 2 - (int) (relativePosition.Y / _surface.CellSize);

            return new Coordinate(coordX, coordY);
        }



        //TODO:Review if the surface version can be used instead
        private Coordinate ToLayerCoordinatesUpper(Vector2D worldPosition)
        {
            var relativePosition = worldPosition - _surface.Origin;

            var coordX = (int) Math.Ceiling(relativePosition.X / _surface.CellSize);
            var coordY = NumRows - 1 - (int) Math.Floor(relativePosition.Y / _surface.CellSize);

            return new Coordinate(coordX, coordY);
        }
    }
}