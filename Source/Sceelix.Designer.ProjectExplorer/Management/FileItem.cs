using System;
using System.IO;
using System.Runtime.InteropServices;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Helpers;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    public sealed class FileItem : ProjectItem
    {
        private readonly string _extension;
        



        /// <summary>
        /// Creates an instance of FileItem with the given name, extension and parent folder item.
        /// 
        /// Note: this does not add this object as a child of the folder item. That has to be done afterwards, if desired.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="extension"></param>
        /// <param name="folderItem"></param>
        public FileItem(string name, string extension, FolderItem folderItem)
        {
            Name = name;
            _extension = extension.ToLower();
            ParentFolder = folderItem;
            Guid = Guid.NewGuid();

            Project = folderItem.Project;
        }



        /// <summary>
        /// Creates an instance of FileItem with the given filename, guid and parent folder item.
        /// 
        /// Note: this does not add this object as a child of the folder item. That has to be done afterwards, if desired.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="extension"></param>
        /// <param name="folderItem"></param>
        public FileItem(string fileName, FolderItem folderItem, Guid guid)
        {
            Name = Path.GetFileNameWithoutExtension(fileName);
            _extension = Path.GetExtension(fileName).ToLower();
            Guid = guid;
            ParentFolder = folderItem;

            Project = folderItem.Project;
        }



        public override void DeleteFromProject()
        {
            //warn any interested components
            Project.Services.Get<MessageManager>().Publish(new FileDeleted(this));

            File.Delete(FullPath);

            //remove the parentFolderItem from the owner list
            ParentFolder.SubItems.Remove(this);

            Project.Save();
        }



        public override void Rename(string newName)
        {
            //save the path of the file before renaming
            String oldPath = FullPath;
            String newPath = PathHelper.Combine(FullContainerPath, newName + Extension);

            //rename the file in the file system
            File.Move(oldPath, newPath);

            Name = newName;

            //warn any interested components
            Project.Services.Get<MessageManager>().Publish(new FileRenamed(this));
            Project.Services.Get<MessageManager>().Publish(new ProjectItemMoved(this, oldPath, FullPath));
        }



        public FileItem Duplicate()
        {
            String duplicateNameSuggestion = Name;
            int index = 1;
            while (ParentFolder.ContainsFileWithName(duplicateNameSuggestion + "_" + index + Extension))
                index++;

            FileItem duplicateItem = new FileItem(duplicateNameSuggestion + "_" + index, Extension, ParentFolder);
            ParentFolder.AddItem(duplicateItem);

            //try to fetch the duplicate function if defined in a file handler, or use the default file copy function
            var fileHandler = Project.Services.Get<FileHandlerManager>().GetFileHandler(this);
            if (fileHandler != null)
            {
                fileHandler.Duplicate(this, duplicateItem);
                duplicateItem.Guid = fileHandler.GetGuid(duplicateItem) ?? Guid.NewGuid();
            }
            else
            {
                File.Copy(FullPath, duplicateItem.FullPath);
                duplicateItem.Guid = Guid.NewGuid();
            }

            //File.Copy(FullPath, duplicateItem.FullPath);

            return duplicateItem;
        }



        public override void MoveTo(FolderItem item)
        {
            String oldPath = FullPath;
            String destination = PathHelper.Combine(item.FullPath, FileName);

            if (destination == FullPath)
                return;

            if (File.Exists(destination))
                File.Delete(destination);

            File.Move(FullPath, destination);

            //now 
            ParentFolder.SubItems.Remove(this);
            ParentFolder = item;
            ParentFolder.SubItems.Add(this);

            Project.Services.Get<MessageManager>().Publish(new ProjectItemMoved(item, oldPath, destination));
            /*if (File.Exists(origin))
                File.Delete(FullPath);
            
            */
        }



        

        #region Properties

        public override String FileName
        {
            get { return Name + Extension; }
        }



        /// <summary>
        /// Extension of the file, including the "."
        /// </summary>
        public String Extension
        {
            get { return _extension; }
        }



        public override String FullPath
        {
            get { return PathHelper.Combine(FullContainerPath, FileName); }
        }



        public override String FullContainerPath
        {
            get { return ParentFolder.FullPath; }
        }



        public override String ProjectRelativePath
        {
            get { return PathHelper.Combine(ProjectRelativeContainerPath, FileName); }
        }


        public String ProjectRelativePathWithoutExtension
        {
            get { return PathHelper.Combine(ProjectRelativeContainerPath, Name); }
        }


        public override String ProjectRelativeContainerPath
        {
            get { return ParentFolder.ProjectRelativePath; }
        }



        public override string Nature
        {
            get { return "File"; }
        }

        public override bool ExistsPhysically
        {
            get { return File.Exists(FullPath); }
        }

        #endregion
    }
}