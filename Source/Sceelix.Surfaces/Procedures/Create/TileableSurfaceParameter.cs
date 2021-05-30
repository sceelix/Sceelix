using Sceelix.Core.Parameters;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Noise;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Creates a random-looking surface that is repeatable/tileable, 
    /// meaning that the height values one edge match the ones at the
    /// opposite edge, which can be used to create circular worlds.
    /// </summary>
    public class TileableSurfaceParameter : SurfaceCreateParameter
    {
        /// <summary>
        /// Width (size in X) of the surface.
        /// </summary>
        private readonly IntParameter _parameterWidth = new IntParameter("Width", 100) {MinValue = 1};

        /// <summary>
        /// Length (size in Y) of the surface.
        /// </summary>
        private readonly IntParameter _parameterLength = new IntParameter("Length", 100) {MinValue = 1};

        /// <summary>
        /// The scale of the surface height values (in Z).
        /// </summary>
        private readonly IntParameter _parameterHeightScale = new IntParameter("Height Scale", 50) {MinValue = 1};

        /// <summary>
        /// Square size of each terrain cell. Should be a multiple of both Width and Length.
        /// </summary>
        private readonly FloatParameter _parameterCellSize = new FloatParameter("Cell Size", 1);

        /// <summary>
        /// The type of interpolation.
        /// </summary>
        private readonly EnumChoiceParameter<SurfaceInterpolation> _parameterSurfaceInterpolation = new EnumChoiceParameter<SurfaceInterpolation>("Interpolation", SurfaceInterpolation.TopLeft);

        /// <summary>
        /// Frequency of the noise function. Smaller values result in tighter "mountains" while greater
        /// values result in wider ones.
        /// </summary>
        private readonly Vector2DParameter _parameterFrequency = new Vector2DParameter("Frequency", new Vector2D(3, 3));

        /// <summary>
        /// Seed of the random generator, which controls the "randomness" look of the surface.
        /// </summary>
        private readonly IntParameter _parameterSeed = new IntParameter("Seed", 0);



        public TileableSurfaceParameter()
            : base("Tileable")
        {
        }



        protected internal override SurfaceEntity Create()
        {
            int columns = (int) (_parameterWidth.Value / _parameterCellSize.Value) + 1;
            int rows = (int) (_parameterLength.Value / _parameterCellSize.Value) + 1;

            float[,] heights = new float[columns, rows];

            //to improve performance, make the access to these variables as fast as possible
            var frequencyX = _parameterFrequency.Value.X;
            var frequencyY = _parameterFrequency.Value.Y;
            var seed = _parameterSeed.Value;
            float height = _parameterHeightScale.Value;

            ParallelHelper.For(0, columns, x =>
            {
                for (int y = 0; y < rows; y++) heights[x, y] = (SimplexNoise.SeamlessNoise(x / (float) (columns - 1), y / (float) (rows - 1), frequencyX, frequencyY, seed) * 0.5f + 0.5f) * height;
            });


            var surfaceEntity = new SurfaceEntity(heights.GetLength(0), heights.GetLength(1), _parameterCellSize.Value);
            surfaceEntity.AddLayer(new HeightLayer(heights) {Interpolation = _parameterSurfaceInterpolation.Value});

            return surfaceEntity;
        }
    }
}