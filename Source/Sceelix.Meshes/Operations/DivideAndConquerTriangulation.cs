using System;
using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Operations
{
    public class DivideAndConquerTriangulation
    {
        private static CircularList<TriangulationVertex> ConnectHoles(CircularList<TriangulationVertex> triangulationVertices, List<CircularList<TriangulationVertex>> holes)
        {
            CircularList<TriangulationVertex> outerVertices = new CircularList<TriangulationVertex>(triangulationVertices);
            //CircularList<Vertex> outerVertices = new CircularList<Vertex>(face.Vertices);

            //get the line segments tht define the inner and outer boundaries
            List<LineSegment3D> holeEdges = new List<LineSegment3D>();
            foreach (CircularList<TriangulationVertex> vertexList in holes)
                for (int i = 0; i < vertexList.Count; i++)
                    holeEdges.Add(new LineSegment3D(vertexList[i].Position, vertexList[i + 1].Position));

            for (int i = 0; i < triangulationVertices.Count; i++)
                holeEdges.Add(new LineSegment3D(triangulationVertices[i].Position, triangulationVertices[i + 1].Position));


            //for each hole in the face
            foreach (CircularList<TriangulationVertex> vertexList in holes)
                //go over the vertices of the hole
                for (int i = 0; i < vertexList.Count; i++)
                    //and try to connect it to a vertex on the face
                for (int j = 0; j < outerVertices.Count; j++)
                {
                    LineSegment3D edgeCut = new LineSegment3D(vertexList[i].Position, outerVertices[j].Position);
                    if (!IntersectsEdges(holeEdges, edgeCut))
                    {
                        CircularList<TriangulationVertex> newList = vertexList.GetRangeAt(i, vertexList.Count - 1);
                        newList.AddRange(vertexList.GetRangeAt(0, i));
                        newList.Add(outerVertices[j]);

                        outerVertices.InsertRange(j + 1, newList);
                        holeEdges.Add(edgeCut);

                        //break the outer and inner loop
                        i = vertexList.Count;
                        break;
                    }
                }


            return outerVertices;
        }



        private static bool ContainedInPolygon(CircularList<TriangulationVertex> tempVertices, LineSegment3D edgeCut, KeyValuePair<TriangulationVertex, TriangulationVertex> edgeCutVertices, Vector3D faceNormal)
        {
            //checks if the cut would intersect with any existing edges
            for (int j = 0; j < tempVertices.Count; j++)
            {
                TriangulationVertex v0 = tempVertices[j];
                TriangulationVertex v1 = tempVertices[j + 1];

                if (v0 == edgeCutVertices.Key && v1 == edgeCutVertices.Value || v0 == edgeCutVertices.Value && v1 == edgeCutVertices.Key)
                    continue;

                LineSegment3D edgeTemp = new LineSegment3D(v0.Position, v1.Position);
                if (edgeCut.Intersects(edgeTemp, false)) return false;
            }

            return true;
        }



        public static bool ContainedInPolygon(CircularList<Vertex> tempVertices, Edge edgeCut, Vector3D faceNormal)
        {
            Plane3D baseCutPlane = new Plane3D(edgeCut.Direction, edgeCut.V0.Position);

            //checks if the cut would intersect with any existing edges
            for (int j = 0; j < tempVertices.Count; j++)
            {
                Vertex v0 = tempVertices[j];
                Vertex v1 = tempVertices[j + 1];

                if (v0 == edgeCut.V0 && v1 == edgeCut.V1 || v0 == edgeCut.V1 && v1 == edgeCut.V0)
                    continue;

                //if (v0 == edgeCut.V0 || v1 == edgeCut.V0 || v0 == edgeCut.V1 || v1 == edgeCut.V1)
                //    continue;


                /*Plane3D plane3D = new Plane3D((v1.Position - v0.Position).Cross(faceNormal), v0.Position);
                Vector3D intersectionPoint;
                Edge.EdgeIntersectionResult edgeIntersectionResult = edgeCut.PlaneIntersection(plane3D, out intersectionPoint);
                if (edgeIntersectionResult == Edge.EdgeIntersectionResult.IntersectingMiddle
                    || (edgeIntersectionResult == Edge.EdgeIntersectionResult.IntersectingV0 && !intersectionPoint.Equals(edgeCut.V0.Position) && !intersectionPoint.Equals(edgeCut.V1.Position))
                    || (edgeIntersectionResult == Edge.EdgeIntersectionResult.IntersectingV1 && !intersectionPoint.Equals(edgeCut.V1.Position) && !intersectionPoint.Equals(edgeCut.V0.Position)))
                {
                    Vector3D ax = intersectionPoint - v0.Position;
                    Vector3D bx = intersectionPoint - v1.Position;

                    if (ax.Dot(bx) > 0)
                    {
                        continue;
                    }

                    return false;
                }
                if (edgeIntersectionResult == Edge.EdgeIntersectionResult.Coincident)
                {
                    //
                    Vector3D ax = edgeCut.V0.Position - v0.Position;
                    Vector3D bx = edgeCut.V1.Position - v0.Position;
                    Vector3D ay = edgeCut.V0.Position - v1.Position;
                    Vector3D by = edgeCut.V1.Position - v1.Position;

                    float dot1 = ax.Dot(bx);
                    float dot2 = ax.Dot(ay);
                    float dot3 = ax.Dot(by);
                    //float dot4 = ay.Dot(by);

                    if ((dot1 >= 0 && dot2 >= 0 && dot3 >= 0) &&  (dot1 > 0 || dot2 > 0 || dot3 > 0))
                        continue;

                    //float val = ay.Dot(by);

                    return false;
                }*/


                Edge edgeTemp = new Edge(v0, v1);
                if (edgeCut.Intersects(edgeTemp, false)) return false;

                /*Vector3D? intersectionPoint = edgeCut.EdgeIntersection(edgeTemp, true);

                //if (intersectionPoint.HasValue && v0 != edgeCut.V0 && v1 != edgeCut.V0 && v0 != edgeCut.V1 && v1 != edgeCut.V1)
                /*if (intersectionPoint.HasValue)
                {
                    return false;
                }*/
            }

            return true;
        }



        private static CircularList<TriangulationVertex> CreateTriangulationVertices(Face face)
        {
            CircularList<TriangulationVertex> triangulationVertices = new CircularList<TriangulationVertex>();
            List<CircularList<TriangulationVertex>> holes = new List<CircularList<TriangulationVertex>>();

            face.RecalculateNormal();

            foreach (Vertex vertex in face.Vertices)
                triangulationVertices.Add(new TriangulationVertex(vertex, vertex.Position, triangulationVertices.Count));

            if (face.HasHoles)
                foreach (CircularList<Vertex> vertexList in face.Holes)
                {
                    CircularList<TriangulationVertex> holeVertices = new CircularList<TriangulationVertex>();
                    foreach (Vertex vertex in vertexList)
                        holeVertices.Add(new TriangulationVertex(vertex, vertex.Position, triangulationVertices.Count));

                    holes.Add(holeVertices);
                }

            //First: if necessary, rotate the face so that it stays in the xy plane
            if (!face.Normal.Equals(Vector3D.ZVector))
            {
                Vector3D rotationAxis = face.Normal.Cross(Vector3D.ZVector);
                if (rotationAxis.Equals(Vector3D.Zero))
                    rotationAxis = Vector3D.XVector;

                float angle = face.Normal.AngleTo(Vector3D.ZVector);

                Matrix matrix = Matrix.CreateAxisAngle(rotationAxis, angle);
                foreach (TriangulationVertex triangulationVertex in triangulationVertices)
                    triangulationVertex.Position = matrix.Transform(triangulationVertex.Position);

                foreach (CircularList<TriangulationVertex> circularList in holes)
                foreach (TriangulationVertex triangulationVertex in circularList)
                    triangulationVertex.Position = matrix.Transform(triangulationVertex.Position);
            }

            //to avoid precision issues, let's really reset the Z to 0
            //this also handles polygons that are not exactly planar
            foreach (TriangulationVertex triangulationVertex in triangulationVertices)
                triangulationVertex.Position = new Vector3D(triangulationVertex.Position.X, triangulationVertex.Position.Y, 0);

            foreach (CircularList<TriangulationVertex> circularList in holes)
            foreach (TriangulationVertex triangulationVertex in circularList)
                triangulationVertex.Position = new Vector3D(triangulationVertex.Position.X, triangulationVertex.Position.Y, 0);


            return face.HasHoles ? ConnectHoles(triangulationVertices, holes) : triangulationVertices;
        }



        public static bool IntersectsEdges(List<LineSegment3D> edges, LineSegment3D edgeCut)
        {
            //checks if the cut would intersect with any existing edges
            foreach (LineSegment3D edge in edges)
                if (edgeCut.Intersects(edge, false))
                    return true;

            return false;
        }



        /// <summary>
        /// Does a more complex triangulation on concave and holed polygon
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        internal static List<FaceTriangle> Triangulate(Face face)
        {
            CircularList<TriangulationVertex> triangulationVertices = CreateTriangulationVertices(face);

            List<FaceTriangle> triangles = new List<FaceTriangle>();

            //now, let's make a queue containing closed vertex lines
            Queue<CircularList<TriangulationVertex>> vertexListsList = new Queue<CircularList<TriangulationVertex>>();

            //add the original polygon line to this queue
            vertexListsList.Enqueue(triangulationVertices);

            //let's analyze one polygon line at the time
            while (vertexListsList.Count > 0)
            {
                CircularList<TriangulationVertex> tempVertices = vertexListsList.Dequeue();

                //if the line has 3 vertices, then make a triangle out of it
                if (tempVertices.Count == 3)
                {
                    //add to the list of triangles - this triangle is guaranteed
                    triangles.Add(new FaceTriangle(tempVertices[0].OriginalVertex, tempVertices[1].OriginalVertex, tempVertices[2].OriginalVertex) {Face = face});
                }
                //if it has more vertices, we'll have to cut it more...
                else
                {
                    bool hasCut = false;

                    //for each vertex, try to find one located at the opposite side, so probably at the middle of the polygon
                    for (int i = 0; (i < tempVertices.Count) & !hasCut; i++)
                    {
                        TriangulationVertex vertexBegin = tempVertices[i];

                        int middle = i + tempVertices.Count / 2;
                        int offset = 1;

                        //also, get the vertex before and after
                        TriangulationVertex vertexBeginMinusOne = tempVertices[i - 1];
                        TriangulationVertex vertexBeginPlusOne = tempVertices[i + 1];

                        //the vertex on the opposite side is located at the middle, by default
                        for (int k = 0; k < tempVertices.Count - 3; k++)
                        {
                            TriangulationVertex vertexMiddle = tempVertices[middle];

                            LineSegment3D edgeCut = new LineSegment3D(vertexBegin.Position, vertexMiddle.Position);

                            Vector3D vectorAfter = vertexBeginPlusOne.Position - vertexBegin.Position;
                            Vector3D vectorMiddle = vertexMiddle.Position - vertexBegin.Position;
                            Vector3D vectorBefore = vertexBeginMinusOne.Position - vertexBegin.Position;

                            //make sure the edge is inside the polygon
                            if (edgeCut.IsValid && vertexMiddle != vertexBeginMinusOne && vertexMiddle != vertexBeginPlusOne && Vector3D.Cross(vectorMiddle, vectorAfter).Normalize().Equals(Vector3D.ZVector) && Vector3D.Cross(vectorBefore, vectorMiddle).Normalize().Equals(Vector3D.ZVector)) //vectorBefore.Cross(vectorAfter).Normalize()
                                //checks if the cut would intersect with any existing edges
                                if (ContainedInPolygon(tempVertices, edgeCut, new KeyValuePair<TriangulationVertex, TriangulationVertex>(vertexBegin, vertexMiddle), face.Normal))
                                {
                                    vertexListsList.Enqueue(tempVertices.GetRangeAt(i, middle));
                                    vertexListsList.Enqueue(tempVertices.GetRangeAt(middle, i));
                                    hasCut = true;
                                    break;
                                }

                            //if it can't connect, the "middle" vertex will be set to the nearest vertex (so +1 vertex, -1 vertex, +2 vertex, -2 vertex, and so on)
                            middle += offset;
                            offset = -(offset + Math.Sign(offset));
                        }
                    }
                }
            }

            return triangles;
        }



        private class TriangulationVertex
        {
            public TriangulationVertex(Vertex originalVertex, Vector3D position, int faceIndex)
            {
                OriginalVertex = originalVertex;
                Position = position;
                FaceIndex = faceIndex;
            }



            public int FaceIndex
            {
                get;
            }


            public Vertex OriginalVertex
            {
                get;
            }


            public Vector3D Position
            {
                get;
                set;
            }
        }


        /*public static CircularList<Vertex> ConnectHoles(Face face)
        {
            CircularList<Vertex> outerVertices = new CircularList<Vertex>(face.Vertices);

            List<Edge> holeEdges = new List<Edge>();
            foreach (CircularList<Vertex> vertexList in face.Holes)
            {
                for (int i = 0; i < vertexList.Count; i++)
                {
                    holeEdges.Add(new Edge(vertexList.GetVertexAt(i), vertexList.GetVertexAt(i+1)));
                }
            }

            foreach (Edge edge in face.Edges)
            {
                holeEdges.Add(edge);
            }



            //for each hole in the face
            foreach (CircularList<Vertex> vertexList in face.Holes)
            {
                //go over the vertices of the hole
                for (int i = 0; i < vertexList.Count; i++)
                {
                    //and try to connect it to a vertex on the face
                    for (int j = 0; j < outerVertices.Count; j++)
                    {
                        Edge edgeCut = new Edge(vertexList.GetVertexAt(i), outerVertices.GetVertexAt(j));
                        if(!IntersectsEdges(holeEdges,edgeCut))
                        {
                            CircularList<Vertex> newList = vertexList.GetRangeAt(i, vertexList.Count-1);
                            newList.AddRange(vertexList.GetRangeAt(0,i));
                            newList.Add(outerVertices[j]);

                            outerVertices.InsertRange(j + 1, newList);
                            holeEdges.Add(edgeCut);

                            //break the outer and inner loop
                            i = vertexList.Count;
                            break;
                        }
                    }
                    
                }
            }



            return outerVertices;
        }


        public static bool IntersectsEdges(List<Edge> edges, Edge edgeCut)
        {
            //checks if the cut would intersect with any existing edges
            foreach (Edge edge in edges)
            {
                if (edgeCut.Intersects(edge, false))
                {
                    return true;
                }
            }

            return false;
        }*/
    }
}