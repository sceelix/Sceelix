using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Sceelix.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Sceelix.Editor
{
    public class AssetReferenceManager
    {
        /// <summary>
        /// In this cleanup process, we get the list of stored asset references.
        /// This file contains a dictionary that indicates, per asset, what scenes are referencing it. 
        /// </summary>
        /// <param name="assetFolder">The asset folder.</param>
        public static void CleanupAndUpdate(String assetFolder)
        {
            Dictionary<String, int> referenceCount = new Dictionary<String, int>();
            var sceelixAssetGuids = AssetDatabase.FindAssets("l:Sceelix", new[] { assetFolder });
            foreach (string assetGuid in sceelixAssetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                referenceCount.Add(assetPath,0);
            }

            //go over existing scene files to see which ones are referencing the assets
            var sceneGuids = AssetDatabase.FindAssets("t: Scene");
            foreach (string sceneGuid in sceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);

                //let's update only the loaded scenes
                var scene = SceneManager.GetSceneByPath(scenePath);
                if (scene.isLoaded)
                {
                    var rootGameObjects = scene.GetRootGameObjects();

                    List<String> assets = GetAssetList(rootGameObjects, assetFolder).ToList();

                    //first, add the assets that may be missing.
                    foreach (string asset in assets)
                    {
                        if (referenceCount.ContainsKey(asset))
                            referenceCount[asset]++;
                    }
                }
                else
                {
                    var dependencies = AssetDatabase.GetDependencies(scenePath);
                    foreach (string dependency in dependencies)
                    {
                        if (referenceCount.ContainsKey(dependency))
                            referenceCount[dependency]++;
                    }
                }
            }

            

            var prefabGuids = AssetDatabase.FindAssets("t: Prefab", new [] { assetFolder });
            foreach (string prefabGuid in prefabGuids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);

                var dependencies = AssetDatabase.GetDependencies(prefabPath, true);
                foreach (var dependency in dependencies)
                {
                    if (referenceCount.ContainsKey(dependency))
                        referenceCount[dependency]++;
                }
            }

            foreach (var reference in referenceCount.Where(x => x.Value == 0))
            {
                Debug.LogWarning(reference.Key);
                //AssetDatabase.DeleteAsset(reference.Key);
            }
        }




        private static IEnumerable<string> GetAssetList(IEnumerable<GameObject> gameObjects, String assetFolder)
        {
            foreach (UnityEngine.Object obj in EditorUtility.CollectDependencies(gameObjects.Cast<UnityEngine.Object>().ToArray()))//
            {
                if (obj is Texture || obj is Material || obj is Mesh)
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (path.StartsWith(assetFolder))
                        yield return path;
                }
            }
        }
    }

}
