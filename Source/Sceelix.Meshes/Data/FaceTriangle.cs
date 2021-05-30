using System.Collections.Generic;

namespace Sceelix.Meshes.Data
{
    public class FaceTriangle : Triangle
    {
        public FaceTriangle(Vertex v0, Vertex v1, Vertex v2)
            : base(v0, v1, v2)
        {
        }



        public FaceTriangle(List<Vertex> vertices)
            : base(vertices)
        {
        }



        public Face Face
        {
            get;
            set;
        }



        public float CalculateArea()
        {
            //http://math.stackexchange.com/questions/128991/how-to-calculate-area-of-3d-triangle
            return (V0.Position - V1.Position).Cross(V0.Position - V2.Position).Length / 2f;
        }
    }
}