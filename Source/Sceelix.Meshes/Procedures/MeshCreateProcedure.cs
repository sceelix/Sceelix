using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Mathematics.Spatial;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Creates Meshes of primitive models (cubes, spheres),
    /// or from other Entity types.
    /// </summary>
    [Procedure("74716153-9685-4a42-8409-a9c79de5f8c4", Label = "Mesh Create", Category = "Mesh")]
    public class MeshCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Mesh created according to the defined parameters and/or inputs.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Type of primitive to create.
        /// </summary>
        private readonly SelectListParameter<PrimitiveMeshParameter> _parameterPrimitive = new SelectListParameter<PrimitiveMeshParameter>("Primitive", "Cube");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterPrimitive.SubParameterLabels);



        protected override void Run()
        {
            foreach (var parameter in _parameterPrimitive.Items)
                _output.Write(parameter.CreateMesh());
        }



        #region Abstract

        public abstract class PrimitiveMeshParameter : CompoundParameter
        {
            protected PrimitiveMeshParameter(string label)
                : base(label)
            {
            }



            protected internal abstract MeshEntity CreateMesh();
        }

        #endregion

        #region Cube

        /// <summary>
        /// Creates a cube.
        /// </summary>
        public class CubeMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Size of each side of the cube.
            /// </summary>
            private readonly FloatParameter _sizeParameter = new FloatParameter("Size", 1) {MinValue = 0};



            public CubeMeshParameter()
                : base("Cube")
            {
            }



            public static MeshEntity CreateCube(float size)
            {
                return BoxMeshParameter.CreateBox(size, size, size);
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateCube(_sizeParameter.Value);
            }
        }

        #endregion

        #region Box

        /// <summary>
        /// Creates a box.
        /// </summary>
        public class BoxMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the box.
            /// </summary>
            private readonly FloatParameter _parameterWidth = new FloatParameter("Width", 1) {MinValue = 0};

            /// <summary>
            /// Length (size in Y) of the box.
            /// </summary>
            private readonly FloatParameter _parameterLength = new FloatParameter("Length", 1) {MinValue = 0};

            /// <summary>
            /// Height (size in Z) of the box.
            /// </summary>
            private readonly FloatParameter _parameterHeight = new FloatParameter("Height", 1) {MinValue = 0};



            public BoxMeshParameter()
                : base("Box")
            {
            }



            public TypedParameterReference<float> ParameterHeight => _parameterHeight.ToReference();
            public TypedParameterReference<float> ParameterLength => _parameterLength.ToReference();

            public TypedParameterReference<float> ParameterWidth => _parameterWidth.ToReference();



            public static MeshEntity CreateBox(float width, float length, float height)
            {
                List<Face> faces = new List<Face>(6);

                //centers the solid at the origin, so half the size
                /*float halfWidth = width / 2;
                float halfLength = length / 2;
                float halfHeight = height / 2;*/

                //create the 8 vertices
                Vertex vBottomBackLeft = new Vertex(new Vector3D(0, 0, 0));
                Vertex vBottomBackRight = new Vertex(new Vector3D(width, 0, 0));
                Vertex vBottomFrontLeft = new Vertex(new Vector3D(0, length, 0));
                Vertex vBottomFrontRight = new Vertex(new Vector3D(width, length, 0));
                Vertex vTopBackLeft = new Vertex(new Vector3D(0, 0, height));
                Vertex vTopBackRight = new Vertex(new Vector3D(width, 0, height));
                Vertex vTopFrontLeft = new Vertex(new Vector3D(0, length, height));
                Vertex vTopFrontRight = new Vertex(new Vector3D(width, length, height));

                //create the 6 faces
                faces.Add(new Face(vTopBackRight, vTopBackLeft, vTopFrontLeft, vTopFrontRight)); //top face
                faces.Add(new Face(vBottomBackRight, vBottomBackLeft, vTopBackLeft, vTopBackRight)); //back face
                faces.Add(new Face(vBottomBackLeft, vBottomFrontLeft, vTopFrontLeft, vTopBackLeft)); //left face
                faces.Add(new Face(vBottomFrontRight, vBottomBackRight, vTopBackRight, vTopFrontRight)); //right face
                faces.Add(new Face(vBottomFrontRight, vBottomFrontLeft, vBottomBackLeft, vBottomBackRight)); //bottom face
                faces.Add(new Face(vBottomFrontLeft, vBottomFrontRight, vTopFrontRight, vTopFrontLeft)); //Front face

                return new MeshEntity(faces);
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateBox(_parameterWidth.Value, _parameterLength.Value, _parameterHeight.Value);
            }
        }

        #endregion

        #region Octahedron

        /// <summary>
        /// Creates an Octahedron.
        /// </summary>
        public class OctahedronMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Radius of the Octahedron
            /// </summary>
            private readonly FloatParameter _parameterRadius = new FloatParameter("Radius", 1) {MinValue = 0};



            public OctahedronMeshParameter()
                : base("Octahedron")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateOctahedron(_parameterRadius.Value);
            }



            public static MeshEntity CreateOctahedron(float radius)
            {
                List<Face> faces = new List<Face>(6);


                //create the 8 vertices
                Vertex v1 = new Vertex(new Vector3D(0, radius, 0));
                Vertex v2 = new Vertex(new Vector3D(radius, 0, 0));
                Vertex v3 = new Vertex(new Vector3D(0, -radius, 0));
                Vertex v4 = new Vertex(new Vector3D(-radius, 0, 0));
                Vertex v5 = new Vertex(new Vector3D(0, 0, radius));
                Vertex v6 = new Vertex(new Vector3D(0, 0, -radius));


                faces.Add(new Face(v1, v2, v5));
                faces.Add(new Face(v2, v3, v5));
                faces.Add(new Face(v3, v4, v5));
                faces.Add(new Face(v4, v1, v5));

                faces.Add(new Face(v2, v1, v6));
                faces.Add(new Face(v3, v2, v6));
                faces.Add(new Face(v4, v3, v6));
                faces.Add(new Face(v1, v4, v6));

                return new MeshEntity(faces, BoxScope.Identity);
            }
        }

        #endregion

        #region Icosahedron

        /// <summary>
        /// Creates an Icosahedron.
        /// </summary>
        public class IcosahedronMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Side length of the Icosahedron
            /// </summary>
            private readonly FloatParameter _parameterSize = new FloatParameter("Size", 1) {MinValue = 0};



            public IcosahedronMeshParameter()
                : base("Icosahedron")
            {
            }



            public static MeshEntity CreateIcosahedron(float size)
            {
                //http://csharphelper.com/blog/2015/12/platonic-solids-part-6-the-icosahedron/
                List<Face> faces = new List<Face>(12);

                // t1 and t3 are actually not used in calculations.
                float S = size;

                float t2 = (float) (Math.PI / 10f);
                float t4 = (float) (Math.PI / 5f);

                float R = (float) (S / 2.0 / Math.Sin(t4));
                float H = (float) (Math.Cos(t4) * R);
                float Cx = (float) (R * Math.Sin(t2));
                float Cz = (float) (R * Math.Cos(t2));
                float H1 = (float) Math.Sqrt(S * S - R * R);
                float H2 = (float) Math.Sqrt((H + R) * (H + R) - H * H);
                float Y2 = (float) ((H2 - H1) / 2.0);
                float Y1 = Y2 + H1;

                Vertex vA = new Vertex(new Vector3D(0, 0, Y1));
                Vertex vB = new Vertex(new Vector3D(0, R, Y2));
                Vertex vC = new Vertex(new Vector3D(Cz, Cx, Y2));
                Vertex vD = new Vertex(new Vector3D(S / 2, -H, Y2));

                Vertex vE = new Vertex(new Vector3D(-S / 2, -H, Y2));
                Vertex vF = new Vertex(new Vector3D(-Cz, Cx, Y2));
                Vertex vG = new Vertex(new Vector3D(0, -R, -Y2));
                Vertex vH = new Vertex(new Vector3D(-Cz, -Cx, -Y2));

                Vertex vI = new Vertex(new Vector3D(-S / 2, H, -Y2));
                Vertex vJ = new Vertex(new Vector3D(S / 2, H, -Y2));
                Vertex vK = new Vertex(new Vector3D(Cz, -Cx, -Y2));
                Vertex vL = new Vertex(new Vector3D(0, 0, -Y1));


                faces.Add(new Face(vB, vC, vA));
                faces.Add(new Face(vC, vD, vA));
                faces.Add(new Face(vD, vE, vA));
                faces.Add(new Face(vE, vF, vA));
                faces.Add(new Face(vF, vB, vA));

                faces.Add(new Face(vC, vB, vJ));
                faces.Add(new Face(vJ, vK, vC));
                faces.Add(new Face(vD, vC, vK));
                faces.Add(new Face(vK, vG, vD));
                faces.Add(new Face(vE, vD, vG));
                faces.Add(new Face(vG, vH, vE));
                faces.Add(new Face(vF, vE, vH));
                faces.Add(new Face(vH, vI, vF));
                faces.Add(new Face(vB, vF, vI));
                faces.Add(new Face(vI, vJ, vB));

                faces.Add(new Face(vK, vJ, vL));
                faces.Add(new Face(vG, vK, vL));
                faces.Add(new Face(vH, vG, vL));
                faces.Add(new Face(vI, vH, vL));
                faces.Add(new Face(vJ, vI, vL));

                return new MeshEntity(faces, BoxScope.Identity);
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateIcosahedron(_parameterSize.Value);
            }
        }

        #endregion

        #region Dodecahedron

        /// <summary>
        /// Creates an Dodecahedron.
        /// </summary>
        public class DodecahedronMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Side length of the Dodecahedron
            /// </summary>
            private readonly FloatParameter _parameterSize = new FloatParameter("Size", 1) {MinValue = 0};



            public DodecahedronMeshParameter()
                : base("Dodecahedron")
            {
            }



            public static MeshEntity CreateDodecahedron(float size)
            {
                //http://csharphelper.com/blog/2015/12/platonic-solids-part-7-the-dodecahedron/
                List<Face> faces = new List<Face>(20);

                // Value t1 is actually never used.
                float t4 = (float) (Math.PI / 5.0);
                float s = size;

                float t2 = (float) (Math.PI / 10.0);
                float t3 = (float) (3.0 * Math.PI / 10.0);

                float d1 = (float) (s / 2.0 / Math.Sin(t4));
                float d2 = (float) (d1 * Math.Cos(t4));
                float d3 = (float) (d1 * Math.Cos(t2));
                float d4 = (float) (d1 * Math.Sin(t2));
                float Fx =
                    (float) ((s * s - 2.0 * d3 * (2.0 * d3) -
                              (d1 * d1 - d3 * d3 - d4 * d4)) /
                             (2.0 * (d4 - d1)));
                float d5 = (float) Math.Sqrt(0.5 *
                                             (s * s + 2.0 * d3 * (2.0 * d3) -
                                              (d1 - Fx) * (d1 - Fx) -
                                              (d4 - Fx) * (d4 - Fx) - d3 * d3));
                float Fy = (float) ((Fx * Fx - d1 * d1 - d5 * d5) / (2.0 * d5));
                float Ay = d5 + Fy;


                Vertex vA = new Vertex(new Vector3D(0, d1, Ay));
                Vertex vB = new Vertex(new Vector3D(d3, d4, Ay));
                Vertex vC = new Vertex(new Vector3D(s / 2, -d2, Ay));
                Vertex vD = new Vertex(new Vector3D(-s / 2, -d2, Ay));

                Vertex vE = new Vertex(new Vector3D(-d3, d4, Ay));
                Vertex vF = new Vertex(new Vector3D(0, Fx, Fy));
                Vertex vG = new Vertex(new Vector3D((float) (Fx * Math.Cos(t2)), (float) (Fx * Math.Sin(t2)), Fy));
                Vertex vH = new Vertex(new Vector3D((float) (Fx * Math.Cos(t3)), (float) (-Fx * Math.Sin(t3)), Fy));

                Vertex vI = new Vertex(new Vector3D((float) (-Fx * Math.Cos(t3)), (float) (-Fx * Math.Sin(t3)), Fy));
                Vertex vJ = new Vertex(new Vector3D((float) (-Fx * Math.Cos(t2)), (float) (Fx * Math.Sin(t2)), Fy));
                Vertex vK = new Vertex(new Vector3D((float) (Fx * Math.Cos(t3)), (float) (Fx * Math.Sin(t3)), -Fy));
                Vertex vL = new Vertex(new Vector3D((float) (Fx * Math.Cos(t2)), (float) (-Fx * Math.Sin(t2)), -Fy));

                Vertex vM = new Vertex(new Vector3D(0, -Fx, -Fy));
                Vertex vN = new Vertex(new Vector3D((float) (-Fx * Math.Cos(t2)), (float) (-Fx * Math.Sin(t2)), -Fy));
                Vertex vO = new Vertex(new Vector3D((float) (-Fx * Math.Cos(t3)), (float) (Fx * Math.Sin(t3)), -Fy));
                Vertex vP = new Vertex(new Vector3D(s / 2, d2, -Ay));

                Vertex vQ = new Vertex(new Vector3D(d3, -d4, -Ay));
                Vertex vR = new Vertex(new Vector3D(0, -d1, -Ay));
                Vertex vS = new Vertex(new Vector3D(-d3, -d4, -Ay));
                Vertex vT = new Vertex(new Vector3D(-s / 2, d2, -Ay));

                faces.Add(new Face(vR, vS, vN, vI, vM));
                faces.Add(new Face(vH, vC, vB, vG, vL));
                faces.Add(new Face(vG, vB, vA, vF, vK));
                faces.Add(new Face(vP, vQ, vL, vG, vK));
                faces.Add(new Face(vT, vO, vJ, vN, vS));
                faces.Add(new Face(vE, vD, vI, vN, vJ));
                faces.Add(new Face(vF, vO, vT, vP, vK));
                faces.Add(new Face(vS, vR, vQ, vP, vT));
                faces.Add(new Face(vH, vL, vQ, vR, vM));
                faces.Add(new Face(vI, vD, vC, vH, vM));
                faces.Add(new Face(vF, vA, vE, vJ, vO));
                faces.Add(new Face(vE, vA, vB, vC, vD));

                var mesh = new MeshEntity(faces, BoxScope.Identity);
                return mesh;
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateDodecahedron(_parameterSize.Value);
            }
        }

        #endregion

        #region Sphere

        /// <summary>
        /// Creates a sphere.
        /// </summary>
        public class SphereMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Number of horizontal segments (parallels) in which the sphere will be divided.
            /// </summary>
            private readonly IntParameter _horizontalSegmentsParameter = new IntParameter("Horizontal Segments", 10) {MinValue = 1};

            /// <summary>
            /// Radius of the sphere.
            /// </summary>
            private readonly FloatParameter _radiusParameter = new FloatParameter("Radius", 1);

            /// <summary>
            /// Number of vertical segments (meridians) in which the sphere will be divided.
            /// </summary>
            private readonly IntParameter _verticalSegmentsParameter = new IntParameter("Vertical Segments", 10) {MinValue = 2};



            public SphereMeshParameter()
                : base("Sphere")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateSphere(_radiusParameter.Value, _horizontalSegmentsParameter.Value, _verticalSegmentsParameter.Value);
            }



            public static MeshEntity CreateSphere(float radius, int horizontalSegments, int verticalSegments)
            {
                int horizontalSegs = horizontalSegments;
                int verticalSegs = verticalSegments;

                float horizontalAngle = (float) (Math.PI * 2) / horizontalSegs;
                float verticalAngle = (float) Math.PI / verticalSegs;

                //these are the points at the extremes
                Vertex topPoint = new Vertex(new Vector3D(0, 0, radius));
                Vertex bottomPoint = new Vertex(new Vector3D(0, 0, -radius));

                List<Vertex> vertices = new List<Vertex>();

                List<Face> faces = new List<Face>();

                //before moving on, some optimization to spare some cos and sin calculations
                float[,] horizontalPrecalculations = new float[horizontalSegs, 2];
                for (int i = 0; i < horizontalSegs; i++)
                {
                    horizontalPrecalculations[i, 0] = (float) Math.Sin(Math.PI * 2 - horizontalAngle * i);
                    horizontalPrecalculations[i, 1] = (float) Math.Cos(Math.PI * 2 - horizontalAngle * i);
                }

                //before moving on, some optimization to spare some cos and sin calculations
                float[,] verticalPrecalculations = new float[verticalSegs, 2];
                for (int j = 0; j < verticalSegs; j++)
                {
                    verticalPrecalculations[j, 0] = (float) Math.Sin(verticalAngle * j);
                    verticalPrecalculations[j, 1] = (float) Math.Cos(verticalAngle * j);
                }

                //The reason for <= is that the last will connect to the bottomPoint
                for (int j = 1; j <= verticalSegs; j++)
                    //for the first circle, connect to the top point
                    if (j == 1)
                    {
                        //for (int i = 0; i < horizontalSegs; i++)
                        //    vertices.Add(new Vertex(new Vector3D((float)Math.Cos(Math.PI * 2 - horizontalAngle * i) * radius * (float)Math.Cos(verticalAngle * j), (float)Math.Sin(Math.PI * 2 - horizontalAngle * i) * radius * (float)Math.Abs(Math.Cos(verticalAngle * j)), radius * (float)Math.Cos(verticalAngle * j))));
                        //vertices = GetSphereLine(horizontalSegs, horizontalAngle, verticalAngle, j, radius);
                        vertices = GetSphereLine(horizontalSegs, j, horizontalPrecalculations, verticalPrecalculations, radius);

                        //foreach vertex in the first circle of vertices, connect to the top one
                        for (int k = 0; k < vertices.Count - 1; k++) faces.Add(new Face(vertices[k], vertices[k + 1], topPoint));
                    }
                    else if (j == verticalSegs)
                    {
                        //foreach vertex in the first circle of vertices, connect to the top one
                        for (int k = 0; k < vertices.Count - 1; k++) faces.Add(new Face(vertices[k + 1], vertices[k], bottomPoint));
                    }
                    else
                    {
                        //List<Vertex> newVertices = GetSphereLine(horizontalSegs, horizontalAngle, verticalAngle, j, radius);
                        List<Vertex> newVertices = GetSphereLine(horizontalSegs, j, horizontalPrecalculations, verticalPrecalculations, radius);

                        //now let's connect the previous and the current vertex list
                        for (int k = 0; k < vertices.Count - 1; k++) faces.Add(new Face(vertices[k + 1], vertices[k], newVertices[k], newVertices[k + 1]));

                        vertices = newVertices;
                    }

                foreach (Face face in faces)
                foreach (Vertex vertex in face.Vertices)
                    vertex.Normal = vertex.Position;

                MeshEntity meshEntity = new MeshEntity(faces, BoxScope.Identity);

                //ResetToOriginProcedure.ResetToOrigin(MeshEntity,true);
                //TranslateShapeProcedure.Translate(MeshEntity, new Vector3D(radius, radius,radius), false);

                return meshEntity;
            }



            private static List<Vertex> GetSphereLine(int horizontalSegs, int j, float[,] horizontalPrecalculations, float[,] verticalPrecalculations, float radius)
            {
                List<Vertex> vertices = new List<Vertex>();

                for (int i = 0; i < horizontalSegs; i++) vertices.Add(new Vertex(new Vector3D(horizontalPrecalculations[i, 1] * radius * verticalPrecalculations[j, 0], horizontalPrecalculations[i, 0] * radius * verticalPrecalculations[j, 0], radius * verticalPrecalculations[j, 1])));

                //"close" the list
                vertices.Add(vertices[0]);

                return vertices;
            }
        }

        #endregion

        #region Cylinder

        /// <summary>
        /// Creates a cylinder.
        /// </summary>
        public class CylinderMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Height of the cylinder.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 1);

            /// <summary>
            /// Radius of the cylinder.
            /// </summary>
            private readonly FloatParameter _radiusParameter = new FloatParameter("Radius", 1);

            /// <summary>
            /// Number of horizontal segments (meridians) of the cylinder.
            /// </summary>
            private readonly IntParameter _segmentsParameter = new IntParameter("Segments", 10);



            public CylinderMeshParameter()
                : base("Cylinder")
            {
            }



            public static MeshEntity CreateCylinder(float radius, int segments, float height)
            {
                MeshEntity meshEntity = CircleMeshParameter.CreateCircle(radius, segments);

                //ExtrudeProcedure.Extrude(meshEntity, height, true);

                return meshEntity.Extrude(height, true);
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateCylinder(_radiusParameter.Value, _segmentsParameter.Value, _heightParameter.Value);
            }
        }

        #endregion

        #region Cone

        /// <summary>
        /// Creates a cone.
        /// </summary>
        public class ConeMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Height of the cone.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 1) {MinValue = 0};

            /// <summary>
            /// Radius of the cone.
            /// </summary>
            private readonly FloatParameter _radiusParameter = new FloatParameter("Radius", 1) {MinValue = 0};

            /// <summary>
            /// Number of horizontal segments (meridians) of the cone.
            /// </summary>
            private readonly IntParameter _segmentsParameter = new IntParameter("Segments", 10) {MinValue = 3};



            public ConeMeshParameter()
                : base("Cone")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                MeshEntity meshEntity = CircleMeshParameter.CreateCircle(_radiusParameter.Value, _segmentsParameter.Value);

                return meshEntity.Pyramidize(_heightParameter.Value, true);
            }
        }

        #endregion

        #region Face

        /// <summary>
        /// Creates a face from a set of coordinates.
        /// </summary>
        public class FaceMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Points defining the boundary of the face. At least 3 points are required and the first and last point should differ.
            /// </summary>
            private readonly ListParameter<Vector3DParameter> _boundaryParameter = new ListParameter<Vector3DParameter>("Boundary", () => new Vector3DParameter("Point", new Vector3D()) {Description = "Point of the boundary"});



            public FaceMeshParameter()
                : base("Face")
            {
                /*Vector3DParameter v1 = (Vector3DParameter)_boundaryParameter.Add("Point");
                Vector3DParameter v2 = (Vector3DParameter)_boundaryParameter.Add("Point");
                v2.Value = new Vector3D(0, 3, 0);

                Vector3DParameter v3 = (Vector3DParameter)_boundaryParameter.Add("Point");
                v3.Value = new Vector3D(3, 0, 0);*/
            }



            public static MeshEntity CreateFace(IEnumerable<Vector3D> vectors)
            {
                return new MeshEntity(new Face(vectors));
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateFace(_boundaryParameter.Items.Select(val => val.Value));
            }
        }

        #endregion

        #region Rectangle

        /// <summary>
        /// Creates a rectangle.
        /// </summary>
        public class RectangleMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the rectangle.
            /// </summary>
            private readonly FloatParameter _widthParameter = new FloatParameter("Width", 1) {MinValue = 0};


            /// <summary>
            /// Height (size in Y) of the rectangle.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 1) {MinValue = 0};



            public RectangleMeshParameter()
                : base("Rectangle")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateRectangle(_widthParameter.Value, _heightParameter.Value);
            }



            public static MeshEntity CreateRectangle(float width, float height)
            {
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;

                Face face = new Face(new Vector3D(halfWidth, -halfHeight), new Vector3D(-halfWidth, -halfHeight),
                    new Vector3D(-halfWidth, halfHeight), new Vector3D(halfWidth, halfHeight));
                return new MeshEntity(face);
            }
        }

        #endregion

        #region LShape

        /// <summary>
        /// Creates an L-shape.
        /// </summary>
        public class LShapeMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the shape.
            /// </summary>
            private readonly FloatParameter _widthParameter = new FloatParameter("Width", 5) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 5) {MinValue = 0};

            /// <summary>
            /// Width (size in X) of the shape body. Should be smaller than the Width.
            /// </summary>
            private readonly FloatParameter _internalWidthParameter = new FloatParameter("Body Width", 2) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape body. Should be smaller than the Height.
            /// </summary>
            private readonly FloatParameter _internalHeightParameter = new FloatParameter("Body Height", 2) {MinValue = 0};



            public LShapeMeshParameter()
                : base("L-Shape")
            {
            }



            public static MeshEntity CreateLShape(float width, float height, float internalWidth, float internalHeight)
            {
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;

                Face face = new Face(
                    new Vector3D(halfWidth, -halfHeight),
                    new Vector3D(-halfWidth, -halfHeight),
                    new Vector3D(-halfWidth, halfHeight),
                    new Vector3D(-halfWidth + internalWidth, halfHeight),
                    new Vector3D(-halfWidth + internalWidth, -halfHeight + internalHeight),
                    new Vector3D(halfWidth, -halfHeight + internalHeight));

                return new MeshEntity(face);
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateLShape(_widthParameter.Value, _heightParameter.Value, _internalWidthParameter.Value, _internalHeightParameter.Value);
            }
        }

        #endregion

        #region TShape

        /// <summary>
        /// Creates an T-shape.
        /// </summary>
        public class TShapeMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the shape.
            /// </summary>
            private readonly FloatParameter _widthParameter = new FloatParameter("Width", 5) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 5) {MinValue = 0};

            /// <summary>
            /// Width (size in X) of the shape body. Should be smaller than the Width.
            /// </summary>
            private readonly FloatParameter _internalWidthParameter = new FloatParameter("Body Width", 2) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape body. Should be smaller than the Height.
            /// </summary>
            private readonly FloatParameter _internalHeightParameter = new FloatParameter("Body Height", 2) {MinValue = 0};



            public TShapeMeshParameter()
                : base("T-Shape")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateTShape(_widthParameter.Value, _heightParameter.Value, _internalWidthParameter.Value, _internalHeightParameter.Value);
            }



            public static MeshEntity CreateTShape(float width, float height, float internalWidth, float internalHeight)
            {
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;
                float sideInternalWidth = (width - internalWidth) / 2;

                Face face = new Face(
                    new Vector3D(halfWidth - sideInternalWidth, -halfHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, -halfHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, halfHeight - internalHeight),
                    new Vector3D(-halfWidth, halfHeight - internalHeight),
                    new Vector3D(-halfWidth, halfHeight),
                    new Vector3D(halfWidth, halfHeight),
                    new Vector3D(halfWidth, halfHeight - internalHeight),
                    new Vector3D(halfWidth - sideInternalWidth, halfHeight - internalHeight));

                return new MeshEntity(face);
            }
        }

        #endregion

        #region UShape

        /// <summary>
        /// Creates an U-shape.
        /// </summary>
        public class UShapeMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the shape.
            /// </summary>
            private readonly FloatParameter _widthParameter = new FloatParameter("Width", 5) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 5) {MinValue = 0};

            /// <summary>
            /// Width (size in X) of the shape body. Should be smaller than the Width.
            /// </summary>
            private readonly FloatParameter _internalWidthParameter = new FloatParameter("Body Width", 2) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape body. Should be smaller than the Height.
            /// </summary>
            private readonly FloatParameter _internalHeightParameter = new FloatParameter("Body Height", 2) {MinValue = 0};



            public UShapeMeshParameter()
                : base("U-Shape")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateUShape(_widthParameter.Value, _heightParameter.Value, _internalWidthParameter.Value, _internalHeightParameter.Value);
            }



            public static MeshEntity CreateUShape(float width, float height, float internalWidth, float internalHeight)
            {
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;


                Face face = new Face(
                    new Vector3D(halfWidth, -halfHeight),
                    new Vector3D(-halfWidth, -halfHeight),
                    new Vector3D(-halfWidth, halfHeight),
                    new Vector3D(-halfWidth + internalWidth, halfHeight),
                    new Vector3D(-halfWidth + internalWidth, -halfHeight + internalHeight),
                    new Vector3D(halfWidth - internalWidth, -halfHeight + internalHeight),
                    new Vector3D(halfWidth - internalWidth, halfHeight),
                    new Vector3D(halfWidth, halfHeight));

                return new MeshEntity(face);
            }
        }

        #endregion

        #region ZShape

        /// <summary>
        /// Creates an Z-shape.
        /// </summary>
        public class ZShapeMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the shape.
            /// </summary>
            private readonly FloatParameter _widthParameter = new FloatParameter("Width", 5) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 5) {MinValue = 0};

            /// <summary>
            /// Width (size in X) of the shape body. Should be smaller than the Width.
            /// </summary>
            private readonly FloatParameter _internalWidthParameter = new FloatParameter("Body Width", 3) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape body. Should be smaller than the Height.
            /// </summary>
            private readonly FloatParameter _internalHeightParameter = new FloatParameter("Body Height", 3) {MinValue = 0};



            public ZShapeMeshParameter()
                : base("Z-Shape")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateZShape(_widthParameter.Value, _heightParameter.Value, _internalWidthParameter.Value, _internalHeightParameter.Value);
            }



            public static MeshEntity CreateZShape(float width, float height, float internalWidth, float internalHeight)
            {
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;
                float sideInternalWidth = (width - internalWidth) / 2;
                float sideInternalHeight = (width - internalHeight) / 2;

                Face face = new Face(
                    new Vector3D(halfWidth, -halfHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, -halfHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, -halfHeight + sideInternalHeight),
                    new Vector3D(-halfWidth, -halfHeight + sideInternalHeight),
                    new Vector3D(-halfWidth, halfHeight),
                    new Vector3D(halfWidth - sideInternalWidth, halfHeight),
                    new Vector3D(halfWidth - sideInternalWidth, halfHeight - sideInternalHeight),
                    new Vector3D(halfWidth, halfHeight - sideInternalHeight));

                return new MeshEntity(face);
            }
        }

        #endregion

        #region Cross

        /// <summary>
        /// Creates an cross shape.
        /// </summary>
        public class CrossMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Width (size in X) of the shape.
            /// </summary>
            private readonly FloatParameter _widthParameter = new FloatParameter("Width", 5) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Height", 5) {MinValue = 0};

            /// <summary>
            /// Width (size in X) of the shape body. Should be smaller than the Width.
            /// </summary>
            private readonly FloatParameter _internalWidthParameter = new FloatParameter("Body Width", 3) {MinValue = 0};

            /// <summary>
            /// Height (size in Y) of the shape body. Should be smaller than the Height.
            /// </summary>
            private readonly FloatParameter _internalHeightParameter = new FloatParameter("Body Height", 3) {MinValue = 0};



            public CrossMeshParameter()
                : base("Cross")
            {
            }



            public static MeshEntity CreateCrossShape(float width, float height, float internalWidth, float internalHeight)
            {
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;
                float sideInternalWidth = (width - internalWidth) / 2;
                float sideInternalHeight = (width - internalHeight) / 2;

                Face face = new Face(
                    new Vector3D(halfWidth - sideInternalWidth, -halfHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, -halfHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, -halfHeight + sideInternalHeight),
                    new Vector3D(-halfWidth, -halfHeight + sideInternalHeight),
                    new Vector3D(-halfWidth, halfHeight - sideInternalHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, halfHeight - sideInternalHeight),
                    new Vector3D(-halfWidth + sideInternalWidth, halfHeight),
                    new Vector3D(halfWidth - sideInternalWidth, halfHeight),
                    new Vector3D(halfWidth - sideInternalWidth, halfHeight - sideInternalHeight),
                    new Vector3D(halfWidth, halfHeight - sideInternalHeight),
                    new Vector3D(halfWidth, -halfHeight + sideInternalHeight),
                    new Vector3D(halfWidth - sideInternalWidth, -halfHeight + sideInternalHeight));

                return new MeshEntity(face);
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateCrossShape(_widthParameter.Value, _heightParameter.Value, _internalWidthParameter.Value, _internalHeightParameter.Value);
            }
        }

        #endregion

        #region Circle

        /// <summary>
        /// Creates a circular shape.
        /// </summary>
        public class CircleMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Radius of the circle.
            /// </summary>
            private readonly FloatParameter _radiusParameter = new FloatParameter("Radius", 1) {MinValue = 0};

            /// <summary>
            /// Number of segments of the circle.
            /// </summary>
            private readonly IntParameter _segmentsParameter = new IntParameter("Segments", 10) {MinValue = 3};



            public CircleMeshParameter()
                : base("Circle")
            {
            }



            public static MeshEntity CreateCircle(float radius, int segments)
            {
                float horizontalAngle = (float) (Math.PI * 2) / segments;

                List<Vector3D> points = new List<Vector3D>();

                for (int i = 0; i < segments; i++)
                {
                    float y = (float) Math.Sin(Math.PI * 2 - horizontalAngle * i) * radius;
                    float x = (float) Math.Cos(Math.PI * 2 - horizontalAngle * i) * radius;

                    points.Add(new Vector3D(x, y, 0));
                }

                MeshEntity meshEntity = new MeshEntity(new Face(points));
                meshEntity.BoxScope = new BoxScope(meshEntity.BoundingBox);

                return meshEntity;
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateCircle(_radiusParameter.Value, _segmentsParameter.Value);
            }
        }

        #endregion

        #region Star

        /// <summary>
        /// Creates a star shape.
        /// </summary>
        public class StarMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Number of segments of the star.
            /// </summary>
            private readonly IntParameter _pointsParameter = new IntParameter("Points", 5) {MinValue = 3};

            /// <summary>
            /// Radius of the first layer of points.
            /// </summary>
            private readonly FloatParameter _radius1Parameter = new FloatParameter("Radius 1", 2) {MinValue = 0};

            /// <summary>
            /// Radius of the second layer of points.
            /// </summary>
            private readonly FloatParameter _radius2Parameter = new FloatParameter("Radius 2", 1) {MinValue = 0};



            public StarMeshParameter()
                : base("Star")
            {
            }



            public static MeshEntity CreateCircle(float radius1, float radius2, int pointNumber)
            {
                int segments = pointNumber * 2;
                float horizontalAngle = (float) (Math.PI * 2) / segments;

                List<Vector3D> points = new List<Vector3D>();

                for (int i = 0; i < segments; i++)
                {
                    var actualRadius = i % 2 == 0 ? radius1 : radius2;

                    float y = (float) Math.Cos(horizontalAngle * i) * actualRadius;
                    float x = (float) Math.Sin(horizontalAngle * i) * actualRadius;

                    points.Add(new Vector3D(x, y, 0));
                }

                MeshEntity meshEntity = new MeshEntity(new Face(points));
                meshEntity.BoxScope = new BoxScope(meshEntity.BoundingBox);

                return meshEntity;
            }



            protected internal override MeshEntity CreateMesh()
            {
                return CreateCircle(_radius1Parameter.Value, _radius2Parameter.Value, _pointsParameter.Value);
            }
        }

        #endregion

        #region Text

        /// <summary>
        /// Creates a set of faces with the shape of a given input text. 
        /// </summary>
        public class TextMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Text Content.
            /// </summary>
            private readonly StringParameter _textParameter = new StringParameter("Text", "Sceelix");

            /// <summary>
            /// Font of the text.
            /// </summary>
            private readonly ChoiceParameter _fontParameter = new ChoiceParameter("Font", "Arial") {Choices = FontFamily.Families.Select(val => val.Name).ToArray()};

            /// <summary>
            /// Size of the text.
            /// </summary>
            private readonly IntParameter _sizeParameter = new IntParameter("Size", 10) {MinValue = 0};

            /// <summary>
            /// Font style.
            /// </summary>
            private readonly EnumChoiceParameter<FontStyle> _styleParameter = new EnumChoiceParameter<FontStyle>("Style", FontStyle.Regular);

            /// <summary>
            /// Stores the index of character in each face. The actual caracter can then be obtained in an expression from the original text.
            /// </summary>
            private readonly AttributeParameter<int> _attributeCharacter = new AttributeParameter<int>("Index", AttributeAccess.Write);



            public TextMeshParameter()
                : base("Text")
            {
            }



            private static void AddFaceOrHole(List<Face> faces, List<Vector3D> pointList)
            {
                Face face = new Face(pointList.ToArray());

                if (face.Normal.Equals(-Vector3D.ZVector))
                    faces.Add(face);
                else
                    faces.Last().AddHole(pointList.Select(val => new Vertex(val)));
            }



            protected internal override MeshEntity CreateMesh()
            {
                var meshEntity = CreateText(_textParameter.Value, _sizeParameter.Value, _styleParameter.Value, _fontParameter.Value, _attributeCharacter);

                //if (_attributeCharacter.IsMapped)
                //    StoreCharacterInformation(meshEntity);

                return meshEntity;
            }



            public static MeshEntity CreateText(string text, int size, FontStyle style, string font, AttributeParameter<int> attributeCharacter = null)
            {
                List<Face> allFaces = new List<Face>();

                float startingX = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    List<Face> faces = new List<Face>();
                    List<Vector3D> pointList = new List<Vector3D>();

                    FontFamily fontFamily = new FontFamily(font);
                    GraphicsPath path = new GraphicsPath();

                    path.AddString(text[i].ToString(), fontFamily, (int) style, size, new Point((int) startingX, 0), StringFormat.GenericDefault);

                    for (int index = 0; index < path.PointCount; index++)
                    {
                        PointF pathPoint = path.PathPoints[index];
                        byte pathType = path.PathTypes[index];

                        if (pathType == 0 && pointList.Count > 0)
                        {
                            AddFaceOrHole(faces, pointList);

                            pointList.Clear();
                        }

                        pointList.Add(new Vector3D(pathPoint.X, pathPoint.Y, 0));
                    }

                    if (pointList.Count > 0)
                        AddFaceOrHole(faces, pointList);

                    var boundingRectangle = new BoundingRectangle(faces.SelectMany(x => x.AllVertices).Select(x => x.Position.ToVector2D()));
                    startingX = boundingRectangle.Max.X;

                    if (attributeCharacter != null)
                        foreach (Face face in faces)
                            attributeCharacter[face] = i;


                    allFaces.AddRange(faces);
                }


                MeshEntity meshEntity = new MeshEntity(allFaces);

                //by default, the letters are facing down (not quite inverted), so we have to rotate it back
                meshEntity.Rotate(180, Vector3D.XVector, Vector3D.Zero);
                //RotateShapeProcedure.Rotate(meshEntity, Vector3D.XVector, Vector3D.Zero, (float)MathHelper.Pi, false);

                //resets the scope
                meshEntity.BoxScope = BoxScope.Identity;

                return meshEntity;
            }
        }

        #endregion

        #region Random Face

        /// <summary>
        /// Creates a random looking face.
        /// </summary>
        public class RandomFaceMeshParameter : PrimitiveMeshParameter
        {
            /// <summary>
            /// Seed of the random generator.
            /// </summary>
            private readonly IntParameter _parameterSeed = new IntParameter("Seed", 0);

            /// <summary>
            /// Size/scale of the face.
            /// </summary>
            private readonly FloatParameter _parameterSize = new FloatParameter("Size", 10) {MinValue = 0};

            /// <summary>
            /// Number of vertices of the face.
            /// </summary>
            private readonly IntParameter _parameterVertexCount = new IntParameter("Number of vertices", 10) {MinValue = 4};



            public RandomFaceMeshParameter()
                : base("Random Face")
            {
            }



            protected internal override MeshEntity CreateMesh()
            {
                Random random = new Random(_parameterSeed.Value);

                if (_parameterVertexCount.Value < 4)
                    throw new ArgumentException("The number of vertices must be greater or equals 4");

                //create a list of inverval values
                List<float> intervals = new List<float>(_parameterVertexCount.Value);
                for (int i = 0; i < _parameterVertexCount.Value; i++)
                {
                    float value;
                    do
                    {
                        value = random.Float(0, _parameterSize.Value); //WRONG!
                    } while (intervals.Any(interval => Math.Abs(interval - value) < Vector3D.Precision));

                    intervals.Add(value);
                }

                List<float> orderedIntervals = intervals.OrderBy(val => val).ToList();
                List<Vector3D> upperVertices = new List<Vector3D>();
                List<Vector3D> lowerVertices = new List<Vector3D>();

                Vector3D minValue = new Vector3D(orderedIntervals[0], random.Float(-_parameterSize.Value, _parameterSize.Value));
                Vector3D maxValue = new Vector3D(orderedIntervals.Last(), random.Float(-_parameterSize.Value, _parameterSize.Value));

                upperVertices.Add(minValue);

                //float length = (maxValue - minValue).Length;
                //Line3D line = new Line3D(maxValue - minValue, minValue);
                Vector3D lineDirection = (maxValue - minValue).Normalize();
                Vector3D updirection = Vector3D.ZVector.Cross(lineDirection).Normalize();

                for (int i = 1; i < orderedIntervals.Count - 1; i++)
                {
                    float x = orderedIntervals[i];
                    float y = random.Float(-_parameterSize.Value, _parameterSize.Value);
                    Vector3D vector3D = minValue + lineDirection * (x - orderedIntervals[0]) + updirection * y;

                    if (y >= 0)
                        upperVertices.Add(vector3D);
                    else
                        lowerVertices.Add(vector3D);
                }


/*foreach (float orderedInterval in orderedIntervals)
                                {
                                    float height = _random.Value.Float(-_size.Value, _size.Value);
                                    if (height >= 0)
                                    {
                                        upperVertices.Add(new Vector3D(orderedInterval, height, 0));
                                    }
                                    else
                                    {
                                        lowerVertices.Add(new Vector3D(orderedInterval, height, 0));
                                    }
                                }*/
                upperVertices.Add(maxValue);
                lowerVertices.Reverse();
                upperVertices.AddRange(lowerVertices);

                return new MeshEntity(new Face(upperVertices));
            }
        }

        #endregion
    }
}