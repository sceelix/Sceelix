using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Data
{
    public enum SurfaceInterpolation
    {
        TopLeft,
        TopRight,
        Bilinear
    }

    public enum SurfaceOperation
    {
        Add,
        Subtract,
        Multiply,
        Set,
        Average
    }


    public interface ISurfaceLayer : IEntity
    {
    }

    [Entity("Layer", TypeBrowsable = false)]
    public abstract class SurfaceLayer : Entity, ISurfaceLayer
    {
        public abstract int NumColumns
        {
            get;
        }


        public abstract int NumRows
        {
            get;
        }

        public virtual SurfaceEntity Surface
        {
            get;
            protected internal set;
        }


        public abstract object Add(object valueA, object valueB);



        public virtual bool CanCombineWith(SurfaceLayer otherLayer)
        {
            return GetType() != otherLayer.GetType();
        }



        /// <summary>
        /// Determines whether this layer contains the given layer coordinates.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <returns>
        ///   <c>true</c> if this layer contains the specified (column,row) coordinates; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Coordinate layerCoordinate)
        {
            return layerCoordinate.X >= 0 && layerCoordinate.Y >= 0 && layerCoordinate.X < NumColumns && layerCoordinate.Y < NumRows;
        }



        public abstract SurfaceLayer CreateEmpty(int numColumns, int numRows);

        public abstract object GetValue(Coordinate surfaceCoordinate);

        public abstract object GetValue(Coordinate surfaceCoordinate, SampleMethod sampleMethod);

        public abstract object GetValue(Vector2D worldPosition, SampleMethod sampleMethod = SampleMethod.Clamp);

        public abstract object Invert(object value);

        public abstract object Minus(object valueA, object valueB);

        public abstract object Multiply(object valueA, object valueB);

        public abstract object MultiplyScalar(object value, float scalar);

        public abstract void SetValue(Coordinate layerCoordinate, object value);
        
        public abstract void Update();
    }
}