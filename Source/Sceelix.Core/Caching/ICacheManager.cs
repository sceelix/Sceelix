using System.ComponentModel;
using Sceelix.Core.Procedures;
using Sceelix.Core.Utils;

namespace Sceelix.Core.Caching
{
    [DefaultValue(typeof(MemoryCacheManager))]
    public interface ICacheManager
    {
        void Clear();
        CacheData GetCache(Procedure procedure, string cacheKey);

        string GetCacheKey(Procedure procedure);

        void SetCache(Procedure procedure, string cacheKey);
    }
}