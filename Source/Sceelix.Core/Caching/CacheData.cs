using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;

namespace Sceelix.Core.Utils
{
    public class CacheData
    {
        public CacheData(DateTime createdTime)
        {
            CreatedTime = createdTime;
            Data = new Dictionary<string, List<IEntity>>();
        }



        public DateTime CreatedTime
        {
            get;
        }


        public Dictionary<string, List<IEntity>> Data
        {
            get;
            set;
        }



        public CacheData DeepClone()
        {
            CacheData cacheData = new CacheData(CreatedTime);

            foreach (KeyValuePair<string, List<IEntity>> keyValuePair in Data)
                cacheData.Data.Add(keyValuePair.Key, keyValuePair.Value.Select(x => x.DeepClone()).ToList());

            return cacheData;
        }
    }
}