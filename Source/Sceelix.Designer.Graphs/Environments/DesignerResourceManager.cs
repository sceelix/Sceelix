using System.IO;
using System.Linq;
using Sceelix.Core.Resources;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Environments
{
    public class DesignerResourceManager : ResourceManager
    {
        //private readonly Dictionary<String,Guid> _procedureGuids = new Dictionary<String, Guid>();

        private readonly Project _project;
        private readonly IServiceLocator _services;



        public DesignerResourceManager(Project project, IServiceLocator services)
            : base(Path.GetFullPath(project.FolderPath))
        {
            _project = project;
            _services = services;
        }



        /*public override string LoadText(string projectRelativePath)
        {
            if (!File.Exists(GetFullPath(projectRelativePath)))
                return null;

            return base.LoadText(projectRelativePath);
        }

E:\Sceelix\Development\Source\Sceelix.Designer.Graphs\ExpressionParsing\ExpressionParser.cs
        public override byte[] LoadBinary(string projectRelativePath)
        {
            if (!File.Exists(GetFullPath(projectRelativePath)))
                return null;

            return base.LoadBinary(projectRelativePath);
        }*/


        /*public override GraphProcedure LoadGraphProcedure(string projectRelativePath)
        {
            GraphProcedure procedure;
            if (!_procedureCache.TryGetValue(projectRelativePath, out procedure))
            {
                procedure = base.LoadGraphProcedure(projectRelativePath);
                _procedureCache.Add(projectRelativePath, procedure);
            }
            else
            {
                procedure = procedure.Deed
            }

            return procedure;

            //return base.LoadGraphProcedure(projectRelativePath);
            /*if (!File.Exists(GetFullPath(projectRelativePath)))
            {
                FileItem fileItem = _project.GetFileItemByGuid(guid);
                if (fileItem == null)
                    return null;

                projectRelativePath = fileItem.ProjectRelativePath;
            }

            return base.LoadGraphProcedure(projectRelativePath);
        }*/


        /*public override Procedure LoadProcedure(Type procedureType, string guid = "")
        {
            //if the type is null, we'll try to look, in the dictionary, for the actual type by its GUID, if possible
            if (procedureType == null && !_procedureTypeGuids.TryGetValue(guid, out procedureType))
                return new InvalidProcedure {Environment = this};

            return base.LoadProcedure(procedureType, guid);
        }*/

        public override string Lookup(string guid)
        {
            ProjectItem projectItem = _project.GetProjectItemByGuid(guid);
            if (projectItem != null)
            {
                return projectItem.ProjectRelativePath;
            }

            return null;
        }

        public override string GetGuid(string path)
        {
            var fileItem = _project.GetSubProjectItem(path);
            if(fileItem != null)
                return fileItem.Guid.ToString();

            return null;
        }



        /*public override bool CheckAndFix(ref string relativePath, string guid)
        {
            if (!File.Exists(GetFullPath(relativePath)))
            {
                FileItem fileItem = _project.GetFileItemByGuid(guid);
                if (fileItem != null)
                {
                    relativePath = fileItem.ProjectRelativePath;
                    return true;
                }

                return false;
            }

            return true;
        }*/



        /*public override bool CheckAndFix(ref Type type, string guid = "")
        {
            //try to fix it by looking into the dictionary
            if (type == null && !SystemProcedureManager.SystemProcedureGuids.TryGetValue(guid, out type))
                return false;

            return true;
        }*/



        /*public override string[] GetFilePaths(string folderPath)
        {
            return Directory.GetFiles(GetFullPath(folderPath)).Select(x => Path.Combine(folderPath,Path.GetFileName(x))).ToArray();
        }*/



        /*public override void Send(Object message)
        {
            _services.Get<MessageManager>().Publish(message);
            //Messenger.MessageManager.Publish(message);
        }*/



        /*public override T Request<T>(Object requestMessage)
        {
            throw new NotImplementedException();
            //services.MessageManager.RequestManager.Request<T>(requestMessage); ;
        }*/



        /*public override void Log(object message, LogType logType = LogType.Information)
        {
            
        }*/



        /*public override void ClearLog()
        {
            _services.Get<MessageManager>().Publish(new LogMessageClear());
        }*/



        /*public override T WaitForResponse<T>(Object messageToSend)
        {
            T message = default(T);

            ManualResetEvent ev = new ManualResetEvent(false);

            var action = new Action<T>(delegate(T obj)
            {
                message = obj;
                ev.Set();
            });

            _services.Get<MessageManager>().Register<T>(action);

            _services.Get<MessageManager>().Publish(messageToSend);

            ev.WaitOne();

            _services.Get<MessageManager>().Unregister(action);

            return message;
        }*/
    }
}