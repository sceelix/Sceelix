using System.Collections.Generic;
using Sceelix.Core.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    public abstract class SurfaceAdjustParameter : CompoundParameter
    {
        protected SurfaceAdjustParameter(string label)
            : base(label)
        {
        }



        protected internal abstract void Run(IEnumerable<SurfaceEntity> surfaces);
    }
}