using Sceelix.Core.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    public abstract class SurfaceCreateParameter : CompoundParameter
    {
        protected SurfaceCreateParameter(string label)
            : base(label)
        {
        }



        protected internal abstract SurfaceEntity Create();
    }
}