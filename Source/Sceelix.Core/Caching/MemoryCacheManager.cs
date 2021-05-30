using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sceelix.Core.Graphs;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;
using Sceelix.Core.Utils;

namespace Sceelix.Core.Caching
{
    public class MemoryCacheManager : ICacheManager
    {
        private static readonly Dictionary<string, CacheData> _cacheDictionary = new Dictionary<string, CacheData>();
        private readonly IResourceManager _resourceManager;



        public MemoryCacheManager()
        {
        }



        public MemoryCacheManager(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }



        public void Clear()
        {
            _cacheDictionary.Clear();
        }



        public CacheData GetCache(Procedure procedure, string key)
        {
            CacheData cacheData;
            if (_cacheDictionary.TryGetValue(key, out cacheData))
            {
                if (_resourceManager != null &&
                    procedure.ExecutionNode.Node is ComponentNode)
                {
                    var executionNodeNode = (ComponentNode) procedure.ExecutionNode.Node;
                    if (_resourceManager.GetLastWriteTime(executionNodeNode.ProjectRelativePath) > cacheData.CreatedTime)
                    {
                        _cacheDictionary.Remove(key);
                        return null;
                    }
                }

                return cacheData;
            }

            return null;
        }



        public string GetCacheKey(Procedure procedure)
        {
            if (procedure.ExecutionNode == null)
                return null;

            var procedureTypePart = procedure.ExecutionNode.Node.ProcedureAttribute.Guid;

            var parameterPart = JsonConvert.SerializeObject(procedure.Parameters.Select(x => new KeyValuePair<string, object>(x.Label, x.Get())));

            return procedureTypePart + ":" + parameterPart;
        }



        public void SetCache(Procedure procedure, string key)
        {
            CacheData cacheData = new CacheData(DateTime.Now);

            foreach (Output output in procedure.SubOutputs) cacheData.Data[output.Label] = output.PeekAll().ToList();

            _cacheDictionary[key] = cacheData.DeepClone();
        }
    }
}