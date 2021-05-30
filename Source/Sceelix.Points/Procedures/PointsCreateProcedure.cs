using System;
using System.Collections.Generic;
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
using Sceelix.Paths.Data;
using Sceelix.Points.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;

namespace Sceelix.Points.Procedures
{
    /// <summary>
    /// Creates points from primitive patterns (grid, random) or from other Entity types. 
    /// </summary>
    [Procedure("fbf27f2b-eee3-4bd6-85e0-f5d616912877", Label = "Points Create", Category = "Point")]
    public class PointsCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Points created according to the defined parameters and/or inputs.
        /// </summary>
        private readonly Output<PointEntity> _output = new Output<PointEntity>("Output");

        /// <summary>
        /// Type/style of points to create.
        /// </summary>
        private readonly SelectListParameter<PointCreateParameter> _parameterType = new SelectListParameter<PointCreateParameter>("Type", "Simple");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterType.SubParameterLabels);



        protected override void Run()
        {
            foreach (var parameter in _parameterType.Items)
                _output.Write(parameter.CreatePoints());
        }



        #region Abstract

        public abstract class PointCreateParameter : CompoundParameter
        {
            protected PointCreateParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IEnumerable<PointEntity> CreatePoints();
        }

        #endregion

        #region Simple

        public class SimplePointsParameter : PointCreateParameter
        {
            /// <summary>
            /// List of locations of the points.
            /// </summary>
            private readonly ListParameter _parameterPoints = new ListParameter("Points",
                () => new Vector3DParameter("Point") {Description = "Location of the point."});



            public SimplePointsParameter()
                : base("Simple")
            {
            }



            protected internal override IEnumerable<PointEntity> CreatePoints()
            {
                return _parameterPoints.Items.OfType<Vector3DParameter>().Select(x => new PointEntity(x.Value));
            }
        }

        #endregion


        #region Grid

        /// <summary>
        /// Creates a 3D grid of points.
        /// </summary>
        public class GridPointsParameter : PointCreateParameter
        {
            /// <summary>
            /// Number of points in the X direction.
            /// </summary>
            private readonly IntParameter _parameterColumns = new IntParameter("Columns", 10);

            /// <summary>
            /// Number of points in the Y direction.
            /// </summary>
            private readonly IntParameter _parameterRows = new IntParameter("Rows", 10);

            /// <summary>
            /// Number of points in the Z direction.
            /// </summary>
            private readonly IntParameter _parameterLayers = new IntParameter("Layers", 10);

            /// <summary>
            /// The spacing in the X,Y,Z dimensions between each point.
            /// </summary>
            private readonly Vector3DParameter _parameterSpacing = new Vector3DParameter("Spacing", new Vector3D(10, 10, 10));



            public GridPointsParameter()
                : base("Grid")
            {
            }



            protected internal override IEnumerable<PointEntity> CreatePoints()
            {
                List<PointEntity> pointEntities = new List<PointEntity>();

                var columns = _parameterColumns.Value;
                var rows = _parameterRows.Value;
                var layers = _parameterLayers.Value;

                var spacing = _parameterSpacing.Value;

                for (int i = 0; i < columns; i++)
                for (int j = 0; j < rows; j++)
                for (int k = 0; k < layers; k++)
                    pointEntities.Add(new PointEntity(new Vector3D(i, j, k) * spacing));

                return pointEntities;
            }
        }

        #endregion

        #region From Path

        /// <summary>
        /// Extracts points from path vertices. Shared vertices are not repeated.
        /// </summary>
        public class FromPathParameter : PointCreateParameter
        {
            /// <summary>
            /// The path from which the points are to be extracted from.
            /// </summary>
            private readonly SingleInput<PathEntity> _inputPath = new SingleInput<PathEntity>("Path");



            public FromPathParameter()
                : base("From Path")
            {
            }



            protected internal override IEnumerable<PointEntity> CreatePoints()
            {
                foreach (PathVertex pathVertex in _inputPath.Read().Vertices)
                {
                    var pointEntity = new PointEntity(pathVertex.Position);
                    pathVertex.Attributes.SetAttributesTo(pointEntity.Attributes);
                    yield return pointEntity;
                }
            }
        }

        #endregion

        #region From Mesh

        /// <summary>
        /// Extracts points from mesh vertices. Shared vertices are not repeated.
        /// </summary>
        public class FromMeshParameter : PointCreateParameter
        {
            /// <summary>
            /// The mesh from which the points are to be extracted from.
            /// </summary>
            private readonly SingleInput<MeshEntity> _inputMesh = new SingleInput<MeshEntity>("Mesh");



            public FromMeshParameter()
                : base("From Mesh")
            {
            }



            protected internal override IEnumerable<PointEntity> CreatePoints()
            {
                return _inputMesh.Read().FaceVerticesWithHoles.Select(x => new PointEntity(x.Position));
            }
        }

        #endregion

        #region From Surface

        /// <summary>
        /// Extracts points from surface vertices.
        /// </summary>
        public class FromSurfaceParameter : PointCreateParameter
        {
            /// <summary>
            /// The surface from which the points are to be extracted from.
            /// </summary>
            private readonly SingleInput<SurfaceEntity> _inputSurface = new SingleInput<SurfaceEntity>("Surface");

            /// <summary>
            /// Attribute to store the surface-relative coordinates (column/row) of the vertices that originated the point.
            /// </summary>
            private readonly AttributeParameter<Vector2D> _attributeIndex = new AttributeParameter<Vector2D>("Coordinates", AttributeAccess.Write);



            public FromSurfaceParameter()
                : base("From Surface")
            {
            }



            protected internal override IEnumerable<PointEntity> CreatePoints()
            {
                var surfaceEntity = _inputSurface.Read();
                var heightLayer = surfaceEntity.GetLayer<HeightLayer>();

                for (int i = 0; i < surfaceEntity.NumColumns; i++)
                for (int j = 0; j < surfaceEntity.NumRows; j++)
                {
                    var position = heightLayer.GetPosition(new Coordinate(i, j));
                    var entity = new PointEntity(position);

                    _attributeIndex[entity] = position.ToVector2D();

                    yield return entity;
                }
            }
        }

        #endregion

        #region Random

        public abstract class ScatterPointsParameter : CompoundParameter
        {
            protected ScatterPointsParameter(string label) : base(label)
            {
            }



            public abstract IEnumerable<PointEntity> CreatePoints(int seed);
        }

        #region Rectangle

        /// <summary>
        /// Scatters points within a 2D rectangular area.
        /// </summary>
        public class RectangleScatterPointsParameter : ScatterPointsParameter
        {
            /// <summary>
            /// The size of the area where to scatter the points.
            /// </summary>
            private readonly Vector2DParameter _parameterSize = new Vector2DParameter("Size", new Vector2D(10, 10));

            /// <summary>
            /// The (minimum) distance between the points. <br/>
            /// In practice, the value will vary between distance and 2 x distance. 
            /// </summary>
            private readonly FloatParameter _parameterMinDistance = new FloatParameter("Distance", 1) {MinValue = 0};



            public RectangleScatterPointsParameter()
                : base("Rectangle")
            {
            }



            public override IEnumerable<PointEntity> CreatePoints(int seed)
            {
                var locations = GeneratePoisson(seed, _parameterSize.Value, _parameterMinDistance.Value);
                foreach (Vector2D location in locations) yield return new PointEntity(new Vector3D(location));
            }



            public List<Vector2D> GeneratePoisson(int seed, Vector2D size, float minDistance, int pointsToTest = 30)
            {
                Random random = new Random(seed);

                var pointList = new List<Vector2D>();

                var cellSize = (float) (minDistance / Math.Sqrt(2));

                //spatial data structure where we will store the indication if there's room available
                var grid = new Vector2D?[(int) Math.Ceiling(size.X / cellSize), (int) Math.Ceiling(size.Y / cellSize)];

                var rectangle = new BoundingRectangle(0, 0, size.X, size.Y);

                //points currently on the loop
                var currentPoints = new Queue<Vector2D>();

                //the first point is random within the bounds
                var firstPoint = new Vector2D(random.Float(0, size.X), random.Float(0, size.Y));
                currentPoints.Enqueue(firstPoint);

                //store it in the grid, too
                var firstCoordinates = PositionToCoordinates(firstPoint, cellSize);
                grid[firstCoordinates.X, firstCoordinates.Y] = firstPoint;

                while (!currentPoints.IsEmpty())
                {
                    var currentPoint = currentPoints.Dequeue();
                    for (int i = 0; i < pointsToTest; i++)
                    {
                        Vector2D newPoint = GenerateRandomPointAround(random, currentPoint, minDistance);

                        //check that the point is in the image region
                        //and no points exists in the point's neighborhood
                        if (rectangle.Contains(newPoint) && !HasPointInNeighbourhood(grid, newPoint, minDistance, cellSize, size))
                        {
                            //update containers
                            currentPoints.Enqueue(newPoint);

                            //add the point to our list of results
                            pointList.Add(newPoint);

                            var coordinate = PositionToCoordinates(newPoint, cellSize);
                            grid[coordinate.X, coordinate.Y] = newPoint;
                        }
                    }
                }

                return pointList;
            }



            private Vector2D GenerateRandomPointAround(Random random, Vector2D point, float minDistance)
            {
                //non-uniform, favours points closer to the inner ring, leads to denser packings
                var r1 = random.NextDouble();
                var r2 = random.NextDouble();

                //random radius between mindist and 2 * mindist
                var radius = minDistance * (r1 + 1);

                //random angle
                var angle = 2 * Math.PI * r2;

                //the new point is generated around the point (x, y)
                var newX = point.X + radius * Math.Cos(angle);
                var newY = point.Y + radius * Math.Sin(angle);

                return new Vector2D((float) newX, (float) newY);
            }



            private bool HasPointInNeighbourhood(Vector2D?[,] grid, Vector2D point, float minDistance, float cellSize, Vector2D size)
            {
                var coordinate = PositionToCoordinates(point, cellSize);
                var cellDistance = (int) Math.Ceiling(minDistance / cellSize);

                int startX = Math.Max(coordinate.X - cellDistance, 0);
                int startY = Math.Max(coordinate.Y - cellDistance, 0);
                int endX = Math.Min(coordinate.X + cellDistance, grid.GetLength(0));
                var endY = Math.Min(coordinate.Y + cellDistance, grid.GetLength(1));


                //get the neighbourhood if the point in the grid
                for (int i = startX; i < endX; i++)
                for (int j = startY; j < endY; j++)
                {
                    var cell = grid[i, j];

                    //if there's a value in the cell
                    if (cell.HasValue)
                    {
                        var value = cell.Value;

                        //and that cell is actually closer than the allowed minimum...
                        if (value.DistanceTo(point) < minDistance)
                            return true;
                    }
                }

                return false;
            }



            private Coordinate PositionToCoordinates(Vector2D point, float cellSize)
            {
                return new Coordinate((int) (point.X / cellSize), (int) (point.Y / cellSize));
            }
        }

        #endregion

        #region Mesh

        /// <summary>
        /// Scatters points on a mesh surface.
        /// </summary>
        public class MeshScatterPointsParameter : ScatterPointsParameter
        {
            /// <summary>
            /// The mesh on which to randomly scatter the points.
            /// </summary>
            private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Mesh");

            /// <summary>
            /// The mesh on which the points were scattered.
            /// </summary>
            private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Mesh");

            /// <summary>
            /// The (minimum) distance between the points. <br/>
            /// In practice, the value will vary between distance and 2 x distance. 
            /// </summary>
            private readonly FloatParameter _parameterDistance = new FloatParameter("Distance", 1);



            public MeshScatterPointsParameter() : base("Mesh")
            {
            }



            private bool ContainsPoint(List<Vector2D> boundaryVertices, List<List<Vector2D>> holeBoundaries, Vector2D newPoint)
            {
                return ContainsPoint(boundaryVertices, newPoint) && !(holeBoundaries != null && holeBoundaries.Any(hole => ContainsPoint(hole, newPoint)));
            }



            public bool ContainsPoint(List<Vector2D> vertices, Vector2D point)
            {
                bool conclusion = false;

                for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
                    if (vertices[i].Y > point.Y != vertices[j].Y > point.Y &&
                        point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X)
                        conclusion = !conclusion;

                return conclusion;
            }



            public override IEnumerable<PointEntity> CreatePoints(int seed)
            {
                var mesh = _input.Read();
                Random random = new Random(seed);
                int pointsToTest = 30;
                float minDistance = _parameterDistance.Value;

                List<BoxScope> scopes = new List<BoxScope>();

                PointOctTree pointOctTree = new PointOctTree((int) Math.Ceiling(minDistance), 5);


                foreach (Face face in mesh.Faces)
                {
                    var planarScope = face.GetAlignedScope();
                    var boundaryVertices = face.Vertices.Select(x => planarScope.ToScopePosition(x.Position).ToVector2D()).ToList();
                    var holeBoundaries = !face.HasHoles ? null : face.Holes.Select(hole => hole.Select(x => planarScope.ToScopePosition(x.Position).ToVector2D()).ToList()).ToList();

                    var currentPoints = new Queue<Vector2D>();
                    currentPoints.Enqueue(boundaryVertices.First());
                    //scopes.Add(new BoxScope(planarScope, translation: planarScope.ToWorldPosition(new Vector3D(boundaryVertices.First()))));
                    while (!currentPoints.IsEmpty())
                    {
                        var currentPoint = currentPoints.Dequeue();
                        for (int i = 0; i < pointsToTest; i++)
                        {
                            Vector2D newPoint = GenerateRandomPointAround(random, currentPoint, minDistance);

                            //check that the point is in the image region
                            //and no points exists in the point's neighborhood
                            if (ContainsPoint(boundaryVertices, holeBoundaries, newPoint))
                            {
                                var newPoint3D = planarScope.ToWorldPosition(new Vector3D(newPoint));

                                if (!HasPointInNeighbourhood(pointOctTree, newPoint3D, minDistance))
                                {
                                    //update containers
                                    currentPoints.Enqueue(newPoint);

                                    //add the point to our list of results
                                    pointOctTree.AddItem(newPoint3D);
                                    //positions.Add(newPoint3D);

                                    //add the oriented scope
                                    scopes.Add(new BoxScope(planarScope, translation: newPoint3D));
                                }
                            }
                        }
                    }
                }

                _output.Write(mesh);

                return scopes.Select(x => new PointEntity(x));
            }



            private Vector2D GenerateRandomPointAround(Random random, Vector2D point, float minDistance)
            {
                //non-uniform, favours points closer to the inner ring, leads to denser packings
                var r1 = random.NextDouble();
                var r2 = random.NextDouble();

                //random radius between mindist and 2 * mindist
                var radius = minDistance * (r1 + 1);

                //random angle
                var angle = 2 * Math.PI * r2;

                //the new point is generated around the point (x, y)
                var newX = point.X + radius * Math.Cos(angle);
                var newY = point.Y + radius * Math.Sin(angle);

                return new Vector2D((float) newX, (float) newY);
            }



            private bool HasPointInNeighbourhood(PointOctTree pointOctTree, Vector3D newPoint, float minDistance)
            {
                return pointOctTree.GetItemsWithinRadius(newPoint, minDistance).Any(x => x.DistanceTo(newPoint) < minDistance);
            }
        }

        #endregion

        #region Surface

        #region SurfaceScatterDistance

        public abstract class SurfaceScatterDistanceParameter : CompoundParameter
        {
            protected SurfaceScatterDistanceParameter(string label) : base(label)
            {
            }



            public abstract IEnumerable<PointEntity> CreatePoints(int seed, SurfaceEntity surfaceEntity);
        }

        /// <summary>
        /// Sets a fixed distance between the points.
        /// </summary>
        public class SurfaceScatterFixedDistanceParameter : SurfaceScatterDistanceParameter
        {
            /// <summary>
            /// The (minimum) distance between the points. <br/>
            /// In practice, the value will vary between distance and 2 x distance. 
            /// </summary>
            private readonly FloatParameter _parameterDistance = new FloatParameter("Distance", 1);



            public SurfaceScatterFixedDistanceParameter()
                : base("Fixed")
            {
            }



            public override IEnumerable<PointEntity> CreatePoints(int seed, SurfaceEntity surfaceEntity)
            {
                Random random = new Random(seed);
                int pointsToTest = 30;
                float minDistance = _parameterDistance.Value;

                var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                var surfaceRectangle = surfaceEntity.BoundingRectangle;


                PointQuadTree pointQuadTree = new PointQuadTree((int) Math.Ceiling(minDistance), 5);

                var currentPoints = new Queue<Vector2D>();
                var firstPoint = new Vector2D(random.Float(surfaceRectangle.Min.X, surfaceRectangle.Max.X), random.Float(surfaceRectangle.Min.Y, surfaceRectangle.Max.Y));
                currentPoints.Enqueue(firstPoint);
                pointQuadTree.AddItem(firstPoint);

                while (!currentPoints.IsEmpty())
                {
                    var currentPoint = currentPoints.Dequeue();

                    //add the point to our list of results
                    var height = heightLayer.SafeGetHeight(currentPoint);
                    var newEntity = new PointEntity(new Vector3D(currentPoint, height));
                    yield return newEntity;


                    for (int i = 0; i < pointsToTest; i++)
                    {
                        Vector2D newPoint = GenerateRandomPointAround(random, currentPoint, minDistance);

                        //check that the point is in the image region
                        //and no points exists in the point's neighborhood
                        if (surfaceRectangle.Contains(newPoint))
                            if (!HasPointInNeighbourhood(pointQuadTree, newPoint, minDistance))
                            {
                                //update containers
                                currentPoints.Enqueue(newPoint);
                                pointQuadTree.AddItem(newPoint);
                            }
                    }
                }
            }



            private Vector2D GenerateRandomPointAround(Random random, Vector2D point, float minDistance)
            {
                //non-uniform, favours points closer to the inner ring, leads to denser packings
                var r1 = random.NextDouble();
                var r2 = random.NextDouble();

                //random radius between mindist and 2 * mindist
                var radius = minDistance * (r1 + 1);

                //random angle
                var angle = 2 * Math.PI * r2;

                //the new point is generated around the point (x, y)
                var newX = point.X + radius * Math.Cos(angle);
                var newY = point.Y + radius * Math.Sin(angle);

                return new Vector2D((float) newX, (float) newY);
            }



            private bool HasPointInNeighbourhood(PointQuadTree pointOctTree, Vector2D newPoint, float minDistance)
            {
                return pointOctTree.GetItemsWithinRadius(newPoint, minDistance).Any(x => x.DistanceTo(newPoint) < minDistance);
            }
        }


        /// <summary>
        /// Sets a variable distance between the points, based on the intensity of a chosen layer.
        /// The intensity values of the layer will be mapped to the generated point density.
        /// Layer values below a certain threshold will be filtered so as to define areas where no points are generated.
        /// </summary>
        public class SurfaceScatterLayerDistanceParameter : SurfaceScatterDistanceParameter
        {
            private const int PointsToTest = 30;

            /// <summary>
            /// Index of the layer which contains the intensity information. Should be a floating-point based layer, such as a height or blend layer.<br/>
            /// By default, the index 0 means the height layer.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Index", 0) {MinValue = 0};

            /// <summary>
            /// The layer value below which no points will be placed (in a 0-1 relative range). 
            /// This works as a mask, meaning that, for instance, in those areas where the layer value is below this value, no points will be placed.
            /// </summary>
            private readonly FloatParameter _parameterThreshold = new FloatParameter("Min. Threshold", 0.1f) {MinValue = 0, MaxValue = 1};

            /// <summary>
            /// The minimum distance between the generated points. This corresponds to the layer areas with highest values, creating therefore areas with higher point density.
            /// </summary>
            private readonly FloatParameter _parameterMinDistance = new FloatParameter("Min. Distance", 5) {MinValue = 0};

            /// <summary>
            /// The maximum distance between the generated points. This corresponds to the layer areas with lowest values, creating therefore areas with lower point density.
            /// </summary>
            private readonly FloatParameter _parameterMaxDistance = new FloatParameter("Max. Distance", 10) {MinValue = 0};

            /// <summary>
            /// The actual distance calculated for the point.
            /// </summary>
            private readonly AttributeParameter<float> _distanceAttribute = new AttributeParameter<float>("Distance", AttributeAccess.Write);



            public SurfaceScatterLayerDistanceParameter()
                : base("Layer")
            {
            }



            public override IEnumerable<PointEntity> CreatePoints(int seed, SurfaceEntity surface)
            {
                //check the inputs
                var surfaceLayers = surface.Layers.ToList();
                if (_parameterLayerIndex.Value >= surfaceLayers.Count)
                    throw new ArgumentException("The indicated layer index exceeds the amount of existing layers.");

                var floatLayer = surfaceLayers[_parameterLayerIndex.Value] as FloatLayer;
                if (floatLayer == null)
                    throw new ArgumentException("The indicated layer index does not correspond to a valid floating-point based layer.");

                var heightLayer = surface.GetLayer<HeightLayer>();

                var baseMinDistance = _parameterMinDistance.Value;
                var topMinDistance = _parameterMaxDistance.Value;

                Random random = new Random(seed);

                var cellSize = (float) (topMinDistance / Math.Sqrt(2));
                var cellDistance = (int) Math.Ceiling(topMinDistance / cellSize);

                var rectangle = surface.BoundingRectangle;


                //spatial data structure where we will store the indication if there's room available
                var grid = new List<PositionDistance>[(int) Math.Ceiling(rectangle.Width / cellSize), (int) Math.Ceiling(rectangle.Height / cellSize)];

                var minValue = floatLayer.MinValue;
                var threshold = _parameterThreshold.Value;
                var distanceRange = topMinDistance - baseMinDistance;
                var heightRange = floatLayer.MaxValue - minValue;

                Func<float, float> interFunc = value => baseMinDistance + distanceRange * (1 - (value - minValue) / heightRange);
                Func<float, bool> maskCheckFunc = value => (value - minValue) / heightRange > threshold;
                //GetInterpolationFunction(floatLayer.MinValue, floatLayer.MaxValue, _parameterThreshold.Value, baseMinDistance, topMinDistance);


                //points currently on the loop
                var currentPoints = new Queue<PositionDistance>();

                //the first point is random within the bounds
                var firstPoint = new Vector2D(random.Float(rectangle.Min.X, rectangle.Max.X), random.Float(rectangle.Min.Y, rectangle.Max.Y));
                var firstPositionDistance = new PositionDistance {Position = firstPoint, Distance = interFunc(floatLayer.GetGenericValue(firstPoint))};
                currentPoints.Enqueue(firstPositionDistance);

                //store it in the grid, too
                var firstCoordinates = PositionToCoordinates(firstPoint, cellSize, rectangle.Min);
                grid[firstCoordinates[0], firstCoordinates[1]] = new List<PositionDistance> {firstPositionDistance};

                while (!currentPoints.IsEmpty())
                {
                    var currentPoint = currentPoints.Dequeue();
                    for (int i = 0; i < PointsToTest; i++)
                    {
                        Vector2D newPoint = GenerateRandomPointAround(random, currentPoint.Position, currentPoint.Distance);

                        //determine the new minimum distance
                        var layerValue = floatLayer.GetGenericValue(newPoint);
                        var newPositionDistance = new PositionDistance {Position = newPoint, Distance = interFunc(layerValue)};

                        //if the point is valid
                        //check that the point is in the image region
                        //and no points exists in the point's neighborhood
                        if (rectangle.Contains(newPoint) && !HasPointInNeighbourhood(grid, newPositionDistance, cellSize, cellDistance, rectangle))
                        {
                            //update containers
                            currentPoints.Enqueue(newPositionDistance);

                            //return the point
                            if (maskCheckFunc(layerValue))
                            {
                                var height = heightLayer.SafeGetHeight(newPositionDistance.Position);
                                var newEntity = new PointEntity(new Vector3D(newPositionDistance.Position, height));
                                _distanceAttribute[newEntity] = newPositionDistance.Distance;
                                yield return newEntity;
                            }

                            var coordinate = PositionToCoordinates(newPoint, cellSize, rectangle.Min);

                            if (grid[coordinate[0], coordinate[1]] == null)
                                grid[coordinate[0], coordinate[1]] = new List<PositionDistance> {newPositionDistance};
                            else
                                grid[coordinate[0], coordinate[1]].Add(newPositionDistance);
                        }
                    }
                }
            }



            private Vector2D GenerateRandomPointAround(Random random, Vector2D point, float minDistance)
            {
                //non-uniform, favours points closer to the inner ring, leads to denser packings
                var r1 = random.NextDouble();
                var r2 = random.NextDouble();

                //random radius between mindist and 2 * mindist
                var radius = minDistance * (r1 + 1);

                //random angle
                var angle = 2 * Math.PI * r2;

                //the new point is generated around the point (x, y)
                var newX = point.X + radius * Math.Cos(angle);
                var newY = point.Y + radius * Math.Sin(angle);

                return new Vector2D((float) newX, (float) newY);
            }



            private bool HasPointInNeighbourhood(List<PositionDistance>[,] grid, PositionDistance positionDistance, float cellSize, int cellDistance, BoundingRectangle rectangle)
            {
                var coordinate = PositionToCoordinates(positionDistance.Position, cellSize, rectangle.Min);

                int maxX = grid.GetLength(0);
                int maxY = grid.GetLength(1);
                int startX = coordinate[0] - cellDistance;
                int startY = coordinate[1] - cellDistance;
                int endX = coordinate[0] + cellDistance;
                var endY = coordinate[1] + cellDistance;

                //get the neighbourhood if the point in the grid
                for (int i = startX; i < endX; i++)
                for (int j = startY; j < endY; j++)
                {
                    var cell = grid[NormalizeIndex(i, maxX), NormalizeIndex(j, maxY)];

                    //if there's a value in the cell
                    if (cell != null)
                        foreach (PositionDistance existingPositionDistance in cell)
                        {
                            var calculablePosition = existingPositionDistance.Position;

                            if (i < 0)
                                calculablePosition -= new Vector2D(rectangle.Width, 0);
                            else if (i >= maxX)
                                calculablePosition += new Vector2D(rectangle.Width, 0);

                            if (j < 0)
                                calculablePosition -= new Vector2D(0, rectangle.Height);
                            else if (j >= maxY)
                                calculablePosition += new Vector2D(0, rectangle.Height);

                            var distanceBetweenPoints = calculablePosition.DistanceTo(positionDistance.Position);

                            //and that cell is actually closer than the allowed minimum...
                            if (distanceBetweenPoints < existingPositionDistance.Distance
                                && distanceBetweenPoints < positionDistance.Distance)
                                return true;
                        }
                }

                return false;
            }



            public int NormalizeIndex(int index, int max)
            {
                if (index < 0)
                    return max + index % max;
                if (index < max) //most cases will probaly fit this case anyway, requiring only 2 simple conditions to reach it
                    return index;

                return index % max;
                //this expression would be simpler, but requires the expensive modulo operation every time
                //return index < 0 ? Count + index % Count : index % Count;
            }



            private int[] PositionToCoordinates(Vector2D point, float cellSize, Vector2D min)
            {
                return new[] {(int) ((point.X - min.X) / cellSize), (int) ((point.Y - min.Y) / cellSize)};
            }



            public class PositionDistance
            {
                public float Distance
                {
                    get;
                    set;
                }

                public Vector2D Position
                {
                    get;
                    set;
                }
            }
        }

        #endregion

        /*#region SurfaceScatterMaskParameter

        public abstract class SurfaceScatterMaskParameter : CompoundParameter
        {
            /// <summary>
            /// Index of the layer which contains the intensity information. Should be a floating-point based layer, such as a height or blend layer.<br/>
            /// By default, the index 0 contains the height layer.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Index", 0) { MinValue = 0 };

            private readonly FloatParameter _parameterThreshhold = 

            protected SurfaceScatterMaskParameter(string label) : base(label)
            {
            }



            public abstract IEnumerable<PointEntity> CreatePoints(int seed, SurfaceEntity surfaceEntity);
        }

        #endregion*/


        public class SurfaceScatterPointsParameter : ScatterPointsParameter
        {
            /// <summary>
            /// The surface on which to randomly scatter the points.
            /// </summary>
            private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Surface");

            /// <summary>
            /// The surface on which the points were scattered.
            /// </summary>
            private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Surface");

            /// <summary>
            /// The (minimum) distance between the points. <br/>
            /// In practice, the value will vary between distance and 2 x distance. 
            /// </summary>
            private readonly SelectListParameter<SurfaceScatterDistanceParameter> _parameterDistance = new SelectListParameter<SurfaceScatterDistanceParameter>("Distance", "Fixed");



            /// <summary>
            /// The (minimum) distance between the points. <br/>
            /// In practice, the value will vary between distance and 2 x distance. 
            /// </summary>
            //private readonly SelectListParameter<SurfaceScatterMaskParameter> _parameterMask = new SelectListParameter<SurfaceScatterMaskParameter>("Mask", "None");
            public SurfaceScatterPointsParameter()
                : base("Surface")
            {
            }



            public override IEnumerable<PointEntity> CreatePoints(int seed)
            {
                var surfaceEntity = _input.Read();
                _output.Write(surfaceEntity);

                return _parameterDistance.SelectedItem.CreatePoints(seed, surfaceEntity);
            }
        }

        #endregion

        /// <summary>
        /// Creates randomly scattered points.
        /// </summary>
        /// <seealso cref="Sceelix.Points.Procedures.PointsCreateProcedure.PointCreateParameter" />
        public class RandomPointsParameter : PointCreateParameter
        {
            /// <summary>
            /// The seed of the random point distribution.
            /// </summary>
            private readonly IntParameter _parameterSeed = new IntParameter("Seed", 1000);

            /// <summary>
            /// Area on which the points are to be distributed.
            /// </summary>
            private readonly SelectListParameter<ScatterPointsParameter> _parameterScatter = new SelectListParameter<ScatterPointsParameter>("Area", "Rectangle");



            public RandomPointsParameter()
                : base("Random")
            {
            }



            protected internal override IEnumerable<PointEntity> CreatePoints()
            {
                return _parameterScatter.SelectedItem.CreatePoints(_parameterSeed.Value);
            }
        }

        #endregion
    }
}