using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;

namespace Sceelix.Meshes.Data
{
    [Entity("Edge", TypeBrowsable = false)]
    public class Edge : Entity
    {
        public enum EdgeIntersectionResult
        {
            NonIntersecting,
            Coincident,
            IntersectingV0,
            IntersectingV1,
            IntersectingMiddle
        }



        public Edge(Vertex vertex1, Vertex vertex2)
        {
            V0 = vertex1;
            V1 = vertex2;

            //could also be done this way..for manifold faces!
            //HalfVertex halfVertex1 = vertex1.HalfVertices.Find(x => x.Next == _vertex2);
            FillAttributes(vertex1, vertex2);
            FillAttributes(vertex2, vertex1);

            IEnumerable<Face> vertex1Faces = vertex1.HalfVertices.Select(x => x.Face);
            IEnumerable<Face> vertex2Faces = vertex2.HalfVertices.Select(x => x.Face);

            AdjacentFaces = new ReadOnlyCollection<Face>(vertex1Faces.Intersect(vertex2Faces).ToList());
        }



        public ReadOnlyCollection<Face> AdjacentFaces
        {
            get;
        }


        public Vector3D Center => (V0.Position + V1.Position) / 2f;


        public Vector3D Direction => (V1.Position - V0.Position).Normalize();



        public IEnumerable<HalfVertex> HalfVertices
        {
            get { return V0.HalfVertices.Where(x => x.Next == V1).Union(V1.HalfVertices.Where(x => x.Next == V0)); }
        }



        public bool IsBoundary => AdjacentFaces.Count == 1;


        public bool IsValid => V0.Position != V1.Position;


        public float Length => (V0.Position - V1.Position).Length;


        public Line3D Line => new Line3D(V1.Position - V0.Position, V0.Position);


        public LineSegment2D LineSegment2D => new LineSegment2D(V0.Position.ToVector2D(), V1.Position.ToVector2D());


        public LineSegment3D LineSegment3D => new LineSegment3D(V0.Position, V1.Position);



        /// <summary>
        /// Normal to the edge, obtained by averaging the normals of the adjacent faces.
        /// </summary>
        public Vector3D Normal
        {
            get { return AdjacentFaces.Select(x => x.Normal).Aggregate((y, yresult) => y + yresult) / AdjacentFaces.Count; }
        }



        public Vertex V0
        {
            get;
        }


        public Vertex V1
        {
            get;
        }



        public IEnumerable<Vertex> Vertices
        {
            get
            {
                yield return V0;
                yield return V1;
            }
        }



        /// <summary>
        /// Checks if this edge shares the same vertices with the given edge
        /// (regardless if they are the source or the target vertices). 
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <returns></returns>
        public bool CoincidentWith(Edge edge)
        {
            return V0.Equals(edge.V0) && V1.Equals(edge.V1)
                   || V0.Equals(edge.V1) && V1.Equals(edge.V0);
        }



        /*public class EdgeLine3DIntersection
        {

            private Vector3D position;
        }*/


        /*public EdgeIntersectionResult LineIntersection(Line3D line, out Vector3D intersectionPoint)
        {
            Vector3D p1 = V0.Position;
            Vector3D p2 = line.Point0;
            Vector3D dir1 = V1.Position - p1;
            Vector3D dir2 = line.Direction;

            Vector3D v1v2 = dir1.Cross(dir2);

            if (v1v2.ExactlyEquals(Vector3D.Zero))
            {
                intersectionPoint = Vector3D.Infinity;
                return EdgeIntersectionResult.Coincident;
            }


            Vector3D p1p2 = p2 - p1;
            Vector3D p1p2dir2 = p1p2.Cross(dir2);

            if (!v1v2.IsCollinear(p1p2dir2))
            {
                intersectionPoint = Vector3D.Infinity;
                return EdgeIntersectionResult.NonIntersecting;
            }


            //REFACTORING NEEDED!!!
            float a = Vector3D.GetCommonMultiplier(p1p2dir2, v1v2);

            Vector3D aV1p1p2 = dir1*a - p1p2;

            double b = Vector3D.GetCommonMultiplier(aV1p1p2, dir2);

            if (Math.Abs(a) < Double.Epsilon)
            {
                intersectionPoint = p1 + dir1*a;
                return EdgeIntersectionResult.IntersectingV0;
            }
            else if (Math.Abs(a - 1) < Double.Epsilon)
            {
                intersectionPoint = p1 + dir1*a;
                return EdgeIntersectionResult.IntersectingV1;
            }

            if (a > 0 && a < 1)
            {
                intersectionPoint = p1 + dir1*a;
                return EdgeIntersectionResult.IntersectingMiddle;
            }

            intersectionPoint = Vector3D.Infinity;

            return EdgeIntersectionResult.NonIntersecting;
            

            //intersectionPoint = Vector3D.Infinity;

            //return EdgeLine3DIntersection.NonIntersecting;
        }*/



        /// <summary>
        /// REFACTORING NEEDED!!!
        /// http://mathforum.org/library/drmath/view/62814.html
        /// </summary>
        /// <param name="otherEdge"></param>
        /// <param name="includingEnds"></param>
        /// <returns></returns>
        /*public Vector3D? EdgeIntersection(Edge otherEdge, bool includingEnds)
        {
            Vector3D p1 = V0.Position;
            Vector3D p2 = otherEdge.V0.Position;
            Vector3D v1 = V1.Position - p1;
            Vector3D v2 = otherEdge.V1.Position - p2;

            Vector3D v1v2 = v1.Cross(v2);

            //if (v1v2.ExactlyEquals(Vector3D.Zero))
            //    return Vector3D.Infinity;

            Vector3D p1p2 = p2 - p1;
            Vector3D p1p2v2 = p1p2.Cross(v2);

            if (!v1v2.IsCollinear(p1p2v2))
                return null;

            //REFACTORING NEEDED!!!
            float a = Vector3D.GetCommonMultiplier(p1p2v2, v1v2);

            Vector3D aV1p1p2 = v1*a - p1p2;

            double b = Vector3D.GetCommonMultiplier(aV1p1p2, v2);

            if (includingEnds)
            {
                if (a >= 0 && a <= 1 && b >= 0 && b <= 1)
                    return p1 + v1*a;

                return null;
            }

            if (a > 0 && a < 1 && b > 0 && b < 1)
                return p1 + v1*a;

            return null;
        }*/
        public override IEntity DeepClone()
        {
            return new Edge(new Vertex(V0), new Vertex(V1));
        }



        private void FillAttributes(Vertex source, Vertex target)
        {
            foreach (KeyValuePair<object, object> keyValuePair in source.HalfVertices.Where(x => x.Next == target).SelectMany(y => y.Attributes).Where(x => x.Key is AttributeKey))
                if (!Attributes.ContainsKey(keyValuePair.Key))
                    Attributes.Add(keyValuePair.Key.Clone(), keyValuePair.Value.Clone());
        }



        /*public bool Intersects(Edge otherEdge, bool includingEnds)
        {
            Vector3D p1 = V0.Position;
            Vector3D p2 = V1.Position;
            Vector3D p3 = otherEdge.V0.Position;
            Vector3D p4 = otherEdge.V1.Position;

            if ((p1 == p3 && p2 == p4) || (p1 == p4 && p2 == p3))
                return true;

            if ((p1 == p3 || p1 == p4 || p2 == p3 || p1 == p4))
                return includingEnds;

            return Triangle.SameSide(p4, p3, p1, p2) && Triangle.SameSide(p4, p3, p2, p1) && Triangle.OtherSide(p4, p1, p2, p3);
        }*/


        /*public bool IntersectsEdge2(Edge otherEdge, bool includingEnds)
        {
            Vector3D p1 = this.V0.Position;
            Vector3D p3 = otherEdge.V0.Position;

            Vector3D v21 = Direction;
            Vector3D v43 = otherEdge.Direction;
            Vector3D v13 = p1 - p3;

            if (Direction.Length < float.Epsilon || otherEdge.Direction.Length < float.Epsilon)
                return null;

            double d1343 = v13.Dot(v43);
            double d4321 = v43.Dot(v21);
            double d1321 = v13.X * (double)v21.X + (double)v13.Y * v21.Y + (double)v13.Z * v21.Z;
            double d4343 = v43.X * (double)v43.X + (double)v43.Y * v43.Y + (double)v43.Z * v43.Z;
            double d2121 = v21.X * (double)v21.X + (double)v21.Y * v21.Y + (double)v21.Z * v21.Z;

            double denom = d2121 * d4343 - d4321 * d4321;
            if (Math.Abs(denom) < float.Epsilon)
            {
                return null;
            }
            double numer = d1343 * d4321 - d1321 * d4343;

            double mua = numer / denom;
            double mub = (d1343 + d4321 * (mua)) / d4343;

            return new LineSegment3D(this[(float)mua], line[(float)mub]);
        }*/



        /// <summary>
        /// Verifies if a 3D edge intersects another.
        /// http://mathforum.org/library/drmath/view/62814.html
        /// </summary>
        /// <param name="otherEdge"></param>
        /// <param name="includingEnds"></param>
        /// <returns></returns>
        public bool Intersects(Edge otherEdge, bool includingEnds)
        {
            return LineSegment3D.Intersects(otherEdge.LineSegment3D, includingEnds);

            //if ((V0 == otherEdge.V0 && V1 == otherEdge.V1 || V0 == otherEdge.V1 && V1 == otherEdge.V0))
            //if (IsCoincident(otherEdge))
            //    return true;


            /*Vector3D p1 = V0.Position;
            Vector3D p2 = otherEdge.V0.Position;
            Vector3D v1 = V1.Position - p1;
            Vector3D v2 = otherEdge.V1.Position - p2;

            Vector3D v1v2 = v1.Cross(v2);

            if (v1v2.ExactlyEquals(Vector3D.Zero))
                return true;

            Vector3D p1p2 = p2 - p1;
            Vector3D p1p2v2 = p1p2.Cross(v2);

            if (!v1v2.IsCollinear(p1p2v2))
                return false;

            //REFACTORING NEEDED!!!
            float a = Vector3D.GetCommonMultiplier(p1p2v2, v1v2);

            Vector3D aV1p1p2 = v1*a - p1p2;

            double b = Vector3D.GetCommonMultiplier(aV1p1p2, v2);

            if (includingEnds)
            {
                if (a >= 0 && a <= 1 && b >= 0 && b <= 1)
                    return true;

                return false;
            }

            if (a > 0 && a < 1 && b > 0 && b < 1)
                return true;

            return false;*/
        }



        /*public void AttachToVertices()
        {
            Source.StreetEdges.Add(this);
            Target.StreetEdges.Add(this);
        }


        public void DetachFromVertices()
        {
            Source.StreetEdges.Remove(this);
            Target.StreetEdges.Remove(this);
        }*/



        public Vertex OtherVertex(Vertex vertex)
        {
            if (vertex == V0)
                return V1;

            if (vertex == V1)
                return V0;

            return null;
        }



        public EdgeIntersectionResult PlaneIntersection(Plane3D plane, out Vector3D intersectionPoint)
        {
            Line3D line = Line;

            float? intersectsPlane = line.IntersectsPlane(plane);
            if (intersectsPlane.HasValue)
            {
                float value = intersectsPlane.Value;
                //Console.WriteLine(value);
                intersectionPoint = line[value];

                if (Math.Abs(value) < Vector3D.Precision) return EdgeIntersectionResult.IntersectingV0;

                if (Math.Abs(value - 1) < Vector3D.Precision) return EdgeIntersectionResult.IntersectingV1;

                if (value > 0 && value < 1)
                {
                    return EdgeIntersectionResult.IntersectingMiddle;
                }

                intersectionPoint = Vector3D.NaN;
                return EdgeIntersectionResult.NonIntersecting;
            }

            //may be parallel or coincident
            if (plane.PointInPlane(line.Point0))
            {
                intersectionPoint = Vector3D.Infinity;
                return EdgeIntersectionResult.Coincident;
            }

            intersectionPoint = Vector3D.NaN;
            return EdgeIntersectionResult.NonIntersecting;
        }



        /// <summary>
        /// The equality comparer for edges that verifies if two edges are coincident 
        /// (have the same vertices, but could have different directions).
        /// </summary>
        public class CoincidencyEqualityComparer : IEqualityComparer<Edge>
        {
            public bool Equals(Edge x, Edge y)
            {
                return x.CoincidentWith(y);
            }



            public int GetHashCode(Edge obj)
            {
                var h0 = obj.V0.GetHashCode();
                var h1 = obj.V1.GetHashCode();

                return h0 + h1;
            }
        }
    }
}