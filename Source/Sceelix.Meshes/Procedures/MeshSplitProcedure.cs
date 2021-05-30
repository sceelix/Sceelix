using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Splits the input mesh into several slices according
    /// to specific constraints.
    /// </summary>
    [Procedure("ce1b6db6-6365-4221-acb2-d2fef8a29ccb", Label = "Mesh Split", Category = "Mesh")]
    public class MeshSplitProcedure : SystemProcedure
    {
        /// <summary>
        /// Mesh that is meant to be split.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");


        /// <summary>
        /// The planes/axes that define the direction of the splitting.
        /// </summary>
        private readonly SelectListParameter<SplitAxisParameter> _parameterSplitAxis = new SelectListParameter<SplitAxisParameter>("Split Axis", "Scope Axis");

        /// <summary>
        /// List of slices to split.
        /// </summary>
        private readonly ListParameter _parameterSplits = new ListParameter("Slices",
            () => new RepetitiveSliceParameter {Description = "Splits the meshes repeatedly while there is available spacing."},
            () => new SliceParameter());


        /// <summary>
        /// Attribute where the index of the 
        /// </summary>
        private readonly AttributeParameter<int> _attributeIndex = new AttributeParameter<int>("Index", AttributeAccess.Write);



        private IEnumerable<Face> BuildCaps(MeshEntity meshEntity, Vector3D normal, List<Edge> cuttedEdges)
        {
            Queue<Edge> lineSegments = new Queue<Edge>(cuttedEdges);

            while (lineSegments.Count > 0)
            {
                Queue<Edge> auxiliaryQueue = new Queue<Edge>();

                EdgeSequence sequence = new EdgeSequence(lineSegments.Dequeue(), 0.001f); //CapPrecision.Value
                while (lineSegments.Count > 0)
                {
                    Edge secondLine = lineSegments.Dequeue();

                    //try add it to the chain
                    //if it does not match, add it to the auxiliary queue, so that it will be considered in the next round
                    if (sequence.TryAdd(secondLine))
                    {
                        if (sequence.IsClosed)
                        {
                            List<Vector3D> positions = sequence.Positions.Select(vertex => vertex.Position).ToList();
                            if (Face.CanCreateValidFace(positions))
                            {
                                Face face = new Face(positions); //.GetRange(0, positions.Count)

                                if (face.Normal.Dot(normal) > 0)
                                {
                                    meshEntity.Add(face);

                                    positions.Reverse();
                                    Face flippedFace = new Face(positions);
                                    yield return flippedFace;
                                }
                                else
                                {
                                    positions.Reverse();
                                    Face flippedFace = new Face(positions);
                                    meshEntity.Add(flippedFace);

                                    yield return face;
                                }
                            }

                            //add the remaining to the auxiliary
                            foreach (Edge edge in lineSegments)
                                auxiliaryQueue.Enqueue(edge);

                            break;
                        }

                        //reset the list
                        foreach (Edge edge in lineSegments)
                            auxiliaryQueue.Enqueue(edge);

                        lineSegments = auxiliaryQueue;
                        auxiliaryQueue = new Queue<Edge>();
                    }
                    else
                    {
                        auxiliaryQueue.Enqueue(secondLine);
                    }
                }

                lineSegments = new Queue<Edge>(auxiliaryQueue);
            }
        }



        private void DetermineFaces(CircularList<ExtendedVertex> extendedBaseVertexList, int index, List<Face> faces, IEnumerable<ExtendedVertex> currentList)
        {
            ExtendedVertex currentVertex = extendedBaseVertexList[index];

            List<ExtendedVertex> extendedVertices = new List<ExtendedVertex>(currentList);

            while (!currentVertex.HasBeenVisited)
            {
                extendedVertices.Add(currentVertex);
                currentVertex.HasBeenVisited = true;

                if (currentVertex.IsCrossPoint)
                {
                    currentVertex.NextCutVertex.HasBeenVisited = true;
                    DetermineFaces(extendedBaseVertexList, index + 1, faces, new[] {currentVertex.NextCutVertex, currentVertex});

                    extendedVertices.Add(currentVertex.NextCutVertex);
                    index = currentVertex.NextCutVertex.Index + 1;
                }
                else
                {
                    index++;
                }

                currentVertex = extendedBaseVertexList[index];
            }

            List<Vertex> vertices = extendedVertices.Select(val => val.CurrentVertex).ToList();

            if (Face.CanCreateValidFace(vertices))
                faces.Add(new Face(vertices));
        }



        /// <summary>
        /// Determines the cutting plane (located at the "bottom" of the scope) and the available size to cut
        /// </summary>
        /// <param name="meshEntity"></param>
        /// <param name="cutDirection"></param>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        private Plane3D GetPlaneAndSize(MeshEntity meshEntity, Vector3D cutDirection, out float availableSize)
        {
            Vector3D[] point = meshEntity.FaceVertices.Select(val => val.Position).ToArray();
            Plane3D plane = new Plane3D(cutDirection, point[0]);
            availableSize = 0;

            for (int i = 1; i < point.Length; i++)
            {
                float distance = plane.DistanceToPoint(point[i]);
                if (distance < 0)
                {
                    plane = new Plane3D(plane.Normal, point[i]);

                    availableSize += Math.Abs(distance);
                }

                availableSize = availableSize > distance ? availableSize : distance;
            }

            return plane;
        }



        protected override void Run()
        {
            //fetch the MeshEntity to be cut
            MeshEntity meshEntity = _input.Read();

            BoxScope parentScope = meshEntity.BoxScope;

            //find out what is the actual direction we should cut
            Vector3D cutDirection = _parameterSplitAxis.Items.First().GetCutDirection(parentScope);
            /*Vector3D cutDirection = SplitAxis.Value.Normalize();

            if (ScopeRelative.Value)
                cutDirection = parentScope.ToWorldDirection(cutDirection);*/

            //find out the cutting plane and how big the MeshEntity is in the given direction
            float availableSize;

            Plane3D plane = GetPlaneAndSize(meshEntity, cutDirection, out availableSize);

            bool completelyCut = SetupSlices(_parameterSplits.Items.OfType<ISliceParameter>().ToList(), availableSize);

            //var slices = SplitsParameter.Items;
            //for (int i = 0; i < slices.Count; i++)
            int counter = 0;

            foreach (var parameter in _parameterSplits.Items)
            {
                var repetitiveSliceParameter = parameter as RepetitiveSliceParameter;
                if (repetitiveSliceParameter != null)
                {
                    for (int i = 0; i < repetitiveSliceParameter.Repetitions; i++)
                        foreach (var sliceParameter in repetitiveSliceParameter.Items)
                            meshEntity = Slice(meshEntity, ref plane, sliceParameter, completelyCut && i == repetitiveSliceParameter.Repetitions - 1 && parameter == _parameterSplits.Items.Last() && sliceParameter == repetitiveSliceParameter.Items.Last(), ref counter);
                        //IndexAttribute[MeshEntity] = counter++;
                }
                else
                {
                    var sliceParameter = (SliceParameter) parameter;

                    meshEntity = Slice(meshEntity, ref plane, sliceParameter, completelyCut && parameter == _parameterSplits.Items.Last(), ref counter);
                    //IndexAttribute[MeshEntity] = counter++;
                }
            }
        }



        private static bool SetupSlices(List<ISliceParameter> splitParameters, float totalSize)
        {
            //step 1: calculate relative sizes
            splitParameters.ForEach(x => x.CalculateDesiredSizes(totalSize));

            float totalDesiredSize = splitParameters.OfType<SliceParameter>().Sum(x => x.DesiredSize);

            //if this is the case, we don't even consider subitems/repetitive splices
            if (totalDesiredSize > totalSize)
            {
                int numFlexibleItems = splitParameters.Sum(x => x.NumFlexibleItems);
                float spaceToRemove = 0;
                if (numFlexibleItems > 0)
                    spaceToRemove = (totalDesiredSize - totalSize) / numFlexibleItems;


                foreach (var splitParameter in splitParameters)
                    if (splitParameter is RepetitiveSliceParameter)
                    {
                        ((RepetitiveSliceParameter) splitParameter).Repetitions = 0;
                    }
                    else if (splitParameter is SliceParameter)
                    {
                        SliceParameter slice = (SliceParameter) splitParameter;

                        if (slice.IsFlexible)
                            slice.ActualSize = Math.Max(0, slice.DesiredSize - spaceToRemove);
                        else
                            slice.ActualSize = slice.DesiredSize;
                    }

                float consumableSpace = totalSize;

                //if the slices go above the maximum size, make their size 0
                foreach (var slice in splitParameters.OfType<SliceParameter>())
                    if (consumableSpace < slice.ActualSize)
                    {
                        slice.ActualSize = consumableSpace;
                        consumableSpace = 0;
                    }
                    else
                    {
                        consumableSpace -= slice.ActualSize;
                    }

                /*foreach (var splitParameter in splitParameters)
                {
                    if (splitParameter is RepetitiveSliceParameter)
                        ((RepetitiveSliceParameter) splitParameter).Repetitions = 0;
                    else if (splitParameter is SliceParameter)
                    {
                        SliceParameter slice = (SliceParameter) splitParameter;

                        if (consumableSpace <= 0)
                        {
                            slice.ActualSize = 0;
                        }
                        else if (slice.IsFlexible)
                        {
                            slice.ActualSize = Math.Max(0, slice.DesiredSize - spaceToRemove);
                            consumableSpace -= slice.ActualSize;
                        }
                        if (consumableSpace > slice.DesiredSize)
                        {
                            slice.ActualSize = slice.DesiredSize;
                            consumableSpace -= slice.DesiredSize;
                        }
                        else
                        {
                            slice.ActualSize = consumableSpace;
                            consumableSpace = 0;
                        }
                    }
                }*/

                return true;
            }

            var allSliceParameters = splitParameters.SelectMany(x => x.AllSubSlices).ToList();

            //determine how much room we have left to share among repeat splits
            var availableSpaceToSplit = totalSize - totalDesiredSize;
            var repetitiveSliceParameters = splitParameters.OfType<RepetitiveSliceParameter>().ToList();
            float amountToShare = availableSpaceToSplit / repetitiveSliceParameters.Count;

            foreach (var repetitiveSliceParameter in repetitiveSliceParameters)
            {
                float takenSpace = repetitiveSliceParameter.UpdateRepetitions(amountToShare);
                totalDesiredSize += takenSpace;
            }

            //calculate the available space to share with the flexibles
            availableSpaceToSplit = totalSize - totalDesiredSize;
            var spaceForEachFlexible = availableSpaceToSplit / splitParameters.Sum(x => x.NumFlexibleItems);

            foreach (var sliceParameter in allSliceParameters)
                if (sliceParameter.IsFlexible)
                    sliceParameter.ActualSize = sliceParameter.DesiredSize + spaceForEachFlexible;
                else
                    sliceParameter.ActualSize = sliceParameter.DesiredSize;

            return allSliceParameters.Any(x => x.IsFlexible) || Math.Abs(totalDesiredSize - totalSize) < float.Epsilon;
        }



        private MeshEntity Slice(MeshEntity meshEntity, ref Plane3D plane, SliceParameter sliceParameter, bool isTheLast, ref int counter)
        {
            if (sliceParameter.ActualSize <= 0)
                return meshEntity;

            if (isTheLast)
            {
                meshEntity.AdjustScope(meshEntity.BoxScope);
                //SliceIndex[MeshEntity] = totalSliceCount - 1;
                meshEntity.CleanFaceConnections();
                MeshUnifyProcedure.UnifyVerticesParameter.Unify(meshEntity, 0.001f);

                //do not forget to save the index
                _attributeIndex[meshEntity] = counter++;

                sliceParameter.Output.Write(meshEntity);
            }
            else
            {
                //shift the slicing plane upwards
                plane = plane.Translate(sliceParameter.ActualSize);

                MeshEntity aboveSlice = SplitMesh(meshEntity, plane, sliceParameter.CapParameter.Value);

                //TODO: Review this later, very special error case
                if (aboveSlice == null)
                    return meshEntity;

                //only output this mesh if it actually has faces in it
                if (meshEntity.Faces.Count > 0)
                {
                    MeshUnifyProcedure.UnifyVerticesParameter.Unify(meshEntity, 0.001f);
                    //SliceIndex[MeshEntity] = index;
                    meshEntity.CleanFaceConnections();

                    //do not forget to save the index
                    _attributeIndex[meshEntity] = counter++;

                    sliceParameter.Output.Write(meshEntity);
                }

                meshEntity = aboveSlice;
            }

            return meshEntity;
        }



        private IEnumerable<Face> SplitFace(MeshEntity baseMeshEntity, Face face, Plane3D cuttingPlane, List<Edge> cuttedEdges)
        {
            //if the cutting plane is either parallel to the face, there's nothing to cut
            if (cuttingPlane.Normal.IsCollinear(face.Normal))
                yield break;

            CircularList<ExtendedVertex> extendedBaseVertexList = new CircularList<ExtendedVertex>();

            List<Edge> edges = face.Edges.ToList();
            for (int index = 0; index < edges.Count; index++)
            {
                var edge = edges[index];
                ExtendedVertex extendedVertex;

                extendedBaseVertexList.Add(extendedVertex = new ExtendedVertex(edge.V0, extendedBaseVertexList.Count));

                Vector3D intersectionPoint;
                Edge.EdgeIntersectionResult edgeLine3DIntersection = edge.PlaneIntersection(cuttingPlane, out intersectionPoint);
                if (edgeLine3DIntersection == Edge.EdgeIntersectionResult.IntersectingV0 || edgeLine3DIntersection == Edge.EdgeIntersectionResult.Coincident)
                    extendedVertex.IsCrossPoint = true;

                if (edgeLine3DIntersection == Edge.EdgeIntersectionResult.IntersectingV1 || edgeLine3DIntersection == Edge.EdgeIntersectionResult.Coincident)
                {
                    //extendedBaseVertexList.Add(new ExtendedVertex(edge.V1, extendedBaseVertexList.Count) { IsCrossPoint = true });

                    //skip the next iteration
                    //index++;
                }

                if (edgeLine3DIntersection == Edge.EdgeIntersectionResult.IntersectingMiddle)
                    extendedBaseVertexList.Add(new ExtendedVertex(new Vertex(intersectionPoint), extendedBaseVertexList.Count) {IsCrossPoint = true});
            }

            if (!extendedBaseVertexList.Where(val => val.IsCrossPoint).Any())
                yield break;

            //try
            //{

            //get the plane at "the end" of the cutting line
            Plane3D planeOrthogonal = Plane3D.GetBackPlane(face.Vertices.Select(val => val.Position).ToList(), face.Normal.Cross(cuttingPlane.Normal));

            //and order all the crosspoints from that plane
            List<ExtendedVertex> orderedCrossPoints = extendedBaseVertexList.Where(val => val.IsCrossPoint).OrderBy(val => planeOrthogonal.DistanceToPoint(val.CurrentVertex.Position)).ToList();

            //then "connect" all the matching crosspoints
            for (int index = 0; index < orderedCrossPoints.Count; index += 2)
            {
                orderedCrossPoints[index].NextCutVertex = orderedCrossPoints[index + 1];
                orderedCrossPoints[index + 1].NextCutVertex = orderedCrossPoints[index];

                //add the cutting edge to the list
                cuttedEdges.Add(new Edge(orderedCrossPoints[index].CurrentVertex, orderedCrossPoints[index + 1].CurrentVertex));
            }

            /*}
            catch (Exception ex)
            {
                //we should show a warning message here
                yield break;
            }*/

            List<Face> newFaces = new List<Face>();
            DetermineFaces(extendedBaseVertexList, 0, newFaces, new List<ExtendedVertex>());

            foreach (Face newFace in newFaces)
                if (cuttingPlane.LocationToPlane(newFace.Centroid) == PointToPlaneLocation.Above)
                    yield return newFace;
                else
                    baseMeshEntity.Add(newFace);
        }



        private MeshEntity SplitMesh(MeshEntity meshEntity, Plane3D cuttingPlane, bool cap)
        {
            List<Face> facesAbove = new List<Face>();
            List<Edge> cuttedEdges = new List<Edge>();

            //if (meshEntity.HasAttribute(new GlobalAttributeKey("Id")) && meshEntity.GetGlobalAttribute<int>("Id") == 0)
            //    Debug.WriteLine("I ran!");

            foreach (Face face in meshEntity.ToList())
            {
                IEnumerable<Face> faces = SplitFace(meshEntity, face, cuttingPlane, cuttedEdges);

                //if the face could not be cut, it could be because it is parallel, above or below the plane
                IEnumerable<Face> faceList = faces as IList<Face> ?? faces.ToList();
                if (!faceList.Any())
                {
                    //if they are all above the cuttng plane, it will belong to the other MeshEntity, not this one
                    if (cuttingPlane.LocationToPlane(face.Centroid) == PointToPlaneLocation.Above) //face.AllAtToPlaneLocation(cuttingPlane, PointToPlaneLocation.Above)
                    {
                        facesAbove.Add(face);

                        //remove from this MeshEntity
                        meshEntity.RemoveAndDetach(face); //or only Remove?
                    }
                }
                else
                {
                    facesAbove.AddRange(faceList);

                    //remove from this MeshEntity
                    meshEntity.RemoveAndDetach(face); //or only remove?
                }
            }

            if (cap)
                facesAbove.AddRange(BuildCaps(meshEntity, cuttingPlane.Normal, cuttedEdges));

            //the mesh that remains below can be empty, so only perform this if possible
            if (meshEntity.Faces.Count > 0) meshEntity.AdjustScope();

            //resize the scope to fit the new MeshEntity

            try
            {
                return meshEntity.CreateDerived(facesAbove);
            }
            catch (Exception)
            {
                //TODO:Review why an exception may be happening at this point
                //I witnessed it happening with half-vertices being null (for some reason)
                return null;
            }
            //if(!facesAbove.Any())
            //    Console.WriteLine();

            //return meshEntity.CreateDerived(facesAbove);
        }



        private class ExtendedVertex
        {
            public ExtendedVertex(Vertex currentVertex, int index)
            {
                CurrentVertex = currentVertex;
                Index = index;
            }



            public Vertex CurrentVertex
            {
                get;
            }


            public bool HasBeenVisited
            {
                get;
                set;
            }


            public int Index
            {
                get;
            }


            public bool IsCrossPoint
            {
                get;
                set;
            }


            public ExtendedVertex NextCutVertex
            {
                get;
                set;
            }
        }

        private class EdgeSequence
        {
            private readonly float _precision;



            public EdgeSequence(Edge startSegment, float precision)
            {
                _precision = precision;
                Positions.Add(startSegment.V0);
                Positions.Add(startSegment.V1);
            }



            public Face Face =>
                //Face face = ;
                new Face(Positions);


            public bool IsClosed => Positions.Count > 2 && Positions.Last().Position.AproxEquals(Positions.First().Position, _precision);


            public List<Vertex> Positions
            {
                get;
            } = new List<Vertex>();



            public bool TryAdd(Edge secondLine)
            {
                Vector3D lastPosition = Positions.Last().Position;
                Vector3D firstPosition = Positions.First().Position;
                if (lastPosition.AproxEquals(secondLine.V0.Position, _precision))
                {
                    Positions.Add(secondLine.V1);
                    return true;
                }

                if (lastPosition.AproxEquals(secondLine.V1.Position, _precision))
                {
                    Positions.Add(secondLine.V0);
                    return true;
                }

                if (firstPosition.AproxEquals(secondLine.V1.Position, _precision))
                {
                    Positions.Insert(0, secondLine.V0);
                    return true;
                }

                if (firstPosition.AproxEquals(secondLine.V0.Position, _precision))
                {
                    Positions.Insert(0, secondLine.V1);
                    return true;
                }

                return false;
            }
        }

        #region Slice Parameters

        public interface ISliceParameter
        {
            IEnumerable<SliceParameter> AllSubSlices
            {
                get;
            }


            int NumFlexibleItems
            {
                get;
            }



            //float Measure(float availableSpace);
            //bool IsFlexible { get; }
            void CalculateDesiredSizes(float totalSize);

            void Reset();
        }

        /// <summary>
        /// Splits the meshes repeatedly while there is available spacing.
        /// </summary>
        public class RepetitiveSliceParameter : ListParameter<SliceParameter>, ISliceParameter
        {
            public RepetitiveSliceParameter()
                : base("Repetitive Slice", () => new SliceParameter()) //() => new RepetitiveSliceParameter(),
            {
            }



            public IEnumerable<SliceParameter> AllSubSlices
            {
                get { return Items.SelectMany(x => ((ISliceParameter) x).AllSubSlices); }
            }



            public int NumFlexibleItems
            {
                get { return Items.Sum(x => x.NumFlexibleItems) * Repetitions; }
            }



            public int Repetitions
            {
                get;
                set;
            }


/*public bool IsFlexible
                        {
                            get { return true; }
                        }*/



            public void CalculateDesiredSizes(float totalSize)
            {
                foreach (ISliceParameter splitParameter in Items)
                    splitParameter.CalculateDesiredSizes(totalSize);
            }



            public void Reset()
            {
                Repetitions = 0;

                foreach (var sliceParameter in Items)
                    sliceParameter.Reset();
            }



            public float UpdateRepetitions(float availableSpace)
            {
                var sum = Items.Sum(x => x.DesiredSize);

                Repetitions = (int) (availableSpace / sum);
                return sum * Repetitions;

                /*if (sum > availableSpace)
                {
                    Repetitions = 0;
                    return 0;
                }
                else
                {
                    Repetitions = (int) (availableSpace/sum);
                    return sum * Repetitions;
                }*/
            }
        }

        /// <summary>
        /// Splits a mesh piece with a given size.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Parameters.CompoundParameter" />
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSplitProcedure.ISliceParameter" />
        public class SliceParameter : CompoundParameter, ISliceParameter
        {
            /// <summary>
            /// The mesh section that was split.
            /// </summary>
            internal readonly Output<MeshEntity> Output = new Output<MeshEntity>("Output");

            /// <summary>
            /// Size of the slice.
            /// </summary>
            internal readonly FloatParameter SizeParameter = new FloatParameter("Size", 1);

            /// <summary>
            /// If "Absolute", the value of "Amount" will be considered as an absolute value, if "Relative", the value will be a percentage. (0 - 1)
            /// </summary>
            internal readonly ChoiceParameter SizingParameter = new ChoiceParameter("Sizing", "Absolute", "Absolute", "Relative");

            /// <summary>
            /// If checked, the size of this slice will be adjusted according to the available space.
            /// </summary>
            internal readonly BoolParameter FlexibleParameter = new BoolParameter("Flexible", true);

            /// <summary>
            /// If the object being split is a closed solid, activating this option will make the inner cut face visible.
            /// </summary>
            internal readonly BoolParameter CapParameter = new BoolParameter("Cap", false);



            public SliceParameter()
                : base("Slice")
            {
            }



            internal float ActualSize
            {
                get;
                set;
            }



            public IEnumerable<SliceParameter> AllSubSlices
            {
                get { yield return this; }
            }



            internal float DesiredSize
            {
                get;
                set;
            }


            public bool IsFlexible => FlexibleParameter.Value;


            public int NumFlexibleItems => IsFlexible ? 1 : 0;



            public void CalculateDesiredSizes(float totalSize)
            {
                if (SizingParameter.Value == "Relative")
                    DesiredSize = SizeParameter.Value * totalSize;
                else
                    DesiredSize = SizeParameter.Value;
            }



            public void Reset()
            {
                DesiredSize = ActualSize = 0;
            }
        }

        #endregion

        #region Split Axis Parameters

        public abstract class SplitAxisParameter : CompoundParameter
        {
            protected SplitAxisParameter(string label)
                : base(label)
            {
            }



            public abstract Vector3D GetCutDirection(BoxScope parentScope);
        }

        /// <summary>
        /// Splits the mesh by a given plane.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSplitProcedure.SplitAxisParameter" />
        public class PlaneNormalParameter : SplitAxisParameter
        {
            /// <summary>
            /// The normal direction of the plane.
            /// </summary>
            private readonly Vector3DParameter _parameterSplitAxis = new Vector3DParameter("Direction", Vector3D.XVector);

            /// <summary>
            /// Indicates if the specified plane normal is relative to the scope orientation or not.
            /// </summary>
            private readonly BoolParameter _parameterScopeRelative = new BoolParameter("Scope Relative", true);



            public PlaneNormalParameter()
                : base("Plane Normal")
            {
            }



            public override Vector3D GetCutDirection(BoxScope parentScope)
            {
                Vector3D cutDirection = _parameterSplitAxis.Value.Normalize();

                if (_parameterScopeRelative.Value)
                    cutDirection = parentScope.ToWorldDirection(cutDirection);

                return cutDirection;
            }
        }

        /// <summary>
        /// Splits the mesh along a given scope axis.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSplitProcedure.SplitAxisParameter" />
        public class ScopeAxisParameter : SplitAxisParameter
        {
            /// <summary>
            /// Scope axis along which the splitting will occur.
            /// </summary>
            private readonly ChoiceParameter _parameterSplitAxis = new ChoiceParameter("Axis", "X", "X", "Y", "Z");



            public ScopeAxisParameter()
                : base("Scope Axis")
            {
            }



            public override Vector3D GetCutDirection(BoxScope parentScope)
            {
                switch (_parameterSplitAxis.Value)
                {
                    case "X":
                        return parentScope.ToWorldDirection(Vector3D.XVector);
                    case "Y":
                        return parentScope.ToWorldDirection(Vector3D.YVector);
                    case "Z":
                        return parentScope.ToWorldDirection(Vector3D.ZVector);
                }

                return Vector3D.Zero;
            }
        }

        /// <summary>
        /// Splits the mesh along an axis based on their relative size.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSplitProcedure.SplitAxisParameter" />
        public class RelativeAxisParameter : SplitAxisParameter
        {
            /// <summary>
            /// The axis along which to split the mesh.<br/>
            /// <b>Minimum</b> means that the shortest axis will be chosen.<br/>
            /// <b>Maximum</b> means that the longest axis will be chosen.<br/>
            /// <b>Intermediate</b> means that the other axis (neither shortest or longest) will be chosen.
            /// </summary>
            private readonly ChoiceParameter _parameterAxis = new ChoiceParameter("Axis", "Maximum", "Minimum", "Intermediate", "Maximum");



            public RelativeAxisParameter()
                : base("Relative Axis")
            {
            }



            public override Vector3D GetCutDirection(BoxScope parentScope)
            {
                var axisInfos = new[] {new {Size = parentScope.Sizes.X, Vector = Vector3D.XVector}, new {Size = parentScope.Sizes.Y, Vector = Vector3D.YVector}, new {Size = parentScope.Sizes.Z, Vector = Vector3D.ZVector}};

                var list = axisInfos.OrderBy(val => val.Size).ToList();

                var selection = list[1].Vector;

                if (_parameterAxis.Value == "Minimum")
                    selection = list[0].Vector;
                if (_parameterAxis.Value == "Maximum")
                    selection = list[2].Vector;

                return parentScope.ToWorldDirection(selection);
            }
        }

        #endregion
    }
}