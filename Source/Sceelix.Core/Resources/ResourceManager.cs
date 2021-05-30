using System;
using System.IO;
using System.Linq;
using System.Net;
using Sceelix.Conversion;
using Sceelix.Helpers;

namespace Sceelix.Core.Resources
{
    /// <summary>
    /// The Load Environment is responsible for several operations that are specific to a particular
    /// deployment platform. Classes that derive from this one can be programmed to load resources
    /// through specific means and locations and perform logging to different targets.
    /// </summary>
    /// <summary>
    /// The ResourceManager is the default implementation of the IResourceManager interface, which is responsible
    /// for loading resources from disk (for project and file system paths), web addresses and resource from memory.
    /// </summary>
    public class ResourceManager : IResourceManager
    {
        public const string ResourcePrefix = "Memory://";

        private readonly object _mutex = new object();

        //keeps a cache of files every time
        private string _projectFolder = string.Empty;



        public ResourceManager()
        {
        }



        /// <summary>
        /// Environment constructor.
        /// </summary>
        /// <param name="projectFolder">Location of the project folder. If it is a relative path, it will be turned into a full path based on the application environment.</param>
        public ResourceManager(string projectFolder)
        {
            _projectFolder = !Path.IsPathRooted(projectFolder) ? Path.GetFullPath(projectFolder) : projectFolder;
        }



        public string ProjectFolder
        {
            get { return _projectFolder; }
            set { _projectFolder = !Path.IsPathRooted(value) ? Path.GetFullPath(value) : value; }
        }



        public virtual bool CheckAndFix(ref Type type, string guid = "")
        {
            //can't really do anything here
            if (type == null)
                return false;

            return true;
        }



        /*public DateTime GetLastWriteTime(string path)
        {
            var fullPath = GetFullPath(path);

            if (IsMemoryPath(fullPath))
            {
                
            }
            else if (IsWebUrl(fullPath))
            {
                //suppose that websites are always being updated
                return DateTime.Now;
            }
            else
            {
                return File.Exists(fullPath) ? File.GetLastWriteTime(fullPath) : DateTime.MinValue;
            }

            return DateTime.MinValue;
        }*/


        /// <summary>
        /// Logs a message to the available output stream. 
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="logType">Type of message.</param>
        /*public virtual void Log(object message, LogType logType = LogType.Information)
        {
            Console.WriteLine(message);
        }*/


        /// <summary>
        /// Clears the available output stream of any logged messages.
        /// </summary>
        /*public virtual void ClearLog()
        {
            Console.Clear();
        }*/


        /*public virtual bool CheckAndFix(ref string relativePath, string guid = "")
        {
            //can't really do anything here
            if (!Exists(GetFullPath(relativePath)))
                return false;

            return true;
        }*/



        /// <summary>
        /// Checks if a given resource exists.
        /// </summary>
        /// <param name="path">The path to the resource (relative or full).</param>
        /// <returns>True if the resource exists, false otherwise.</returns>
        public virtual bool Exists(string path)
        {
            if (IsMemoryPath(path))
                return MemoryStorage.Data.ContainsKey(path);

            if (IsWebUrl(path))
                return WebUrlExists(path);

            var fullPath = GetFullFileFolderPath(path);

            return FileExists(fullPath) || FolderExists(fullPath);
        }



        protected virtual bool FileExists(string fullPath)
        {
            return File.Exists(GetFullPath(fullPath));
        }



        protected virtual bool FolderExists(string fullPath)
        {
            return Directory.Exists(fullPath);
        }



        /// <summary>
        /// Returns a list of all resources inside the given directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string[] GetDirectoryPaths(string folderPath)
        {
            return Directory.GetDirectories(GetFullPath(folderPath)).Select(x => Path.Combine(folderPath, Path.GetFileName(x))).ToArray();
            //return Directory.GetDirectories(folderPath);
        }



        /// <summary>
        /// Returns a list of all resources inside the given directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string[] GetFilePaths(string folderPath)
        {
            return Directory.GetFiles(GetFullPath(folderPath)).Select(x => Path.Combine(folderPath, Path.GetFileName(x))).ToArray();
            //return Directory.GetFiles(folderPath).Select(x => Path.Combine(folderPath, Path.GetFileName(x))).ToArray();
            //return Directory.GetFiles(folderPath);
        }



        protected virtual Stream GetFileStream(string fullPath)
        {
            return new FileStream(fullPath, FileMode.Open);
        }



        protected virtual string GetFullFileFolderPath(string path)
        {
            return Path.Combine(_projectFolder, path);
        }



        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual string GetFullPath(string path)
        {
            if (IsMemoryPath(path))
                return PathHelper.ToUniversalPath(path);

            if (IsWebUrl(path))
                return PathHelper.ToUniversalPath(path);

            return PathHelper.ToUniversalPath(GetFullFileFolderPath(path));
        }



        public virtual string GetGuid(string path)
        {
            return null;
        }



        public DateTime GetLastWriteTime(string path)
        {
            var fullPath = GetFullPath(path);

            if (IsMemoryPath(path))
                return DateTime.MinValue;

            return File.Exists(fullPath) ? File.GetLastWriteTime(fullPath) : DateTime.MinValue;
        }



        protected virtual Stream GetWebStream(string fullPath)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(fullPath);

                return new MemoryStream(data);
            }
        }



        private bool IsMemoryPath(string path)
        {
            return path.StartsWith(ResourcePrefix);
        }



        /// <summary>
        /// Indicates if the given path is an URL.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsWebUrl(string path)
        {
            Uri uriResult;

            return Uri.TryCreate(path, UriKind.Absolute, out uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }



        /// <summary>
        /// Loads a resource from this environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">Path to the resource, relative to the project or not.</param>
        /// <returns>The loaded resource, if it exists.</returns>
        public T Load<T>(string path)
        {
            lock (_mutex)
            {
                var fullPath = GetFullPath(path);

                if (!Exists(fullPath))
                    throw new FileNotFoundException("Could not find resource '" + path + "'.");

                if (IsMemoryPath(fullPath))
                {
                    object dataObject = MemoryStorage.Data[fullPath];

                    return ConvertHelper.Convert<T>(dataObject);
                }

                Stream stream;
                if (IsWebUrl(fullPath))
                    stream = GetWebStream(fullPath);
                else
                    stream = GetFileStream(fullPath);

                var value = ConvertHelper.Convert<T>(stream);

                //if the value is of type stream, it should be disposed directly, not by this function
                //otherwise, dispose the stream
                if (!typeof(Stream).IsAssignableFrom(typeof(T)))
                    stream.Close();

                return value;
            }
        }



        /// <summary>
        /// Lookups the specified unique identifier. This default class
        /// is not able to look for guids.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>Null.</returns>
        public virtual string Lookup(string guid)
        {
            return null;
        }



        public string SaveResource(string path, object resource)
        {
            var realPath = ResourcePrefix + path;

            MemoryStorage.Data[realPath] = resource;

            return realPath;
        }



        protected virtual bool WebUrlExists(string fullPath)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(fullPath) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }



        /*public virtual void Send(Object message)
        {
        }



        public virtual T WaitForResponse<T>(Object message)
        {
            return default(T);
        }*/
    }
}