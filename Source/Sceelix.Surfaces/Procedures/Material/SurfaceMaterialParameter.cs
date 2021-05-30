using Sceelix.Core.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    public abstract class SurfaceMaterialParameter : CompoundParameter
    {
        protected SurfaceMaterialParameter(string label)
            : base(label)
        {
        }



        protected internal abstract void SetMaterial(SurfaceEntity surfaceEntity);
    }
}