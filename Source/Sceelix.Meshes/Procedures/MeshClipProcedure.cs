using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Algorithms;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Extensions;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Performs Clipping operations (Union, Intersection, Difference, Xor) between mesh faces that lie on the same plane.
    /// </summary>
    [Procedure("09b038d3-ee37-4e0b-9091-9aff6f690ce6", Label = "Mesh Clip", Category = "Mesh")]
    public class MeshClipProcedure : SystemProcedure
    {
        /// <summary>
        /// The first/source/minuend mesh used in the clipping operation.
        /// </summary>
        private readonly SingleInput<MeshEntity> _inputSource = new SingleInput<MeshEntity>("Source");

        /// <summary>
        /// The second/target/subtrahend mesh used in the clipping operation.
        /// </summary>
        private readonly SingleInput<MeshEntity> _inputTarget = new SingleInput<MeshEntity>("Target");

        /// <summary>
        /// Resulting mesh from the clipping operation.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Type of clipping operation to apply.<br/>
        /// <b>Union</b> means that the result is the sum of the face areas.<br/>
        /// <b>Intersection</b> means that the result is the intersection of the faces areas.<br/>
        /// <b>Difference</b> means that the result is the difference between the faces areas (where the target removes intersecting areas from the source).<br/>
        /// <b>Difference</b> means that the result corresponds to the areas that do not overlap.<br/>
        /// </summary>
        private readonly ChoiceParameter _parameterClipType = new ChoiceParameter("Operation", "Union", "Union", "Intersection", "Difference", "Xor");



        private IEnumerable<Face> PolyTreeToFaceList(PolyNode polyNode, BoxScope alignedScope, Face parentFace = null, bool reverse = true)
        {
            Face futureParent = null;

            if (polyNode.IsHole && parentFace != null)
            {
                var positions = polyNode.Contour.Select(point => point.ToVector3D(alignedScope));

                if (reverse)
                    positions = positions.Reverse();

                parentFace.AddHole(positions);
            }
            else if (polyNode.Contour.Any())
            {
                var positions = polyNode.Contour.Select(point => point.ToVector3D(alignedScope));

                if (reverse)
                    positions = positions.Reverse();

                futureParent = new Face(positions);

                yield return futureParent;
            }

            foreach (Face subface in polyNode.Childs.SelectMany(x => PolyTreeToFaceList(x, alignedScope, futureParent, reverse)))
                yield return subface;
        }



        protected override void Run()
        {
            ClipType clipType = (ClipType) Enum.Parse(typeof(ClipType), "ct" + _parameterClipType.Value);

            var sourceMesh = _inputSource.Read();
            var targetMesh = _inputTarget.Read();

            //identify the target faces
            foreach (Face face in targetMesh)
                face.SetLocalAttribute("IsTarget", this, true);

            List<Face> totalMeshFaces = new List<Face>();

            var groupingByPlane = sourceMesh.Union(targetMesh).GroupBy(val => new {Normal = val.Normal.Round(), DistanceToPoint = Math.Round(val.Plane.DistanceToPoint(Vector3D.Zero), 3)});
            foreach (var group in groupingByPlane)
            {
                //for each group, we need a target (the clip) and a source (the subject)
                var faces = group.ToList();
                if (faces.Any(x => x.GetLocalAttribute<bool>("IsTarget", this))
                    && faces.Any(x => !x.GetLocalAttribute<bool>("IsTarget", this)))
                {
                    var alignedScope = faces.First().GetAlignedScope();
                    Clipper clipper = new Clipper();

                    foreach (Face face in faces)
                    {
                        var sourceIntList = face.Vertices.Select(x => x.Position.ToIntPoint(alignedScope)).ToList();

                        var polyType = face.GetLocalAttribute<bool>("IsTarget", this) ? PolyType.ptClip : PolyType.ptSubject;

                        clipper.AddPath(sourceIntList, polyType, true);

                        if (face.HasHoles)
                            foreach (CircularList<Vertex> circularList in face.Holes)
                                clipper.AddPath(circularList.Select(x => x.Position.ToIntPoint(alignedScope)).ToList(), polyType, true);
                    }

                    PolyTree result = new PolyTree();
                    clipper.Execute(clipType, result);

                    var resultingFaces = result.PolyTreeToFaceList(alignedScope);
                    totalMeshFaces.AddRange(resultingFaces);
                }
                else
                {
                    totalMeshFaces.AddRange(faces);
                }
            }

            if (totalMeshFaces.Count > 0)
            {
                var resultMesh = new MeshEntity(totalMeshFaces);
                sourceMesh.Attributes.SetAttributesTo(resultMesh.Attributes);
                targetMesh.Attributes.MergeAttributesTo(resultMesh.Attributes);

                _output.Write(resultMesh);
            }


            /*List<Face> totalMeshFaces = new List<Face>();

            foreach (Face sourceFace in sourceMesh.Faces.ToList())
            {
                Face currentFace = sourceFace;

                var alignedScope = currentFace.GetAlignedScope();

                foreach (Face targetFace in targetMesh.Faces.ToList())
                {
                    if (currentFace.Plane.CoincidentAndWithSameDirection(targetFace.Plane))
                    {
                        var sourceIntList = currentFace.Vertices.Select(x => x.Position.ToIntPoint(alignedScope)).ToList();

                        Clipper clipper = new Clipper();
                        clipper.AddPath(sourceIntList, PolyType.ptSubject, true);

                        if (currentFace.HasHoles)
                        {
                            foreach (CircularList<Vertex> circularList in currentFace.Holes)
                            {
                                clipper.AddPath(circularList.Select(x => x.Position.ToIntPoint(alignedScope)).ToList(),PolyType.ptSubject, true);
                            }
                        }

                        var targetIntList = targetFace.Vertices.Select(x => x.Position.ToIntPoint(alignedScope)).ToList();
                        clipper.AddPath(targetIntList, PolyType.ptSubject, true);

                        if (targetFace.HasHoles)
                        {
                            foreach (CircularList<Vertex> circularList in targetFace.Holes)
                            {
                                clipper.AddPath(circularList.Select(x => x.Position.ToIntPoint(alignedScope)).ToList(), PolyType.ptSubject, true);
                            }
                        }


                        PolyTree result = new PolyTree();

                        //clipper.AddPaths(GetPaths(polygon).ToList(), PolyType.ptSubject, true);
                        clipper.Execute(clipType, result);

                        var faces = PolyTreeToFaceList(result, alignedScope);
                        totalMeshFaces.AddRange(faces);

                        targetFace.SetLocalAttribute("Is Clipped",this,true);
                    }
                }

                //if(currentFace != sourceFace)
                //    sourceMesh.Add(currentFace);
            }

            if (totalMeshFaces.Count > 0)
            {
                var resultMesh = new MeshEntity(totalMeshFaces);
                sourceMesh.Attributes.SetAttributesTo(resultMesh.Attributes);
                targetMesh.Attributes.MergeAttributesTo(resultMesh.Attributes);

                _output.Write(resultMesh);
            }*/
        }
    }
}