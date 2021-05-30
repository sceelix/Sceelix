using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// The default material, which simply renders the surface with a grid pattern. Simple for surface visualization.
    /// </summary>
    /// <seealso cref="SurfaceMaterialParameter" />
    public class DefaultSurfaceMaterialParameter : SurfaceMaterialParameter
    {
        public DefaultSurfaceMaterialParameter()
            : base("Default")
        {
        }



        protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
        {
            surfaceEntity.Material = new DefaultSurfaceMaterial();
        }
    }
}