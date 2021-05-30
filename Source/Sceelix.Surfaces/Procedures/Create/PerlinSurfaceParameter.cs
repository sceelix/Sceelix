using Sceelix.Core.Parameters;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Noise;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Creates a random-looking surface using the perlin noise algorithm.
    /// </summary>
    public class PerlinSurfaceParameter : SurfaceCreateParameter
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
        /// The type of height data interpolation. Because surfaces are often rendered as triangle or quad meshes, there is more than one possible setup.
        /// Different target platforms may use different setups, which could affect, for instance, the positioning of the objects on the surface.<br/>
        /// <b>Top Left</b> means that the quads are split from the top left corner to the bottom right corner.<br/>
        /// <b>Top Right</b> means that the quads are split from the top right corner to the bottom left corner.<br/>
        /// <b>Bilinear</b> assumes that the quad is not split and the height values are bilinearly interpolated.
        /// </summary>
        private readonly EnumChoiceParameter<SurfaceInterpolation> _parameterSurfaceInterpolation = new EnumChoiceParameter<SurfaceInterpolation>("Interpolation", SurfaceInterpolation.TopLeft);

        /// <summary>
        /// Frequency of the noise function. Smaller values result in tighter "mountains" while greater
        /// values result in wider ones.
        /// </summary>
        private readonly DoubleParameter _parameterFrequency = new DoubleParameter("Frequency", 5);

        /// <summary>
        /// Roughness of the surface. Lower values result in smoother surfaces.
        /// </summary>
        private readonly DoubleParameter _parameterRoughness = new DoubleParameter("Roughness", 5);

        /// <summary>
        /// Seed of the random generator, which controls the "randomness" look of the surface.
        /// </summary>
        private readonly IntParameter _parameterSeed = new IntParameter("Seed", 0);

        /// <summary>
        /// Offset of the noise function. One can see this as a "translation" of the function.
        /// </summary>
        private readonly Vector2DParameter _parameterOffset = new Vector2DParameter("Offset");



        public PerlinSurfaceParameter()
            : base("Perlin")
        {
        }



        protected internal override SurfaceEntity Create()
        {
            PerlinNoise2D perlin = new PerlinNoise2D(_parameterSeed.Value, 8, _parameterRoughness.Value * 0.1f, _parameterFrequency.Value * 0.01f, 0.5f);

            int columns = (int) (_parameterWidth.Value / _parameterCellSize.Value) + 1;
            int rows = (int) (_parameterLength.Value / _parameterCellSize.Value) + 1;
            Vector2D offset = _parameterOffset.Value;

            float[,] heights = new float[columns, rows];

            float cellSize = _parameterCellSize.Value;
            float height = _parameterHeightScale.Value;

            ParallelHelper.For(0, columns, x =>
            {
                for (int y = 0; y < rows; y++)
                {
                    var actualX = x * cellSize + offset.X;
                    var actualY = (rows - y - 1) * cellSize + offset.Y;

                    var value = (float) perlin.Noise2D(actualX, actualY) * 0.5f + 0.5f;

                    heights[x, y] = value * height;
                }
            });

            var surfaceEntity = new SurfaceEntity(heights.GetLength(0), heights.GetLength(1), _parameterCellSize.Value);
            surfaceEntity.AddLayer(new HeightLayer(heights) {Interpolation = _parameterSurfaceInterpolation.Value});

            return surfaceEntity;
        }
    }
}