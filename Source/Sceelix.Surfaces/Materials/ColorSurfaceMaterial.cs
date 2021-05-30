using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Materials
{
    public class ColorSurfaceMaterial : SurfaceMaterial
    {
        public ColorSurfaceMaterial()
        {
            DefaultColor = Color.White;
        }



        public Color DefaultColor
        {
            get;
            set;
        }
    }
}