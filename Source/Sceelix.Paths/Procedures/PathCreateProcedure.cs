using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Handles;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;
using Sceelix.Paths.Algorithms;
using Sceelix.Paths.Data;
using Sceelix.Paths.Handles;
using Edge = Sceelix.Meshes.Data.Edge;

namespace Sceelix.Paths.Procedures
{
    /// <summary>
    /// Creates Paths of primitive patterns (grid, voronoi) or from other Entity types. 
    /// </summary>
    [Procedure("fe4221f3-991c-4161-bbae-58f5a60f8d42", Label = "Path Create", Category = "Path")]
    public class PathCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Path created according to the defined parameters and/or inputs.
        /// </summary>
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Output");

        /// <summary>
        /// Type of path pattern/method to create.
        /// </summary>
        private readonly SelectListParameter<PrimitivePathParameter> _parameterPrimitive =
            new SelectListParameter<PrimitivePathParameter>("Type", "Grid");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterPrimitive.SubParameterLabels);



        protected override void Run()
        {
            var operation = _parameterPrimitive.Items.FirstOrDefault();
            if (operation != null) _output.Write(operation.CreatePath());
        }



        #region Abstract Parameter

        public abstract class PrimitivePathParameter : CompoundParameter
        {
            protected PrimitivePathParameter(string label)
                : base(label)
            {
            }



            protected internal abstract PathEntity CreatePath();
        }

        #endregion

        #region Single

        /// <summary>
        /// Creates a path from a sequence of points.
        /// </summary>
        public class CreateSinglePathParameter : PrimitivePathParameter
        {
            /// <summary>
            /// List of points that define the path.
            /// </summary>
            public ListParameter Points = new ListParameter("Points",
                () => new Vector3DParameter("Point") {Description = "Coordinate of the point."});



            public CreateSinglePathParameter()
                : base("Single")
            {
            }



            protected internal override PathEntity CreatePath()
            {
                PathEntity pathEntity =
                    PathEntity.CreateSequence(Points.Items.OfType<Vector3DParameter>().Select(x => x.Value));

#if DEBUG

                var visualHandleManager = ProcedureEnvironment.GetService<IVisualHandleManager>();
                if (visualHandleManager.CreateVisualHandles)
                    visualHandleManager.AddVisualHandle(Points, new PathDrawHandle());
#endif

                return pathEntity;
            }
        }

        #endregion

        #region Grid

        /// <summary>
        /// Creates a path network with a grid shape.
        /// </summary>
        public class CreateGridPathParameter : PrimitivePathParameter
        {
            /// <summary>
            /// Number of points in the X direction.
            /// </summary>
            private readonly IntParameter _parameterColumns = new IntParameter("Number of Columns", 10) {MinValue = 0};


            /// <summary>
            /// Number of points in the Y direction.
            /// </summary>
            private readonly IntParameter _parameterRows = new IntParameter("Number of Rows", 10) {MinValue = 0};


            /// <summary>
            /// Spacing between column points (in X).
            /// </summary>
            private readonly IntParameter _parameterColumnWidth = new IntParameter("Column Width", 1) {MinValue = 0};

            /// <summary>
            /// Spacing between row points (in Y).
            /// </summary>
            private readonly IntParameter _parameterRowHeight = new IntParameter("Row Height", 1) {MinValue = 0};



            public CreateGridPathParameter()
                : base("Grid")
            {
            }



            protected internal override PathEntity CreatePath()
            {
                List<PathEdge> newEdges = new List<PathEdge>();
                PathVertex[,] vertices = new PathVertex[_parameterColumns.Value + 1, _parameterRows.Value + 1];

                for (int i = 0; i < _parameterColumns.Value + 1; i++)
                for (int j = 0; j < _parameterRows.Value + 1; j++)
                {
                    vertices[i, j] = new PathVertex(new Vector3D(i * _parameterColumnWidth.Value,
                        j * _parameterRowHeight.Value));

                    if (j > 0)
                    {
                        var streetEdge = i % 2 == 0
                            ? new PathEdge(vertices[i, j - 1], vertices[i, j])
                            : new PathEdge(vertices[i, j], vertices[i, j - 1]);

                        //streetEdge.AttachToVertices();
                        newEdges.Add(streetEdge);
                    }

                    if (i > 0)
                    {
                        var streetEdge = j % 2 == 0
                            ? new PathEdge(vertices[i, j], vertices[i - 1, j])
                            : new PathEdge(vertices[i - 1, j], vertices[i, j]);
                        //streetEdge.AttachToVertices();
                        newEdges.Add(streetEdge);
                    }
                }

                var network = new PathEntity(newEdges);
                return network;
            }
        }

        #endregion

        #region Voronoi

        /// <summary>
        /// Creates a path network with a cellular appearance using the Voronoi algorithm.
        /// </summary>
        public class CreateVoronoiPathParameter : PrimitivePathParameter
        {
            /// <summary>
            /// Height (size in Y) of the path network.
            /// </summary>
            private readonly IntParameter _heightParameter = new IntParameter("Height", 100);

            /// <summary>
            /// Width (size in X) of the path network.
            /// </summary>
            private readonly IntParameter _widthParameter = new IntParameter("Width", 100);

            /// <summary>
            /// Average spacing between the path intersections.
            /// </summary>
            private readonly FloatParameter _spacingParameter = new FloatParameter("Spacing", 10);

            /// <summary>
            /// Maximum offset of each path intersection.
            /// A value of 0 will result in a grid-like appearance. 
            /// </summary>
            private readonly FloatParameter _maxOffsetParameter = new FloatParameter("Max Offset", 5);

            /// <summary>
            /// Seed that defines the randomness of the path.
            /// </summary>
            private readonly IntParameter _seedParameter = new IntParameter("Seed", 0);



            public CreateVoronoiPathParameter()
                : base("Voronoi")
            {
            }



            protected internal override PathEntity CreatePath()
            {
                Voronoi voronoiGenerator = new Voronoi(0.1);

                Random random = new Random(_seedParameter.Value);

                List<double> doublesX = new List<double>();
                List<double> doublesY = new List<double>();

                float spacing = _spacingParameter.Value;
                float maxOffset = _maxOffsetParameter.Value;

                var numPointsWidth = _widthParameter.Value / spacing;
                var numPointsHeight = _heightParameter.Value / spacing;


                //create the two lists of doubles with the X and Y coordinates and add the desired offset.
                for (int i = 0; i <= numPointsWidth; i++)
                for (int j = 0; j <= numPointsHeight; j++)
                {
                    doublesX.Add(i * spacing + random.Double(-maxOffset, maxOffset));
                    doublesY.Add(j * spacing + random.Double(-maxOffset, maxOffset));
                }


                Dictionary<Vector3D, PathVertex> vertices = new Dictionary<Vector3D, PathVertex>();
                List<PathEdge> newEdges = new List<PathEdge>();

                var points = voronoiGenerator.GenerateVoronoi(doublesX.ToArray(), doublesY.ToArray(), 0,
                    _widthParameter.Value, 0, _heightParameter.Value);
                foreach (var graphEdge in points)
                {
                    Vector3D v1 = new Vector3D((float) graphEdge.x1, (float) graphEdge.y1);
                    Vector3D v2 = new Vector3D((float) graphEdge.x2, (float) graphEdge.y2);

                    PathVertex vertex1, vertex2;

                    if (!vertices.TryGetValue(v1, out vertex1))
                        vertices.Add(v1, vertex1 = new PathVertex(v1));

                    if (!vertices.TryGetValue(v2, out vertex2))
                        vertices.Add(v2, vertex2 = new PathVertex(v2));

                    //for some reason, the algorithm can cause this, so let's check
                    if (vertex1 != vertex2)
                    {
                        var newPathEdge = new PathEdge(vertex1, vertex2);
                        //newPathEdge.AttachToVertices();
                        newEdges.Add(newPathEdge);
                    }
                }

                return new PathEntity(newEdges);
            }
        }

        #endregion

        #region From Mesh

        /// <summary>
        /// Creates a path from the given mesh edges.
        /// </summary>
        public class CreateFromMeshParameter : PrimitivePathParameter
        {
            /// <summary>
            /// Mesh whose edges are to be extracted.
            /// </summary>
            private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");



            public CreateFromMeshParameter()
                : base("From Mesh")
            {
            }



            protected internal override PathEntity CreatePath()
            {
                MeshEntity meshEntity = _input.Read();

                Dictionary<Vertex, PathVertex> mapping = new Dictionary<Vertex, PathVertex>();
                foreach (Vertex vertex in meshEntity.FaceVerticesWithHoles)
                    mapping.Add(vertex, new PathVertex(vertex.Position));


                //int index = 0;
                List<PathEdge> edges = new List<PathEdge>();
                foreach (Face face in meshEntity)
                foreach (Edge faceEdge in face.AllEdges)
                {
                    PathEdge pathEdge = new PathEdge(mapping[faceEdge.V0], mapping[faceEdge.V1]);
                    //pathEdge.AttachToVertices();
                    edges.Add(pathEdge);
                }

                var pathEntity = new PathEntity(edges);
                meshEntity.Attributes.SetAttributesTo(pathEntity.Attributes);
                pathEntity.BoxScope = meshEntity.BoxScope;

                return pathEntity;
            }
        }

        #endregion
    }
}