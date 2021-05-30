using System;
using System.IO;
using Sceelix.Core.Annotations;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("IO")]
    public class IOFunctions
    {
        public static object ChangeExtension(dynamic path, dynamic extension)
        {
            return Path.ChangeExtension(path, extension);
        }



        public static object ChangeFileName(dynamic path, dynamic fileName)
        {
            var newfileName = Path.GetFileNameWithoutExtension(fileName);
            if (Path.HasExtension(path))
                newfileName += Path.GetExtension(path);

            //we combine the old path (and old extension) with the new filename
            return Path.Combine(Path.GetDirectoryName(path), newfileName);
        }



        public static object ChangeFileNameAndExtension(dynamic path, dynamic fileName)
        {
            return Path.Combine(Path.GetDirectoryName(path), fileName);
        }



        public static object DesktopPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }



        public static object DocumentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }



        public static object Exists(Procedure procedure, dynamic path)
        {
            return procedure.Environment.GetService<IResourceManager>().Exists(path);
        }



        [Obsolete("Please use the other Exists.")]
        public static object ExistsOld(Procedure procedure, dynamic path)
        {
            return File.Exists(path);
        }



        public static object GetDirPath(dynamic path)
        {
            return Path.GetDirectoryName(path);
        }



        public static object GetFileNameWithoutExtension(dynamic path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }



        public static object GetTempPath()
        {
            return Path.GetTempFileName();
        }
    }
}