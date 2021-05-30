using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Paths.Data;

namespace Sceelix.Paths.Procedures
{
    /// <summary>
    /// Applies sequences of transformations to the given input path.
    /// </summary>
    [Procedure("83c5de12-e6e5-49d6-ad38-5bfe4c3dfff4", Label = "Path Modify", Category = "Path")]
    public class PathModifyProcedure : TransferProcedure<PathEntity>
    {
        /// <summary>
        /// The operation to be applied to the mesh.
        /// </summary>
        private readonly ListParameter<PathModificationParameter> _parameterOperation = new ListParameter<PathModificationParameter>("Operation");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterOperation.SubParameterLabels);



        protected override PathEntity Process(PathEntity pathEntity)
        {
            foreach (PathModificationParameter transformMeshParameter in _parameterOperation.Items)
                pathEntity = transformMeshParameter.Transform(pathEntity);

            return pathEntity;
        }



        #region Abstract Parameter

        public abstract class PathModificationParameter : CompoundParameter
        {
            protected PathModificationParameter(string label)
                : base(label)
            {
            }



            public abstract PathEntity Transform(PathEntity pathEntity);
        }

        #endregion

        #region Increase Precision

        /// <summary>
        /// Increases the number of vertices in which the path is divided.
        /// </summary>
        /// <seealso cref="Sceelix.Paths.Procedures.PathModifyProcedure.PathModificationParameter" />
        public class IncreasePrecisionParameter : PathModificationParameter
        {
            /// <summary>
            /// The maximum size of each one of the path segments.
            /// </summary>
            private readonly FloatParameter _parameterMaxSize = new FloatParameter("Max Segment Size", 1);



            public IncreasePrecisionParameter()
                : base("Increase Precision")
            {
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                foreach (PathEdge pathEdge in pathEntity.Edges.ToList())
                {
                    PathEdge currentEdge = pathEdge;

                    Vector3D direction = pathEdge.Target.Position - pathEdge.Source.Position;
                    float distance = (pathEdge.Source.Position - pathEdge.Target.Position).Length;
                    if (distance > _parameterMaxSize.Value)
                    {
                        int numNewPoints = (int) (distance / _parameterMaxSize.Value);
                        Vector3D normalizedDirection = direction.Normalize();

                        //actual spacing, just in case the value is a float
                        float spacing = distance / numNewPoints; // + 1

                        for (int i = 0; i < numNewPoints - 1; i++)
                        {
                            PathVertex newVertex = new PathVertex(currentEdge.Source.Position + normalizedDirection * spacing);

                            //introduces the new vertex and returns the resulting edge
                            currentEdge = pathEntity.IntroduceVertex(currentEdge, newVertex).Last();
                        }
                    }
                }


                return pathEntity;
            }
        }

        #endregion

        #region Simplify

        /// <summary>
        /// Simplifies paths by removing vertices which do not introduce a noticeable detail. Only affects vertices shared by two and only two edges.
        /// </summary>
        public class SimplifyParameter : PathModificationParameter
        {
            /// <summary>
            /// The angle tolerance, in degrees. Vertices whose edge angles differ less than this value from 180 degrees (meaning that they are rather straight) are removed.
            /// </summary>
            private readonly FloatParameter _parameterAngleTolerance = new FloatParameter("Angle Tolerance", 0.1f);



            public SimplifyParameter()
                : base("Simplify")
            {
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                foreach (PathVertex vertex in pathEntity.Vertices.ToList())
                    if (vertex.Degree == 2)
                    {
                        var edge1Direction = vertex.Edges.First().GetDirectionFrom(vertex);
                        var edge2Direction = vertex.Edges.Last().GetDirectionFrom(vertex);

                        var angle = Math.Abs(MathHelper.ToDegrees(edge1Direction.AngleTo(edge2Direction)));
                        if (180 - angle < _parameterAngleTolerance.Value) pathEntity.RemoveVertex(vertex, true);
                    }

                return pathEntity;
            }
        }

        #endregion

        #region Smooth Path

        /// <summary>
        /// Smooths a path by introducing more vertices following a bezier function.
        /// </summary>
        /// <seealso cref="Sceelix.Paths.Procedures.PathModifyProcedure.PathModificationParameter" />
        public class SmoothParameter : PathModificationParameter
        {
            /// <summary>
            /// The number of smoothing steps to use. The higher, the smoother it can look.
            /// </summary>
            private readonly IntParameter _parameterSmoothSteps = new IntParameter("Smooth Steps", 1);

            /// <summary>
            /// The intensity of the smoothing.
            /// </summary>
            private readonly FloatParameter _parameterBend = new FloatParameter("Bend", 0.5f) {MinValue = 0, MaxValue = 1, Increment = 0.1f};



            public SmoothParameter()
                : base("Smooth")
            {
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                List<PathEdge> pathEdges = pathEntity.Edges.ToList();

                //foreach (var streetEdge in streetEdges)
                //    streetEdge.Marked = false;
                float[,] steps = new float[_parameterSmoothSteps.Value, 4];
                float increment = 1f / (_parameterSmoothSteps.Value + 1);
                for (int i = 0; i < _parameterSmoothSteps.Value; i++)
                {
                    var t = (i + 1) * increment;

                    //P(t) = (1 − 3t^2 + 2t^3) * P(0) +(3t^2 − 2t^3)*P(1) +(t − 2t^2 + t^3)*P'(0) + (-t^2 + t^3)*P'(1)
                    steps[i, 0] = 1 - 3 * t * t + 2 * t * t * t;
                    steps[i, 1] = 3 * t * t - 2 * t * t * t;
                    steps[i, 2] = t - 2 * t * t + t * t * t;
                    steps[i, 3] = -t * t + t * t * t;
                }


                foreach (PathEdge streetEdge in pathEdges)
                    //we need a sequence of 4 points
                    if (streetEdge.Source.Edges.Count == 2 && streetEdge.Target.Edges.Count == 2)
                    {
                        var previousEdge = streetEdge.Source.Edges.First(val => val != streetEdge);
                        var afterEdge = streetEdge.Target.Edges.First(val => val != streetEdge);

                        //the 2 main point, p1 and p2, and their previous and next points
                        var p0 = previousEdge.OtherVertex(streetEdge.Source).Position;

                        var p1 = streetEdge.Source.Position;
                        var p2 = streetEdge.Target.Position;

                        var p3 = afterEdge.OtherVertex(streetEdge.Target).Position;

                        //the tangents
                        var tang0 = (p2 - p0) * _parameterBend.Value;
                        var tang1 = (p3 - p1) * _parameterBend.Value;

                        PathEdge currentPathEdge = streetEdge;

                        for (int i = 0; i < _parameterSmoothSteps.Value; i++)
                        {
                            var position = p1 * steps[i, 0]
                                           + p2 * steps[i, 1]
                                           + tang0 * steps[i, 2]
                                           + tang1 * steps[i, 3];

                            //PathVertex pathVertex = ;
                            //pathEntity.Vertices.Add(pathVertex);

                            //introduce the new vertex
                            var subStreetEdges = pathEntity.IntroduceVertex(currentPathEdge, new PathVertex(position));
                            currentPathEdge = subStreetEdges[1];
                        }
                    }

                return pathEntity;
            }
        }

        #endregion

        #region Round Vertices

        /// <summary>
        /// Rounds the coordinate values of the path vertices, reducing possible mathematical errors in floating point values.
        /// </summary>
        /// <seealso cref="Sceelix.Paths.Procedures.PathModifyProcedure.PathModificationParameter" />
        public class RoundVerticesParameter : PathModificationParameter
        {
            /// <summary>
            /// Number of decimal cases to which the values should be rounded to.
            /// </summary>
            private readonly IntParameter _parameterPrecision = new IntParameter("Precision", 2) {MinValue = 0};



            public RoundVerticesParameter()
                : base("Round")
            {
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                var precision = _parameterPrecision.Value;

                foreach (var entityVertex in pathEntity.Vertices)
                    //round the position of the vertices to clear possible precision issues
                    entityVertex.Position = entityVertex.Position.Round(precision);

                return pathEntity;
            }
        }

        #endregion

        #region Blend

        /// <summary>
        /// Blends edges and vertices within paths. Intersecting edges result in shared vertices. Overlapping vertices are joined.
        /// </summary>
        /// <seealso cref="Sceelix.Paths.Procedures.PathModifyProcedure.PathModificationParameter" />
        public class BlendParameter : PathModificationParameter
        {
            public BlendParameter()
                : base("Blend")
            {
            }



            private void AddVertex(Dictionary<Vector3D, List<PathVertex>> vertices, PathVertex vertex)
            {
                List<PathVertex> value;

                if (!vertices.TryGetValue(vertex.Position, out value))
                {
                    value = new List<PathVertex>();
                    vertices.Add(vertex.Position, value);
                }


                value.Add(vertex);
            }



            private PathVertex CheckForExistingVertex(Vector3D intersection, StaticPathEdge first)
            {
                if (intersection.Equals(first.Source.Position))
                    return first.Source;

                if (intersection.Equals(first.Target.Position))
                    return first.Target;

                return null;
            }



            /// <summary>
            /// Step 1
            /// </summary>
            /// <param name="network"></param>
            /// <returns></returns>
            private Dictionary<Vector3D, List<PathVertex>> FindEdgeIntersections(PathEntity network)
            {
                //create a dictionary that will hold the cross points
                Dictionary<Vector3D, List<PathVertex>> vertices = new Dictionary<Vector3D, List<PathVertex>>();

                Queue<StaticPathEdge> streetEdgesQueue = new Queue<StaticPathEdge>(network.Edges.Select(val => new StaticPathEdge(val)));
                Queue<StaticPathEdge> auxQueue = new Queue<StaticPathEdge>();

                while (streetEdgesQueue.Count > 0)
                {
                    StaticPathEdge first = streetEdgesQueue.Dequeue();

                    while (streetEdgesQueue.Count > 0)
                    {
                        StaticPathEdge second = streetEdgesQueue.Dequeue();

                        //Do the checks only if the the edges are at least close to each other (simple spatial optimization)
                        if (first.IsAtCollidableRange(second))
                        {
                            Vector3D intersection;

                            if (first.Source.Position.Equals(second.Source.Position) || first.Source.Position.Equals(second.Target.Position))
                                intersection = first.Source.Position;
                            else if (first.Target.Position.Equals(second.Source.Position) || first.Target.Position.Equals(second.Target.Position))
                                intersection = first.Target.Position;
                            else
                                intersection = first.CalculateIntersection(second, true);

                            if (intersection.IsInfinity) // && first.IsAtSameLine(second)
                            {
                                /*
                                 * TODO: Consider these (rather rare) cases
                                 * 
                                 * */
                                /*if (first.HasPointInBetween(second.V0.Position))
                                {
                                    StreetEdge[] introduceVertex = first.IntroduceVertex(second.V0);
                                    first = new StaticStreetEdge(introduceVertex[0]);
                                    StaticStreetEdge other = new StaticStreetEdge(introduceVertex[0]);
                                    if (first.HasPointInBetween(second.V1.Position))
                                    {
                                        StreetEdge[] newVertices = first.IntroduceVertex(second.V1);
                                        first = new StaticStreetEdge(newVertices[0]);
                                        auxQueue.Enqueue(new StaticStreetEdge(newVertices[1]));
                                        auxQueue.Enqueue(other);
                                    }
                                    else if (other.HasPointInBetween(second.V1.Position))
                                    {
                                        StreetEdge[] newVertices = other.IntroduceVertex(second.V1);
                                        auxQueue.Enqueue(new StaticStreetEdge(newVertices[0]));
                                        auxQueue.Enqueue(new StaticStreetEdge(newVertices[1]));
                                    }
                                }
                                else if (second.HasPointInBetween(first.V0.Position))
                                {
                                    StreetEdge[] introduceVertex = first.IntroduceVertex(second.V0);
                                    first = new StaticStreetEdge(introduceVertex[0]);
                                    StaticStreetEdge other = new StaticStreetEdge(introduceVertex[0]);
                                    if (first.HasPointInBetween(second.V1.Position))
                                    {
                                        StreetEdge[] newVertices = first.IntroduceVertex(second.V1);
                                        first = new StaticStreetEdge(newVertices[0]);
                                        auxQueue.Enqueue(new StaticStreetEdge(newVertices[1]));
                                        auxQueue.Enqueue(other);
                                    }
                                    else if (other.HasPointInBetween(second.V1.Position))
                                    {
                                        StreetEdge[] newVertices = other.IntroduceVertex(second.V1);
                                        auxQueue.Enqueue(new StaticStreetEdge(newVertices[0]));
                                        auxQueue.Enqueue(new StaticStreetEdge(newVertices[1]));
                                    }
                                }*/

                                /*if(first.HasPointInBetween(second.V1.Position))
                                    first.IntroduceVertex(second.V1);

                                if (second.HasPointInBetween(first.V0.Position))
                                    second.IntroduceVertex(first.V0);

                                if (second.HasPointInBetween(first.V1.Position))
                                    second.IntroduceVertex(first.V1);*/

                                auxQueue.Enqueue(second);
                            }
                            else if (!intersection.IsNaN)
                            {
                                PathVertex firstIntersection = CheckForExistingVertex(intersection, first);
                                PathVertex secondIntersection = CheckForExistingVertex(intersection, second);

                                if (firstIntersection == null && secondIntersection == null)
                                {
                                    PathVertex vertex = new PathVertex(intersection);
                                    //network.Vertices.Add(vertex);

                                    //introduces 2 new edges, replaces the current edge with the first half and adds the rest to the queue
                                    PathEdge[] newEdges = network.IntroduceVertex(first.Edge, vertex);
                                    //PathEdge[] newEdges = first.IntroduceVertex(vertex);
                                    first = new StaticPathEdge(newEdges[0]);
                                    auxQueue.Enqueue(new StaticPathEdge(newEdges[1]));

                                    newEdges = network.IntroduceVertex(second.Edge, vertex);
                                    auxQueue.Enqueue(new StaticPathEdge(newEdges[0]));
                                    auxQueue.Enqueue(new StaticPathEdge(newEdges[1]));

                                    AddVertex(vertices, vertex);
                                }
                                else if (firstIntersection == null)
                                {
                                    PathVertex vertex = new PathVertex(intersection);

                                    PathEdge[] newEdges = network.IntroduceVertex(first.Edge, vertex);
                                    first = new StaticPathEdge(newEdges[0]);
                                    auxQueue.Enqueue(new StaticPathEdge(newEdges[1]));

                                    auxQueue.Enqueue(second);

                                    AddVertex(vertices, vertex);
                                    AddVertex(vertices, secondIntersection);
                                }
                                else if (secondIntersection == null)
                                {
                                    PathVertex vertex = new PathVertex(intersection);

                                    PathEdge[] newEdges = network.IntroduceVertex(second.Edge, vertex);
                                    auxQueue.Enqueue(new StaticPathEdge(newEdges[0]));
                                    auxQueue.Enqueue(new StaticPathEdge(newEdges[1]));

                                    AddVertex(vertices, vertex);
                                    AddVertex(vertices, firstIntersection);
                                }
                                else
                                {
                                    auxQueue.Enqueue(second);

                                    AddVertex(vertices, firstIntersection);
                                    AddVertex(vertices, secondIntersection);
                                }
                            }
                            else
                            {
                                auxQueue.Enqueue(second);
                            }
                        }
                        else
                        {
                            auxQueue.Enqueue(second);
                        }
                    }

                    streetEdgesQueue = auxQueue;
                    auxQueue = new Queue<StaticPathEdge>();
                }

                return vertices;
            }



            /// <summary>
            /// Step 3
            /// </summary>
            /// <param name="network"></param>
            private void FixComplementaryEdges(PathEntity network)
            {
                //Now replace all direct edge connections with 
                foreach (PathVertex streetVertex in network.Vertices.ToList())
                foreach (PathEdge streetEdge in streetVertex.Edges)
                    if (!streetEdge.GetLocalAttribute<bool>("Marked", Procedure))
                    {
                        PathVertex otherVertex = streetEdge.OtherVertex(streetVertex);
                        Vector3D searchDirection = (otherVertex.Position - streetVertex.Position).Normalize();

                        RecursiveEdgeReplacement(streetVertex, otherVertex, searchDirection, new[] {streetEdge});
                    }
                /*if (!streetEdge.Marked)
                        {
                            PathVertex otherVertex = streetEdge.OtherVertex(streetVertex);
                            Vector3D searchDirection = (otherVertex.Position - streetVertex.Position).Normalize();

                            RecursiveEdgeReplacement(streetVertex, otherVertex, searchDirection, new[] {streetEdge});
                        }*/

                //remove now the edges that have been marked for deletion
                /*foreach (PathEdge streetVertex in network.Edges.Where(val => val.Marked).ToList())
                    streetVertex.DetachFromVertices();*/
                foreach (PathEdge streetVertex in network.Edges.Where(val => val.GetLocalAttribute<bool>("Marked", Procedure)).ToList())
                    streetVertex.DetachFromVertices();
            }



            private void FixComplementaryEdges2(PathEntity network)
            {
                foreach (PathVertex streetVertex in network.Vertices.ToList())
                foreach (PathEdge pathEdge in streetVertex.Edges)
                    if (pathEdge.GetLocalAttribute<bool>("Marked", Procedure))
                    {
                        PathVertex startingVertex = streetVertex;
                        PathVertex otherVertex = pathEdge.OtherVertex(startingVertex);

                        foreach (PathEdge otherEdge in otherVertex.Edges.Where(val => val.Connects(startingVertex, otherVertex)))
                            otherEdge.SetLocalAttribute("Marked", Procedure, true);

                        //the previous intruction will set this to true
                        pathEdge.SetLocalAttribute("Marked", Procedure, false);
                    }
                /*if (!streetEdge.Marked)
                        {
                            PathVertex startingVertex = streetVertex;
                            PathVertex otherVertex = streetEdge.OtherVertex(startingVertex);

                            foreach (PathEdge otherEdge in otherVertex.Edges.Where(val => val.Connects(startingVertex, otherVertex)))
                                otherEdge.Marked = true;

                            //the previous intruction will set this to true
                            streetEdge.Marked = false;
                        }*/

                //remove now the edges that have been marked for deletion
                /*foreach (PathEdge streetVertex in network.Edges.Where(val => val.Marked).ToList())
                    streetVertex.DetachFromVertices();*/
                foreach (PathEdge streetVertex in network.Edges.Where(val => val.GetLocalAttribute<bool>("Marked", Procedure)).ToList())
                    streetVertex.DetachFromVertices();
            }



            private void MergeVertices(List<PathVertex> vertexList)
            {
                PathVertex mainVertex = vertexList[0];

                for (int i = 1; i < vertexList.Count; i++)
                {
                    PathVertex pathVertex = vertexList[i];
                    foreach (PathEdge edge in pathVertex.Edges.ToList()) edge.ReplaceVertex(pathVertex, mainVertex);
                }
            }



            private void RecursiveEdgeReplacement(PathVertex startingVertex, PathVertex currentVertex, Vector3D searchDirection, IEnumerable<PathEdge> streetEdges)
            {
                IEnumerable<PathEdge> list = streetEdges as IList<PathEdge> ?? streetEdges.ToList();

                foreach (PathEdge pathEdge in currentVertex.Edges.Where(val => val.Connects(startingVertex, currentVertex)).Except(list))
                    pathEdge.SetLocalAttribute("Marked", Procedure, true);
                //streetEdge.Marked = true;

                //IEnumerable<PathEdge> edges = currentVertex.Edges.Where(edge => !edge.Marked && (edge.OtherVertex(currentVertex).Position - currentVertex.Position).Normalize().Equals(searchDirection));
                IEnumerable<PathEdge> edges = currentVertex.Edges.Where(edge => !edge.GetLocalAttribute<bool>("Marked", Procedure) && (edge.OtherVertex(currentVertex).Position - currentVertex.Position).Normalize().Equals(searchDirection));
                foreach (PathEdge streetEdge in edges) RecursiveEdgeReplacement(startingVertex, streetEdge.OtherVertex(currentVertex), searchDirection, list.Union(new[] {streetEdge}));
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                Queue<PathEdge> streetEdges = new Queue<PathEdge>(pathEntity.Edges.ToList());
                while (streetEdges.Count > 0)
                {
                    PathEdge pathEdge = streetEdges.Dequeue();
                    foreach (PathVertex streetVertex in pathEntity.Vertices)
                        if (streetVertex != pathEdge.Source && streetVertex != pathEdge.Target)
                            if (!pathEdge.Source.Position.Equals(streetVertex.Position) && !pathEdge.Target.Position.Equals(streetVertex.Position)
                                                                                        && pathEdge.HasPointInBetween(streetVertex.Position))
                            {
                                foreach (PathEdge subEdge in pathEntity.IntroduceVertex(pathEdge, streetVertex))
                                    streetEdges.Enqueue(subEdge);

                                break;
                            }
                }

                /*streetEdges = new Queue<StreetEdge>(network.Edges.ToList());

                while (streetEdges.Count > 0)
                {
                    StreetEdge streetEdge = streetEdges.Dequeue();
                    foreach (StreetEdge edge in streetEdges.ToList())
                    {
                        if (streetEdge.CoincidentWith(edge))
                            edge.DetachFromVertices();
                    }
                }*/

                //FindEdgeIntersections(network);
                Dictionary<Vector3D, List<PathVertex>> vertices = FindEdgeIntersections(pathEntity);

                //step 2: look now for vertices sharing the same position and merge them
                foreach (KeyValuePair<Vector3D, List<PathVertex>> keyValuePair in vertices)
                    MergeVertices(keyValuePair.Value);

                FixComplementaryEdges2(pathEntity);


                //Step 3: remove now the vertices that have no edges now
                //pathEntity.Vertices.RemoveAll(val => val.Edges.Count == 0);


                return pathEntity;
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Performs path cleaning by grouping/merging vertices that are closer than a specified range. 
        /// This avoids possible issues when, for instance, transforming the path into a mesh.
        /// </summary>
        /// <seealso cref="Sceelix.Paths.Procedures.PathModifyProcedure.PathModificationParameter" />
        public class CleanupParameter : PathModificationParameter
        {
            /// <summary>
            /// Maximum distance between vertices allowed before a merge between them will take place.
            /// (Entity Expression reads values from individual vertices) 
            /// </summary>
            private readonly FloatParameter _parameterDistance = new FloatParameter("Distance", 1) {EntityEvaluation = true};

            private readonly BoolParameter _parameterCrossOnly = new BoolParameter("Cross Only", true) {EntityEvaluation = true};



            public CleanupParameter()
                : base("Cleanup")
            {
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                Queue<PathVertexGroup> streetVertexGroups = new Queue<PathVertexGroup>();

                //int index = 0;
                //foreach (StreetVertex streetVertex in network.Vertices)
                //    streetVertex.Name += index++;

                //IEnumerable<StreetVertex> streetVertices = network.Vertices.Where(val => val.HalfStreetVertices.Count != val.HalfStreetVertices.Distinct().Count());

                //IEnumerable<StreetVertex> vs = network.Vertices.Where(val => val.HalfStreetVertices.Count > 1);

                foreach (PathVertex streetVertex in pathEntity.Vertices.OrderByDescending(val => val.Edges.Count))
                    //var width = _parameterDistance.Get(streetVertex);
                    //var width = streetVertex.Attributes.GetSafe("Width") ?? _parameterDistance.Value;

                    streetVertexGroups.Enqueue(new PathVertexGroup(streetVertex, _parameterDistance, _parameterCrossOnly.Value));


                Queue<PathVertexGroup> auxGroups = new Queue<PathVertexGroup>();
                while (streetVertexGroups.Count > 0)
                {
                    if (streetVertexGroups.Peek().Checked)
                        break;

                    PathVertexGroup group1 = streetVertexGroups.Dequeue();
                    /*if (group1.Checked)
                    {
                        streetVertexGroups.Enqueue(group1);
                        break;
                    }*/


                    while (streetVertexGroups.Count > 0)
                    {
                        PathVertexGroup group2 = streetVertexGroups.Dequeue();

                        if (group1.Intersects(group2))
                            group1.Incorporate(group2);
                        else
                            auxGroups.Enqueue(group2);
                    }

                    //put back the group into the list
                    group1.Checked = true;
                    auxGroups.Enqueue(group1);

                    streetVertexGroups = auxGroups;
                    auxGroups = new Queue<PathVertexGroup>();
                }

                IEnumerable<PathVertexGroup> vertexGroups = streetVertexGroups.Where(val => val.Vertices.Count > 1);
                foreach (PathVertexGroup streetVertexGroup in vertexGroups)
                    streetVertexGroup.CreateMidVertices(pathEntity);


                foreach (PathEdge streetEdge in pathEntity.Edges.ToList())
                    if (!streetEdge.IsValid)
                        streetEdge.DetachFromVertices();

                //remove now the vertices that have 0 incoming edges
                //pathEntity.Vertices.RemoveAll(val => val.Edges.Count == 0);

                //IEnumerable<StreetVertex> streetVertices2 = network.Vertices.Where(val => val.HalfStreetVertices.Count != val.HalfStreetVertices.Distinct().Count());


                /*Queue<StreetVertex> orderedVertices = new Queue<StreetVertex>();

                while (orderedVertices.Count > 0)
                {
                    StreetVertex streetVertexMain = orderedVertices.Dequeue();

                    //let's focus only on the crosspoints
                    if (streetVertexMain.HalfStreetVertices.Count < 2)
                        break;


                    PathVertexGroup group = new PathVertexGroup(streetVertexMain);
                }


                Dictionary<StreetVertex,float> maxWidths = new Dictionary<StreetVertex, float>();

                //precalculate all the streetvertex widths
                foreach (StreetVertex streetVertex in network.Vertices)
                    maxWidths.Add(streetVertex,streetVertex.HalfStreetVertices.Max(val => val.Width));


                //HashSet<StreetVertex> streetVertices = new HashSet<StreetVertex>(network.Vertices);

            

                //Queue<StreetVertex> crossVertices = new Queue<StreetVertex>(streetVertices.Where(val => val.HalfStreetVertices.Count > 1));

                Queue<StreetVertex> auxCrossVertices = new Queue<StreetVertex>();
                while (orderedVertices.Count > 0)
                {
                    StreetVertex streetVertexMain = orderedVertices.Dequeue();

                    //let's focus only on the crosspoints
                    if(streetVertexMain.HalfStreetVertices.Count < 2)
                        break;


                    //create a new group and add that vertex
                    List<StreetVertex> vertexGroup = new List<StreetVertex>();
                    vertexGroup.Add(streetVertexMain);


                    while (orderedVertices.Count > 0)
                    {
                        StreetVertex streetVertexOther = orderedVertices.Dequeue();
                        float maxWidthOther = maxWidths[streetVertexOther];
                        bool gotMerged = false;

                        //compare only with other crosspoints
                        foreach (StreetVertex streetVertex in vertexGroup.Where(val => val.HalfStreetVertices.Count > 1).ToList())
                        {
                            float maxWidth = maxWidths[streetVertex];
                            float minDistance = maxWidth + maxWidthOther;
                            if ((streetVertexMain.Position - streetVertexOther.Position).Length < minDistance)
                            {
                                vertexGroup.Add(streetVertexOther);
                                gotMerged = true;
                                break;
                            }
                        }

                        if (!gotMerged)
                        {
                            auxCrossVertices.Enqueue(streetVertexOther);
                        }
                    }

                    orderedVertices = auxCrossVertices;
                    auxCrossVertices = new Queue<StreetVertex>();
                }*/


                /*List<Street> newStreets = new List<Street>(); 

                for (int i = 0; i < network.Streets.Count; i++)
                {
                    Street street1 = network.Streets[i];

                    //for each vertex of this street
                    for (int k = 0; k < street1.Vertices.Count; k++)
                    {
                        //bool foundMatch = false;
                        StreetVertex vertex1 = street1.Vertices[k];
                        float maxWidth1 = vertex1.HalfStreetVertices.Max(val => val.Width);

                        // compare with all vertices of all other streets (very inefficient, for now)...
                        for (int j = i + 1; j < network.Streets.Count; j++)
                        {
                            Street street2 = network.Streets[j];
                            for (int l = 0; l < street2.Vertices.Count; l++)
                            {
                                StreetVertex vertex2 = street2.Vertices[l];
                                if (vertex1 != vertex2)
                                {
                                    float maxWidth2 = vertex2.HalfStreetVertices.Max(val => val.Width);

                                    float minDistance = maxWidth1 + maxWidth2;

                                    Vector3D vector3D = vertex1.Position - vertex2.Position;
                                    if (vector3D.Length < minDistance)
                                    {
                                        //create a midpoint vertex
                                        StreetVertex midPointVertex = new StreetVertex(vertex2.Position + vector3D/2f);
                                    
                                        midPointVertex.HalfStreetVertices.AddRange(vertex1.HalfStreetVertices);
                                         midPointVertex.HalfStreetVertices.AddRange(vertex2.HalfStreetVertices);

                                        //foundMatch = true;
                                        street1.Vertices[k] = midPointVertex;
                                        street2.Vertices[l] = midPointVertex;
                                        //break;
                                    }
                                }
                            }
                        }
                    }
            
                }*/

                //network.Streets.Clear();
                //network.Streets.AddRange(newStreets);

                return pathEntity;
            }



            private class PathVertexGroup
            {
                private readonly bool _crossOnly;



                public PathVertexGroup(PathVertex vertex, FloatParameter parameterDistance, bool crossOnly)
                {
                    _crossOnly = crossOnly;
                    Vertices.Add(vertex);

                    /*if (widthValue <= 0)
                        _widths.Add(vertex.StreetEdges.Max(val => val.TotalWidth));
                    else
                    {
                        _widths.Add(widthValue);
                    }*/
                    Widths.Add(vertex.Edges.Max(edge => parameterDistance.Get(edge)));
                }



                public bool Checked
                {
                    get;
                    set;
                }


                public List<PathVertex> Vertices
                {
                    get;
                } = new List<PathVertex>();


                public List<float> Widths
                {
                    get;
                } = new List<float>();



                public void CreateMidVertices(PathEntity network)
                {
                    //calculate the centroid of all the considered points
                    Vector3D sum = Vector3D.Zero;

                    foreach (PathVertex streetVertex in Vertices)
                        sum += streetVertex.Position;

                    sum /= Vertices.Count;

                    //create a new vertex at that point
                    PathVertex midPointVertex = new PathVertex(sum);
                    //network.Vertices.Add(midPointVertex);

                    foreach (PathVertex streetVertex in Vertices)
                    foreach (PathEdge edge in streetVertex.Edges.ToList())
                        if (edge.OtherVertex(streetVertex) == midPointVertex)
                        {
                            network.RemoveEdge(edge);
                        }
                        else
                        {
                            edge.ReplaceVertex(streetVertex, midPointVertex);
                            streetVertex.Attributes.ComplementAttributesTo(midPointVertex.Attributes);
                        }


                    //IEnumerable<IGrouping<Street, HalfStreetVertex>> groupBy = _vertices.SelectMany(val => val.HalfStreetVertices).GroupBy(val => val.Street);
                    //foreach (IGrouping<Street, HalfStreetVertex> streetVertices in groupBy)
                    //{
                    //    streetVertices.Average(val => val.Width);
                    //}

                    /*HashSet<HalfStreetVertex> halfStreetVertices = new HashSet<HalfStreetVertex>();

                    //add all connections to that vertex
                    foreach (StreetVertex streetVertex in _vertices)
                        halfStreetVertices.UnionWith(streetVertex.HalfStreetVertices);

                    midPointVertex.HalfStreetVertices = new List<HalfStreetVertex>(halfStreetVertices);

                    midPointVertex.Name = "Mid ";

                    //replace the old vertices with this one
                    foreach (HalfStreetVertex halfStreetVertex in midPointVertex.HalfStreetVertices)
                        halfStreetVertex.Street.Vertices[halfStreetVertex.StreetIndex] = midPointVertex;

                    //remove repeated vertices 
                    foreach (Street street in midPointVertex.HalfStreetVertices.Select(val => val.Street).ToList().Distinct())
                    {
                        List<HalfStreetVertex> removeRepeatedVertices = street.RemoveRepeatedVertices();
                        //foreach (HalfStreetVertex removeRepeatedVertex in removeRepeatedVertices)
                        {
                        
                        }
                    }*/
                }



                public void Incorporate(PathVertexGroup group2)
                {
                    Vertices.AddRange(group2.Vertices);
                    Widths.AddRange(group2.Widths);
                }



                public bool Intersects(PathVertexGroup group2)
                {
                    //if (group2._vertices[0].Name == "13" && _vertices[0].Name == "14")
                    //    Console.WriteLine();

                    for (int i = 0; i < Vertices.Count; i++)
                    for (int j = 0; j < group2.Vertices.Count; j++)
                    {
                        PathVertex vertex1 = Vertices[i];
                        float vertex1Width = Widths[i] / 2f;

                        PathVertex vertex2 = group2.Vertices[j];
                        float vertex2Width = group2.Widths[j] / 2f;

                        var isValue = !_crossOnly || IsCrossVertex(vertex1) || IsCrossVertex(vertex2);


                        if (isValue
                            && (vertex1.Position - vertex2.Position).Length < vertex1Width + vertex2Width)
                            return true;
                    }

                    return false;
                }



                private bool IsCrossVertex(PathVertex pathVertex)
                {
                    return pathVertex.Edges.Count > 2;
                }
            }
        }

        #endregion

        #region Edge Cleanup

        /// <summary>
        /// Performs edge cleanup by merging edges that are too close together. 
        /// </summary>
        /// <seealso cref="Sceelix.Paths.Procedures.PathModifyProcedure.PathModificationParameter" />
        public class EdgeCleanupParameter : PathModificationParameter
        {
            /// <summary>
            /// Expected size around the edge that must be kept clean. Half of this size is used for each side of the edge. Evaluated per path edge.
            /// </summary>
            private readonly FloatParameter _parameterWidth = new FloatParameter("Width", 1) {EntityEvaluation = true};

            /// <summary>
            /// Minimum angle tolerance (in degrees) between edges in order for them to be included in the cleanup. 
            /// This value should only be lowered if very small angles are known to exist, otherwise it could result in mathematical precision problems.
            /// </summary>
            private readonly FloatParameter _parameterAngleTolerance = new FloatParameter("Angle Tolerance", 3) {EntityEvaluation = true};



            public EdgeCleanupParameter()
                : base("Edge Cleanup")
            {
            }



            private bool CheckAndDeleteVertex(PathEntity pathEntity, PathEdge e, PathVertex eVertex, float lengthOnEdge, float edgeSize)
            {
                if (lengthOnEdge > edgeSize)
                {
                    if (eVertex.Edges.Count > 2)
                        return false;

                    var newEdges = pathEntity.RemoveVertex(eVertex, true); //eVertex.Delete()

                    foreach (PathEdge newEdge in newEdges)
                        e.Attributes.ComplementAttributesTo(newEdge.Attributes);

                    return true;
                }

                return false;
            }



            private bool CheckSideSize(PathEntity pathEntity, float toleranceDegrees, PathEdge e1, PathEdge e2, PathVertex e1Vertex, PathVertex e2Vertex, Vector3D e1Direction, Vector3D e2Direction, float e1HalfWidth, float e2HalfWidth, float e1Size, float e2Size, Vector3D e1Right, Vector3D e2Left)
            {
                var angle1 = e2Direction.AngleTo(e1Right);
                var angle2 = e1Direction.AngleTo(e2Left);

                var toleranceRadians = MathHelper.ToRadians(toleranceDegrees);

                if (angle1 >= MathHelper.PiOver2 - toleranceRadians || angle2 >= MathHelper.PiOver2 - toleranceRadians)
                    return false;

                var sideAVector = e2Direction * (e1HalfWidth / (float) Math.Cos(angle1));
                var sideBVector = e1Direction * (e2HalfWidth / (float) Math.Cos(angle2));

                //the corner position is at
                var sumVector = sideAVector + sideBVector;
                var sumSize = sumVector.Length;

                var lengthOnE1 = Math.Abs(sumSize * (float) Math.Cos(e1Direction.AngleTo(sumVector)));
                var lengthOnE2 = Math.Abs(sumSize * (float) Math.Cos(e2Direction.AngleTo(sumVector)));

                if (CheckAndDeleteVertex(pathEntity, e1, e1Vertex, lengthOnE1, e1Size))
                    return true;

                if (CheckAndDeleteVertex(pathEntity, e2, e2Vertex, lengthOnE2, e2Size))
                    return true;

                return false;
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                foreach (PathVertex currentVertex in pathEntity.Vertices.ToList())
                {
                    bool vertexDeleted;
                    float tolerance = _parameterAngleTolerance.Get(currentVertex);

                    do
                    {
                        vertexDeleted = false;

                        var edges = currentVertex.Edges.ToList();
                        for (int i = 0; i < edges.Count; i++)
                        {
                            var edge1 = edges[i];
                            var e1Vertex = edge1.OtherVertex(currentVertex);
                            var e1Direction = (e1Vertex.Position - currentVertex.Position).ProjectToPlane(Vector3D.ZVector);
                            var e1Size = e1Direction.Length;
                            e1Direction = e1Direction.Normalize();

                            var e1HalfWidth = _parameterWidth.Get(edge1) / 2f;
                            var originale1HalfWidth = e1HalfWidth;
                            var e1Right = e1Direction.Cross(Vector3D.ZVector).Normalize();
                            var e1Left = -e1Right;

                            for (int j = i + 1; j < edges.Count; j++)
                            {
                                var edge2 = edges[j];
                                var e2Vertex = edge2.OtherVertex(currentVertex);
                                var e2Direction = (e2Vertex.Position - currentVertex.Position).ProjectToPlane(Vector3D.ZVector);
                                var e2Size = edge2.Length;
                                e2Direction = e2Direction.Normalize();

                                //vertices with more than 2 edges (crossings) are not even eligible for deletion
                                if (e1Vertex.Edges.Count > 2 && e2Vertex.Edges.Count > 2)
                                    continue;

                                var e2HalfWidth = _parameterWidth.Get(edge2) / 2f;

                                var e2Right = e2Direction.Cross(Vector3D.ZVector).Normalize();
                                var e2Left = -e2Right;

                                var sum = (originale1HalfWidth + e2HalfWidth) / 2f;
                                e1HalfWidth = sum;
                                e2HalfWidth = sum;

                                if (CheckSideSize(pathEntity, tolerance, edge1, edge2, e1Vertex, e2Vertex, e1Direction, e2Direction, e1HalfWidth, e2HalfWidth, e1Size, e2Size, e1Right, e2Left)
                                    || CheckSideSize(pathEntity, tolerance, edge1, edge2, e1Vertex, e2Vertex, e1Direction, e2Direction, e1HalfWidth, e2HalfWidth, e1Size, e2Size, e1Left, e2Right))
                                {
                                    vertexDeleted = true;
                                    break;
                                }
                            }

                            if (vertexDeleted)
                                break;
                        }
                    } while (vertexDeleted);
                }


                //remove all vertices that have 
                //pathEntity.Vertices.RemoveAll(x => x.Edges.Count == 0);

                return pathEntity;
            }
        }

        #endregion

        #region Trim

        /// <summary>
        /// Trims the paths leaf edges, i.e. those edges that contain non-shared vertices, by a certain amount.
        /// </summary>
        public class TrimParameter : PathModificationParameter
        {
            /// <summary>
            /// The amount to cut on the leaf edges.
            /// </summary>
            private readonly FloatParameter _parameterAmount = new FloatParameter("Amount", 0.1f) {MinValue = 0};



            public TrimParameter()
                : base("Trim")
            {
            }



            public override PathEntity Transform(PathEntity pathEntity)
            {
                var mainAmount = _parameterAmount.Value;
                if (mainAmount <= 0)
                    return pathEntity;

                var tuples = new Queue<Tuple<PathVertex, float>>(pathEntity.Vertices.Where(x => x.Degree == 1)
                    .Select(x => new Tuple<PathVertex, float>(x, mainAmount)));

                while (tuples.Any())
                {
                    var dequeue = tuples.Dequeue();
                    var vertex = dequeue.Item1;
                    var amount = dequeue.Item2;
                    var pathEdge = vertex.Edges.First();
                    var otherVertex = pathEdge.OtherVertex(vertex);

                    //if we ended up already deleting this vertex's edge before
                    if (amount <= 0 || vertex.Degree == 0)
                        continue;

                    if (amount > pathEdge.Length)
                    {
                        pathEntity.RemoveEdge(pathEdge);

                        //if the other vertex is connected to one other edge
                        //we add that vertex to the queue, to trim what's left
                        if (otherVertex.Degree == 1)
                            tuples.Enqueue(new Tuple<PathVertex, float>(otherVertex, amount - pathEdge.Length));
                    }
                    else
                    {
                        vertex.Position = vertex.Position + (otherVertex.Position - vertex.Position).Normalize() * amount;
                    }
                }

                return pathEntity;
            }
        }

        #endregion
    }
}