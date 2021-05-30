using System.Collections.Generic;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Data
{
    [Entity("Group")]
    public interface IEntityGroup : IEntity
    {
        IEnumerable<IEntity> SubEntities
        {
            get;
        }
    }
}