using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Sets up UV mapping for meshes.
    /// </summary>
    [Procedure("9470e48f-5ee8-4d84-a371-949f27786f0f", Label = "Mesh UV Map", Category = "Mesh")]
    public class MeshUVMapProcedure : TransferProcedure<MeshEntity>
    {
        /// <summary>
        /// The UV mapping operation/shape to apply.
        /// </summary>
        private readonly SelectListParameter<UVMappingParameter> _parameterUVMapping = new SelectListParameter<UVMappingParameter>("Operation", "Face UV");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterUVMapping.SubParameterLabels);



        protected override MeshEntity Process(MeshEntity entity)
        {
            foreach (var parameter in _parameterUVMapping.Items)
                entity = parameter.UVMap(entity);

            return entity;
        }



        /// <summary>
        /// Defines the coordinate sizing for the given coordinate.
        /// </summary>
        /// <seealso cref="Sceelix.Core.Parameters.CompoundParameter" />
        public class CoordinateSizingParameter : CompoundParameter
        {
            /// <summary>
            /// Scale of the coordinate mapping.
            /// </summary>
            public readonly FloatParameter ParameterSize = new FloatParameter("Size", 1);

            /// <summary>
            ///  Indicates if the defined size is an absolute value (in world space), or relative to the chosen surrounding shape's size.
            /// </summary>
            public readonly BoolParameter ParameterAbsolute = new BoolParameter("Absolute", true);



            public CoordinateSizingParameter(string label)
                : base(label)
            {
            }
        }

        #region Abstract Parameter

        public abstract class UVMappingParameter : CompoundParameter
        {
            protected UVMappingParameter(string label)
                : base(label)
            {
            }



            protected internal abstract MeshEntity UVMap(MeshEntity meshEntity);
        }

        #endregion

        #region FlipUV

        /// <summary>
        /// Flips the current UV mapping coordinates in the U or V axis.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUVMapProcedure.UVMappingParameter" />
        public class FlipUVParameter : UVMappingParameter
        {
            /// <summary>
            /// Indicates if the U coordinate should be flipped.
            /// </summary>
            private readonly BoolParameter _parameterFlipU = new BoolParameter("Flip U", true);

            /// <summary>
            /// Indicates if the V coordinate should be flipped.
            /// </summary>
            private readonly BoolParameter _parameterFlipV = new BoolParameter("Flip V", true);



            public FlipUVParameter()
                : base("Flip UV")
            {
            }



            public static MeshEntity Apply(MeshEntity meshEntity, bool flipU, bool flipV)
            {
                foreach (Face face in meshEntity)
                foreach (HalfVertex halfVertex in face.HalfVertices)
                {
                    Vector2D textureCoordinate = halfVertex.UV0;
                    float u = textureCoordinate.X, v = textureCoordinate.Y;
                    if (flipU)
                        u = -u;
                    if (flipV)
                        v = -v;

                    halfVertex.UV0 = new Vector2D(u, v);
                }

                return meshEntity;
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterFlipU.Value, _parameterFlipV.Value);
            }
        }

        #endregion

        #region Scale UV

        /// <summary>
        /// Scale the UV coordinates.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUVMapProcedure.UVMappingParameter" />
        public class ScaleUVParameter : UVMappingParameter
        {
            /// <summary>
            /// The value to multiply with the UV coordinates.
            /// </summary>
            private readonly Vector2DParameter _parameterAmount = new Vector2DParameter("Amount", new Vector2D(1, 1));



            public ScaleUVParameter()
                : base("Scale UV")
            {
            }



            public static MeshEntity Apply(MeshEntity meshEntity, Vector2D amount)
            {
                foreach (Face face in meshEntity)
                foreach (HalfVertex halfVertex in face.HalfVertices)
                {
                    Vector2D textureCoordinate = halfVertex.UV0;
                    halfVertex.UV0 = new Vector2D(textureCoordinate.X * amount.X, textureCoordinate.Y * amount.Y);
                }

                return meshEntity;
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterAmount.Value);
            }
        }

        #endregion

        #region Cylinder UV Mapping

        /// <summary>
        /// Applies UV mapping coordinates based on a surrounding cylinder shape.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUVMapProcedure.UVMappingParameter" />
        public class CylinderUVMappingParameter : UVMappingParameter
        {
            /// <summary>
            /// U coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterU = new CoordinateSizingParameter("U");

            /// <summary>
            /// V coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterV = new CoordinateSizingParameter("V");



            public CylinderUVMappingParameter()
                : base("Cylinder UV")
            {
            }



            public static MeshEntity Apply(MeshEntity meshEntity, float uSize, bool absoluteU, float vSize, bool absoluteV)
            {
                Vector3D centroid = meshEntity.Centroid;
                Vector2D centroid2D = meshEntity.BoxScope.ToScopePosition(centroid).ToVector2D();

                float perimeter = (float) (Math.Max(meshEntity.BoxScope.Sizes.X, meshEntity.BoxScope.Sizes.Y) * Math.PI);

                /*foreach (var vertex in meshEntity.FaceVertices)
                {
                    Vector3D direction = vertex.Position - centroid;
                    //Vector3D scopeDirection2 = MeshEntity.BoxScope.ToScopeDirection(direction);
                    Vector3D scopeDirection = meshEntity.BoxScope.ToScopeDirection(direction);

                    Vector3D normalizedScopeDirection = scopeDirection.Normalize();

                    float u = 0.5f + (float) (Math.Atan2(normalizedScopeDirection.X, normalizedScopeDirection.Y)/(2.0*Math.PI));
                    float v = -meshEntity.BoxScope.ToScopePosition(vertex.Position).Z/meshEntity.BoxScope.Sizes.Z;

                    u *= absoluteU ? uSize*perimeter : uSize;
                    v *= absoluteV ? vSize*meshEntity.BoxScope.Sizes.Z : vSize;

                    vertex.SetAttribute(new GlobalAttributeKey("Section",new HighlightMeta()), u);

                    foreach (HalfVertex halfVertex in vertex.HalfVertices)
                        halfVertex.UV0 = new Vector2D(u, v);
                }*/

                foreach (var face in meshEntity)
                foreach (var halfVertex in face.HalfVertices)
                {
                    Vector3D scopeDirection = meshEntity.BoxScope.ToScopePosition(halfVertex.Vertex.Position) - centroid2D.ToVector3D(halfVertex.Vertex.Position.Z);
                    if (Math.Abs(scopeDirection.Length) < float.Epsilon)
                        scopeDirection = halfVertex.Face.Normal;

                    Vector3D normalizedScopeDirection = scopeDirection.Normalize();

                    float u = 1 - (0.5f + (float) (Math.Atan2(normalizedScopeDirection.X, normalizedScopeDirection.Y) / (2.0 * Math.PI)));
                    float v = -meshEntity.BoxScope.ToScopePosition(halfVertex.Vertex.Position).Z / meshEntity.BoxScope.Sizes.Z;

                    u *= absoluteU ? uSize * perimeter : uSize;
                    v *= absoluteV ? vSize * meshEntity.BoxScope.Sizes.Z : vSize;

                    halfVertex.UV0 = new Vector2D(u, v);
                }


                foreach (var face in meshEntity)
                foreach (var halfVertex in face.HalfVertices)
                {
                    Edge emanatingEdge = halfVertex.GetEmanatingEdge();

                    Vector3D vector3D = emanatingEdge.Direction.Cross(halfVertex.Face.Normal);

                    Vector3D d = meshEntity.BoxScope.ToScopeDirection(vector3D);

                    if (d.Z >= 0 && halfVertex.UV0.X < emanatingEdge.V1[face].UV0.X)
                        halfVertex.UV0 = new Vector2D(halfVertex.UV0.X + (absoluteU ? perimeter : 1.0f), halfVertex.UV0.Y);

                    else if (d.Z < 0 && halfVertex.UV0.X > emanatingEdge.V1[face].UV0.X)
                        emanatingEdge.V1[face].UV0 = new Vector2D(emanatingEdge.V1[face].UV0.X + (absoluteU ? perimeter : 1.0f), emanatingEdge.V1[face].UV0.Y);
                }

                return meshEntity;
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterU.ParameterSize.Value, _parameterU.ParameterAbsolute.Value, _parameterV.ParameterSize.Value, _parameterV.ParameterAbsolute.Value);
            }
        }

        #endregion

        #region FaceUV Mapping

        /// <summary>
        /// Applies UV mapping coordinates based on each face's surrounding plane scope shape.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUVMapProcedure.UVMappingParameter" />
        public class FaceUVMappingParameter : UVMappingParameter
        {
            /// <summary>
            /// U coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterU = new CoordinateSizingParameter("U");

            /// <summary>
            /// V coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterV = new CoordinateSizingParameter("V");



            public FaceUVMappingParameter()
                : base("Face UV")
            {
            }



            public static MeshEntity Apply(MeshEntity meshEntity, float uSize, bool absoluteU, float vSize, bool absoluteV)
            {
                foreach (Face face in meshEntity)
                {
                    //PlaneScope planeScope = new PlaneScope(face);
                    BoxScope planeScope = face.GetDerivedScope(meshEntity.BoxScope);
                    UVMap(face, planeScope, absoluteU, absoluteV, uSize, vSize);

                    //face.CalculateTangentAndBinormal();
                }

                return meshEntity;
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterU.ParameterSize.Value, _parameterU.ParameterAbsolute.Value, _parameterV.ParameterSize.Value, _parameterV.ParameterAbsolute.Value);
            }



            public static void UVMap(Face face, BoxScope planeScope, bool absoluteSizingU, bool absoluteSizingV, float u, float v)
            {
                foreach (Vertex vertex in face.Vertices)
                    UVMap(vertex, planeScope, face, absoluteSizingU, absoluteSizingV, u, v);

                if (face.HasHoles)
                    foreach (CircularList<Vertex> vertexList in face.Holes)
                    foreach (Vertex vertex in vertexList)
                        UVMap(vertex, planeScope, face, absoluteSizingU, absoluteSizingV, u, v);
            }



            private static void UVMap(Vertex vertex, BoxScope planeScope, Face face, bool absoluteSizingU, bool absoluteSizingV, float u, float v)
            {
                Vector3D relativePosition = vertex.Position - planeScope.Translation;

                if (relativePosition == Vector3D.Zero)
                {
                    vertex[face].UV0 = new Vector2D(0, 0);
                    vertex[face].Tangent = planeScope.XAxis;
                    vertex[face].Binormal = -planeScope.YAxis;
                }
                else
                {
                    float xScopeLength = planeScope.Sizes.X;
                    float yScopeLength = planeScope.Sizes.Y;

                    double dX = relativePosition.Dot(planeScope.XAxis);
                    double dY = relativePosition.Dot(planeScope.YAxis);

                    dX = absoluteSizingU ? dX / u : dX / xScopeLength * u;
                    dY = absoluteSizingV ? dY / v : dY / yScopeLength * v;


                    vertex[face].UV0 = new Vector2D((float) dX, -(float) dY);
                    vertex[face].Tangent = planeScope.XAxis;
                    vertex[face].Binormal = -planeScope.YAxis;
                }
            }
        }

        #endregion

        #region BoxUV Mapping

        /// <summary>
        /// Applies UV mapping coordinates based on a surrounding box shape.
        /// </summary>
        public class BoxUVMappingParameter : UVMappingParameter
        {
            /// <summary>
            /// U coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterU = new CoordinateSizingParameter("U");

            /// <summary>
            /// V coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterV = new CoordinateSizingParameter("V");



            public BoxUVMappingParameter()
                : base("Box UV")
            {
            }



            public static MeshEntity Apply(MeshEntity meshEntity, float uSize, bool absoluteU, float vSize, bool absoluteV)
            {
                List<BoxScope> sideScopes = GetSideScopes(meshEntity.BoxScope).ToList();

                foreach (Face face in meshEntity)
                {
                    KeyValuePair<BoxScope, float> keyValuePair = sideScopes.SelectMin(val => val.ZAxis.AngleTo(face.Normal));

                    //PlaneScope planeScope = new PlaneScope(face);
                    UVMap(face, keyValuePair.Key, absoluteU, absoluteV, uSize, vSize);
                }

                return meshEntity;
            }



            private static IEnumerable<BoxScope> GetSideScopes(BoxScope boxScope)
            {
                //top
                yield return boxScope;

                //bottom
                yield return new BoxScope(-boxScope.XAxis, boxScope.YAxis, -boxScope.ZAxis, boxScope.Translation + boxScope.SizedXAxis + boxScope.SizedZAxis, boxScope.Sizes);

                //front
                yield return new BoxScope(-boxScope.XAxis, boxScope.ZAxis, boxScope.YAxis, boxScope.Translation + boxScope.SizedXAxis, new Vector3D(boxScope.Sizes.X, boxScope.Sizes.Z, boxScope.Sizes.Y));

                //back
                yield return new BoxScope(boxScope.XAxis, boxScope.ZAxis, -boxScope.YAxis, boxScope.Translation + boxScope.SizedYAxis, new Vector3D(boxScope.Sizes.X, boxScope.Sizes.Z, boxScope.Sizes.Y));

                //left
                yield return new BoxScope(-boxScope.YAxis, boxScope.ZAxis, -boxScope.XAxis, boxScope.Translation + boxScope.SizedYAxis, new Vector3D(boxScope.Sizes.Y, boxScope.Sizes.Z, boxScope.Sizes.X));

                //right
                yield return new BoxScope(boxScope.YAxis, boxScope.ZAxis, boxScope.XAxis, boxScope.Translation + boxScope.SizedXAxis, new Vector3D(boxScope.Sizes.Y, boxScope.Sizes.Z, boxScope.Sizes.X));
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterU.ParameterSize.Value, _parameterU.ParameterAbsolute.Value, _parameterV.ParameterSize.Value, _parameterV.ParameterAbsolute.Value);
            }



            public static void UVMap(Face face, BoxScope planeScope, bool absoluteSizingU, bool absoluteSizingV, float u, float v)
            {
                foreach (Vertex vertex in face.Vertices)
                    UVMap(vertex, planeScope, face, absoluteSizingU, absoluteSizingV, u, v);

                if (face.HasHoles)
                    foreach (CircularList<Vertex> vertexList in face.Holes)
                    foreach (Vertex vertex in vertexList)
                        UVMap(vertex, planeScope, face, absoluteSizingU, absoluteSizingV, u, v);
            }



            private static void UVMap(Vertex vertex, BoxScope planeScope, Face face, bool absoluteSizingU, bool absoluteSizingV, float u, float v)
            {
                Vector3D relativePosition = vertex.Position - planeScope.Translation;

                if (relativePosition == Vector3D.Zero)
                {
                    vertex[face].UV0 = new Vector2D(0, 0);
                    vertex[face].Tangent = planeScope.XAxis;
                    vertex[face].Binormal = -planeScope.YAxis;
                }
                else
                {
                    float xScopeLength = planeScope.Sizes.X;
                    float yScopeLength = planeScope.Sizes.Y;

                    double dX = relativePosition.Dot(planeScope.XAxis);
                    double dY = relativePosition.Dot(planeScope.YAxis);

                    dX = absoluteSizingU ? dX / u : dX / xScopeLength * u;
                    dY = absoluteSizingV ? dY / v : dY / yScopeLength * v;


                    vertex[face].UV0 = new Vector2D((float) dX, -(float) dY);
                    vertex[face].Tangent = planeScope.XAxis;
                    vertex[face].Binormal = -planeScope.YAxis;
                }
            }
        }

        #endregion

        #region Scope UV

        /// <summary>
        /// Applies UV mapping coordinates based on the surrounding scope shape.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUVMapProcedure.UVMappingParameter" />
        public class ScopeUVMappingParameter : UVMappingParameter
        {
            /// <summary>
            /// U coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterU = new CoordinateSizingParameter("U");

            /// <summary>
            /// V coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterV = new CoordinateSizingParameter("V");



            public ScopeUVMappingParameter()
                : base("Scope UV")
            {
            }



            public static MeshEntity Apply(MeshEntity meshEntity, float uSize, bool absoluteU, float vSize, bool absoluteV)
            {
                foreach (Face face in meshEntity) UVMap(face, meshEntity.BoxScope, absoluteU, absoluteV, uSize, vSize);

                return meshEntity;
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterU.ParameterSize.Value, _parameterU.ParameterAbsolute.Value, _parameterV.ParameterSize.Value, _parameterV.ParameterAbsolute.Value);
            }



            public static void UVMap(Face face, BoxScope boxScope, bool absoluteSizingU, bool absoluteSizingV, float u, float v)
            {
                foreach (Vertex vertex in face.Vertices)
                    UVMap(vertex, boxScope, face, absoluteSizingU, absoluteSizingV, u, v);

                if (face.HasHoles)
                    foreach (CircularList<Vertex> vertexList in face.Holes)
                    foreach (Vertex vertex in vertexList)
                        UVMap(vertex, boxScope, face, absoluteSizingU, absoluteSizingV, u, v);
            }



            private static void UVMap(Vertex vertex, BoxScope boxScope, Face face, bool absoluteSizingU, bool absoluteSizingV, float u, float v)
            {
                Vector3D relativePosition = vertex.Position - boxScope.Translation;

                if (relativePosition == Vector3D.Zero)
                {
                    vertex[face].UV0 = new Vector2D(0, 0);
                    vertex[face].Tangent = boxScope.XAxis;
                    vertex[face].Binormal = -boxScope.YAxis;
                }
                else
                {
                    float xScopeLength = boxScope.Sizes.X;
                    float yScopeLength = boxScope.Sizes.Y;

                    double dX = relativePosition.Dot(boxScope.XAxis);
                    double dY = relativePosition.Dot(boxScope.YAxis);

                    dX = absoluteSizingU ? dX / u : dX / xScopeLength * u;
                    dY = absoluteSizingV ? dY / v : dY / yScopeLength * v;


                    vertex[face].UV0 = new Vector2D((float) dX, -(float) dY);
                    vertex[face].Tangent = boxScope.XAxis;
                    vertex[face].Binormal = -boxScope.YAxis;
                }
            }
        }

        #endregion

        #region Sphere UV

        /// <summary>
        /// Applies UV mapping coordinates based on a surrounding sphere shape.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshUVMapProcedure.UVMappingParameter" />
        public class SphereUVMapping : UVMappingParameter
        {
            /// <summary>
            /// U coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterU = new CoordinateSizingParameter("U");

            /// <summary>
            /// V coordinate map sizing. 
            /// </summary>
            private readonly CoordinateSizingParameter _parameterV = new CoordinateSizingParameter("V");



            public SphereUVMapping()
                : base("Sphere UV")
            {
            }



            private MeshEntity Apply(MeshEntity meshEntity, float uSize, bool absoluteU, float vSize, bool absoluteV)
            {
                Vector3D centroid = meshEntity.Centroid;
                float perimeter = (float) (Math.Max(meshEntity.BoxScope.Sizes.X, meshEntity.BoxScope.Sizes.Y) * Math.PI);

                foreach (var vertex in meshEntity.FaceVertices)
                {
                    Vector3D direction = vertex.Position - centroid;
                    Vector3D scopeDirection = meshEntity.BoxScope.ToScopeDirection(direction).Normalize();

                    float u = 0.5f + (float) (Math.Atan2(scopeDirection.X, scopeDirection.Y) / (2.0 * Math.PI));
                    float v = 0.5f - (float) (Math.Asin(scopeDirection.Z) / Math.PI);

                    u *= absoluteU ? uSize * perimeter : uSize;
                    v *= absoluteV ? vSize * meshEntity.BoxScope.Sizes.Z : vSize;

                    foreach (HalfVertex halfVertex in vertex.HalfVertices)
                        halfVertex.UV0 = new Vector2D(u, v);

                    //if (position.Z > 9f && position.Z < 11f)
                    //    Console.WriteLine(new Vector2D(u,v));
                }

                foreach (var face in meshEntity)
                foreach (var halfVertex in face.HalfVertices)
                {
                    Edge emanatingEdge = halfVertex.GetEmanatingEdge();

                    Vector3D vector3D = emanatingEdge.Direction.Cross(halfVertex.Vertex.Position - centroid);

                    Vector3D d = meshEntity.BoxScope.ToScopeDirection(vector3D);

                    if (d.Z < 0 && halfVertex.UV0.X < emanatingEdge.V1[face].UV0.X)
                        halfVertex.UV0 = new Vector2D(halfVertex.UV0.X + 1.0f, halfVertex.UV0.Y);
                    else if (d.Z > 0 && halfVertex.UV0.X > emanatingEdge.V1[face].UV0.X)
                        emanatingEdge.V1[face].UV0 = new Vector2D(emanatingEdge.V1[face].UV0.X + 1.0f, emanatingEdge.V1[face].UV0.Y);
                }

                return meshEntity;
            }



            protected internal override MeshEntity UVMap(MeshEntity meshEntity)
            {
                return Apply(meshEntity, _parameterU.ParameterSize.Value, _parameterU.ParameterAbsolute.Value, _parameterV.ParameterSize.Value, _parameterV.ParameterAbsolute.Value);
            }
        }

        #endregion
    }
}