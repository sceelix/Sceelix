using Sceelix.Core.Parameters;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Sets a fur-looking effect to a surface. Useful to simulate grass or otherwise furry materials.
    /// </summary>
    /// <seealso cref="SurfaceMaterialParameter" />
    public class FurTextureSurfaceMaterialParameter : SurfaceMaterialParameter
    {
        /// <summary>
        /// The underlying texture to set on the terrain.
        /// </summary>
        private readonly FileParameter _textureParameter = new FileParameter("Texture", string.Empty) {ExtensionFilter = BitmapExtension.SupportedFileExtensions};

        /// <summary>
        /// The UV sizing coordinates for texture mapping.
        /// </summary>
        private readonly Vector2DParameter _mappingMultiplierParameter = new Vector2DParameter("UV", Vector2D.One);

        /// <summary>
        /// Indicates if the defined UV coordinates represent an absolute size in world space, or relative to the surface size.
        /// </summary>
        private readonly BoolParameter _absoluteSizingParameter = new BoolParameter("Absolute Sizing", true);


        /// <summary>
        /// Density of the fur effect of the material.
        /// </summary>
        private readonly FloatParameter _furDensityParameter = new FloatParameter("Fur Density", 0.8f);

        /// <summary>
        /// The jitter map scale of the material effect.
        /// </summary>
        private readonly FloatParameter _jitterMapScaleParameter = new FloatParameter("Jitter Map Scale", 0.5f);

        /// <summary>
        /// The maximum length of each fur hair.
        /// </summary>
        private readonly FloatParameter _maxFurLengthParameter = new FloatParameter("Max. Fur Length", 0.5f);

        /// <summary>
        /// The strength of the self shadowing effect.
        /// </summary>
        private readonly FloatParameter _selfShadowStrengthParameter = new FloatParameter("Self Shadow Strength", 1f);



        public FurTextureSurfaceMaterialParameter()
            : base("Fur")
        {
        }



        protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
        {
            surfaceEntity.Material = new FurSurfaceMaterial
            {
                Texture = _textureParameter.Value,
                MaxFurLength = _maxFurLengthParameter.Value,
                FurDensity = _furDensityParameter.Value,
                SelfShadowStrength = _selfShadowStrengthParameter.Value,
                JitterMapScale = _jitterMapScaleParameter.Value,
                UVTiling = _absoluteSizingParameter.Value ? new Vector2D(surfaceEntity.Width / _mappingMultiplierParameter.Value.X, surfaceEntity.Length / _mappingMultiplierParameter.Value.Y) : _mappingMultiplierParameter.Value
            };
        }
    }
}