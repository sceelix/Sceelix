using Sceelix.Core.Handles;
using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Handles
{
    public class SurfaceManipulateHandle : VisualHandle
    {
        public SurfaceManipulateHandle(Vector3D position, float cellSize, float[,] heights, float[] diffValues, string fileName)
        {
            Position = position;
            CellSize = cellSize;
            Heights = heights;
            DiffValues = diffValues;
            FileName = fileName;
        }



        public float CellSize
        {
            get;
        }


        public float[] DiffValues
        {
            get;
        }


        public string FileName
        {
            get;
        }


        public float[,] Heights
        {
            get;
        }


        public Vector3D Position
        {
            get;
        }
    }
}