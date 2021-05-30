using Sceelix.Core.Annotations;
using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Data
{
    [Entity("Normal Layer", TypeBrowsable = false)]
    public class NormalLayer : GenericSurfaceLayer<Vector3D>
    {
        //private HeightLayer _heightLayer;
        //private Func<int, int, Vector3D> _getDelegate;



        public NormalLayer(Vector3D[,] normals)
            : base(normals)
        {
            //_getDelegate = base.GetValue;
        }



        public NormalLayer(Vector3D[,] normals, Vector3D defaultValue)
            : base(normals)
        {
            this.Fill(defaultValue);
        }



        /*private Vector3D CalculateGeometryNormal(int layerColumn, int layerRow)
        {
            Vector3D?[] directions = new Vector3D?[4];
            Vector3D centralPosition = _heightLayer.GetLayerPosition(layerColumn, layerRow);

            if (layerRow > 0)
                directions[0] = _heightLayer.GetPosition(layerColumn, layerRow - 1) - centralPosition;

            if (layerColumn > 0)
                directions[1] = _heightLayer.GetPosition(layerColumn - 1, layerRow) - centralPosition;

            if (layerRow < _surface.NumRows - 1)
                directions[2] = _heightLayer.GetPosition(layerColumn, layerRow + 1) - centralPosition;

            if (layerColumn < _surface.NumColumns - 1)
                directions[3] = _heightLayer.GetPosition(layerColumn + 1, layerRow) - centralPosition;


            Vector3D normal = Vector3D.Zero;
            for (int i = 0; i < 4; i++)
            {
                Vector3D? direction1 = directions[i];
                Vector3D? direction2 = i + 1 > 3 ? directions[0] : directions[i + 1];

                if (direction1.HasValue && direction2.HasValue)
                    normal += Vector3D.Cross(direction1.Value, direction2.Value);
            }

            return normal.Normalize();
        }*/


        /*public void RecalculateNormals()
        {
            if (_values != null)
            {
                Parallel.For(0, NumColumns, (x) =>
                {
                    for (int y = 0; y < NumRows; y++)
                    {
                        _values[x, y] = CalculateGeometryNormal(x, y);
                    }
                });
            }
        }*/



        protected override Vector3D Add(Vector3D valueA, Vector3D valueB)
        {
            return valueA + valueB;
        }



        /*public override Vector3D GetLayerValue(int layerColumn, int layerRow)
        {
            if (_values == null)
                return CalculateGeometryNormal(layerColumn, layerRow);

            return base.GetLayerValue(layerColumn, layerRow);
        }
        */


        /*protected internal override void Initialize(SurfaceEntity surface)
        {
            base.Initialize(surface);

            _heightLayer = surface.GetLayer<HeightLayer>();
        }*/


        /*public Vector3D GetNormal(int surfaceColumn, int surfaceRow)
        {
            return CalculateGeometryNormal(surfaceColumn, surfaceRow);
        }*/



        public override SurfaceLayer CreateEmpty(int numColumns, int numRows)
        {
            return new NormalLayer(new Vector3D[numColumns, numRows]);
        }



        protected override Vector3D InvertValue(Vector3D value)
        {
            return -value;
        }



        protected override Vector3D Minus(Vector3D valueA, Vector3D valueB)
        {
            return valueA - valueB;
        }



        protected override Vector3D Multiply(Vector3D value1, Vector3D value2)
        {
            //TODO: review
            return value1;
        }



        protected override Vector3D MultiplyScalar(Vector3D value, float scalar)
        {
            return value * scalar;
        }


        public override void Update()
        {
            //do nothing
        }
    }
}