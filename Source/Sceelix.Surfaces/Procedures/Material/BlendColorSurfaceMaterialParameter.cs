using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Renders the surface with the colors corresponding to the first 4 blend layers as RGBA components.
    /// </summary>
    public class BlendColorSurfaceMaterialParameter : SurfaceMaterialParameter
    {
        public BlendColorSurfaceMaterialParameter()
            : base("Blend Color")
        {
        }



        protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
        {
            surfaceEntity.Material = new BlendColorSurfaceMaterial();
        }
    }
}