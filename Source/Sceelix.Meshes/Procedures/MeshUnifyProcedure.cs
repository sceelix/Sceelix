using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Algorithms;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Extensions;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Unifies faces and vertices of meshes.
    /// </summary>
    [Procedure("a66e6c1f-3448-4813-a959-6c4fb3d01ea7", Label = "Mesh Unify", Category = "Mesh")]
    public class MeshUnifyProcedure : TransferProcedure<MeshEntity>
    {
        /// <summary>
        /// Type of unification operation to perform.
        /// </summary>
        private readonly SelectListParameter<MeshUnifyParameter> _parameterUnify = new SelectListParameter<MeshUnifyParameter>("Operations", "Unify Vertices");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterUnify.SubParameterLabels);



        protected override MeshEntity Process(MeshEntity meshEntity)
        {
            foreach (MeshUnifyParameter transformMeshParameter in _parameterUnify.Items)
                meshEntity = transformMeshParameter.Transform(meshEntity);

            return meshEntity;
        }



        #region Abstract Parameter

        public abstract class MeshUnifyParameter : CompoundParameter
        {
            protected MeshUnifyParameter(string label)
                : base(label)
            {
            }



            public abstract MeshEntity Transform(MeshEntity meshEntity);
        }

        #endregion

        #region Unify Planar Faces

        /// <summary>
        /// Unifies faces that lie on the same plane. Vertices should be shared, so applying a vertex unification
        /// first may be necessary.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUnifyProcedure.MeshUnifyParameter" />
        public class UnifyPlanarFacesParameter : MeshUnifyParameter
        {
            //private readonly FloatParameter _parameterAngleTolerance = new FloatParameter("Angle Tolerance", 0.1f);



            public UnifyPlanarFacesParameter()
                : base("Unify Planar Faces")
            {
            }



            private void CreateFacesFromEdges(MeshEntity meshEntity, Queue<Edge> edges, List<EdgeSequence> sequencesToAdd)
            {
                while (edges.Count > 0)
                {
                    Queue<Edge> auxiliaryQueue = new Queue<Edge>();

                    EdgeSequence sequence = new EdgeSequence(edges.Dequeue());
                    while (edges.Count > 0)
                    {
                        Edge secondEdge = edges.Dequeue();

                        //try add it to the chain
                        //if it does not match, add it to the auxiliary queue, so that it will be considered in the next round
                        if (sequence.TryAdd(secondEdge))
                        {
                            if (sequence.IsClosed)
                            {
                                sequencesToAdd.Add(sequence);

                                //add the remaining to the auxiliary
                                foreach (Edge edge in edges)
                                    auxiliaryQueue.Enqueue(edge);

                                break;
                            }

                            //reset the list
                            foreach (Edge edge in edges)
                                auxiliaryQueue.Enqueue(edge);

                            edges = auxiliaryQueue;
                            auxiliaryQueue = new Queue<Edge>();
                        }
                        else
                        {
                            auxiliaryQueue.Enqueue(secondEdge);
                        }
                    }

                    edges = new Queue<Edge>(auxiliaryQueue);
                }
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                //List<Face> totalMeshFaces = new List<Face>();

                var groupingByPlane = meshEntity.GroupBy(val => new {Normal = val.Normal.Round(), DistanceToPoint = Math.Round(val.Plane.DistanceToPoint(Vector3D.Zero), 3)}).ToList();
                foreach (var group in groupingByPlane)
                {
                    var faces = group.ToList();
                    if (faces.Count == 1)
                        continue;
                    foreach (Face face in faces) meshEntity.RemoveAndDetach(face);

                    var alignedScope = faces.First().GetAlignedScope();
                    Clipper clipper = new Clipper();


                    for (int index = 0; index < faces.Count; index++)
                    {
                        Face face = faces[index];

                        var sourceIntList = face.Vertices.Select(x => x.Position.ToIntPoint(alignedScope)).ToList();

                        var clip = index > 0 ? PolyType.ptClip : PolyType.ptSubject;

                        //var polyType = face.GetLocalAttribute<bool>("IsTarget", this) ? PolyType.ptClip : PolyType.ptSubject;

                        clipper.AddPath(sourceIntList, clip, true);

                        if (face.HasHoles)
                            foreach (CircularList<Vertex> circularList in face.Holes)
                                clipper.AddPath(circularList.Select(x => x.Position.ToIntPoint(alignedScope)).ToList(), clip, true);
                    }

                    PolyTree result = new PolyTree();
                    clipper.Execute(ClipType.ctUnion, result);

                    var resultingFaces = result.PolyTreeToFaceList(alignedScope);

                    meshEntity.AddRange(resultingFaces);
                }

                return meshEntity;
            }



            public MeshEntity Transform2(MeshEntity meshEntity)
            {
                //List<Edge> edges = 
                List<Face> facesToDelete = new List<Face>();
                List<EdgeSequence> sequencesToAdd = new List<EdgeSequence>();

                //group faces by their normal. If the normal is the same
                foreach (IGrouping<Vector3D, Face> grouping in meshEntity.GroupBy(val => val.Normal.Round()))
                {
                    //skip "groups" of faces that haven't been grouped at all
                    if (grouping.Count() < 2)
                        continue;

                    List<Face> faces = grouping.ToList();

                    //get all the edges from all the faces of this group, but exclude the edges that are shared
                    IEnumerable<Edge> edges = grouping.SelectMany(val => val.Edges);

                    //IEnumerable<Edge> isolatedEdges = edges.Where(val => val.AdjacentFaces.Count(face => face.Normal.Round().Equals(grouping.Key)) < 2);
                    IGrouping<Vector3D, Face> grouping1 = grouping;
                    IEnumerable<Edge> isolatedEdges = edges.Where(val => val.AdjacentFaces.Intersect(grouping1).Count() < 2);

                    CreateFacesFromEdges(meshEntity, new Queue<Edge>(isolatedEdges), sequencesToAdd);

                    facesToDelete.AddRange(grouping);
                }

                //add the new faces to the MeshEntity
                foreach (EdgeSequence sequence in sequencesToAdd)
                {
                    Face face = sequence.Face;
                    if (face != null)
                        meshEntity.Add(face);
                }

                //and delete the old ones
                foreach (Face face in facesToDelete) meshEntity.RemoveAndDetach(face);

                return meshEntity;
            }



            private class EdgeSequence
            {
                private readonly List<Vertex> _vertices = new List<Vertex>();



                public EdgeSequence(Edge startSegment)
                {
                    _vertices.Add(startSegment.V0);
                    _vertices.Add(startSegment.V1);
                }



                public Face Face
                {
                    get
                    {
                        Face face = new Face(_vertices);

                        if (face.Normal.Dot(Vector3D.ZVector) < 0) return null;

                        return face;
                    }
                }



                public bool IsClosed => _vertices.Count > 2 && _vertices.Last() == _vertices.First();



                public bool TryAdd(Edge secondLine)
                {
                    if (_vertices.Last() == secondLine.V0)
                    {
                        _vertices.Add(secondLine.V1);
                        return true;
                    }

                    if (_vertices.First() == secondLine.V1)
                    {
                        _vertices.Insert(0, secondLine.V0);
                        return true;
                    }

                    return false;
                }
            }
        }

        #endregion

        #region Unify Vertices

        /// <summary>
        /// Merges mesh vertices that are overlapping (or positioned close together) into the same vertex reference.<br/>
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUnifyProcedure.MeshUnifyParameter" />
        public class UnifyVerticesParameter : MeshUnifyParameter
        {
            /// <summary>
            /// The distance tolerance used for determining if two vertices should be joined.
            /// </summary>
            private readonly FloatParameter _parameterTolerance = new FloatParameter("Tolerance", 0.01f);



            public UnifyVerticesParameter()
                : base("Unify Vertices")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                if (_parameterTolerance.Value > 0)
                    Unify(meshEntity, _parameterTolerance.Value);
                else
                    Unify(meshEntity);

                return meshEntity;
            }



            public static void Unify(MeshEntity meshEntity)
            {
                Dictionary<Vector3D, Vertex> vertices = new Dictionary<Vector3D, Vertex>();
                List<Vertex> vertexList = meshEntity.FaceVertices.ToList();

                foreach (Vertex vertex in vertexList)
                {
                    Vertex existingVertex;

                    if (vertices.TryGetValue(vertex.Position, out existingVertex))
                    {
                        //also, check if the vertex is not the same, of course!
                        if (vertex != existingVertex)
                            vertex.MergeInto(existingVertex);
                    }
                    else
                    {
                        vertices.Add(vertex.Position, vertex);
                    }
                }
            }



            /// <summary>
            /// Much slower approach! Needs a quadtree, at least.
            /// </summary>
            /// <param name="meshEntity"></param>
            /// <param name="maximumDistance"></param>
            public static void Unify(MeshEntity meshEntity, float maximumDistance)
            {
                HashSet<Vertex> leadingVertices = new HashSet<Vertex>();
                List<Vertex> vertexList = meshEntity.FaceVertices.ToList();

                foreach (Vertex vertex in vertexList)
                    if (!leadingVertices.Contains(vertex))
                    {
                        bool gotMerged = false;

                        foreach (Vertex leadingVertex in leadingVertices)
                        {
                            //measures the distance to every other point (not really efficient, but...) and if it is lower than this HARDCODED VALUE (:P), merge it to the existing vertex
                            float length = (vertex.Position - leadingVertex.Position).Length;
                            if (length < maximumDistance)
                            {
                                vertex.MergeInto(leadingVertex);
                                gotMerged = true;
                                break;
                            }
                        }

                        if (!gotMerged)
                            leadingVertices.Add(vertex);
                    }
            }
        }

        #endregion

        #region Remove Coincident Faces

        /// <summary>
        /// Removes faces that may be connecting the same vertices.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUnifyProcedure.MeshUnifyParameter" />
        public class RemoveCoincidentFacesParameter : MeshUnifyParameter
        {
            public RemoveCoincidentFacesParameter()
                : base("Remove Coincident Faces")
            {
            }



            private bool AreCoincident(List<Vertex> first, List<Vertex> second)
            {
                int count1 = first.Count();
                int count2 = second.Count();

                if (count1 != count2)
                    return false;

                int count = first.Intersect(second).Count();

                return count == count1;
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                HashSet<Face> facesForDeletion = new HashSet<Face>();

                for (int i = 0; i < meshEntity.Faces.Count; i++)
                for (int j = i + 1; j < meshEntity.Faces.Count; j++)
                    if (AreCoincident(meshEntity.Faces[i].Vertices.ToList(), meshEntity.Faces[j].Vertices.ToList())) //.IsCoincidentWith(MeshEntity.Faces[j])
                    {
                        facesForDeletion.Add(meshEntity.Faces[i]);
                        facesForDeletion.Add(meshEntity.Faces[j]);
                        break;
                    }

                foreach (Face face in facesForDeletion) meshEntity.RemoveAndDetach(face);

                return meshEntity;
            }
        }

        #endregion
    }
}