using System;
using System.IO;

namespace Sceelix.Helpers
{
    public class PathHelper
    {
        public static string UniversalDirectorySeparator => "/";



        /// <summary>
        /// Combines two paths, returning the result as a universal path.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            return ToUniversalPath(Path.Combine(paths));
        }



        /// <summary>
        /// Makes the path defined in "filePath" relative to "referencePath".
        /// For instance, if filePath="C:\Path\to\my\file.txt" and referencePath="C:\Path\", then the result is "\to\my\file.txt"
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="referencePath"></param>
        /// <returns></returns>
        public static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).OriginalString);
        }



        /// <summary>
        /// Converts forward slashes (/) to forward slashes (\\) in Windows platforms.
        /// Converts backslashes (\\) to forward slashes (/) in Unix platforms.
        /// </summary>
        public static string ToPlatformPath(string str)
        {
#if WINDOWS

            return str.Replace("/", Path.DirectorySeparatorChar.ToString());

#else
            return str.Replace("\\", Path.DirectorySeparatorChar.ToString());

#endif
        }



        /// <summary>
        /// Converts backslashes (\\) to forward slashes (/) in the given path. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUniversalPath(string str)
        {
            return str.Replace("\\", "/");
        }
    }
}