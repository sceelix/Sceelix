namespace Sceelix.Core.Resources
{
    public static class ResourceLoaderExtension
    {
        /// <summary>
        /// Loads a resource into an array of bytes. Equivalent to calling Load<byte[]>.
        /// </summary>
        /// <param name="path">Path to the file, preferably relative to the project.</param>
        /// <returns>Array of bytes with the resource content.</returns>
        public static byte[] LoadBinary(this IResourceManager environment, string path)
        {
            return environment.Load<byte[]>(path);
        }



        /// <summary>
        /// Loads a resource into an array of strings (one for each line of the resource text). Equivalent to calling Load<String[]>.
        /// </summary>
        /// <param name="path">Path to the file, preferably relative to the project.</param>
        /// <returns>String with the resource content.</returns>
        public static string[] LoadLines(this IResourceManager environment, string projectRelativePath)
        {
            return environment.Load<string[]>(projectRelativePath);
        }



        /// <summary>
        /// Loads a resource into a string. Equivalent to calling Load<String>.
        /// </summary>
        /// <param name="path">Path to the file, preferably relative to the project.</param>
        /// <returns>String with the resource content.</returns>
        public static string LoadText(this IResourceManager environment, string projectRelativePath)
        {
            return environment.Load<string>(projectRelativePath);
        }



        public static void SaveInMemory(this IResourceManager environment, string projectRelativePath, object data)
        {
            environment.SaveResource(ResourceManager.ResourcePrefix + projectRelativePath, data);
        }
    }
}