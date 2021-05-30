using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;
using Sceelix.Mathematics.Spatial;
using Sceelix.Paths.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Procedures;

namespace Sceelix.Paths.Parameters
{
    /// <summary>
    /// Adjusts the surface around paths.
    /// </summary>
    /// <seealso cref="SurfaceAdjustParameter" />
    public class PathOnSurfaceAdjustParameter : SurfaceAdjustParameter
    {
        /// <summary>
        /// Path entities around which the surface should be adjusted.
        /// </summary>
        private readonly CollectiveInput<PathEntity> _input = new CollectiveInput<PathEntity>("Path");

        /// <summary>
        /// Path entities around which the surface was adjusted.
        /// </summary>
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Path");

        /// <summary>
        /// The distance around the path that should be adjusted. This value can be set as an expression based on edge properties. 
        /// The @@attributeName will refer to the attributes of each edge.
        /// </summary>
        private readonly FloatParameter _parameterWidth = new FloatParameter("Width", 1) {EntityEvaluation = true};



        public PathOnSurfaceAdjustParameter()
            : base("Path")
        {
        }



        private float GetMinDistanceBetweenCellPoints(float cellSize, Vector2D direction, Vector2D crossdirection)
        {
            var distancePoint = new Vector2D(Math.Sign(crossdirection.X) * cellSize, Math.Sign(crossdirection.Y) * cellSize);
            Line2D distanceLine = new Line2D(direction, Vector2D.Zero);

            return distanceLine.MinDistanceToPoint(distancePoint);
        }



        protected override void Run(IEnumerable<SurfaceEntity> surfaces)
        {
            var pathEntities = _input.Read().ToList();

            foreach (var surface in surfaces)
            {
                //try to get the height layer
                //if it does not exist, create it,
                //otherwise force the layer to have the same size as the surface
                //since we need to set data on it
                var heightLayer = surface.GetLayer<HeightLayer>();
                if (heightLayer == null)
                    surface.AddLayer(heightLayer = new HeightLayer(new float[surface.NumColumns, surface.NumRows]));

                foreach (PathEntity pathEntity in pathEntities)
                {
                    bool[,] edited = new bool[surface.NumColumns, surface.NumRows];
                    byte[,] editedSpecific = new byte[surface.NumColumns, surface.NumRows];

                    foreach (var pathEdge in pathEntity.Edges)
                    {
                        var halfWidth = _parameterWidth.Get(pathEdge) / 2f;

                        //var direction = pathEdge.Direction;
                        var lateralDirection = Vector3D.ZVector.Cross(pathEdge.Direction).Normalize();
                        var lateralDirection2D = lateralDirection.ToVector2D().Normalize();

                        var direction2D = pathEdge.Direction.ToVector2D().Normalize();
                        var source2D = pathEdge.Source.Position.ToVector2D();
                        var target2D = pathEdge.Target.Position.ToVector2D();

                        //determine the distance between point given the direction of the line
                        var frontalDistance = GetMinDistanceBetweenCellPoints(surface.CellSize, direction2D, lateralDirection2D);
                        var lateralDistance = GetMinDistanceBetweenCellPoints(surface.CellSize, lateralDirection2D, direction2D);


                        var sizedlateralDirection2D = lateralDirection2D * (halfWidth + lateralDistance);

                        Plane3D plane = new Plane3D(pathEdge.Direction.Cross(lateralDirection).Normalize(), pathEdge.Source.Position);


                        BoundingRectangle boundingRectangle = new BoundingRectangle();
                        boundingRectangle.AddPoint(pathEdge.Source.Position.ToVector2D());
                        boundingRectangle.AddPoint(pathEdge.Target.Position.ToVector2D());
                        boundingRectangle.AddPoint(pathEdge.Source.Position.ToVector2D() + sizedlateralDirection2D);
                        boundingRectangle.AddPoint(pathEdge.Source.Position.ToVector2D() - sizedlateralDirection2D);
                        boundingRectangle.AddPoint(pathEdge.Target.Position.ToVector2D() + sizedlateralDirection2D);
                        boundingRectangle.AddPoint(pathEdge.Target.Position.ToVector2D() - sizedlateralDirection2D);

                        boundingRectangle.Expand(surface.CellSize);

                        if (!surface.Contains(boundingRectangle.Min) || !surface.Contains(boundingRectangle.Max))
                            continue;

                        var minCoords = surface.ToCoordinates(boundingRectangle.Min);
                        var maxCoords = surface.ToCoordinates(boundingRectangle.Max);

                        Line2D line = new Line2D(pathEdge.Direction.ToVector2D(), pathEdge.Source.Position.ToVector2D());
                        //line.

                        for (int i = minCoords.X; i <= maxCoords.X; i++)
                        for (int j = maxCoords.Y; j <= minCoords.Y; j++)
                        {
                            //var height surfaceEntity.Heights[i, j];
                            Coordinate coordinate = new Coordinate(i, j);
                            Vector2D gridCornerPosition = surface.ToWorldPosition(coordinate);

                            //must be within the bound of the line - direction2D* frontalDistance
                            if ((gridCornerPosition - (source2D - direction2D * frontalDistance)).Dot(direction2D) > 0 &&
                                (gridCornerPosition - (target2D + direction2D * frontalDistance)).Dot(-direction2D) > 0)
                            {
                                var distance = line.MinDistanceToPoint(gridCornerPosition);
                                if (distance <= halfWidth + lateralDistance)
                                {
                                    //surfaceEntity.Heights[i, j] = 0;
                                    var newHeight = plane.GetHeightAt(gridCornerPosition);

                                    //if the location already has not been edited, set the new height
                                    if (editedSpecific[i, j] == 0)
                                    {
                                        heightLayer.SetValue(coordinate, newHeight);

                                        //if its on top of the road, set to 1, or 2 otherwise
                                        if (distance <= halfWidth)
                                            editedSpecific[i, j] = 1;
                                        else
                                            editedSpecific[i, j] = 2;
                                    }
                                    //if it has already been set, but is on the side
                                    else if (editedSpecific[i, j] == 2)
                                    {
                                        //and this one is below a road
                                        if (distance <= halfWidth)
                                        {
                                            heightLayer.SetValue(coordinate, newHeight);
                                            editedSpecific[i, j] = 1;
                                        }
                                        //if this is also off the road, but has a lower height
                                        else if (heightLayer.GetGenericValue(coordinate) > newHeight)
                                        {
                                            heightLayer.SetValue(coordinate, newHeight);
                                        }
                                    }


                                    //only lower the terrain, not lift it
                                    if (!edited[i, j] || heightLayer.GetGenericValue(coordinate) > newHeight)
                                    {
                                        heightLayer.SetValue(coordinate, newHeight);
                                        //surfaceEntity.Colors[i, j] = new Color(0, 255, 0, 0);

                                        edited[i, j] = true;
                                    }


                                    /*color[1] += 1 - distance / width;

                                        var sum = color.Sum();

                                        for (int k = 0; k < 4; k++)
                                        {
                                            color[k] = (color[k]) / sum;
                                        }

                                        surfaceEntity.Colors[i, j] = new Color(color);*/
                                }
                            }

                            //surfaceEntity.Colors[i, j] = new Color(0, 255, 0, 0);
                        }
                    }
                }
            }


            _output.Write(pathEntities);
        }
    }
}