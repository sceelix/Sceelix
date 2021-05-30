using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    public class Project
    {
        public const String FileExtension = "slxp";
        private readonly IServiceLocator _services;
        private readonly string _folderPath = @"";

        private readonly string _name = "MyProject";

        private String _author = Environment.UserName;

        private readonly FolderItem _baseFolder;
        private String _company = "Private";
        private List<String> _exportLocations = new List<String>();

        private List<String> _openedFiles = new List<String>();
        //private FolderItem _tempFolder;
        private String _version = "1.0.0.0";



        public Project()
        {
        }



        public Project(IServiceLocator services, string folderPath, string name)
        {
            _services = services;

            _folderPath = folderPath;
            _name = name;

            _baseFolder = new FolderItem(this);
            //_tempFolder = new FolderItem(".Sceelix",_baseFolder);
        }



        public static string DefaultProjectsFolder
        {
            get { return SceelixApplicationInfo.IsPortable ? Path.Combine(SceelixApplicationInfo.SceelixMainFolder, "Projects") : Path.Combine(SceelixApplicationInfo.DocumentsFolder, "Projects"); }
        }



        public static String DefaultTutorialFolder
        {
            get { return Path.Combine(SceelixApplicationInfo.ExtrasFolder, "Samples"); }
        }



        public static String DefaultTutorialFile
        {
            get { return Path.Combine(DefaultTutorialFolder, "Sceelix Tutorial Samples.slxp"); }
        }



        public static String DefaultTutorialZip
        {
            get { return Path.Combine(SceelixApplicationInfo.ContentFolder, "Samples.zip"); }
        }

        #region Creating, Saving and Loading the project

        public static Project CreateProject(IServiceLocator services, String folderPath, String name)
        {
            Project project = new Project(services, folderPath, name);

            //create a 
            project.BaseFolder.CreatePhysicalDirectory();

            //create some default folders
            project.BaseFolder.CreateDirectoryItem("Graphs");
            project.BaseFolder.CreateDirectoryItem("Textures");
            //project.BaseFolder.CreateDirectoryItem("Maps");

            //save the project definitions to a file
            project.Save();

            return project;
        }



        /*public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented,new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Objects});

            File.WriteAllText(ProjectFilePath, json);
        }


        public static Project Load(String projectFilePath)
        {
            //read the file (which is in json format)
            try
            {
                String jsonText = File.ReadAllText(projectFilePath);

                //convert the json to object
                Project loadedProject = JsonConvert.DeserializeObject<Project>(jsonText,new JsonSerializerSettings{PreserveReferencesHandling =PreserveReferencesHandling.Objects,TypeNameHandling =TypeNameHandling.Objects});

                loadedProject._name = Path.GetFileNameWithoutExtension(projectFilePath);
                loadedProject._folderPath = Path.GetDirectoryName(projectFilePath);
                loadedProject._baseFolder = new FolderItem(loadedProject);
                
                //now, go over the subdirectories looking for files
                loadedProject.BaseFolder.RefreshTreeList(GUIManager.FileController.GetSupportedFileExtensions());

                ProjectIO.Save(loadedProject, projectFilePath + "p");

                return loadedProject;

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return null;
        }*/

        #endregion

        public FileItem GetFileItemByGuid(string guid)
        {
            return _baseFolder.GetFileNameByGuid(guid);
        }

        public ProjectItem GetProjectItemByGuid(string guid)
        {
            return _baseFolder.GetProjectItemByGuid(guid);
        }

        public ProjectItem GetSubProjectItem(string projectRelativePath)
        {
            return BaseFolder.GetSubProjectItem(projectRelativePath);
        }

        public FileItem GetSubFileItem(string projectRelativePath)
        {
            return BaseFolder.GetSubFileItem(projectRelativePath);
        }



        public FolderItem GetSubFolderItem(string projectRelativePath)
        {
            return BaseFolder.GetSubFolderItem(projectRelativePath);
        }

        #region Folder-Related Properties

        public string Name
        {
            get { return _name; }
        }



        public string FolderPath
        {
            get { return _folderPath; }
        }



        public string ProjectFilePath
        {
            get { return Path.Combine(_folderPath, Name + "." + FileExtension); }
        }



        public FolderItem BaseFolder
        {
            get { return _baseFolder; }
        }



        /*public FolderItem TempFolder
        {
            get { return _tempFolder; }
        }*/



        /*public List<ProjectItemReference> ProjectItemReferences
        {
            get { return _baseFolder.SubItems.Select(val => new ProjectItemReference(val)).ToList(); }
        }*/

        #endregion

        #region Accessors

        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }



        public string Company
        {
            get { return _company; }
            set { _company = value; }
        }



        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }



        public List<string> OpenedFiles
        {
            get { return _openedFiles; }
            set { _openedFiles = value; }
        }



        public List<string> ExportLocations
        {
            get { return _exportLocations; }
            set { _exportLocations = value; }
        }



        internal IServiceLocator Services
        {
            get { return _services; }
        }

        #endregion
    }
}