using System.Collections.Generic;
using Sceelix.Mathematics.Data;

namespace Sceelix.Meshes.Data
{
    public class Triangle
    {
        protected Vector3D _normal;
        protected List<Vertex> vertices;



        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            vertices = new List<Vertex>(new[] {v0, v1, v2});

            CalculateNormal();
        }



        public Triangle(IEnumerable<Vertex> vertices)
        {
            this.vertices = new List<Vertex>(vertices);

            CalculateNormal();
        }



        public Vector3D Normal => _normal;


        public Vertex V0 => vertices[0];


        public Vertex V1 => vertices[1];


        public Vertex V2 => vertices[2];



        public List<Vertex> Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }



        private void CalculateNormal()
        {
            _normal = Vector3D.Cross(V2.Position - V0.Position, V1.Position - V0.Position);
            _normal = _normal.Normalize();
        }



        public static bool OtherSide(Vector3D p1, Vector3D a, Vector3D b, Vector3D c)
        {
            Vector3D cp1 = Vector3D.Cross(b - a, p1 - a);
            Vector3D cp2 = Vector3D.Cross(c - a, b - a);

            if (Vector3D.IsCollinear(cp1, cp2) && Vector3D.Dot(cp1, cp2) >= 0)
                //Console.WriteLine(Vector3.Dot(cp1, cp2));
                //if ()
                return true;

            return false;
        }



        // http://softsurfer.com/Archive/algorithm_0104/algorithm_0104.htm
        //    Input:  P = a 3D point
        //            PL = a plane with point V0 and normal n
        //    Output: *B = base point on PL of perpendicular from P
        //    Return: the distance from P to the plane PL
        private double PointDistanceToPlane(Vector3D point)
        {
            float sn = -Vector3D.Dot(_normal, point - V0.Position);
            float sd = Vector3D.Dot(_normal, _normal);
            float sb = sn / sd;

            Vector3D pointPerpendicular = point + _normal * sb;

            return Vector3D.Distance(point, pointPerpendicular);
        }



        public bool PointInTriangle(Vector3D p)
        {
            //first check if the point is on the same plane
            //if (PointDistanceToPlane(p) > 0)
            //    return false;

            //now, check for the boundaries of the triangle
            if (SameSide(p, V0.Position, V1.Position, V2.Position) &&
                SameSide(p, V1.Position, V2.Position, V0.Position) &&
                SameSide(p, V2.Position, V0.Position, V1.Position))
                return true;

            return false;
        }



        /// <summary>
        /// 
        /// http://www.blackpawn.com/texts/pointinpoly/default.html
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool SameSide(Vector3D p1, Vector3D a, Vector3D b, Vector3D c)
        {
            Vector3D cp1 = Vector3D.Cross(b - a, p1 - a);
            Vector3D cp2 = Vector3D.Cross(b - a, c - a);

            //if (Vector3.Dot(cp1, cp2) >= 0)
            //Console.WriteLine(Vector3.Dot(cp1, cp2));
            //if (Vector3.IsCollinear(cp1,cp2))
            if (Vector3D.IsCollinear(cp1, cp2) && Vector3D.Dot(cp1, cp2) >= 0)
                return true;

            return false;
        }
    }
}