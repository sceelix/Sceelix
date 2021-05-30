using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Materials
{
    public class FurSurfaceMaterial : SurfaceMaterial
    {
        public float FurDensity
        {
            get;
            set;
        }


        public float JitterMapScale
        {
            get;
            set;
        }


        public float MaxFurLength
        {
            get;
            set;
        }


        public float SelfShadowStrength
        {
            get;
            set;
        }


        public string Texture
        {
            get;
            set;
        }


        public Vector2D UVTiling
        {
            get;
            set;
        }
    }
}