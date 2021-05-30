using System;
using System.Collections.Generic;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    /// <summary>
    /// A project item is some file or folder that can be assigned to a project.
    /// </summary>
    public abstract class ProjectItem
    {
        private Guid _guid;
        private Dictionary<String, String> _properties = new Dictionary<string, string>();

        /// <summary>
        /// File name (with extension, if applicable)
        /// </summary>
        public virtual String FileName
        {
            get { return Name; }
        }


        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }


        public Project Project
        {
            get;
            protected set;
        }



        /// <summary>
        /// The name of the file (or folder), without extensions of any sort
        /// </summary>
        public String Name
        {
            get;
            protected set;
        }



        /// <summary>
        /// The full file path (with extension, if applicable)
        /// </summary>
        public abstract String FullPath
        {
            get;
        }



        /// <summary>
        /// The full path of the direttory where the file (or folder) is contained
        /// </summary>
        public virtual String FullContainerPath
        {
            get;
            set;
        }



        /// <summary>
        /// The project relative file path (with extension, if applicable)
        /// </summary>
        public abstract String ProjectRelativePath
        {
            get;
        }



        /// <summary>
        /// The project relative path of the directory where the file (or folder) is contained
        /// </summary>
        public abstract String ProjectRelativeContainerPath
        {
            get;
        }



        /// <summary>
        /// The type of item being handled ("File" or "Folder")
        /// </summary>
        public abstract String Nature
        {
            get;
        }



        public FolderItem ParentFolder
        {
            get;
            set;
        }



        public abstract bool ExistsPhysically
        {
            get;
        }



        /// <summary>
        /// Removes himself from the project and filesystem
        /// </summary>
        public abstract void DeleteFromProject();



        public abstract void Rename(string newName);


        public abstract void MoveTo(FolderItem item);


        public bool MatchesGuid(string guidString)
        {
            return _guid.ToString() == guidString;
        }


        public void ExcludeFromProject()
        {
            //remove the parentFolderItem from the owner list
            ParentFolder.SubItems.Remove(this);

            Project.Save();
        }



        public Dictionary<string, string> Properties
        {
            get { return _properties; }
        }
    }
}