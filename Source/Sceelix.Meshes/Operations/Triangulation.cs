using System;
using System.Collections.Generic;
using System.Linq;
using LibTessDotNet;
using Sceelix.Collections;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Operations
{
    public static class Triangulation
    {
        public static bool StandardTriangulation = false;

        private static readonly object LockObject = new object();



        /// <summary>
        /// Adds a contour of a polygon to the external library
        /// </summary>
        /// <param name="tess"></param>
        /// <param name="vertices"></param>
        /// <param name="angle"></param>
        /// <param name="hole"></param>
        /// <param name="rotationMatrix"></param>
        /// <returns></returns>
        private static IEnumerable<TriangulationEdge> AddContourToLibTess(Tess tess, CircularList<Vertex> vertices, Matrix? rotationMatrix, bool hole = false)
        {
            ContourVertex[] contourVertices;

            if (rotationMatrix.HasValue)
                contourVertices = vertices.Select(vertex => new ContourVertex {Position = ToVec3(rotationMatrix.Value.Transform(vertex.Position)), Data = vertex}).ToArray();
            else
                contourVertices = vertices.Select(vertex => new ContourVertex {Position = ToVec3(vertex.Position), Data = vertex}).ToArray();

            //return triangulation edges that we can use to do interpolations, if needed
            for (int i = 1; i < contourVertices.Length; i++)
                yield return new TriangulationEdge((Vertex) contourVertices[i - 1].Data, (Vertex) contourVertices[i].Data, ToVector3D(contourVertices[i - 1].Position), ToVector3D(contourVertices[i].Position));

            tess.AddContour(contourVertices, hole ? ContourOrientation.CounterClockwise : ContourOrientation.Clockwise);

            yield return new TriangulationEdge((Vertex) contourVertices.Last().Data, (Vertex) contourVertices.First().Data, ToVector3D(contourVertices.Last().Position), ToVector3D(contourVertices.First().Position));
            //yield break;
        }



        private static Vertex CreateInterpolatedVertex(Face face, Vector3D position, List<TriangulationEdge> edges)
        {
            foreach (TriangulationEdge triangulationEdge in edges)
                if (triangulationEdge.ContainsPoint(position))
                    return triangulationEdge.InterpolatePoint(face, position);

            return null;
        }



        private static Vec3 ToVec3(Vector3D position)
        {
            return new Vec3 {X = position.X, Y = position.Y, Z = position.Z};
        }



        private static Vector3D ToVector3D(Vec3 position)
        {
            return new Vector3D(position.X, position.Y, position.Z);
        }



        /// <summary>
        /// Triangulates a polygon with holes.
        /// </summary>
        /// <param name="face">Face to be triangulated</param>
        /// <returns>List of triangles</returns>
        public static List<FaceTriangle> Triangulate(this Face face)
        {
            //if it's already a triangle, just return that
            if (face.Vertices.Count() == 3 && !face.HasHoles)
                return new List<FaceTriangle>(new[] {new FaceTriangle(face[0], face[1], face[2])});

            //if it's convex and has no holes, it's easy and fast
            if (face.IsConvex && !face.HasHoles)
                return TriangulateConvex(face);

            //otherwise, use LibTessDotNet to do the triangulation

            if (StandardTriangulation)
                return DivideAndConquerTriangulation.Triangulate(face);
            lock (LockObject)
            {
                return TriangulateLibTessDotNet(face);
            }
        }



        /// <summary>
        /// Does a simple triangulation for convex triangles
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private static List<FaceTriangle> TriangulateConvex(Face face)
        {
            List<FaceTriangle> triangles = new List<FaceTriangle>();

            //takes the first vertex
            Vertex v1 = face[0];

            // and connects to all the remaining ones
            for (int i = 1; i < face.Vertices.Count() - 1; i++)
                triangles.Add(new FaceTriangle(v1, face[i], face[i + 1]) {Face = face});

            return triangles;
        }



        /// <summary>
        /// Uses a external library to do the triangulation
        /// Does a more complex triangulation on concave and holed polygon
        /// </summary>
        /// <param name="face"></param>
        /// <returns>List of face triangles</returns>
        private static List<FaceTriangle> TriangulateLibTessDotNet(Face face)
        {
            List<FaceTriangle> triangles = new List<FaceTriangle>();

            //try
            //{
            Tess tess = new Tess();

            Matrix? rotationMatrix = null;

            List<TriangulationEdge> triangulationEdges = new List<TriangulationEdge>();

            if (!face.Normal.Equals(Vector3D.ZVector))
            {
                Vector3D rotationAxis = face.Normal.Cross(Vector3D.ZVector);
                if (rotationAxis.Equals(Vector3D.Zero))
                    rotationAxis = Vector3D.XVector;

                float angle = face.Normal.AngleTo(Vector3D.ZVector);

                rotationMatrix = Matrix.CreateAxisAngle(rotationAxis, angle);
            }

            // Adding contour of the face itself to the library
            triangulationEdges.AddRange(AddContourToLibTess(tess, new CircularList<Vertex>(face.Vertices),
                rotationMatrix));

            // Adding contours of the holes to the library
            if (face.HasHoles)
                foreach (CircularList<Vertex> hole in face.Holes)
                    triangulationEdges.AddRange(AddContourToLibTess(tess, hole, rotationMatrix, true));


            // Triangulate
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            /*
             * Perhaps we should try to figure out first WHY the polygons are invalid, but...
            */
            for (int i = 0; i < tess.Vertices.Length; i++)
                //if a new vertex had to be created, we have to create complete new vertex data
                if (tess.Vertices[i].Data == null)
                {
                    Vec3 position = tess.Vertices[i].Position;
                    tess.Vertices[i].Data =
                        CreateInterpolatedVertex(face, ToVector3D(position), triangulationEdges);
                }

            // Convert results to FaceTriangles
            for (int i = 0; i < tess.ElementCount; i++)
            {
                List<Vertex> vertices = new List<Vertex>();
                for (int j = 0; j < 3; j++)
                {
                    int index = tess.Elements[i * 3 + j];
                    if (index == -1)
                        continue;

                    //var v = new Vertex(Vector3D.New(tess.Vertices[index].Position.X, tess.Vertices[index].Position.Y, tess.Vertices[index].Position.Z));
                    vertices.Add((Vertex) tess.Vertices[index].Data);
                }

                vertices.Reverse();
                if (vertices.All(val => val != null))
                    triangles.Add(new FaceTriangle(vertices));
            }
            /*}
            catch (Exception ex)
            {
                
            }*/


            return triangles;
        }
    }

    /// <summary>
    /// Auxiliary class for the triangulation of self-intersecting faces that result in the creation of new vertices, whose propertes need to be interpolated.
    /// </summary>
    internal class TriangulationEdge
    {
        private readonly Vector3D _rotatedV0Pos;
        private readonly Vector3D _rotatedV1Pos;
        private readonly Vertex _v0;
        private readonly Vertex _v1;



        public TriangulationEdge(Vertex v0, Vertex v1, Vector3D rotatedV0Pos, Vector3D rotatedV1Pos)
        {
            _v0 = v0;
            _v1 = v1;
            _rotatedV0Pos = rotatedV0Pos;
            _rotatedV1Pos = rotatedV1Pos;
        }



        public bool ContainsPoint(Vector3D position)
        {
            float dot = (position - _rotatedV0Pos).Normalize().Dot((position - _rotatedV1Pos).Normalize());
            return Math.Abs(dot + 1) < Vector3D.Precision;
        }



        public Vertex InterpolatePoint(Face face, Vector3D position)
        {
            return _v0.CreateInterpolatedVertex(face, position, _v1);
        }
    }
}