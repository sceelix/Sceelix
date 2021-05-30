using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Renders the surface with one color.
    /// </summary>
    /// <seealso cref="SurfaceMaterialParameter" />
    public class ColorSurfaceMaterialParameter : SurfaceMaterialParameter
    {
        /// <summary>
        /// The color to render the surface with.
        /// </summary>
        private readonly ColorParameter _parameterColor = new ColorParameter("Color", Color.Red);



        public ColorSurfaceMaterialParameter()
            : base("Color")
        {
        }



        protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
        {
            surfaceEntity.Material = new ColorSurfaceMaterial {DefaultColor = _parameterColor.Value};
        }
    }
}