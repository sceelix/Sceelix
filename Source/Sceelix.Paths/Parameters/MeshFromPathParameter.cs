using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Extensions;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Logging;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Procedures;
using Sceelix.Paths.Data;

namespace Sceelix.Paths.Parameters
{
    /// <summary>
    /// Creates a mesh from a path entity.
    /// </summary>
    /// <seealso cref="Sceelix.Meshes.Procedures.MeshCreateProcedure.PrimitiveMeshParameter" />
    public class MeshFromPathParameter : MeshCreateProcedure.PrimitiveMeshParameter
    {
        /// <summary>
        /// Defines the +/- angle to add to the 180º special case
        /// </summary>
        private const float AngleTolerance = 15;

        private const float DistanceTolerance = -0.01f;

        /// <summary>
        /// Path entity to be transformed into a mesh.
        /// </summary>
        private readonly SingleInput<PathEntity> _input = new SingleInput<PathEntity>("Input");

        /// <summary>
        /// Indicates if the insides of the closed paths should be transformed into faces.
        /// </summary>
        private readonly BoolParameter _parameterCloseInsides = new BoolParameter("Close Insides", false);

        /// <summary>
        /// The width of the face created for each edge. Can be set as an expression based on the attributes of each edge using the @@attributeName notation.
        /// </summary>
        private readonly FloatParameter _parameterWidth = new FloatParameter("Width", 1) {EntityEvaluation = true};

        /// <summary>
        /// Attribute that will store the name the name of the section in the face.
        /// Values can be "Inside", "Joint" or "Path" and can be extracted with a "Mesh Subselect" node.
        /// </summary>
        private readonly AttributeParameter<string> _attributeSectionType = new AttributeParameter<string>("Section", AttributeAccess.Write);



        public MeshFromPathParameter()
            : base("From Path")
        {
        }



        /// <summary>
        /// Closes the "holes" between the faces
        /// </summary>
        /// <param name="faces"></param>
        private IEnumerable<Face> CloseInsideAreas(List<Face> faces)
        {
            HashSet<HalfVertex> verticesToVisit = new HashSet<HalfVertex>(faces.SelectMany(x => x.HalfVertices).Where(x => x.IsBoundary));
            while (verticesToVisit.Count > 0)
            {
                var currentHalfVertex = verticesToVisit.First();

                if (!currentHalfVertex.IsBoundary)
                {
                    verticesToVisit.Remove(currentHalfVertex);
                    continue;
                }

                //otherwise, let's add this first half-edge to a chain
                LinkedList<List<HalfVertex>> vertexChains = new LinkedList<List<HalfVertex>>();
                vertexChains.AddLast(new List<HalfVertex> {currentHalfVertex});


                while (vertexChains.Count > 0)
                {
                    //let's look at the first item in the list
                    var vertexChain = vertexChains.First.Value;
                    vertexChains.RemoveFirst();

                    var firstHalfEdge = vertexChain.First();
                    var lastHalfEdge = vertexChain.Last();

                    //if, at this point, the half-edge has already been visited, ignore this path
                    if (!verticesToVisit.Contains(lastHalfEdge))
                        continue;

                    //if the 
                    if (firstHalfEdge.Vertex == lastHalfEdge.Next)
                    {
                        if (vertexChain.Count < 3)
                            continue;

                        var newFace = new Face(vertexChain.Select(x => x.Vertex).AsEnumerable().Reverse());
                        yield return newFace;
                    }
                    else
                    {
                        //get the next connections
                        var nextHalfVertices = lastHalfEdge.Next.HalfVertices.Where(x => x.IsBoundary).ToList();
                        if (nextHalfVertices.Count == 1)
                        {
                            //append the new vertex
                            vertexChain.Add(nextHalfVertices.First());

                            //add this item again, but at the top
                            vertexChains.AddFirst(vertexChain);
                        }
                        else
                        {
                            foreach (HalfVertex nextHalfVertex in nextHalfVertices)
                                if (nextHalfVertex.Face != lastHalfEdge.Face)
                                {
                                    var newChain = new List<HalfVertex>(vertexChain);
                                    newChain.Add(nextHalfVertex);

                                    //add this item again, but at the top
                                    vertexChains.AddFirst(vertexChain);
                                }
                                else
                                {
                                    //this is a new chain, give lowest priority   
                                    vertexChains.AddLast(new List<HalfVertex> {nextHalfVertex});
                                }
                        }
                    }


                    //we've looked at this half-edge, now remove it from the list of visited ones
                    verticesToVisit.Remove(lastHalfEdge);
                }
            }
        }



        private IEnumerable<Face> CreateJuntionFaces(Vertex centerVertex, PathVertex currentVertex, Vector3D normal, List<PathEdge> orderedEdges, List<Vertex> midVertices)
        {
            //now that we have the vertexBetweenEdges, let's go again over the outgoing edges
            for (int i = 0; i < orderedEdges.Count; i++)
            {
                var leftVertex = midVertices[(midVertices.Count + i - 1) % midVertices.Count];
                var rightVertex = midVertices[i];

                var edge = orderedEdges[i];
                var edgeDirection = edge.GetDirectionFrom(currentVertex).ProjectToPlane(normal);
                var edgeDirectionNormalized = edgeDirection.Normalize();
                var vectorLeft = leftVertex.Position - centerVertex.Position;
                var vectorRight = rightVertex.Position - centerVertex.Position;
                var normalizedVectorLeft = vectorLeft.Normalize();
                var normalizedVectorRight = vectorRight.Normalize();


                var leftAngle = normalizedVectorLeft.AngleTo(edgeDirectionNormalized);
                var rightAngle = normalizedVectorRight.AngleTo(edgeDirectionNormalized);


                var distanceFromLeft = vectorLeft.Length * (float) Math.Cos(leftAngle);
                var distanceFromRight = vectorRight.Length * (float) Math.Cos(rightAngle);


                if (distanceFromRight > distanceFromLeft && distanceFromRight > 0)
                {
                    var leftWidth = edge.GetLocalAttribute<float>("LeftWidth", Procedure);

                    var middlePoint = centerVertex.Position + edgeDirectionNormalized * distanceFromRight;
                    var oppositePoint = middlePoint + (middlePoint - rightVertex.Position).Normalize() * leftWidth;
                    var oppositeVertex = new Vertex(oppositePoint);
                    var middleVertex = new Vertex(middlePoint); //rightVertex.Position + (oppositeVertex.Position - rightVertex.Position)/2f

                    var newFace = new Face(oppositeVertex, middleVertex, rightVertex, centerVertex, leftVertex);
                    centerVertex.Attributes.SetAttributesTo(newFace.Attributes);

                    if (currentVertex == edge.Source)
                    {
                        oppositeVertex[newFace].UV0 = new Vector2D(0, 0);
                        middleVertex[newFace].UV0 = new Vector2D(0.5f, 0);
                        rightVertex[newFace].UV0 = new Vector2D(1, 0);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, -1);
                        leftVertex[newFace].UV0 = new Vector2D(0, -1);

                        edge.SetLocalAttribute("SourceLeft", Procedure, oppositeVertex);
                        edge.SetLocalAttribute("SourceCenter", Procedure, middleVertex);
                        edge.SetLocalAttribute("SourceRight", Procedure, rightVertex);
                    }
                    else if (currentVertex == edge.Target)
                    {
                        oppositeVertex[newFace].UV0 = new Vector2D(1, 0);
                        middleVertex[newFace].UV0 = new Vector2D(0.5f, 0);
                        rightVertex[newFace].UV0 = new Vector2D(0, 0);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, 1);
                        leftVertex[newFace].UV0 = new Vector2D(1, 1);

                        edge.SetLocalAttribute("TargetLeft", Procedure, rightVertex);
                        edge.SetLocalAttribute("TargetCenter", Procedure, middleVertex);
                        edge.SetLocalAttribute("TargetRight", Procedure, oppositeVertex);
                    }

                    newFace.CalculateTangentAndBinormal();
                    _attributeSectionType[newFace] = "Joint";

                    yield return newFace;
                }
                else if (distanceFromLeft > distanceFromRight && distanceFromLeft > 0)
                {
                    var rightWidth = edge.GetLocalAttribute<float>("RightWidth", Procedure);

                    var middlePoint = centerVertex.Position + edgeDirectionNormalized * distanceFromLeft;
                    //var middleVertex = new Vertex(middlePoint);
                    var oppositePoint = middlePoint + (middlePoint - leftVertex.Position).Normalize() * rightWidth;
                    var oppositeVertex = new Vertex(oppositePoint);
                    var middleVertex = new Vertex(middlePoint); //leftVertex.Position + (oppositeVertex.Position - leftVertex.Position)/2f

                    var newFace = new Face(leftVertex, middleVertex, oppositeVertex, rightVertex, centerVertex);
                    centerVertex.Attributes.SetAttributesTo(newFace.Attributes);

                    if (currentVertex == edge.Source)
                    {
                        leftVertex[newFace].UV0 = new Vector2D(0, 0);
                        middleVertex[newFace].UV0 = new Vector2D(0.5f, 0);
                        oppositeVertex[newFace].UV0 = new Vector2D(1, 0);
                        rightVertex[newFace].UV0 = new Vector2D(1, -1);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, -1);

                        edge.SetLocalAttribute("SourceLeft", Procedure, leftVertex);
                        edge.SetLocalAttribute("SourceCenter", Procedure, middleVertex);
                        edge.SetLocalAttribute("SourceRight", Procedure, oppositeVertex);
                    }
                    else if (currentVertex == edge.Target)
                    {
                        leftVertex[newFace].UV0 = new Vector2D(1, 0);
                        middleVertex[newFace].UV0 = new Vector2D(0.5f, 0);
                        oppositeVertex[newFace].UV0 = new Vector2D(0, 0);
                        rightVertex[newFace].UV0 = new Vector2D(0, 1);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, 1);

                        edge.SetLocalAttribute("TargetLeft", Procedure, oppositeVertex);
                        edge.SetLocalAttribute("TargetCenter", Procedure, middleVertex);
                        edge.SetLocalAttribute("TargetRight", Procedure, leftVertex);
                    }

                    _attributeSectionType[newFace] = "Joint";
                    newFace.CalculateTangentAndBinormal();
                    yield return newFace;
                }
                else if (distanceFromRight < DistanceTolerance && distanceFromLeft < DistanceTolerance)
                {
                    var leftWidth = edge.GetLocalAttribute<float>("LeftWidth", Procedure);
                    var rightWidth = edge.GetLocalAttribute<float>("RightWidth", Procedure);

                    var edge1Right = edgeDirection.Cross(normal).Normalize();
                    var edge1Left = -edge1Right;

                    var newVertexLeft = new Vertex(centerVertex.Position + edge1Left * leftWidth);
                    var newVertexRight = new Vertex(centerVertex.Position + edge1Right * rightWidth);

                    var newFace = new Face(newVertexLeft, newVertexRight, rightVertex, centerVertex, leftVertex);
                    centerVertex.Attributes.SetAttributesTo(newFace.Attributes);

                    if (currentVertex == edge.Source)
                    {
                        newVertexLeft[newFace].UV0 = new Vector2D(0, 0);
                        newVertexRight[newFace].UV0 = new Vector2D(1, 0);
                        rightVertex[newFace].UV0 = new Vector2D(1, -1);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, -1);
                        leftVertex[newFace].UV0 = new Vector2D(0, -1);

                        edge.SetLocalAttribute("SourceLeft", Procedure, newVertexLeft);
                        edge.SetLocalAttribute("SourceRight", Procedure, newVertexRight);
                    }
                    else if (currentVertex == edge.Target)
                    {
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, 1);
                        rightVertex[newFace].UV0 = new Vector2D(0, 1);
                        newVertexRight[newFace].UV0 = new Vector2D(0, 0);
                        leftVertex[newFace].UV0 = new Vector2D(1, 1);
                        newVertexLeft[newFace].UV0 = new Vector2D(1, 0);


                        edge.SetLocalAttribute("TargetLeft", Procedure, newVertexRight);
                        edge.SetLocalAttribute("TargetRight", Procedure, newVertexLeft);
                    }

                    _attributeSectionType[newFace] = "Joint";
                    newFace.CalculateTangentAndBinormal();

                    //newFace.Material = new Sceelix.Meshes.Materials.ColorMaterial(Color.Red);
                    yield return newFace;
                }
                //for 180ª cases and for straight crossroads
                else if (Math.Abs(distanceFromRight - distanceFromLeft) < float.Epsilon && distanceFromRight > float.Epsilon && distanceFromLeft > float.Epsilon)
                {
                    var middleVertex = new Vertex(rightVertex.Position + (leftVertex.Position - rightVertex.Position) / 2f);

                    var newFace = new Face(leftVertex, middleVertex, rightVertex, centerVertex);
                    centerVertex.Attributes.SetAttributesTo(newFace.Attributes);

                    if (currentVertex == edge.Source)
                    {
                        leftVertex[newFace].UV0 = new Vector2D(0, 0);
                        middleVertex[newFace].UV0 = new Vector2D(0.5f, 0);
                        rightVertex[newFace].UV0 = new Vector2D(1, 0);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, -1);


                        edge.SetLocalAttribute("SourceLeft", Procedure, leftVertex);
                        edge.SetLocalAttribute("SourceCenter", Procedure, middleVertex);
                        edge.SetLocalAttribute("SourceRight", Procedure, rightVertex);
                    }
                    else if (currentVertex == edge.Target)
                    {
                        leftVertex[newFace].UV0 = new Vector2D(1, 0);
                        middleVertex[newFace].UV0 = new Vector2D(0.5f, 0);
                        rightVertex[newFace].UV0 = new Vector2D(0, 0);
                        centerVertex[newFace].UV0 = new Vector2D(0.5f, 1);

                        edge.SetLocalAttribute("TargetLeft", Procedure, rightVertex);
                        edge.SetLocalAttribute("TargetCenter", Procedure, middleVertex);
                        edge.SetLocalAttribute("TargetRight", Procedure, leftVertex);
                    }

                    newFace.CalculateTangentAndBinormal();
                    _attributeSectionType[newFace] = "Joint";

                    yield return newFace;
                }
            }
        }



        protected override MeshEntity CreateMesh()
        {
            var pathEntity = _input.Read();

            //make sure we don't have loose vertices
            //pathEntity.RemoveEmptyVertices();

            List<Face> faces = new List<Face>();

            foreach (PathVertex currentVertex in pathEntity.Vertices)
            {
                var normal = Vector3D.ZVector;

                //determine the first edge, as we are ordering the vertices around this one
                var firstEdgeDirection = currentVertex.Edges.First().GetDirectionFrom(currentVertex).ProjectToPlane(normal);

                //create the vertex at the center and copy the attributes from the path vertex
                Vertex centerVertex = new Vertex(currentVertex.Position);
                currentVertex.Attributes.SetAttributesTo(centerVertex.Attributes);
                currentVertex.SetLocalAttribute("CenterVertex", Procedure, centerVertex);


                //first special case: there is only one outgoing edge
                if (currentVertex.Edges.Count == 1)
                {
                    var edge1 = currentVertex.Edges[0];
                    var edge1Direction = edge1.GetDirectionFrom(currentVertex);
                    var edge1HalfWidth = _parameterWidth.Get(edge1) / 2f;
                    var edge1Right = edge1Direction.Cross(normal).Normalize();
                    var edge1Left = -edge1Right;

                    var vertexLeft = new Vertex(currentVertex.Position + edge1Left * edge1HalfWidth);
                    var vertexRight = new Vertex(currentVertex.Position + edge1Right * edge1HalfWidth);

                    if (currentVertex == edge1.Source)
                    {
                        edge1.SetLocalAttribute("SourceRight", Procedure, vertexRight);
                        edge1.SetLocalAttribute("SourceCenter", Procedure, centerVertex);
                        edge1.SetLocalAttribute("SourceLeft", Procedure, vertexLeft);
                    }
                    else if (currentVertex == edge1.Target)
                    {
                        edge1.SetLocalAttribute("TargetRight", Procedure, vertexLeft);
                        edge1.SetLocalAttribute("TargetCenter", Procedure, centerVertex);
                        edge1.SetLocalAttribute("TargetLeft", Procedure, vertexRight);
                    }

                    //nothing more to do, skip to the next vertex
                    continue;
                }

                //all other cases start here
                //order clockwise around the normal vector
                var orderedEdges = currentVertex.Edges.OrderBy(pathEdge => GetAngle(currentVertex, firstEdgeDirection, normal, pathEdge)).ToList();

                //we will store all the calculated mid points here
                List<Vertex> midVertices = new List<Vertex>();

                //go over all edges and match every 2 edges
                for (int i = 0; i < orderedEdges.Count; i++)
                {
                    var edge1 = orderedEdges[i];
                    var edge1Direction = edge1.GetDirectionFrom(currentVertex).ProjectToPlane(normal);
                    var edge1HalfWidth = _parameterWidth.Get(edge1) / 2f;
                    var edge1Right = edge1Direction.Cross(normal).Normalize();

                    var edge2 = orderedEdges[(i + 1) % orderedEdges.Count];
                    var edge2Direction = edge2.GetDirectionFrom(currentVertex).ProjectToPlane(normal);
                    var edge2HalfWidth = _parameterWidth.Get(edge2) / 2f;
                    var edge2Left = -edge2Direction.Cross(normal).Normalize();

                    var sideADot = edge2Direction.Dot(edge1Right);
                    var sideBDot = edge1Direction.Dot(edge2Left);


                    //there is a special case, when the edges are at 180º
                    Vector3D sumVector;
                    if (MathHelper.ToDegrees(edge1Direction.AngleTo(edge2Direction)) > 180 - AngleTolerance)
                    {
                        var halfWidth = (edge1HalfWidth + edge2HalfWidth) / 2f;
                        sumVector = edge1Right * halfWidth;

                        edge1.SetLocalAttribute("RightWidth", Procedure, halfWidth);
                        edge2.SetLocalAttribute("LeftWidth", Procedure, halfWidth);
                    }
                    else
                    {
                        var sideAVector = edge2Direction * (edge1HalfWidth / sideADot);
                        var sideBVector = edge1Direction * (edge2HalfWidth / sideBDot);
                        sumVector = sideAVector + sideBVector;

                        edge1.SetLocalAttribute("RightWidth", Procedure, edge1HalfWidth);
                        edge2.SetLocalAttribute("LeftWidth", Procedure, edge2HalfWidth);
                    }

                    //the corner position is at
                    var vertexBetweenEdges = new Vertex(currentVertex.Position + sumVector);

                    if (currentVertex == edge1.Source)
                        edge1.SetLocalAttribute("SourceRight", Procedure, vertexBetweenEdges);
                    else if (currentVertex == edge1.Target)
                        edge1.SetLocalAttribute("TargetLeft", Procedure, vertexBetweenEdges);

                    if (currentVertex == edge2.Source)
                        edge2.SetLocalAttribute("SourceLeft", Procedure, vertexBetweenEdges);
                    else if (currentVertex == edge2.Target)
                        edge2.SetLocalAttribute("TargetRight", Procedure, vertexBetweenEdges);

                    //now, for the center vertex, we only need to do this for the "edge1", i.e. the first vertex
                    if (currentVertex == edge1.Source)
                        edge1.SetLocalAttribute("SourceCenter", Procedure, centerVertex);
                    else if (currentVertex == edge1.Target)
                        edge1.SetLocalAttribute("TargetCenter", Procedure, centerVertex);


                    midVertices.Add(vertexBetweenEdges);
                }

                //create the faces around the junction
                faces.AddRange(CreateJuntionFaces(centerVertex, currentVertex, normal, orderedEdges, midVertices));
            }


            //creates the faces that correspond to the middle, 
            //"Street" sections between the junctions
            faces.AddRange(CreateMiddleFaces(pathEntity));


            //closes the holes between the "street" faces
            //reject faces that are facing the opposite direction
            if (_parameterCloseInsides.Value)
                foreach (var newFace in CloseInsideAreas(faces))
                    if (newFace.Normal.Dot(Vector3D.ZVector) < 0)
                    {
                        newFace.Detach();
                    }
                    else
                    {
                        _attributeSectionType[newFace] = "Inside";
                        faces.Add(newFace);
                    }


            //if no errors occur and and at least one face has been produced
            //output the new mesh entity with the same attributes as the pathentity
            if (faces.Count > 0)
            {
                var newMeshEntity = new MeshEntity(faces);
                pathEntity.Attributes.SetAttributesTo(newMeshEntity.Attributes);
                newMeshEntity.AdjustScope(pathEntity.BoxScope);

                return newMeshEntity;
            }

            throw new Exception("Could not build any faces from the given path.");
        }



        private IEnumerable<Face> CreateMiddleFaces(PathEntity pathEntity)
        {
            List<Face> newFaces = new List<Face>();

            foreach (PathEdge pathEdge in pathEntity.Edges)
                try
                {
                    var sourceRight = pathEdge.GetLocalAttribute<Vertex>("SourceRight", Procedure);
                    var sourceCenter = pathEdge.GetLocalAttribute<Vertex>("SourceCenter", Procedure);
                    var sourceLeft = pathEdge.GetLocalAttribute<Vertex>("SourceLeft", Procedure);
                    var targetRight = pathEdge.GetLocalAttribute<Vertex>("TargetRight", Procedure);
                    var targetCenter = pathEdge.GetLocalAttribute<Vertex>("TargetCenter", Procedure);
                    var targetLeft = pathEdge.GetLocalAttribute<Vertex>("TargetLeft", Procedure);


                    var newFace = new Face(sourceRight, sourceCenter, sourceLeft, targetLeft, targetCenter, targetRight);

                    sourceRight[newFace].UV0 = new Vector2D(1, 0);
                    sourceCenter[newFace].UV0 = new Vector2D(0.5f, 0);
                    sourceLeft[newFace].UV0 = new Vector2D(0, 0);
                    targetLeft[newFace].UV0 = new Vector2D(0, 1);
                    targetCenter[newFace].UV0 = new Vector2D(0.5f, 1);
                    targetRight[newFace].UV0 = new Vector2D(1, 1);

                    //copy the attributes of the original edge
                    pathEdge.Attributes.SetAttributesTo(newFace.Attributes);

                    //identify the type of face
                    _attributeSectionType[newFace] = "Path";
                    newFace.CalculateTangentAndBinormal();

                    newFaces.Add(newFace);
                }
                catch (Exception)
                {
                    ProcedureEnvironment.GetService<ILogger>().Log("Could not create face.", LogType.Error);
                }

            return newFaces;
        }



        /// <summary>
        /// Function used in a ordering function to determine the angle around a vertex (and a given axis normal).
        /// </summary>
        /// <returns>The angle between that edge and the first edge direction.</returns>
        private double GetAngle(PathVertex centralVertex, Vector3D firstDirection, Vector3D normalVector, PathEdge pathEdge)
        {
            var direction = pathEdge.GetDirectionFrom(centralVertex).ProjectToPlane(normalVector);

            if (firstDirection.Cross(direction).Dot(normalVector) > 0)
                return Math.PI + direction.AngleTo(-firstDirection);

            return direction.AngleTo(firstDirection);
        }
    }
}