using System;
using System.ComponentModel;

namespace Sceelix.Core.Resources
{
    /// <summary>
    /// Controls the access to resources, such as files, streams, variables, etc. This can go beyond a simple
    /// access to files, being applicable to assembly resources, zip packages and more.
    /// </summary>
    [DefaultValue(typeof(ResourceManager))]
    public interface IResourceManager
    {
        /// <summary>
        /// Checks if a given resource exists.
        /// </summary>
        /// <param name="path">The path to the resource (relative or full).</param>
        /// <returns>True if the resource exists, false otherwise.</returns>
        bool Exists(string path);



        /// <summary>
        /// Returns a list of all directories inside the given directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string[] GetDirectoryPaths(string folderPath);



        /// <summary>
        /// Returns a list of all resources inside the given directory.
        /// </summary>
        /// <param name="folderPath">The full folder path.</param>
        /// <returns></returns>
        string[] GetFilePaths(string folderPath);



        /// <summary>
        /// Gets the full path from a relative one. If the path is already full, it will be returned.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string GetFullPath(string path);



        /// <summary>
        /// Gets the guid from a relative path.
        /// </summary>
        /// <param name="path">The path to the resource</param>
        /// <returns>The guid of the resource</returns>
        string GetGuid(string path);



        /// <summary>
        /// Gets the last write time of a given resource.
        /// Can be used to check for updates to a given resource.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <returns></returns>
        //DateTime GetLastWriteTime(string fullPath);
        DateTime GetLastWriteTime(string fullPath);



        /// <summary>
        /// Loads the specified relative or full path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        T Load<T>(string path);



        /// <summary>
        /// Looks up a resource path from a specified unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>The project relative path of the resource, if it exists, or null otherwise.</returns>
        string Lookup(string guid);



        /// <summary>
        /// Saves the resource to the given path.
        /// </summary>
        /// <param name="path">The path where to save the resource.</param>
        /// <param name="resource">The resource to save.</param>
        /// <returns></returns>
        string SaveResource(string path, object resource);
    }
}