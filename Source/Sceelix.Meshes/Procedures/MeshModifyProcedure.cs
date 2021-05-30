using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Extensions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Logging;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Algorithms;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Extensions;
using Sceelix.Meshes.Operations;
using StraightSkeletonNet;
using StraightSkeletonNet.Primitives;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Applies sequences of geometrical, scope or property 
    /// transformations to the given input mesh.
    /// </summary>
    /// <Coiso></Coiso>
    [Procedure("75cdfc0b-7cdd-485a-85d2-8a781b3a79bc", Label = "Mesh Modify", Category = "Mesh")]
    public class MeshModifyProcedure : TransferProcedure<MeshEntity>
    {
        /// <summary>
        /// The operation to be applied to the mesh.
        /// </summary>
        private readonly SelectListParameter<MeshModifyParameter> _parameterOperation = new SelectListParameter<MeshModifyParameter>("Operation", "Extrude");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterOperation.SubParameterLabels);



        protected override MeshEntity Process(MeshEntity meshEntity)
        {
            foreach (MeshModifyParameter transformMeshParameter in _parameterOperation.Items) meshEntity = transformMeshParameter.Transform(meshEntity);

            return meshEntity;
        }



        #region Abstract Parameter

        public abstract class MeshModifyParameter : CompoundParameter
        {
            protected MeshModifyParameter(string label)
                : base(label)
            {
            }



            public abstract MeshEntity Transform(MeshEntity meshEntity);
        }

        #endregion

        #region Lathe

        /// <summary>
        /// Performs a lathe operation that creates a new mesh from rotating the input mesh around a given axis.
        /// </summary>
        public class LatheParameter : MeshModifyParameter
        {
            /// <summary>
            /// The axis around which the lathe operation should be performed.
            /// </summary>
            private readonly ChoiceParameter _parameterAxis = new ChoiceParameter("Axis", "X", "X", "Y", "Z");

            /// <summary>
            /// Distance from the axis. 
            /// </summary>
            private readonly FloatParameter _parameterDistance = new FloatParameter("Distance From Axis", 1);

            /// <summary>
            /// Number of segments that the created mesh should posses.
            /// </summary>
            private readonly IntParameter _parameterSegments = new IntParameter("Segments", 3);



            public LatheParameter()
                : base("Lathe")
            {
            }



            private List<Face> Lathe(Face face, Vector3D xAxis, Vector3D yAxis, Vector3D zAxis, Vector3D translation)
            {
                float horizontalAngle = (float) (Math.PI * 2) / _parameterSegments.Value;

                Vector3D[] xDirs = new Vector3D[_parameterSegments.Value];
                Vector3D[] yDirs = new Vector3D[_parameterSegments.Value];

                List<Face> faces = new List<Face>();
                List<CircularList<Vertex>> listVertexList = new List<CircularList<Vertex>>();


                for (int i = 0; i < _parameterSegments.Value; i++)
                {
                    xDirs[i] = xAxis * (float) Math.Cos(Math.PI * 2 - horizontalAngle * i);
                    yDirs[i] = yAxis * (float) Math.Sin(Math.PI * 2 - horizontalAngle * i);

                    //points.Add(new Vector3D(x, y, 0));
                }

                foreach (Vertex vertex in face.Vertices)
                {
                    Vector3D positionInsideScope = vertex.Position - translation;

                    float heightOnAxis = positionInsideScope.Dot(zAxis);
                    Vector3D heightAxis = zAxis * heightOnAxis;
                    float distance = (heightAxis - positionInsideScope).Length;

                    CircularList<Vertex> vertexList = new CircularList<Vertex>(_parameterSegments.Value);

                    for (int i = 0; i < _parameterSegments.Value; i++)
                        vertexList.Add(new Vertex(heightAxis + xDirs[i] * (_parameterDistance.Value + distance) + yDirs[i] * (_parameterDistance.Value + distance)));

                    listVertexList.Add(vertexList);
                }

                for (int i = 0; i < listVertexList.Count - 1; i++)
                for (int j = 0; j < listVertexList[i].Count; j++)
                    faces.Add(new Face(listVertexList[i + 1][j + 1], listVertexList[i][j + 1], listVertexList[i][j], listVertexList[i + 1][j]));

                int lastIndex = listVertexList.Count - 1;
                for (int j = 0; j < listVertexList[lastIndex].Count; j++)
                    faces.Add(new Face(listVertexList[0][j + 1], listVertexList[lastIndex][j + 1], listVertexList[lastIndex][j], listVertexList[0][j]));

                return faces;
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                List<Face> startingFaces = meshEntity.ToList();

                Vector3D xAxis;
                Vector3D yAxis;
                Vector3D zAxis;

                switch (_parameterAxis.Value)
                {
                    case "X":
                        zAxis = meshEntity.BoxScope.XAxis;
                        xAxis = meshEntity.BoxScope.YAxis;
                        yAxis = meshEntity.BoxScope.ZAxis;
                        break;
                    case "Y":
                        zAxis = meshEntity.BoxScope.YAxis;
                        yAxis = meshEntity.BoxScope.XAxis;
                        xAxis = meshEntity.BoxScope.ZAxis;
                        break;
                    case "Z":
                        xAxis = meshEntity.BoxScope.XAxis;
                        yAxis = meshEntity.BoxScope.YAxis;
                        zAxis = meshEntity.BoxScope.ZAxis;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (Face face in startingFaces)
                {
                    meshEntity.RemoveAndDetach(face);
                    meshEntity.AddRange(Lathe(face, xAxis, yAxis, zAxis, meshEntity.BoxScope.Translation));
                }

                return meshEntity;
            }
        }

        #endregion

        #region Triangulate

        /// <summary>
        /// Triangulates the mesh so that the created mesh is only made out of triangles.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class TriangulateParameter : MeshModifyParameter
        {
            public TriangulateParameter()
                : base("Triangulate")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                foreach (Face face in meshEntity.ToList())
                {
                    List<FaceTriangle> faceTriangles = face.Triangulate();

                    foreach (FaceTriangle faceTriangle in faceTriangles)
                    {
                        var triangulatedFace = new Face(faceTriangle.V0, faceTriangle.V1, faceTriangle.V2);
                        face.Attributes.SetAttributesTo(triangulatedFace.Attributes);
                        triangulatedFace.Material = face.Material;

                        foreach (var halfVertex in triangulatedFace.HalfVertices)
                        {
                            var otherHalfVertex = halfVertex.Vertex.HalfVertices.First(x => x.Face == face);
                            otherHalfVertex.Attributes.SetAttributesTo(halfVertex.Attributes);
                        }

                        meshEntity.Add(triangulatedFace);
                    }


                    meshEntity.RemoveAndDetach(face);
                }

                return meshEntity;
            }
        }

        #endregion

        #region Manipulate Normals

        /// <summary>
        /// Updates the normal vectors at the mesh vertices.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class ManipulateNormalsParameter : MeshModifyParameter
        {
            /// <summary>
            /// Type of normal calculation to perform.<br/>
            /// <b>Flat</b> means that the normals of the vertices will match the corresponding face normal, creating a flat shading effect.<br/>
            /// <b>Smooth</b> means that the normals of the vertices will be averaged from the faces they belong to, creating a smooth shading effect.
            /// </summary>
            private readonly ChoiceParameter _parameterType = new ChoiceParameter("Type", "Flat", "Flat", "Smooth");



            public ManipulateNormalsParameter()
                : base("Manipulate Normals")
            {
            }



            public static void FlatNormals(MeshEntity meshEntity)
            {
                foreach (Face face in meshEntity)
                foreach (HalfVertex halfVertex in face.HalfVertices)
                    halfVertex.Normal = face.Normal;
            }



            public static void SmoothNormals(MeshEntity meshEntity)
            {
                foreach (Vertex vertex in meshEntity.FaceVertices)
                {
                    Vector3D normal = Vector3D.Zero;

                    foreach (HalfVertex halfVertex in vertex.HalfVertices)
                        normal += halfVertex.Face.Normal;

                    normal /= vertex.HalfVertices.Count;

                    foreach (HalfVertex halfVertex in vertex.HalfVertices) halfVertex.Normal = normal;
                }
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                switch (_parameterType.Value)
                {
                    case "Flat":
                        FlatNormals(meshEntity);
                        break;
                    case "Smooth":
                        SmoothNormals(meshEntity);
                        break;
                }

                return meshEntity;
            }
        }

        #endregion

        #region Pyramidize

        /// <summary>
        /// Creates pyramid geometries from each mesh face.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class PyramidizeParameter : MeshModifyParameter
        {
            /// <summary>
            /// The height of the created pyramids.
            /// </summary>
            private readonly FloatParameter _parameterHeight = new FloatParameter("Height", 1);

            /// <summary>
            /// Indicate if the original face should be included in the final result as well
            /// </summary>
            private readonly BoolParameter _parameterCap = new BoolParameter("Cap", false);



            public PyramidizeParameter()
                : base("Pyramidize")
            {
            }



            private void CreatePyramid(MeshEntity meshEntity, Face face, Vector3D sizedDirection, bool cap)
            {
                Vector3D centroid = face.Centroid;

                centroid += sizedDirection;

                Vertex vertexCentroid = new Vertex(centroid);

                for (int i = 0; i < face.Vertices.Count(); i++)
                {
                    Face newFace = new Face(face[i], face[i + 1], vertexCentroid);
                    newFace.Material = face.Material;
                    meshEntity.Add(newFace);
                }

                if (!cap)
                    meshEntity.RemoveAndDetach(face);
                else
                    face.Flip();
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                foreach (Face face in meshEntity.ToList())
                    CreatePyramid(meshEntity, face, face.Normal * _parameterHeight.Value, _parameterCap.Value);

                return meshEntity;
            }
        }

        #endregion

        #region Convexify

        /// <summary>
        /// Replaces the mesh faces with convex polygonal ones.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class ConvexifyParameter : MeshModifyParameter
        {
            public ConvexifyParameter()
                : base("Convexify")
            {
            }



            private Face Convexify(Face face)
            {
                CircularList<Vertex> vertices = new CircularList<Vertex>(face.Vertices);

                //List<Vertex> finalVertices = new List<Vertex>();
                bool foundConcaveAngles;

                do
                {
                    foundConcaveAngles = false;

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        Vector3D previousPosition = vertices[i - 1].Position;
                        Vector3D currentPosition = vertices[i].Position;
                        Vector3D nextPosition = vertices[i + 1].Position;

                        if ((previousPosition - currentPosition).Cross(nextPosition - currentPosition).Dot(face.Normal) < 0)
                        {
                            vertices.RemoveAt(i);
                            foundConcaveAngles = true;
                            break;
                        }
                    }
                } while (foundConcaveAngles);

                face.Detach();

                return new Face(vertices);
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                foreach (Face face in meshEntity.ToList())
                {
                    meshEntity.Add(Convexify(face));

                    meshEntity.RemoveAndDetach(face);
                }

                return meshEntity;
            }
        }

        #endregion

        #region Spherify

        /// <summary>
        /// Creates hemisphere geometries from each mesh face.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class SpherifyParameter : MeshModifyParameter
        {
            /// <summary>
            /// Height of the hemispheres.
            /// </summary>
            private readonly FloatParameter _parameterHeight = new FloatParameter("Height", 1);

            /// <summary>
            /// Angle defining the hemisphere curvature.
            /// </summary>
            private readonly FloatParameter _parameterCircleSectionAngle = new FloatParameter("Circle Section Angle", 45);

            /// <summary>
            /// Number of segments to interpolate the hemispheres..
            /// </summary>
            private readonly IntParameter _parameterSegments = new IntParameter("Segments", 5);

            /// <summary>
            /// Indicate if the original face should be included in the final result as well
            /// </summary>
            protected readonly BoolParameter CapParameter = new BoolParameter("Cap", false);



            public SpherifyParameter()
                : base("Spherify")
            {
            }



            private static void Spherify(MeshEntity meshEntity, Face face, float height, float circleSectionAngle, int segments)
            {
                Vector3D centroid = face.Centroid;
                Vector3D sizedDirection = face.Normal * height;

                Vertex vertexCentroid = new Vertex(centroid + sizedDirection);
                Vector3D lastPoint = vertexCentroid.Position;

                CircularList<CircularList<Vertex>> circularList = new CircularList<CircularList<Vertex>>();

                //for each vertex at the base, draw an arc until the last point
                foreach (Vertex firstPoint in face.Vertices)
                {
                    CircularList<Vertex> vertexList = new CircularList<Vertex>(segments);

                    Vector3D xDirection = lastPoint - firstPoint.Position;
                    Vector3D midPoint = firstPoint.Position + xDirection / 2f;

                    xDirection = xDirection.Normalize();

                    Vector3D planeNormal = (lastPoint - centroid).Cross(firstPoint.Position - centroid).Normalize();

                    Vector3D directionToCenter = xDirection.Cross(planeNormal).Normalize();
                    float lengthToCenter = (float) ((lastPoint - midPoint).Length / Math.Tan(MathHelper.ToRadians(circleSectionAngle / 2f)));
                    Vector3D rotationPivot = midPoint + directionToCenter * lengthToCenter;

                    float angleRadians = MathHelper.ToRadians(circleSectionAngle / segments);

                    Matrix matrix = Matrix.CreateTranslation(rotationPivot.X, rotationPivot.Y, rotationPivot.Z) * Matrix.CreateAxisAngle(planeNormal, -angleRadians) * Matrix.CreateTranslation(-rotationPivot.X, -rotationPivot.Y, -rotationPivot.Z);

                    Vector3D currentPosition = firstPoint.Position;

                    //given the section angle, pick the first point and rotate it (segments-1) times around the discoverd pivot
                    vertexList.Add(firstPoint);
                    for (int i = 0; i < segments - 1; i++)
                    {
                        currentPosition = matrix.Transform(currentPosition);
                        vertexList.Add(new Vertex(currentPosition));
                    }

                    vertexList.Add(vertexCentroid);

                    circularList.Add(vertexList);
                }

                //now, connect all the points in the calculated arcs
                //but consider that 
                for (int i = 0; i < circularList.Count; i++)
                for (int j = 0; j < circularList[i].Count - 1; j++)
                    if (circularList[i][j + 1] == circularList[i + 1][j + 1])
                        meshEntity.Add(new Face(circularList[i][j], circularList[i + 1][j], circularList[i][j + 1]) {Material = face.Material});
                    else
                        meshEntity.Add(new Face(circularList[i][j], circularList[i + 1][j], circularList[i + 1][j + 1], circularList[i][j + 1]) {Material = face.Material});
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                List<Face> faces = meshEntity.ToList();

                if (!CapParameter.Value)
                    foreach (Face face in faces)
                    {
                        meshEntity.RemoveAndDetach(face);
                        face.Detach();
                    }


                foreach (Face face in faces) Spherify(meshEntity, face, _parameterHeight.Value, _parameterCircleSectionAngle.Value, _parameterSegments.Value);

                return meshEntity;
            }
        }

        #endregion

        #region Simplify

        /// <summary>
        /// Simplifies faces of the mesh by removing vertices which do not introduce a noticeable detail.
        /// </summary>
        public class SimplifyParameter : MeshModifyParameter
        {
            /// <summary>
            /// The angle tolerance, in degrees. Vertices whose edge angles differ less than this value from 180 degrees (meaning that they are rather straight) are removed.
            /// </summary>
            private readonly FloatParameter _parameterAngleTolerance = new FloatParameter("Angle Tolerance", 0.1f);



            public SimplifyParameter()
                : base("Simplify")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                foreach (Vertex vertex in meshEntity.FaceVertices.ToList())
                    if (vertex.HalfVertices.All(val => val.Angle < _parameterAngleTolerance.Value))
                        vertex.DeleteFromFaces();

                return meshEntity;
            }
        }

        #endregion

        #region Offset

        /// <summary>
        /// Replaces all faces with offset versions that either go inward or outward.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class OffsetParameter : MeshModifyParameter
        {
            /// <summary>
            /// Amount to be offset.
            /// </summary>
            private readonly FloatParameter _parameterAmount = new FloatParameter("Amount", 1);

            /// <summary>
            /// Attribute where the type of created face will be stored. Resulting faces of this operation will have this attribute set at either
            /// "Inside" or "Outside".
            /// </summary>
            private readonly AttributeParameter<string> _attributeSection = new AttributeParameter<string>("Section", AttributeAccess.Write);



            public OffsetParameter()
                : base("Offset")
            {
            }



            /// <summary>
            /// Offsets a given shape to the inside.
            /// </summary>
            /// <param name="meshEntity">The MeshEntity with the face.</param>
            /// <param name="face">Face to offset</param>
            /// <param name="amount">Amount of offset. Must be a positive value.</param>
            /// <returns>The inside of the offset. The outside faces are added to the passed MeshEntity.</returns>
            private MeshEntity OffsetInside(MeshEntity meshEntity, Face face, float amount)
            {
                int vertexCount = face.Vertices.Count();
                CircularList<Vertex> holeVertices = new CircularList<Vertex>();

                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3D firstVector = (face[i - 1].Position - face[i].Position).Normalize();
                    Vector3D secondVector = (face[i + 1].Position - face[i].Position).Normalize();

                    float actualAmount = (float) (amount / Math.Sin(firstVector.AngleTo(secondVector) / 2f));

                    Vector3D direction = (secondVector + firstVector).Normalize();

                    if (firstVector.IsCollinear(secondVector))
                        direction = face.Normal.Cross(firstVector).Normalize();

                    if (secondVector.Cross(firstVector).Normalize().Equals(face.Normal))
                        holeVertices.Add(new Vertex(face[i].Position - direction * actualAmount));
                    else
                        holeVertices.Add(new Vertex(face[i].Position + direction * actualAmount));

                    //if it is a convex angle or a concave angle
                    /*holeVertices.Add(secondVector.Cross(firstVector).IsCollinear(face.Normal)
                        ? new Vertex(face[i].Position + direction*amount)
                        : new Vertex(face[i].Position - direction*amount));*/
                }

                //for (int i = 0; i < vertexCount; i++)
                //MeshEntity.Add();
                for (int i = 0; i < vertexCount; i++)
                {
                    Face outerFace = new Face(face[i], face[i + 1], holeVertices[i + 1], holeVertices[i]);
                    _attributeSection[outerFace] = "Outside";
                    meshEntity.Add(outerFace);
                }

                var innerFace = new Face(holeVertices);
                _attributeSection[innerFace] = "Inside";
                meshEntity.Add(innerFace);

                //remove the original face
                meshEntity.RemoveAndDetach(face);

                return meshEntity;
            }



            /// <summary>
            /// Offsets a given MeshEntity to the outside.
            /// </summary>
            /// <param name="meshEntity">The MeshEntity with the face.</param>
            /// <param name="face">Face to offset</param>
            /// <param name="amount">Amount of offset. Must be a positive value.</param>
            /// <returns>The outside of the offset. The passed MeshEntity remains unaltered.</returns>
            private MeshEntity OffsetOutside(MeshEntity meshEntity, Face face, float amount)
            {
                int vertexCount = face.Vertices.Count();
                CircularList<Vertex> outsideRing = new CircularList<Vertex>();
                List<Face> outsideFaces = new List<Face>();

                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3D firstVector = (face[i - 1].Position - face[i].Position).Normalize();
                    Vector3D secondVector = (face[i + 1].Position - face[i].Position).Normalize();

                    Vector3D direction = (secondVector + firstVector).Normalize();

                    float actualAmount = (float) (amount / Math.Sin(firstVector.AngleTo(secondVector) / 2f));

                    //if it is a convex angle or a concave angle
                    outsideRing.Add(secondVector.Cross(firstVector).Normalize().Equals(face.Normal)
                        ? new Vertex(face[i].Position + direction * actualAmount)
                        : new Vertex(face[i].Position - direction * actualAmount));
                }

                //for (int i = 0; i < vertexCount; i++)
                //    outsideFaces.Add(new Face(face[i], outsideRing[i], outsideRing[i + 1], face[i + 1]));

                for (int i = 0; i < vertexCount; i++)
                {
                    Face outerFace = new Face(face[i + 1], face[i], outsideRing[i], outsideRing[i + 1]);
                    _attributeSection[outerFace] = "Outside";
                    meshEntity.Add(outerFace);
                }

                //the inside face is already there
                _attributeSection[face] = "Inside";

                return meshEntity; //MeshEntity.CreateDerived(outsideFaces);
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                //negative offset, meaning an offset to the INSIDE
                if (_parameterAmount.Value < 0)
                    foreach (Face face in meshEntity.ToList())
                    {
                        OffsetInside(meshEntity, face, -_parameterAmount.Value);

                        meshEntity.RemoveAndDetach(face);
                    }
                //positive offset, meaning an offset to the OUTSIDE
                else if (_parameterAmount.Value > 0)
                    foreach (Face face in meshEntity.ToList())
                        OffsetOutside(meshEntity, face, _parameterAmount.Value);


                return meshEntity;
            }
        }

        #endregion

        #region Advanced Offset

        /// <summary>
        /// Replaces all faces with offset versions that either go inward or outward.
        /// Similar to the "Offset" option, but returns better results for more complex, concave faces, yet has some limitations on some simpler cases.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class AdvancedOffsetParameter : MeshModifyParameter
        {
            private const float Precision = 10000;

            /// <summary>
            /// Amount to be offset.
            /// </summary>
            private readonly FloatParameter _parameterAmount = new FloatParameter("Amount", 1);

            /// <summary>
            /// Style of corners to generate. Applies to positive (outward) offsets only.
            /// </summary>
            private readonly ChoiceParameter _parameterStyle = new ChoiceParameter("Style", "Sharp", "Sharp", "Round", "Square");

            /// <summary>
            /// Indicates if the "Inside" section (and the performed hole) should be kept (in case of positive offset)
            /// or if the "Outside" section (with its hole) should be kept (in case of negative offset).
            /// </summary>
            private readonly BoolParameter _parameterKeepOriginal = new BoolParameter("Keep Original", true);

            /// <summary>
            /// Attribute where the type of created face will be stored. Resulting faces of this operation will have this attribute set at either
            /// "Inside" or "Outside".
            /// </summary>
            private readonly AttributeParameter<string> _attributeSection = new AttributeParameter<string>("Section", AttributeAccess.Write);



            public AdvancedOffsetParameter()
                : base("Offset (Advanced)")
            {
            }



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



            private IEnumerable<List<IntPoint>> PolyTreeToIntPointList(PolyNode polyNode)
            {
                if (polyNode.IsHole)
                {
                    yield return polyNode.Contour;
                }
                else if (polyNode.Contour.Any())
                {
                    polyNode.Contour.Reverse();
                    yield return polyNode.Contour;
                }

                foreach (var list in polyNode.Childs.SelectMany(x => PolyTreeToIntPointList(x)))
                    yield return list;
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                double value = _parameterAmount.Value * Precision;

                var joinTypeStyle = _parameterStyle.Value == "Sharp" ? JoinType.jtMiter : _parameterStyle.Value == "Round" ? JoinType.jtRound : JoinType.jtSquare;

                //each MeshEntity can contain several faces/polygons
                //make it into a list because we will be adding some more faces as this loop goes on
                foreach (Face face in meshEntity.ToList())
                {
                    var alignedScope = face.GetAlignedScope();

                    ClipperOffset clipperOffset = new ClipperOffset();

                    var originalIntList = face.Vertices.Select(x => x.Position.ToIntPoint(alignedScope)).ToList();

                    clipperOffset.AddPath(originalIntList, joinTypeStyle, EndType.etClosedPolygon);

                    if (face.HasHoles)
                        foreach (CircularList<Vertex> circularList in face.Holes)
                            clipperOffset.AddPath(circularList.Select(x => x.Position.ToIntPoint(alignedScope)).ToList(), joinTypeStyle, EndType.etClosedPolygon);

                    //List<List<IntPoint>> solution = new List<List<IntPoint>>();
                    //clipperOffset.Execute(ref solution, value);

                    PolyTree offsetPolyTree = new PolyTree();
                    clipperOffset.Execute(ref offsetPolyTree, value);


                    if (_parameterKeepOriginal.Value)
                    {
                        var offsettedPaths = PolyTreeToIntPointList(offsetPolyTree).ToList();


                        Clipper clipper = new Clipper();

                        //if we are offsetting to the outside, we need to cut a hole where the original face was
                        if (value > 0)
                        {
                            clipper.AddPaths(offsettedPaths, PolyType.ptSubject, true);
                            clipper.AddPath(originalIntList, PolyType.ptClip, true);
                        }
                        else
                        {
                            //if we are offsetting to the inside, the original face is the one that will have some holes cut
                            clipper.AddPath(originalIntList, PolyType.ptSubject, true);
                            clipper.AddPaths(offsettedPaths, PolyType.ptClip, true);
                        }


                        PolyTree clipPolyTree = new PolyTree();

                        clipper.Execute(ClipType.ctDifference, clipPolyTree);

                        try
                        {
                            var newFaces = PolyTreeToFaceList(clipPolyTree, alignedScope).ToList();

                            if (value > 0)
                            {
                                _attributeSection[face] = "Inside";

                                foreach (Face newFace in newFaces)
                                    _attributeSection[newFace] = "Outside";
                            }
                            else
                            {
                                foreach (Face newFace in newFaces)
                                    face.Attributes.SetAttributesTo(newFace.Attributes);

                                //the original face disappears
                                meshEntity.RemoveAndDetach(face);

                                //and a new outside face (with holes) is added
                                var insideFaces =
                                    PolyTreeToFaceList(offsetPolyTree, alignedScope, reverse: false).ToList();
                                foreach (Face insideFace in insideFaces)
                                    _attributeSection[insideFace] = "Inside";

                                meshEntity.AddRange(insideFaces);

                                foreach (Face newFace in newFaces)
                                    _attributeSection[newFace] = "Outside";
                            }

                            meshEntity.AddRange(newFaces);
                        }
                        catch (Exception)
                        {
                            ProcedureEnvironment.GetService<ILogger>().Log("Some faces could not be created.", LogType.Warning);
                        }
                    }
                    else
                    {
                        var createdFaces = PolyTreeToFaceList(offsetPolyTree, alignedScope, reverse: true).ToList();

                        if (value > 0)
                        {
                            foreach (Face newFace in createdFaces)
                                _attributeSection[newFace] = "Outside";

                            meshEntity.AddRange(createdFaces);

                            meshEntity.RemoveAndDetach(face);
                        }
                        else
                        {
                            foreach (Face newFace in createdFaces)
                            {
                                face.Attributes.SetAttributesTo(newFace.Attributes);
                                _attributeSection[newFace] = "Inside";
                            }

                            meshEntity.AddRange(createdFaces);
                            meshEntity.RemoveAndDetach(face);
                        }
                    }
                }


                return meshEntity;
            }
        }

        #endregion

        #region Prismify

        /// <summary>
        /// Creates prism geometries from each mesh face.
        /// </summary>
        public class PrismifyParameter : MeshModifyParameter
        {
            /// <summary>
            /// Dimension of the prism to create. This can either be set as the height value or the angle between the planes.
            /// </summary>
            private readonly SelectListParameter _parameterAmount = new SelectListParameter("Amount",
                () => new FloatParameter("Angle", 45) {Description = "The angle between the planes."},
                () => new FloatParameter("Height", 1) {Description = "The prism height."});

            /// <summary>
            /// Type of prism to create.
            /// </summary>
            private readonly SelectListParameter _parameterStyle = new SelectListParameter("Style",
                () => new CompoundParameter("Simple") {Description = "A simple prism-like solid, built from a central axis defined by the scope orientation."},
                () => new CompoundParameter("Skeleton") {Description = "A prism-like solid built from a central skeleton axis defined by the face shape."},
                () => new Vector2DParameter("Overhang", new Vector2D(1, 1)) {Description = "Similar to the \"simple\", but creating extra overhang faces, resembling a roof-like structure."});


            /// <summary>
            /// Attribute where the type of created face will be stored. Possible values are "Top" and "Side".
            /// </summary>
            private readonly AttributeParameter<string> _attributeSection = new AttributeParameter<string>("Section", AttributeAccess.Write);



            public PrismifyParameter()
                : base("Prismify")
            {
                _parameterAmount.Set("Height");
                _parameterStyle.Set("Overhang");
            }



            /// <summary>
            /// https://github.com/reinterpretcat/csharp-libs/tree/master/straight_skeleton
            /// </summary>
            /// <param name="face"></param>
            /// <param name="value"></param>
            /// <param name="angle"></param>
            /// <returns></returns>
            private void BuildSkeletonRoof(MeshEntity meshEntity, Face face, float value, bool angle)
            {
                var alignedScope = face.GetAlignedScope();

                try
                {
                    var points = face.Vertices.Select(x => ToVector2d(x.Position, alignedScope)).ToList();
                    points.Reverse();
                    Skeleton skeleton;

                    if (face.HasHoles)
                    {
                        var holes = face.Holes.Select(x => x.Select(y => ToVector2d(y.Position, alignedScope)).ToList()).ToList();
                        skeleton = SkeletonBuilder.Build(points, holes);
                    }
                    else
                    {
                        skeleton = SkeletonBuilder.Build(points);
                    }


                    var maxDistance = skeleton.Distances.Values.Max();
                    var actualValue = angle ? (float) (maxDistance / Math.Tan(MathHelper.ToRadians(value / 2))) : value;


                    List<Face> faces = new List<Face>();
                    foreach (EdgeResult edgeResult in skeleton.Edges)
                    {
                        var polygon = edgeResult.Polygon;

                        List<Vector3D> positions = new List<Vector3D>();

                        foreach (Vector2d vector2D in polygon)
                        {
                            var distance = skeleton.Distances[vector2D];
                            var fraction = distance / maxDistance;

                            positions.Add(FromVector2d(vector2D, (float) (fraction * actualValue), alignedScope)); //
                        }

                        positions.Reverse();

                        var newFace = new Face(positions);
                        face.Attributes.SetAttributesTo(newFace.Attributes);
                        _attributeSection[newFace] = "Top";
                        newFace.Material = face.Material;

                        faces.Add(newFace);
                    }

                    meshEntity.AddRange(faces);
                }
                catch (Exception)
                {
                    var actualValue = value;

                    if (angle)
                    {
                        var maxDistance = Math.Max(Math.Max(alignedScope.Sizes.X, alignedScope.Sizes.Y), alignedScope.Sizes.Z) / 2;

                        actualValue = (float) (maxDistance / Math.Tan(MathHelper.ToRadians(value / 2)));
                    }

                    meshEntity.AddRange(CreatePyramid(face, face.Normal * actualValue));
                }
            }



            private IEnumerable<Face> CreatePyramid(Face face, Vector3D sizedDirection)
            {
                Vector3D centroid = face.Centroid;

                centroid += sizedDirection;

                Vertex vertexCentroid = new Vertex(centroid);

                for (int i = 0; i < face.Vertices.Count(); i++)
                {
                    Face newFace = new Face(face[i], face[i + 1], vertexCentroid);
                    newFace.Material = face.Material;
                    _attributeSection[newFace] = "Top";
                    yield return newFace;
                }
            }



            /// <summary>
            /// Defines the 2 planes that limit the roof.
            /// </summary>
            /// <param name="planeScope"></param>
            /// <param name="height"></param>
            /// <param name="plane0"></param>
            /// <param name="plane1"></param>
            /// <param name="planeOrthogonal"></param>
            /// <returns></returns>
            private Line3D DefinePlanes(BoxScope planeScope, float height, out Plane3D plane0, out Plane3D plane1, out Plane3D planeOrthogonal)
            {
                Vector3D xSizedAxis = planeScope.XAxis * planeScope.Sizes.X;
                Vector3D ySizedAxis = planeScope.YAxis * planeScope.Sizes.Y;

                //first, define the actual 2 rectangles that define the roof
                Vector3D point0 = new Vector3D(planeScope.Translation + xSizedAxis);
                Vector3D point1 = new Vector3D(planeScope.Translation);
                Vector3D point2 = new Vector3D(planeScope.Translation + ySizedAxis / 2f + planeScope.ZAxis * height);
                Vector3D point3 = new Vector3D(planeScope.Translation + xSizedAxis + ySizedAxis / 2f + planeScope.ZAxis * height);

                Vector3D point4 = new Vector3D(planeScope.Translation + ySizedAxis);
                Vector3D point5 = new Vector3D(planeScope.Translation + xSizedAxis + ySizedAxis);

                Vector3D plane0Normal = (point0 - point1).Cross(point2 - point1);
                Vector3D plane1Normal = (point4 - point5).Cross(point3 - point5);

                plane0 = new Plane3D(plane0Normal, point0);
                plane1 = new Plane3D(plane1Normal, point4);
                planeOrthogonal = new Plane3D(planeScope.XAxis, planeScope.Translation + ySizedAxis / 2f);

                return new Line3D(planeScope.XAxis, planeScope.Translation + ySizedAxis / 2f);
            }



            private void DetermineFaces(CircularList<ExtendedVertex> extendedBaseVertexList, int index, List<List<int>> facesIndices, IEnumerable<ExtendedVertex> currentList)
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
                        DetermineFaces(extendedBaseVertexList, index + 1, facesIndices, new[] {currentVertex.NextCutVertex, currentVertex});

                        extendedVertices.Add(currentVertex.NextCutVertex);
                        index = currentVertex.NextCutVertex.Index + 1;
                    }
                    else
                    {
                        index++;
                    }

                    currentVertex = extendedBaseVertexList[index];
                }

                facesIndices.Add(extendedVertices.Select(val => val.Index).ToList());
            }



            private Vector3D FromVector2d(Vector2d position, float z, BoxScope planarScope)
            {
                return planarScope.ToWorldPosition(new Vector3D((float) position.X, (float) position.Y, z));
            }



            /// <summary>
            /// Creates the faces that define the roof.
            /// </summary>
            /// <param name="face"></param>
            /// <param name="middleLine"></param>
            /// <param name="planeOrthogonal"></param>
            /// <param name="extendedBaseVertexList"></param>
            /// <param name="orthogonal"></param>
            /// <returns></returns>
            private List<List<int>> ProcessTopFaces(Face face, Line3D middleLine, Plane3D planeOrthogonal, out CircularList<ExtendedVertex> extendedBaseVertexList)
            {
                extendedBaseVertexList = new CircularList<ExtendedVertex>();

                Plane3D middleLinePlane = new Plane3D(face.Normal.Cross(middleLine.Direction), middleLine.Point0);

                List<Edge> edges = face.Edges.ToList();
                for (int index = 0; index < edges.Count; index++)
                {
                    var edge = edges[index];
                    ExtendedVertex extendedVertex;

                    extendedBaseVertexList.Add(extendedVertex = new ExtendedVertex(edge.V0, extendedBaseVertexList.Count));

                    Vector3D intersectionPoint;
                    Edge.EdgeIntersectionResult edgeLine3DIntersection = edge.PlaneIntersection(middleLinePlane, out intersectionPoint);
                    if (edgeLine3DIntersection == Edge.EdgeIntersectionResult.IntersectingV0 || edgeLine3DIntersection == Edge.EdgeIntersectionResult.Coincident) extendedVertex.IsCrossPoint = true;

                    if (edgeLine3DIntersection == Edge.EdgeIntersectionResult.IntersectingV1 || edgeLine3DIntersection == Edge.EdgeIntersectionResult.Coincident)
                    {
                        extendedBaseVertexList.Add(new ExtendedVertex(edge.V1, extendedBaseVertexList.Count) {IsCrossPoint = true});

                        //skip the next iteration
                        index++;
                    }

                    if (edgeLine3DIntersection == Edge.EdgeIntersectionResult.IntersectingMiddle) extendedBaseVertexList.Add(new ExtendedVertex(new Vertex(intersectionPoint), extendedBaseVertexList.Count) {IsCrossPoint = true});
                }

                List<ExtendedVertex> orderedCrossPoints = extendedBaseVertexList.Where(val => val.IsCrossPoint).OrderBy(val => planeOrthogonal.DistanceToPoint(val.CurrentVertex.Position)).ToList();
                for (int index = 0; index < orderedCrossPoints.Count; index += 2)
                {
                    orderedCrossPoints[index].NextCutVertex = orderedCrossPoints[index + 1];
                    orderedCrossPoints[index + 1].NextCutVertex = orderedCrossPoints[index];
                }

                List<List<int>> facesIndices = new List<List<int>>();
                DetermineFaces(extendedBaseVertexList, 0, facesIndices, new List<ExtendedVertex>());

                return facesIndices;
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="face"></param>
            /// <param name="extendedBaseVertexList"></param>
            /// <param name="plane0"></param>
            /// <param name="plane1"></param>
            /// <param name="faceIndices"></param>
            /// <returns></returns>
            private void ProcessWallFaces(MeshEntity meshEntity, Face face, CircularList<ExtendedVertex> extendedBaseVertexList, Plane3D plane0, Plane3D plane1, List<List<int>> faceIndices)
            {
                //List<Face> wallFaces = new List<Face>();
                CircularList<Vertex> extendedTopVertexList = new CircularList<Vertex>();

                //create a ring of vertices just like the original face, but translated
                foreach (Vertex vertex in extendedBaseVertexList.Select(val => val.CurrentVertex))
                {
                    Line3D line3D = new Line3D(face.Normal.Round(), vertex.Position);
                    float? distance1 = line3D.IntersectsPlane(plane0);
                    float? distance2 = line3D.IntersectsPlane(plane1);

                    //float distance1 = plane0.DistanceToPoint(vertex.Position);
                    //float distance2 = plane1.DistanceToPoint(vertex.Position);
                    if (distance1.HasValue && distance2.HasValue)
                    {
                        float minDistance = Math.Min(distance1.Value, distance2.Value);

                        extendedTopVertexList.Add(minDistance > 0 ? new Vertex(vertex.Position + face.Normal * minDistance) : vertex); // + face.Normal * minDistance
                    }
                    else
                    {
                        extendedTopVertexList.Add(vertex);
                    }
                }

                for (int i = 0; i < extendedBaseVertexList.Count; i++)
                {
                    Vertex vertex0 = extendedBaseVertexList[i].CurrentVertex;
                    Vertex vertex0Top = extendedTopVertexList[i];
                    Vertex vertex1 = extendedBaseVertexList[i + 1].CurrentVertex;
                    Vertex vertex1Top = extendedTopVertexList[i + 1];

                    Face wallFace = null;

                    if (vertex0 != vertex0Top && vertex1 != vertex1Top)
                        wallFace = new Face(vertex0, vertex1, vertex1Top, vertex0Top) {Material = face.Material}; //, Color = face.Color
                    else if (vertex0 != vertex0Top)
                        wallFace = new Face(vertex0, vertex1, vertex0Top) {Material = face.Material}; //, Color = face.Color
                    else if (vertex1 != vertex1Top)
                        wallFace = new Face(vertex0, vertex1, vertex1Top) {Material = face.Material}; //, Color = face.Color

                    if (wallFace != null)
                    {
                        _attributeSection[wallFace] = "Side";
                        meshEntity.Add(wallFace);
                    }
                }

                foreach (List<int> faceIndex in faceIndices)
                {
                    List<Vertex> vertices = new List<Vertex>();
                    foreach (int index in faceIndex) vertices.Add(extendedTopVertexList[index]);

                    var roofFace = new Face(vertices) {Material = face.Material};
                    _attributeSection[roofFace] = "Top";
                    meshEntity.Add(roofFace);
                }
            }



            private void ShedRoof(MeshEntity meshEntity, Face face, float height, float overhangSizeX, float overhangSizeY)
            {
                BoxScope planeScope = face.GetDerivedScope(meshEntity.BoxScope);

                Vector3D xOverhang = planeScope.XAxis.Normalize() * overhangSizeX;
                Vector3D yOverhang = planeScope.YAxis.Normalize() * overhangSizeY;

                Vector3D xSizedAxis = planeScope.XAxis * planeScope.Sizes.X;
                Vector3D ySizedAxis = planeScope.YAxis * planeScope.Sizes.Y;

                //first, create the actual 2 rectangles that define the roof
                Vertex point0 = new Vertex(planeScope.Translation + xSizedAxis + xOverhang - yOverhang);
                Vertex point1 = new Vertex(planeScope.Translation - xOverhang - yOverhang);
                Vertex point2 = new Vertex(planeScope.Translation + ySizedAxis / 2f + planeScope.ZAxis * height - xOverhang);
                Vertex point3 = new Vertex(planeScope.Translation + xSizedAxis + ySizedAxis / 2f + planeScope.ZAxis * height + xOverhang);

                Vertex point4 = new Vertex(planeScope.Translation + ySizedAxis - xOverhang + yOverhang);
                Vertex point5 = new Vertex(planeScope.Translation + xSizedAxis + ySizedAxis + xOverhang + yOverhang);

                Face face0 = new Face(point0, point1, point2, point3) {Material = face.Material};
                Face face1 = new Face(point4, point5, point3, point2) {Material = face.Material};

                _attributeSection[face0] = "Top";
                _attributeSection[face1] = "Top";
                meshEntity.Add(face0);
                meshEntity.Add(face1);
                //RoofOutput.Write(meshEntity.CreateDerived(new Face[] { face0, face1 }));

                //now, we need to create a wall connection between the roof and base
                Plane3D plane0 = new Plane3D(face0.Normal, face0[0].Position);
                Plane3D plane1 = new Plane3D(face1.Normal, face1[0].Position);

                //Edge middleEdge = new Edge(new Vertex(planeScope.Translation + ySizedAxis/2f - xOverhang), new Vertex(planeScope.Translation + xSizedAxis + ySizedAxis/2f + xOverhang));
                //Line3D middleLine = new Line3D(planeScope.XAxis, planeScope.Translation + ySizedAxis / 2f);
                Plane3D middlePlane = new Plane3D(planeScope.XAxis.Cross(face.Normal), planeScope.Translation + ySizedAxis / 2f);

                CircularList<Vertex> extendedBaseVertexList = new CircularList<Vertex>();
                CircularList<Vertex> extendedTopVertexList = new CircularList<Vertex>();

                foreach (var edge in face.Edges)
                {
                    extendedBaseVertexList.Add(edge.V0);

                    Vector3D intersectionPoint;
                    Edge.EdgeIntersectionResult intersection = edge.PlaneIntersection(middlePlane, out intersectionPoint);
                    //Edge.EdgeIntersectionResult intersection = edge.LineIntersection(middleLine, out intersectionPoint);
                    if (intersection == Edge.EdgeIntersectionResult.IntersectingMiddle)
                        extendedBaseVertexList.Add(new Vertex(intersectionPoint));

                    /*if (intersectsEdge.HasValue)
                        if(!intersectsEdge.Value.IsInfinity)
                            extendedBaseVertexList.Add(new Vertex(intersectsEdge.Value));*/
                }

                //CircularList<Vertex> vertices = new CircularList<Vertex>();
                List<Face> wallFaces = new List<Face>();

                //create a ring of vertices just like the original face, but translated
                foreach (Vertex vertex in extendedBaseVertexList)
                {
                    Line3D line3D = new Line3D(face.Normal, vertex.Position);
                    float? distance1 = line3D.IntersectsPlane(plane0);
                    float? distance2 = line3D.IntersectsPlane(plane1);

                    //float distance1 = plane0.DistanceToPoint(vertex.Position);
                    //float distance2 = plane1.DistanceToPoint(vertex.Position);
                    if (distance1.HasValue && distance2.HasValue)
                    {
                        float minDistance = Math.Min(distance1.Value, distance2.Value);

                        extendedTopVertexList.Add(minDistance > 0 ? new Vertex(vertex.Position + face.Normal * minDistance) : vertex);
                    }
                    else
                    {
                        extendedTopVertexList.Add(vertex);
                    }
                }

                for (int i = 0; i < extendedBaseVertexList.Count; i++)
                {
                    Vertex vertex0 = extendedBaseVertexList[i];
                    Vertex vertex0Top = extendedTopVertexList[i];
                    Vertex vertex1 = extendedBaseVertexList[i + 1];
                    Vertex vertex1Top = extendedTopVertexList[i + 1];

                    Face wallFace = null;

                    if (vertex0 != vertex0Top && vertex1 != vertex1Top)
                        wallFace = new Face(vertex0, vertex1, vertex1Top, vertex0Top) {Material = face.Material}; //, Color = face.Color
                    else if (vertex0 != vertex0Top)
                        wallFace = new Face(vertex0, vertex1, vertex0Top) {Material = face.Material}; //, Color = face.Color
                    else if (vertex1 != vertex1Top)
                        wallFace = new Face(vertex0, vertex1, vertex1Top) {Material = face.Material}; //, Color = face.Color

                    if (wallFace != null)
                    {
                        _attributeSection[wallFace] = "Side";
                        meshEntity.Add(wallFace);
                    }
                }
            }



            private Vector2d ToVector2d(Vector3D position, BoxScope planarScope)
            {
                var rotatedVector = planarScope.ToScopePosition(position);

                return new Vector2d(rotatedVector.X, rotatedVector.Y);
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                var amountItem = _parameterAmount.Items.FirstOrDefault() as FloatParameter;
                var styleItem = _parameterStyle.Items.FirstOrDefault();

                if (amountItem != null && styleItem != null)
                    foreach (Face face in meshEntity.ToList())
                    {
                        BoxScope planeScope = face.GetDerivedScope(meshEntity.BoxScope);

                        //determine the actual height
                        float height = amountItem.Label == "Angle" ? planeScope.Sizes.X / 2f / (float) Math.Tan(MathHelper.ToRadians(amountItem.Value / 2)) : amountItem.Value;

                        if (styleItem.Label == "Overhang")
                        {
                            var overhang = (Vector2DParameter) styleItem;
                            ShedRoof(meshEntity, face, height, overhang.Value.X, overhang.Value.Y);
                        }
                        else if (styleItem.Label == "Skeleton")
                        {
                            BuildSkeletonRoof(meshEntity, face, amountItem.Value, amountItem.Label == "Angle");
                        }
                        else
                        {
                            Plane3D plane0, plane1, planeOrthogonal;
                            Line3D middleEdge = DefinePlanes(planeScope, height, out plane0, out plane1, out planeOrthogonal);

                            CircularList<ExtendedVertex> extendedBaseVertexList;
                            List<List<int>> faceIndices = ProcessTopFaces(face, middleEdge, planeOrthogonal, out extendedBaseVertexList);
                            //_roof.Write(MeshEntity.CreateDerived(topFaces));

                            ProcessWallFaces(meshEntity, face, extendedBaseVertexList, plane0, plane1, faceIndices);
                        }

                        //remove the original face from the set
                        meshEntity.RemoveAndDetach(face);
                    }

                return meshEntity;
            }



            /*private float GetMinDistance(Vertex vertex, Plane3D plane0, Plane3D plane1)
            {
                float distance1 = plane0.DistanceToPoint(vertex.Position);
                float distance2 = plane1.DistanceToPoint(vertex.Position);
                return Math.Min(-distance1, -distance2);
            }*/

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
        }

        #endregion

        #region Shared Inset

        /// <summary>
        /// Replaces all faces with inset versions based on the edges that are not shared.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class SharedInsetParameter : MeshModifyParameter
        {
            /// <summary>
            /// Amount to be inset.
            /// </summary>
            private readonly FloatParameter _parameterAmount = new FloatParameter("Amount", 1) {MinValue = 0};

            /// <summary>
            /// Attribute where the type of created face will be stored. Resulting faces of this operation will have this attribute set at either
            /// "Inside" or "Outside".
            /// </summary>
            private readonly AttributeParameter<string> _attributeSection = new AttributeParameter<string>("Section", AttributeAccess.Write);



            public SharedInsetParameter()
                : base("Shared Inset")
            {
            }



            private void OffsetSpecial(MeshEntity meshEntity, Face face, float value)
            {
                CircularList<VertexProperty> innerVertexProperties = new CircularList<VertexProperty>();


                var faceVertices = new CircularList<Vertex>(face.Vertices);
                for (int i = 0; i < faceVertices.Count; i++)
                {
                    var previousVertex = faceVertices[i - 1];
                    var currentVertex = faceVertices[i];
                    var nextVertex = faceVertices[i + 1];

                    var boundaryType = currentVertex.GetLocalAttribute<VertexBoundaryType>("BoundaryType", Procedure);

                    switch (boundaryType)
                    {
                        case VertexBoundaryType.NonBoundary:

                            innerVertexProperties.Add(new VertexProperty {InnerVertex = currentVertex, TextureCoordinate = currentVertex[face].UV0});
                            break;
                        case VertexBoundaryType.SemiBoundary:

                            var furthestVertex = previousVertex[face].GetLocalAttribute<Vertex>("FurthestEndVertex", Procedure);
                            if (furthestVertex != null)
                                //innerFaceVertices.Add(furthestVertex);
                                innerVertexProperties.Add(new VertexProperty {InnerVertex = furthestVertex, BoundaryVertex = currentVertex, IsFurthestEndVertex = true, TextureCoordinate = previousVertex[face].GetLocalAttribute<Vector2D>("FurthestTextureCoord", Procedure)});


                            var closestVertex = currentVertex[face].GetLocalAttribute<Vertex>("ClosestEndVertex", Procedure);
                            if (closestVertex != null)
                                //innerFaceVertices.Add(closestVertex);
                                innerVertexProperties.Add(new VertexProperty {InnerVertex = closestVertex, BoundaryVertex = currentVertex, IsClosestEndVertex = true, TextureCoordinate = currentVertex[face].GetLocalAttribute<Vector2D>("ClosestTextureCoord", Procedure)});
                            break;
                        case VertexBoundaryType.FullBoundary:

                            var centerPosition = currentVertex.Position;

                            Vector3D firstVector = previousVertex.Position - centerPosition;
                            Vector3D secondVector = nextVertex.Position - centerPosition;

                            Vector3D normalizedFirstVector = firstVector.Normalize();
                            Vector3D normalizedSecondVector = secondVector.Normalize();

                            float actualAmount = -(float) (value / Math.Sin(normalizedFirstVector.AngleTo(normalizedSecondVector) / 2f));

                            Vector3D direction = (normalizedSecondVector + normalizedFirstVector).Normalize();

                            if (normalizedFirstVector.IsCollinear(normalizedSecondVector))
                                direction = face.Normal.Cross(normalizedFirstVector).Normalize();


                            var firstVectorFraction = value / firstVector.Length;
                            var secondVectorFraction = value / secondVector.Length;

                            var firstTextureVector = previousVertex[face].UV0 - currentVertex[face].UV0;
                            var secondTextureVector = nextVertex[face].UV0 - currentVertex[face].UV0;

                            var newVertexTexCoordinate = currentVertex[face].UV0 + firstTextureVector.Normalize() * firstTextureVector.Length * firstVectorFraction + secondTextureVector.Normalize() * secondTextureVector.Length * secondVectorFraction;


                            //Vector3D newPosition = centerPosition + direction * actualAmount;
                            if (normalizedSecondVector.Cross(normalizedFirstVector).Normalize().Equals(face.Normal))
                                //innerFaceVertices.Add(new Vertex(centerPosition + direction * actualAmount));
                                innerVertexProperties.Add(new VertexProperty {BoundaryVertex = currentVertex, InnerVertex = new Vertex(centerPosition + direction * actualAmount), TextureCoordinate = newVertexTexCoordinate});
                            else
                                innerVertexProperties.Add(new VertexProperty {BoundaryVertex = currentVertex, InnerVertex = new Vertex(centerPosition - direction * actualAmount), TextureCoordinate = newVertexTexCoordinate});


                            break;
                    }
                }

                //let's go over te vertices again, this time to create the outer faces
                for (int i = 0; i < innerVertexProperties.Count; i++)
                {
                    var currentVertex = innerVertexProperties[i];
                    var nextVertex = innerVertexProperties[i + 1];
                    if (!currentVertex.IsClosestEndVertex
                        && !(currentVertex.BoundaryVertex == null && nextVertex.IsFurthestEndVertex))
                    {
                        // && !(nextVertexProperties.BoundaryVertex == null && currentVertexProperties.IsFurthestEndVertex)
                        List<Vertex> vs = new List<Vertex>();
                        List<Vector2D> textureCoords = new List<Vector2D>();

                        vs.Add(nextVertex.InnerVertex);
                        vs.Add(currentVertex.InnerVertex);
                        textureCoords.Add(nextVertex.TextureCoordinate);
                        textureCoords.Add(currentVertex.TextureCoordinate);

                        if (currentVertex.BoundaryVertex != null)
                        {
                            vs.Add(currentVertex.BoundaryVertex);
                            textureCoords.Add(currentVertex.BoundaryVertex[face].UV0);
                        }

                        if (nextVertex.BoundaryVertex != null && nextVertex.BoundaryVertex != currentVertex.BoundaryVertex)
                        {
                            vs.Add(nextVertex.BoundaryVertex);
                            textureCoords.Add(nextVertex.BoundaryVertex[face].UV0);
                        }

                        if (vs.Count > 2)
                        {
                            var newFace = new Face(vs) {Material = face.Material};

                            face.Attributes.SetAttributesTo(newFace.Attributes);
                            _attributeSection[newFace] = "Outside";

                            var halfVertices = newFace.HalfVertices.ToList();
                            for (int j = 0; j < halfVertices.Count; j++)
                                halfVertices[j].UV0 = textureCoords[j];

                            meshEntity.Add(newFace);
                        }
                    }
                }


                //create the inner face, copy the attributes from the main one
                //and set the identifiable attribute
                var innerFace = new Face(innerVertexProperties.Select(x => x.InnerVertex)) {Material = face.Material};
                face.Attributes.SetAttributesTo(innerFace.Attributes);
                _attributeSection[innerFace] = "Inside";

                var innerFaceHalfVertices = innerFace.HalfVertices.ToList();
                for (int i = 0; i < innerFaceHalfVertices.Count; i++) innerFaceHalfVertices[i].UV0 = innerVertexProperties[i].TextureCoordinate;

                meshEntity.Add(innerFace);
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                var amount = _parameterAmount.Value;

                if (Math.Abs(amount) > float.Epsilon)
                {
                    foreach (var vertex in meshEntity.FaceVertices.Distinct())
                    {
                        vertex.SetLocalAttribute("BoundaryType", Procedure, vertex.BoundaryType);

                        foreach (Edge edge in vertex.AllEdges.Where(e => !e.IsBoundary))
                        {
                            var otherVertex = edge.OtherVertex(vertex);

                            var insetPosition = vertex.Position + (otherVertex.Position - vertex.Position).Normalize() * amount;
                            var newVertex = new Vertex(insetPosition);
                            vertex.Attributes.SetAttributesTo(newVertex.Attributes);

                            foreach (HalfVertex halfVertex in vertex.HalfVertices.Where(x => x.Next == otherVertex))
                            {
                                var nextHv = halfVertex.Next[halfVertex.Face];

                                var size = (nextHv.Vertex.Position - halfVertex.Vertex.Position).Length;
                                var textureVector = nextHv.UV0 - halfVertex.UV0;
                                var textureSize = textureVector.Length;

                                var fraction = amount / size;

                                var newVertexTexCoordinate = halfVertex.UV0 + textureVector.Normalize() * textureSize * fraction;

                                //halfVertex.TextureCoordinate = 

                                //halfVertex.TextureCoordinate
                                halfVertex.SetLocalAttribute("ClosestEndVertex", Procedure, newVertex);
                                halfVertex.SetLocalAttribute("ClosestTextureCoord", Procedure, newVertexTexCoordinate);
                            }


                            foreach (HalfVertex halfVertex in otherVertex.HalfVertices.Where(x => x.Next == vertex))
                            {
                                var nextHv = halfVertex.Next[halfVertex.Face];

                                var size = (nextHv.Vertex.Position - halfVertex.Vertex.Position).Length;
                                var textureVector = nextHv.UV0 - halfVertex.UV0;
                                var textureSize = textureVector.Length;

                                var fraction = 1 - amount / size;

                                var newVertexTexCoordinate = halfVertex.UV0 + textureVector.Normalize() * textureSize * fraction;

                                halfVertex.SetLocalAttribute("FurthestTextureCoord", Procedure, newVertexTexCoordinate);
                                halfVertex.SetLocalAttribute("FurthestEndVertex", Procedure, newVertex);
                            }
                        }
                    }

                    foreach (Face face in meshEntity.ToList())
                    {
                        OffsetSpecial(meshEntity, face, _parameterAmount.Value);

                        meshEntity.RemoveAndDetach(face);
                    }
                }

                return meshEntity;
            }



            private class VertexProperty
            {
                public Vertex BoundaryVertex
                {
                    get;
                    set;
                }

                public Vertex InnerVertex
                {
                    get;
                    set;
                }


                public bool IsClosestEndVertex
                {
                    get;
                    set;
                }


                public bool IsFurthestEndVertex
                {
                    get;
                    set;
                }


                public Vector2D TextureCoordinate
                {
                    get;
                    set;
                }
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Removes vertices that are too close together.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class CleanupParameter : MeshModifyParameter
        {
            /// <summary>
            /// The minimum allowed distance between the vertices. Vertices whose distance is lower than this value will be removed.
            /// </summary>
            private readonly FloatParameter _parameterDistance = new FloatParameter("Min. Distance", 0.01f);



            public CleanupParameter()
                : base("Cleanup")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                var distance = _parameterDistance.Value;

                foreach (Face face in meshEntity.Faces.ToList())
                {
                    bool modifiedFace = false;

                    foreach (Vertex vertex in face.AllVertices.ToList())
                        if (vertex[face].Next.Position.DistanceTo(vertex.Position) < distance)
                        {
                            vertex.DeleteFromFaces();
                            modifiedFace = true;
                        }
                        else if (vertex[face].Previous.Position.DistanceTo(vertex.Position) < distance)
                        {
                            vertex.DeleteFromFaces();
                            modifiedFace = true;
                        }

                    if (modifiedFace)
                    {
                        face.RecalculateIfConvex();
                        face.RecalculateIsPlanar();
                        face.RecalculateNormal();
                    }
                }

                return meshEntity;
            }
        }

        #endregion

        #region Align Scope

        public abstract class AlignScopeTypeParameter : CompoundParameter
        {
            protected AlignScopeTypeParameter(string label)
                : base(label)
            {
            }



            public abstract MeshEntity Transform(MeshEntity meshEntity);
        }

        /// <summary>
        /// Aligns the scope to the first edge of a given face.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.AlignScopeTypeParameter" />
        public class AlignScopeToFaceParameter : AlignScopeTypeParameter
        {
            /// <summary>
            /// Index of the face whose first edge will serve as base for scope alignment.
            /// </summary>
            private readonly IntParameter _parameterFaceIndex = new IntParameter("Face Index", 0);



            public AlignScopeToFaceParameter()
                : base("To Face")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                Face face = meshEntity[_parameterFaceIndex.Value];
                meshEntity.BoxScope = face.GetDerivedScope(meshEntity.BoxScope);
                meshEntity.AdjustScope();

                return meshEntity;
            }
        }

        /// <summary>
        /// Aligns the scope to the given edge of a given face.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.AlignScopeTypeParameter" />
        public class AlignScopeToEdgeParameter : AlignScopeTypeParameter
        {
            /// <summary>
            /// Index of the edge (for the selected face) that will serve as base for scope alignment.
            /// </summary>
            private readonly IntParameter _parameterFaceIndex = new IntParameter("Face Index", 0);

            /// <summary>
            /// Index of the face whose edge will serve as base for scope alignment.
            /// </summary>
            private readonly IntParameter _parameterEdgeIndex = new IntParameter("Edge Index", 0);



            public AlignScopeToEdgeParameter()
                : base("To Edge")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                Face face = meshEntity[_parameterFaceIndex.Value];

                meshEntity.BoxScope = face.GetAlignedScope(_parameterEdgeIndex.Value);
                meshEntity.AdjustScope();

                return meshEntity;
            }
        }

        /// <summary>
        /// Aligns the mesh scope to a particular face or edge.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class AlignScopeParameter : MeshModifyParameter
        {
            /// <summary>
            /// Type of scope alignment to perform.
            /// </summary>
            private readonly SelectListParameter<AlignScopeTypeParameter> _parameterAlignScopeType = new SelectListParameter<AlignScopeTypeParameter>("Alignment", "To Face");



            /// <summary>
            /// Scope axis to which the alignment should be performed.
            /// </summary>
            /*private readonly ChoiceParameter _parameterSplitAxis = new ChoiceParameter("Axis", "X", "X", "Y", "Z");*/
            public AlignScopeParameter()
                : base("Align Scope")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                var item = _parameterAlignScopeType.Items.FirstOrDefault();
                if (item != null) return item.Transform(meshEntity);

                return meshEntity;
            }
        }

        #endregion

        #region Extrude

        public abstract class ExtrudeMethodParameter : CompoundParameter
        {
            protected ExtrudeMethodParameter(string label)
                : base(label)
            {
            }



            protected void CopyFaceAttributes(Face sourceFace, Face targetFace)
            {
                var sourceHvs = sourceFace.HalfVertices.ToList();
                var targetHvs = targetFace.HalfVertices.ToList();

                for (int i = 0; i < sourceHvs.Count; i++) sourceHvs[i].Attributes.ComplementAttributesTo(targetHvs[i].Attributes);
            }



            public abstract void Run(MeshEntity meshEntity, AttributeParameter<string> parameterAttributeSection, float amount, bool createCap);
        }

        /// <summary>
        /// Extrudes along the face normals.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.ExtrudeMethodParameter" />
        public class NormalExtrudeParameter : ExtrudeMethodParameter
        {
            /// <summary>
            /// Amount to offset at the end of the extrusion. 
            /// Positive values means the end will be larger, negative values means that it will be smaller.
            /// </summary>
            private readonly FloatParameter _parameterOffsetAmount = new FloatParameter("Offset", 0);


            private AttributeParameter<string> _parameterAttributeSection;



            protected NormalExtrudeParameter()
                : base("Normal")
            {
            }



            /// <summary>
            /// Standard extrusion, without any offset.
            /// </summary>
            /// <param name="meshEntity"></param>
            /// <param name="amount"></param>
            /// <param name="createCap"></param>
            public void Extrude(MeshEntity meshEntity, float amount)
            {
                if (Math.Abs(amount) < float.Epsilon)
                    return;


                foreach (var face in meshEntity.Faces.ToList()) ExtrudeAlongDirection(meshEntity, face, face.Normal * amount);
            }



            private void ExtrudeAlongDirection(MeshEntity meshEntity, Face face, Vector3D sizedDirection)
            {
                List<Vertex> vertices = new List<Vertex>();

                //create a ring of vertices just like the original face, but translated
                foreach (Vertex vertex in face.Vertices)
                    vertices.Add(new Vertex(vertex.Position + sizedDirection));

                Face topFace = new Face(vertices) {Material = face.Material}; //, Color = face.Color
                _parameterAttributeSection[topFace] = "Top";
                meshEntity.Add(topFace);

                if (face.HasHoles)
                    foreach (CircularList<Vertex> hole in face.Holes)
                    {
                        CircularList<Vertex> topFaceHole = new CircularList<Vertex>();

                        foreach (Vertex holeVertex in hole)
                            topFaceHole.Add(new Vertex(holeVertex.Position + sizedDirection));

                        for (int i = 0; i < hole.Count; i++)
                        {
                            var newFace = new Face(hole[i], hole[i + 1], topFaceHole[i + 1], topFaceHole[i]) {Material = face.Material}; //, Color = face.Color
                            _parameterAttributeSection[newFace] = "Inside";
                            meshEntity.Add(newFace);
                        }


                        topFace.AddHole(topFaceHole);
                    }

                //now add the side faces
                for (int i = 0; i < face.Vertices.Count(); i++)
                {
                    var sideFace = new Face(face[i], face[i + 1], topFace[i + 1], topFace[i]) {Material = face.Material}; //, Color = face.Color
                    _parameterAttributeSection[sideFace] = "Side";
                    meshEntity.Add(sideFace);
                }

                //copy the attribute information, including textures and tangents, if applicable
                CopyFaceAttributes(face, topFace);
            }



            /// <summary>
            /// Offsets a given MeshEntity to the inside.
            /// </summary>
            /// <param name="meshEntity">The MeshEntity with the face.</param>
            /// <param name="parameterAttributeSection"></param>
            /// <param name="face">Face to offset</param>
            /// <param name="amount">Amount of offset. Must be a positive value.</param>
            /// <param name="extrudeAmount"></param>
            /// <returns>The inside of the offset. The outside faces are added to the passed shape.</returns>
            private void ExtrudeOffsetInside(MeshEntity meshEntity, Face face, float amount, float extrudeAmount)
            {
                Vector3D extrudeDirection = face.Normal;
                int vertexCount = face.Vertices.Count();
                CircularList<Vertex> insideVertices = new CircularList<Vertex>();

                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3D firstVector = (face[i - 1].Position - face[i].Position).Normalize();
                    Vector3D secondVector = (face[i + 1].Position - face[i].Position).Normalize();

                    Vector3D direction = (secondVector + firstVector).Normalize();

                    float actualAmount = (float) (amount / Math.Sin(firstVector.AngleTo(secondVector) / 2f));

                    if (firstVector.IsCollinear(secondVector))
                        direction = face.Normal.Cross(firstVector).Normalize();

                    if (secondVector.Cross(firstVector).Normalize().Equals(face.Normal))
                        insideVertices.Add(new Vertex(face[i].Position - direction * actualAmount + extrudeDirection * extrudeAmount));
                    else
                        insideVertices.Add(new Vertex(face[i].Position + direction * actualAmount + extrudeDirection * extrudeAmount));
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    var sideFace = new Face(face[i], face[i + 1], insideVertices[i + 1], insideVertices[i]);
                    _parameterAttributeSection[sideFace] = "Side";
                    meshEntity.Add(sideFace);
                }

                var topFace = new Face(insideVertices);
                _parameterAttributeSection[topFace] = "Top";
                meshEntity.Add(topFace);

                //copy the attribute information, including textures and tangents, if applicable
                CopyFaceAttributes(face, topFace);
            }



            public override void Run(MeshEntity meshEntity, AttributeParameter<string> parameterAttributeSection, float amount, bool createCap)
            {
                _parameterAttributeSection = parameterAttributeSection;

                var baseFaces = meshEntity.Faces.ToList();

                if (Math.Abs(_parameterOffsetAmount.Value) > 0)
                    foreach (Face face in meshEntity.ToList())
                        ExtrudeOffsetInside(meshEntity, face, -_parameterOffsetAmount.Value, amount);
                else
                    Extrude(meshEntity, amount);

                //Add or remove the base face
                if (createCap)
                    foreach (var baseFace in baseFaces)
                    {
                        baseFace.Flip();
                        _parameterAttributeSection[baseFace] = "Cap";
                    }
                else
                    foreach (var baseFace in baseFaces)
                        meshEntity.RemoveAndDetach(baseFace);
            }
        }

        /// <summary>
        /// Extrudes along a given direction.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.ExtrudeMethodParameter" />
        public class DirectionalExtrudeParameter : ExtrudeMethodParameter
        {
            /// <summary>
            /// Direction of the extrusion.
            /// </summary>
            private readonly Vector3DParameter _parameterDirection = new Vector3DParameter("Direction", new Vector3D(1, 0, 1));

            /// <summary>
            /// Amount of offset at the end of the extrude. 
            /// Positive values means the end will be larger, negative values means that it will be smaller.
            /// </summary>
            private readonly FloatParameter _parameterOffsetAmount = new FloatParameter("Offset", 0);

            /// <summary>
            /// Indicates if the operation should be relative to the scope's direction or to the world.
            /// </summary>
            private readonly ChoiceParameter _parameterRelativeTo = new ChoiceParameter("Relative To", "Scope", "Scope", "World");


            /// <summary>
            /// Indicates if the top should be bent, making the top face's normal match the extruded direction.
            /// </summary>
            private readonly BoolParameter _parameterBendTop = new BoolParameter("Bend Top", false);


            private AttributeParameter<string> _parameterAttributeSection;



            protected DirectionalExtrudeParameter()
                : base("Directional")
            {
            }



            /// <summary>
            /// Standard extrusion, without any offset.
            /// </summary>
            /// <param name="meshEntity"></param>
            /// <param name="direction"></param>
            /// <param name="bendTop"></param>
            /// <param name="amount"></param>
            /// <param name="createCap"></param>
            public void DirectionalExtrude(MeshEntity meshEntity, Vector3D direction, bool bendTop)
            {
                if (direction.IsNaN || direction == Vector3D.Zero)
                    return;

                foreach (var face in meshEntity.Faces.ToList()) ExtrudeAlongDirection(meshEntity, face, direction, bendTop);
            }



            private void ExtrudeAlongDirection(MeshEntity meshEntity, Face face, Vector3D sizedDirection, bool bendTop)
            {
                Vector3D normalDirection = sizedDirection.Normalize();
                Plane3D plane3D = new Plane3D(-normalDirection, face.Centroid + sizedDirection);

                List<Vertex> vertices = new List<Vertex>();

                //create a ring of vertices just like the original face, but translated
                foreach (Vertex vertex in face.Vertices)
                {
                    if (bendTop)
                        sizedDirection = normalDirection * Math.Max(0, plane3D.DistanceToPoint(vertex.Position));

                    vertices.Add(new Vertex(vertex.Position + sizedDirection));
                }


                Face topFace = new Face(vertices) {Material = face.Material}; //, Color = face.Color
                _parameterAttributeSection[topFace] = "Top";
                meshEntity.Add(topFace);

                //copy the attribute information, including textures and tangents, if applicable
                CopyFaceAttributes(face, topFace);

                if (face.HasHoles)
                    foreach (CircularList<Vertex> hole in face.Holes)
                    {
                        CircularList<Vertex> topFaceHole = new CircularList<Vertex>();

                        foreach (Vertex holeVertex in hole)
                            topFaceHole.Add(new Vertex(holeVertex.Position + sizedDirection));

                        for (int i = 0; i < hole.Count; i++)
                        {
                            var newFace = new Face(hole[i], hole[i + 1], topFaceHole[i + 1], topFaceHole[i]) {Material = face.Material}; //, Color = face.Color
                            _parameterAttributeSection[newFace] = "Inside";
                            meshEntity.Add(newFace);
                        }


                        topFace.AddHole(topFaceHole);
                    }

                //now add the side faces
                for (int i = 0; i < face.Vertices.Count(); i++)
                {
                    var sideFace = new Face(face[i], face[i + 1], topFace[i + 1], topFace[i]) {Material = face.Material}; //, Color = face.Color
                    _parameterAttributeSection[sideFace] = "Side";
                    meshEntity.Add(sideFace);
                }
            }



            /// <summary>
            /// Offsets a given MeshEntity to the inside.
            /// </summary>
            /// <param name="meshEntity">The MeshEntity with the face.</param>
            /// <param name="parameterAttributeSection"></param>
            /// <param name="face">Face to offset</param>
            /// <param name="offsetAmount">Amount of offset. Must be a positive value.</param>
            /// <param name="extrudeAmount"></param>
            /// <returns>The inside of the offset. The outside faces are added to the passed shape.</returns>
            private void ExtrudeOffsetInside(MeshEntity meshEntity, Face face, float offsetAmount, Vector3D extrudeDirection, bool bendTop)
            {
                int vertexCount = face.Vertices.Count();
                CircularList<Vertex> holeVertices = new CircularList<Vertex>();

                Vector3D normalDirection = extrudeDirection.Normalize();
                Plane3D plane3D = new Plane3D(-normalDirection, face.Centroid + extrudeDirection);

                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3D firstVector = (face[i - 1].Position - face[i].Position).Normalize();
                    Vector3D secondVector = (face[i + 1].Position - face[i].Position).Normalize();

                    Vector3D direction = (secondVector + firstVector).Normalize();

                    float actualAmount = (float) (offsetAmount / Math.Sin(firstVector.AngleTo(secondVector) / 2f));

                    if (firstVector.IsCollinear(secondVector))
                        direction = face.Normal.Cross(firstVector).Normalize();

                    if (secondVector.Cross(firstVector).Normalize().Equals(face.Normal))
                    {
                        if (bendTop)
                            extrudeDirection = normalDirection * plane3D.DistanceToPoint(face[i].Position - direction * offsetAmount);

                        holeVertices.Add(new Vertex(face[i].Position - direction * actualAmount + extrudeDirection));
                    }
                    else
                    {
                        Vector3D offsettedPoint = face[i].Position + direction * offsetAmount;
                        float distanceToPoint = plane3D.DistanceToPoint(offsettedPoint);

                        if (bendTop)
                            extrudeDirection = normalDirection * distanceToPoint;

                        holeVertices.Add(new Vertex(offsettedPoint + extrudeDirection));
                    }
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    var sideFace = new Face(face[i], face[i + 1], holeVertices[i + 1], holeVertices[i]);
                    _parameterAttributeSection[sideFace] = "Side";
                    meshEntity.Add(sideFace);
                }

                var topFace = new Face(holeVertices);
                _parameterAttributeSection[topFace] = "Top";
                meshEntity.Add(topFace);

                //copy the attribute information, including textures and tangents, if applicable
                CopyFaceAttributes(face, topFace);
            }



            public override void Run(MeshEntity meshEntity, AttributeParameter<string> parameterAttributeSection, float amount, bool createCap)
            {
                _parameterAttributeSection = parameterAttributeSection;

                Vector3D direction = _parameterDirection.Value;

                var bendTop = _parameterBendTop.Value;

                if (_parameterRelativeTo.Value == "Scope")
                    direction = meshEntity.BoxScope.ToWorldDirection(direction); //.XAxis * direction.X + meshEntity.BoxScope.YAxis * direction.Y + meshEntity.BoxScope.ZAxis * direction.Z;

                var baseFaces = meshEntity.Faces.ToList();

                if (Math.Abs(_parameterOffsetAmount.Value) > 0)
                    foreach (Face face in meshEntity.ToList())
                        ExtrudeOffsetInside(meshEntity, face, -_parameterOffsetAmount.Value, direction * amount, bendTop);
                else
                    DirectionalExtrude(meshEntity, direction * amount, bendTop);

                //Add or remove the base face
                if (createCap)
                    foreach (var baseFace in baseFaces)
                    {
                        baseFace.Flip();
                        _parameterAttributeSection[baseFace] = "Cap";
                    }
                else
                    foreach (var baseFace in baseFaces)
                        meshEntity.RemoveAndDetach(baseFace);
            }
        }

        /// <summary>
        /// Extrudes along the shared edges.
        /// </summary>
        public class SharedExtrudeParameter : ExtrudeMethodParameter
        {
            private readonly HashSet<Vertex> _innerVertices = new HashSet<Vertex>();
            private readonly Dictionary<Vertex, Vertex> _vertexDictionary = new Dictionary<Vertex, Vertex>();
            private AttributeParameter<string> _attributeSection;



            protected SharedExtrudeParameter()
                : base("Shared")
            {
            }



            public override void Run(MeshEntity meshEntity, AttributeParameter<string> parameterAttributeSection, float amount, bool createCap)
            {
                _attributeSection = parameterAttributeSection;
                _vertexDictionary.Clear();
                _innerVertices.Clear();

                SharedExtrude(meshEntity, amount, createCap);
            }



            private void SharedExtrude(MeshEntity meshEntity, float amount, bool createCap)
            {
                List<Face> baseFaces = meshEntity.Faces.ToList();

                foreach (Vertex vertex in meshEntity.FaceVertices)
                {
                    //List<HalfVertex> halfVertices = vertex.HalfVertices;
                    var adjacentFaces = vertex.AdjacentFaces.ToList();
                    Vector3D vector3D = adjacentFaces[0].Normal;
                    for (int i = 1; i < adjacentFaces.Count; i++)
                    {
                        //vector3D = vector3D + ((adjacentFaces[i].Normal - vector3D)/2f);
                        var angleHalf = vector3D.AngleTo(adjacentFaces[i].Normal) / 2f;
                        vector3D = (vector3D + adjacentFaces[i].Normal).Normalize() / (float) Math.Cos(angleHalf);
                    }

                    if (adjacentFaces.Count > 1)
                        _innerVertices.Add(vertex);

                    _vertexDictionary.Add(vertex, new Vertex(vertex) {Position = vertex.Position + vector3D * amount});
                }

                //apply the operation to the faces only, leave the remaining items as they were
                foreach (Face face in baseFaces) SharedExtrude(meshEntity, face);

                //if the user requests for the faces to remain, leave them, but flip them
                if (createCap)
                    foreach (Face baseFace in baseFaces)
                    {
                        _attributeSection[baseFace] = "Cap";
                        baseFace.Flip();
                    }
                //otherwise, remove the caps
                else
                    foreach (var cap in baseFaces)
                        meshEntity.RemoveAndDetach(cap);
            }



            private void SharedExtrude(MeshEntity meshEntity, Face face)
            {
                List<Vertex> vertices = new List<Vertex>();

                //create a ring of vertices just like the original face, but translated
                foreach (Vertex vertex in face.Vertices)
                    vertices.Add(_vertexDictionary[vertex]);

                Face topFace = new Face(vertices) {Material = face.Material}; //, Color = face.Color
                _attributeSection[topFace] = "Top";
                meshEntity.Add(topFace);

                //now add the side faces
                for (int i = 0; i < face.Vertices.Count(); i++)
                    if (face[i].AdjacentFaces.Intersect(face[i + 1].AdjacentFaces).Count() <= 1)
                    {
                        var sideFace = new Face(face[i], face[i + 1], topFace[i + 1], topFace[i]) {Material = face.Material}; //, Color = face.Color
                        _attributeSection[sideFace] = "Side";
                        meshEntity.Add(sideFace);
                    }

                //copy the attribute information, including textures and tangents, if applicable
                CopyFaceAttributes(face, topFace);
            }
        }

        /// <summary>
        /// Extrudes along the face normals while performing scaling and rotation transformations.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.ExtrudeMethodParameter" />
        public class TransformExtrudeExtrude : ExtrudeMethodParameter
        {
            /// <summary>
            /// The angle to be rotated.
            /// </summary>
            private readonly FloatParameter _parameterAngle = new FloatParameter("Rotation Angle", 45);

            /// <summary>
            /// The angle around which to rotate.
            /// </summary>
            private readonly Vector3DParameter _parameterAxis = new Vector3DParameter("Axis", Vector3D.XVector);

            /// <summary>
            /// The type of sizing to consider.<br/>
            /// <b>Radius</b> means that the indicated value corresponds to the radius for rotation.<br/>
            /// <b>Length</b> means that the indicated value corresponds to the length of the extrusion block.
            /// </summary>
            private readonly ChoiceParameter _parameterSizing = new ChoiceParameter("Sizing", "Length", "Radius", "Length");

            /// <summary>
            /// The amount of scaling to perform, from the bottom to the top.
            /// </summary>
            private readonly FloatParameter _parameterScaling = new FloatParameter("Scaling", 1);

            /// <summary>
            /// The number of segments in which the extrusion should be divided.
            /// </summary>
            private readonly IntParameter _parameterSegments = new IntParameter("Segments", 3);


            private AttributeParameter<string> _parameterAttributeSection;



            protected TransformExtrudeExtrude()
                : base("Transform")
            {
            }



            private Plane3D CalculateRotationPlane(Face face, Vector3D axis, float radius)
            {
                Plane3D plane3D = new Plane3D(face.Normal.Cross(axis), face.Vertices.First().Position);

                foreach (Vertex vertex in face.Vertices)
                    Plane3DHelper.MovePlane(ref plane3D, vertex.Position, 0);

                plane3D = new Plane3D(plane3D.Normal, plane3D.Point0 - plane3D.Normal * radius);

                return plane3D;
            }



            private void RotationExtrude(MeshEntity meshEntity, Face face, Vector3D axis, float radius, float angle, int segments)
            {
                Plane3D rotationPlane = CalculateRotationPlane(face, axis, radius);

                float angleRadians = -MathHelper.ToRadians(angle / segments);

                List<Vector3D> basePoints = face.Vertices.Select(x => meshEntity.BoxScope.ToScopePosition(x.Position)).ToList();

                float segmentScale = (_parameterScaling.Value - 1) / segments;

                CircularList<CircularList<Vertex>> segmentList = new CircularList<CircularList<Vertex>>();

                //add the base one
                segmentList.Add(new CircularList<Vertex>(face.Vertices));

                for (int i = 1; i <= segments; i++)
                {
                    Matrix matrix = Matrix.CreateTranslation(rotationPlane.Point0.X, rotationPlane.Point0.Y, rotationPlane.Point0.Z)
                                    * Matrix.CreateAxisAngle(axis, -angleRadians * i)
                                    * Matrix.CreateTranslation(-rotationPlane.Point0.X, -rotationPlane.Point0.Y, -rotationPlane.Point0.Z);

                    CircularList<Vertex> sectionList = new CircularList<Vertex>();

                    foreach (Vector3D point in basePoints)
                    {
                        Vector3D scaledPoint = point * (1 + segmentScale * i);

                        Vector3D worldPosition = meshEntity.BoxScope.ToWorldPosition(scaledPoint);

                        worldPosition = matrix.Transform(worldPosition);
                        sectionList.Add(new Vertex(worldPosition));
                    }

                    segmentList.Add(sectionList);
                }

                //List<Face> sideFaces = new List<Face>();

                for (int i = 0; i < segments; i++)
                {
                    CircularList<Vertex> circularList = segmentList[i];

                    for (int j = 0; j < circularList.Count; j++)
                    {
                        var sideFace = new Face(segmentList[i][j], segmentList[i][j + 1], segmentList[i + 1][j + 1], segmentList[i + 1][j]); // { Tag = i + "," + j }
                        _parameterAttributeSection[sideFace] = "Side";
                        meshEntity.Add(sideFace);
                    }
                }

                Face topFace = new Face(segmentList.Last());
                _parameterAttributeSection[topFace] = "Top";
                meshEntity.Add(topFace);

                //copy the attribute information, including textures and tangents, if applicable
                CopyFaceAttributes(face, topFace);
            }



            public override void Run(MeshEntity meshEntity, AttributeParameter<string> parameterAttributeSection, float amount, bool createCap)
            {
                _parameterAttributeSection = parameterAttributeSection;

                Vector3D scopePosition = meshEntity.BoxScope.ToWorldDirection(_parameterAxis.Value).Normalize();

                float radius = _parameterSizing.Value == "Radius" ? amount : amount / (_parameterAngle.Value / 360 * 2 * 3.1415926535f);

                foreach (var face in meshEntity.ToList())
                {
                    RotationExtrude(meshEntity, face, scopePosition, radius, _parameterAngle.Value, _parameterSegments.Value);
                    if (createCap)
                    {
                        face.Flip();
                        _parameterAttributeSection[face] = "Cap";
                    }
                    else
                    {
                        meshEntity.RemoveAndDetach(face);
                    }
                }
            }
        }

        /// <summary>
        /// Creates extruded geometries from each mesh face.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshModifyProcedure.MeshModifyParameter" />
        public class ExtrudeParameter : MeshModifyParameter
        {
            /// <summary>
            /// Amount to be extruded.
            /// </summary>
            private readonly FloatParameter _parameterAmount = new FloatParameter("Amount", 1);

            /// <summary>
            /// Extrusion method to be applied.
            /// </summary>
            private readonly SelectListParameter<ExtrudeMethodParameter> _parameterExtrudeMethod = new SelectListParameter<ExtrudeMethodParameter>("Method", "Normal");


            /// <summary>
            /// Indicates if the original face should be included in the final result as well
            /// </summary>
            private readonly BoolParameter _parameterCap = new BoolParameter("Cap", false);

            /// <summary>
            /// Attribute where to store the name of the section for each created face. Values can be "Top" (for the top faces), "Side" 
            /// (for the side faces), "Cap" (for the bottom faces, if the Cap option is checked), or "Inside" (for the side faces that result from holes).
            /// </summary>
            private readonly AttributeParameter<string> _parameterAttributeSection = new AttributeParameter<string>("Section", AttributeAccess.Write);



            public ExtrudeParameter()
                : base("Extrude")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                /*if (Procedure.CreateVisualHandles)
                {
                    foreach (var face in meshEntity)
                    {
                        AddVisualHandle(_parameterAmount.FullName,new NumericSizerVisualHandle(face.Centroid + face.Normal * _parameterAmount.Value, face.Normal));//AmountParameter
                    }
                }*/

                var extrudeMethod = _parameterExtrudeMethod.Items.FirstOrDefault();
                if (extrudeMethod != null) extrudeMethod.Run(meshEntity, _parameterAttributeSection, _parameterAmount.Value, _parameterCap.Value);

                return meshEntity;
            }
        }

        #endregion
    }
}