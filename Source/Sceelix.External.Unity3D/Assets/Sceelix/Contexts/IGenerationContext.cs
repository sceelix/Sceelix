using System;
using UnityEngine;

namespace Assets.Sceelix.Contexts
{
    /// <summary>
    /// Interface to define the properties and functions of a generation context (the 
    /// </summary>
    public interface IGenerationContext
    {
        /// <summary>
        /// Used to inform when the generation operation starts.
        /// </summary>
        void ReportStart();



        /// <summary>
        /// Used to report the progress of the generation operation.
        /// </summary>
        void ReportProgress(float percentage);



        /// <summary>
        /// Used to inform when the generation operation ends.
        /// </summary>
        void ReportEnd();



        /// <summary>
        /// Used to inform that the generation operation produced a gameobject.
        /// </summary>
        void ReportObjectCreation(GameObject sceneGameObject);



        /// <summary>
        /// Instantiates and returns a prefab.
        /// </summary>
        /// <param name="prefabPath">Path to the prefab</param>
        /// <returns>The instantiated prefab.</returns>
        GameObject InstantiatePrefab(string prefabPath);



        /// <summary>
        /// Gets an existing resource from disk or project.
        /// </summary>
        /// <typeparam name="T">Type of asset to get.</typeparam>
        /// <param name="assetPath">Path of the asset.</param>
        /// <returns>The asset if it exists, throws exception if it does not.</returns>
        T GetExistingResource<T>(string assetPath) where T : UnityEngine.Object;



        /// <summary>
        /// Creates or gets an existing resource from disk or project.
        /// </summary>
        /// <typeparam name="T">Type of asset to get.</typeparam>
        /// <param name="assetPath">Path of the asset.</param>
        /// <param name="creationFunction">Creation function to be called if the requested asset does not exist</param>
        /// <returns>The asset if it exists, throws exception if it does not.</returns>
        T CreateOrGetAssetOrResource<T>(string assetPath, Func<T> creationFunction) where T : UnityEngine.Object;



        /// <summary>
        /// Adds a tags to a given game object.
        /// </summary>
        /// <param name="gameObject">GameObject to set the tag.</param>
        /// <param name="tag">Tag to be applied.</param>
        void AddTag(GameObject gameObject, string tag);



        /// <summary>
        /// Indicates if the newly created gameobjects should have this flag enabled. 
        /// If so, they will be removed next time a generation process occurs.
        /// </summary>
        bool RemoveOnRegeneration { get; set;}
    }
}
