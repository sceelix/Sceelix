using System.Collections.Generic;
using Sceelix.Core.Data;

namespace Sceelix.Core.Messages
{
    public class EntityDataReady
    {
        public EntityDataReady(params IEntity[] data)
        {
            Data = data;
        }



        public EntityDataReady(IEnumerable<IEntity> data)
        {
            Data = data;
        }



        public IEnumerable<IEntity> Data
        {
            get;
        }
    }
}