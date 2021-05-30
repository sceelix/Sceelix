using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.IO;
using Sceelix.Mathematics.Data;
using Sceelix.Paths.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Procedures;

namespace Sceelix.Paths.Parameters
{
    /// <summary>
    /// Places paths on the surface.
    /// </summary>
    public class PathPlacementParameter : SurfacePlaceProcedure.SurfacePlacementParameter
    {
        /// <summary>
        /// Paths to be placed on the surface.
        /// </summary>
        private readonly CollectiveInput<PathEntity> _input = new CollectiveInput<PathEntity>("Path");

        /// <summary>
        /// Paths that were placed on the surface.
        /// </summary>
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Path");



        protected PathPlacementParameter()
            : base("Path")
        {
        }



        protected override void Run(IEnumerable<SurfaceEntity> surfaceEntities)
        {
            IEnumerable<PathEntity> pathEntities = _input.Read().ToList();

            foreach (var surfaceEntity in surfaceEntities)
            {
                var surfaceRectangle = surfaceEntity.BoundingRectangle;

                var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                foreach (PathEntity pathEntity in pathEntities)
                {
                    //skip if the surface and path are not even on the same space
                    if (!surfaceRectangle.Intersects(pathEntity.BoundingBox.BoundingRectangle))
                        continue;

                    foreach (var pathVertex in pathEntity.Vertices)
                        if (surfaceEntity.Contains(pathVertex.Position.ToVector2D()))
                        {
                            float z = heightLayer != null ? heightLayer.GetGenericValue(new Vector2D(pathVertex.Position.X, pathVertex.Position.Y)) : 0;

                            if (!float.IsInfinity(z))
                                pathVertex.Position = new Vector3D(pathVertex.Position.X, pathVertex.Position.Y, z);
                        }

                    pathEntity.AdjustScope();
                }
            }

            _output.Write(pathEntities);
        }
    }
}