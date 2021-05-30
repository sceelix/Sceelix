using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Sceelix.Contexts
{
    public class RuntimeGenerationContext : IGenerationContext
    {
        private readonly Dictionary<String,Object> _cachedResources = new Dictionary<string, Object>();
        private readonly Dictionary<string, object> _options = new Dictionary<string, object>();

        public void ReportStart()
        {
            //do nothing
        }

        public void ReportProgress(float percentage)
        {
            //do nothing
        }

        public void ReportEnd()
        {
            //do nothing
        }

        public GameObject InstantiatePrefab(string prefabPath)
        {
            GameObject prefab = (GameObject)Resources.Load(prefabPath);

            return prefab;
        }

        public T GetExistingResource<T>(string assetPath) where T : Object
        {
            if (!assetPath.StartsWith("Resources/"))
                assetPath = "Resources/" + assetPath;

            return (T)Resources.Load(assetPath);
        }

        public T CreateOrGetAssetOrResource<T>(string assetPath, Func<T> creationFunction, GameObject gameObject) where T : Object
        {
            Object asset;
            if (!_cachedResources.TryGetValue(assetPath, out asset))
            {
                asset = creationFunction.Invoke();

                if (asset != null)
                    _cachedResources.Add(assetPath, asset);
            }

            return (T)asset;
        }

        public T CreateOrGetAssetOrResource<T>(string assetPath, Func<T> creationFunction) where T : Object
        {
            Object unityObject;
            if (!_cachedResources.TryGetValue(assetPath, out unityObject))
            {
                unityObject = creationFunction.Invoke();

                _cachedResources.Add(assetPath, unityObject);
            }

            return (T)unityObject;
        }


        public void AddTag(GameObject gameObject, string tag)
        {
            //in runtime, we are not able to create tags if they don't yet exist, so...
            gameObject.tag = tag;
        }

        public bool RemoveOnRegeneration { get; set; }
        public void ReportObjectCreation(GameObject sceneGameObject)
        {
            
        }

        public Dictionary<string, object> Options
        {
            get { return _options; }
        }
    }
}
