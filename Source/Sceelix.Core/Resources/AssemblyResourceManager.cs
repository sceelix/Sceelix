using System.IO;
using System.Reflection;
using Sceelix.Helpers;

namespace Sceelix.Core.Resources
{
    public class AssemblyResourceManager : ResourceManager
    {
        private readonly Assembly _assembly;
        private string[] _resourceNames;



        public AssemblyResourceManager(Assembly assembly)
        {
            _assembly = assembly;
            _resourceNames = _assembly.GetManifestResourceNames();
        }



        protected override bool FileExists(string fullPath)
        {
            return _assembly.GetManifestResourceStream(fullPath) != null;
        }



        public override string[] GetDirectoryPaths(string folderPath)
        {
            return base.GetDirectoryPaths(folderPath);
        }



        public override string[] GetFilePaths(string folderPath)
        {
            return base.GetFilePaths(folderPath);
        }



        protected override Stream GetFileStream(string fullPath)
        {
            return _assembly.GetManifestResourceStream(fullPath);
        }



        protected override string GetFullFileFolderPath(string path)
        {
            return _assembly.GetName().Name + "." + PathHelper.ToUniversalPath(path).Replace("/", ".");
        }
    }
}