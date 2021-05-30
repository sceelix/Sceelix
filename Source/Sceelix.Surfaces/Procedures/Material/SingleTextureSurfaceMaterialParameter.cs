using Sceelix.Core.Parameters;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Sets a single texture as the default diffuse texture of the terrain.
    /// </summary>
    /// <seealso cref="SurfaceMaterialParameter" />
    public class SingleTextureSurfaceMaterialParameter : SurfaceMaterialParameter
    {
        /// <summary>
        /// The diffuse texture to set on the terrain.
        /// </summary>
        private readonly FileParameter _parameterDiffuseTexture = new FileParameter("Texture", "", BitmapExtension.SupportedFileExtensions);

        /// <summary>
        /// The UV sizing coordinates for texture mapping.
        /// </summary>
        private readonly Vector2DParameter _parameterMappingMultiplier = new Vector2DParameter("UV", Vector2D.One);

        /// <summary>
        /// Indicates if the defined UV coordinates represent an absolute size in world space, or relative to the surface size.
        /// </summary>
        private readonly BoolParameter _parameterAbsoluteSizing = new BoolParameter("Absolute Sizing", true);



        public SingleTextureSurfaceMaterialParameter()
            : base("Texture")
        {
        }



        protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
        {
            surfaceEntity.Material = new SingleTextureSurfaceMaterial(_parameterDiffuseTexture.Value)
            {
                UVTiling = _parameterAbsoluteSizing.Value ? new Vector2D(surfaceEntity.Width / _parameterMappingMultiplier.Value.X, surfaceEntity.Length / _parameterMappingMultiplier.Value.Y) : _parameterMappingMultiplier.Value
            };
        }
    }
}