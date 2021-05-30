using Sceelix.Core.Annotations;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Data
{
    [Entity("Height Layer", TypeBrowsable = false)]
    public class HeightLayer : FloatLayer, I3DLayer
    {
        public HeightLayer(float[,] values)
            : base(values)
        {
        }



        private float BaseHeight
        {
            get;
            set;
        }


        public float Height => MaxHeight - MinHeight;


        public float MaxHeight => MaxValue;


        public override float MaxValue => base.MaxValue + BaseHeight;


        public float MinHeight => MinValue;


        public override float MinValue => base.MinValue + BaseHeight;



        /// <summary>
        /// Creates an empty, zero-filled layer with the given number of columns and rows.
        /// </summary>
        /// <param name="numColumns">The number columns.</param>
        /// <param name="numRows">The number rows.</param>
        /// <returns></returns>
        public override SurfaceLayer CreateEmpty(int numColumns, int numRows)
        {
            return new HeightLayer(new float[numColumns, numRows]) {Interpolation = Interpolation};
        }



        /// <summary>
        /// Gets the 3D world position sampled at given SURFACE coordinates.
        /// </summary>
        /// <param name="surfaceColumn">The surface column.</param>
        /// <param name="surfaceRow">The surface row.</param>
        /// <returns></returns>
        public Vector3D GetPosition(Coordinate surfaceCoordinate)
        {
            var worldPosition2D = Surface.ToWorldPosition(surfaceCoordinate);

            return new Vector3D(worldPosition2D, (float) GetValue(surfaceCoordinate));
        }



        public Vector3D GetPosition(Coordinate surfaceCoordinate, SampleMethod sampleMethod)
        {
            var worldPosition2D = Surface.ToWorldPosition(surfaceCoordinate);

            return new Vector3D(worldPosition2D, (float) GetValue(surfaceCoordinate, sampleMethod));
        }



        public override object GetValue(Coordinate surfaceCoordinate)
        {
            return BaseHeight + (float) base.GetValue(surfaceCoordinate);
        }



        public void ScaleVertically(float amount)
        {
            var minValue = MinHeight;

            ParallelHelper.For(0, NumColumns, i =>
            {
                for (int j = 0; j < NumRows; j++) _values[i, j] = (_values[i, j] + BaseHeight - minValue) * amount + minValue - BaseHeight;
            });

            _min *= amount;
            _max *= amount;
        }



        /// <summary>
        /// Sets the absolute height value at given LAYER coordinates.
        /// </summary>
        /// <param name="layerColumn">The layer column.</param>
        /// <param name="layerRow">The layer row.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(Coordinate layerCoordinate, object value)
        {
            base.SetValue(layerCoordinate, (float) value - BaseHeight);
        }



        public void TranslateVertically(float amount)
        {
            BaseHeight += amount;
        }
    }
}