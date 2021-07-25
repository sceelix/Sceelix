using System;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;
using Sceelix.Meshes.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Creates a surface from a mesh by sampling it uniformly in the XY plane.
    /// </summary>
    public class FromMeshSurfaceParameter : SurfaceCreateParameter
    {
        /// <summary>
        /// Mesh to be transformed to a surface.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");


        /// <summary>
        /// Square size of each terrain cell. Should be a multiple of both Width and Length.
        /// </summary>
        private readonly FloatParameter _parameterCellSize = new FloatParameter("Cell Size", 1);

        /// <summary>
        /// The type of interpolation.
        /// </summary>
        private readonly EnumChoiceParameter<SurfaceInterpolation> _parameterSurfaceInterpolation = new EnumChoiceParameter<SurfaceInterpolation>("Interpolation", SurfaceInterpolation.TopLeft);



        public FromMeshSurfaceParameter()
            : base("From Mesh")
        {
        }



        protected internal override SurfaceEntity Create()
        {
            var meshEntity = _input.Read();
            var cellSize = _parameterCellSize.Value;

            var meshEntityBoundingRectangle = meshEntity.BoundingRectangle;
            var columns = (int) (meshEntityBoundingRectangle.Width / cellSize);
            var rows = (int) (meshEntityBoundingRectangle.Height / cellSize);

            var actualCellSize = meshEntityBoundingRectangle.Width / columns;
            var startingPosition = meshEntityBoundingRectangle.Min;

            ObjectQuadTree<Face> faceQuadTree = new ObjectQuadTree<Face>(meshEntity.BoundingRectangle, 10, x => x.BoundingRectangle);
            faceQuadTree.AddItems(meshEntity);

            float[,] heights = new float[columns, rows];
            ParallelHelper.For(0, columns, x =>
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2D position2D = new Vector2D(startingPosition.X + x * actualCellSize, startingPosition.Y + (rows - y - 1) * actualCellSize);
                    Vector3D position3D = position2D.ToVector3D();
                    var itemsWithinRadius = faceQuadTree.GetItemsWithinRadius(position2D, 0.1f);
                    float? heightValue = null;
                    foreach (var face in itemsWithinRadius)
                    {
                        var containsPoint = face.ContainsPoint(position2D);
                        if (containsPoint)
                        {
                            var distanceToPlane = face.Plane.DistanceToPoint(position3D) * -1;
                            if (heightValue.HasValue)
                            {
                                if (Math.Abs(distanceToPlane) > Math.Abs(heightValue.Value))
                                    heightValue = distanceToPlane;
                            }
                            else
                            {
                                heightValue = distanceToPlane;
                            }
                        }
                    }

                    heights[x, y] = heightValue ?? 0;
                }
            });

            var surfaceEntity = new SurfaceEntity(heights.GetLength(0), heights.GetLength(1), actualCellSize);
            surfaceEntity.AddLayer(new HeightLayer(heights) {Interpolation = _parameterSurfaceInterpolation.Value});
            surfaceEntity.Origin = startingPosition;

            return surfaceEntity;
        }
    }
}