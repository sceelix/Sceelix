using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Helpers;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    public sealed class FolderItem : ProjectItem
    {
        private List<ProjectItem> _subItems = new List<ProjectItem>();



        public FolderItem(Project project)
        {
            Name = Path.GetFileName(project.FolderPath);
            Project = project;
            Guid = Guid.NewGuid();
        }



        public FolderItem(String name, FolderItem parentFolder)
        {
            Name = name;
            ParentFolder = parentFolder;
            Project = parentFolder.Project;
            Guid = Guid.NewGuid();
        }


        public FolderItem(string name, FolderItem parentFolder, Guid guid)
        {
            Name = name;
            ParentFolder = parentFolder;
            Project = parentFolder.Project;
            Guid = guid;
        }


        public override void DeleteFromProject()
        {
            var fileTree = GetFileItemTree().ToList();
            foreach (FileItem fileItem in fileTree)
            {
                //warn any interested components
                Project.Services.Get<MessageManager>().Publish(new FileDeleted(fileItem));
            }

            //delete from disk
            Directory.Delete(FullPath, true);

            //remove the parentFolderItem from the owner list
            ParentFolder.SubItems.Remove(this);

            Project.Save();
        }



        public override void Rename(string newName)
        {
            //save the path of the file before renaming
            String oldPath = FullPath;

            Name = newName;

            //rename the file in the file system
            Directory.Move(oldPath, FullPath);

            Project.Services.Get<MessageManager>().Publish(new ProjectItemMoved(this, oldPath, FullPath));
        }



        public override void MoveTo(FolderItem item)
        {
            String origin = FullPath;
            String destination = PathHelper.Combine(item.FullPath, Name);

            //if this fails, then the whole process will be interrupted
            Directory.Move(origin, destination);

            ParentFolder.SubItems.Remove(this);
            ParentFolder = item;
            ParentFolder.SubItems.Add(this);

            Project.Services.Get<MessageManager>().Publish(new ProjectItemMoved(this, origin, destination));
        }



        public bool ContainsFileWithName(String fileName)
        {
            return File.Exists(PathHelper.Combine(FullPath, fileName));
        }



        public bool ContainsFileItemWithFileName(String fileName)
        {
            return _subItems.OfType<FileItem>().Any(value => value.FileName == fileName);
        }



        public bool ContainsFileItemWithFilePath(String fileName)
        {
            return _subItems.OfType<FileItem>().Any(value => value.FileName == fileName);
        }



        public bool ContainsFolderItemWithName(String name)
        {
            return _subItems.OfType<FolderItem>().Any(value => value.Name == name);
        }



        public FileItem GetFileItemByFullPath(String fullpath)
        {
            return _subItems.OfType<FileItem>().FirstOrDefault(value => value.FullPath == fullpath);
        }



        public FolderItem GetFolderItemByFullPath(String fullpath)
        {
            return _subItems.OfType<FolderItem>().FirstOrDefault(value => value.FullPath == fullpath);
        }



        public FolderItem GetFolderItemByName(String name)
        {
            return _subItems.OfType<FolderItem>().FirstOrDefault(value => value.Name == name);
        }



        public bool ContainsFolderWithName(string folderName)
        {
            return Directory.Exists(PathHelper.Combine(FullPath, folderName));
        }



        public FolderItem CreateDirectoryItem(String name)
        {
            FolderItem newFolderItem = new FolderItem(name, this);

            //creates the physical location on the disk
            newFolderItem.CreatePhysicalDirectory();

            //adds the parentFolderItem to the tree
            SubItems.Add(newFolderItem);

            Project.Save();

            return newFolderItem;
        }



        public void AddItem(ProjectItem projectItem)
        {
            projectItem.ParentFolder = this;

            SubItems.Add(projectItem);
        }



        public void RemoveItem(ProjectItem projectItem)
        {
            SubItems.Remove(projectItem);
        }



        public bool IsAncestorOf(ProjectItem projectItem)
        {
            return projectItem.FullPath.Contains(FullPath);
        }



        public void CreatePhysicalDirectory()
        {
            Directory.CreateDirectory(FullPath);
        }



        public void SortItems()
        {
            _subItems = _subItems.OrderByDescending(val => val.Nature).ThenBy(val => val.FileName).ToList();
        }

        public ProjectItem GetProjectItemByGuid(string guid)
        {
            foreach (ProjectItem projectItem in SubItems)
            {
                if (projectItem.MatchesGuid(guid))
                    return projectItem;

                FolderItem subFolderItem = projectItem as FolderItem;
                if (subFolderItem != null)
                {
                    ProjectItem subProjectItem = subFolderItem.GetProjectItemByGuid(guid);
                    if (subProjectItem != null)
                        return subProjectItem;
                }
            }

            return null;
        }

        public FileItem GetFileNameByGuid(string guid)
        {
            foreach (ProjectItem projectItem in SubItems)
            {
                FolderItem subFolderItem = projectItem as FolderItem;
                if (subFolderItem != null)
                {
                    FileItem fileNameByGuid = subFolderItem.GetFileNameByGuid(guid);
                    if (fileNameByGuid != null)
                        return fileNameByGuid;
                }
                else if (projectItem is FileItem && ((FileItem) projectItem).MatchesGuid(guid))
                    return (FileItem) projectItem;
            }

            return null;
        }

        public ProjectItem GetSubProjectItem(string projectRelativePath)
        {
            //this could, of course, be optimized by looking at the path itself, but...
            foreach (ProjectItem projectItem in SubItems)
            {
                FolderItem subFolderItem = projectItem as FolderItem;
                if (subFolderItem != null)
                {
                    //if this is the item we are looking for, returning it
                    if (subFolderItem.ProjectRelativePath == projectRelativePath)
                        return projectItem;

                    //otherwise, if the folderitem's path indicates that path is inside it
                    //look recursively inside the folder
                    if (projectRelativePath.StartsWith(subFolderItem.ProjectRelativePath))
                    {
                        ProjectItem fileNameByGuid = subFolderItem.GetSubProjectItem(projectRelativePath);
                        if (fileNameByGuid != null)
                            return fileNameByGuid;
                    }
                }
                else if (projectItem is FileItem && projectItem.ProjectRelativePath == projectRelativePath)
                    return projectItem;
            }

            return null;
        }


        public FileItem GetSubFileItem(string projectRelativePath)
        {
            //this could, of course, be optimized by looking at the path itself, but...
            foreach (ProjectItem projectItem in SubItems)
            {
                FolderItem subFolderItem = projectItem as FolderItem;
                if (subFolderItem != null)
                {
                    FileItem fileNameByGuid = subFolderItem.GetSubFileItem(projectRelativePath);
                    if (fileNameByGuid != null)
                        return fileNameByGuid;
                }
                else if (projectItem is FileItem && ((FileItem) projectItem).ProjectRelativePath == projectRelativePath)
                    return (FileItem) projectItem;
            }

            return null;
        }



        public FolderItem GetSubFolderItem(string projectRelativePath)
        {
            if (ProjectRelativePath == projectRelativePath)
                return this;

            foreach (ProjectItem projectItem in SubItems)
            {
                FolderItem subFolderItem = projectItem as FolderItem;
                if (subFolderItem != null)
                {
                    FolderItem returnedFolderItem = subFolderItem.GetSubFolderItem(projectRelativePath);
                    if (returnedFolderItem != null)
                        return returnedFolderItem;
                }
            }

            return null;
        }



        public IEnumerable<FileItem> GetFileItemTree()
        {
            return GetFileItemTree(this);
        }



        private IEnumerable<FileItem> GetFileItemTree(FolderItem item)
        {
            foreach (FolderItem folderItem in item.SubItems.OfType<FolderItem>())
                foreach (FileItem fileItem in GetFileItemTree(folderItem))
                    yield return fileItem;

            foreach (FileItem fileItem in item.SubItems.OfType<FileItem>())
                yield return fileItem;
        }



        /*public void RefreshTreeList(String[] fileExtensions)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(FullPath);
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                FolderItem subDirItem = new FolderItem(subDirInfo.Name, this);
                SubItems.Add(subDirItem);
                subDirItem.RefreshTreeList(fileExtensions);
            }

            foreach (FileInfo subFileInfo in dirInfo.GetFiles())
            {
                FileItem fileItem = new FileItem(subFileInfo, this);

                if (fileExtensions.Contains(fileItem.Extension))
                    SubItems.Add(fileItem);
            }
        }*/

        #region Properties

        public override string FullPath
        {
            get { return ParentFolder != null ? PathHelper.Combine(FullContainerPath, Name) : PathHelper.ToUniversalPath(Project.FolderPath); }
        }



        public override string FullContainerPath
        {
            get { return ParentFolder != null ? ParentFolder.FullPath : PathHelper.ToUniversalPath(Project.FolderPath); }
        }



        public override string ProjectRelativePath
        {
            get { return ParentFolder != null ? PathHelper.Combine(ProjectRelativeContainerPath, Name) : String.Empty; }
        }



        public override string ProjectRelativeContainerPath
        {
            get { return ParentFolder != null ? ParentFolder.ProjectRelativePath : String.Empty; }
        }



        public override string Nature
        {
            get { return "Folder"; }
        }



        public override bool ExistsPhysically
        {
            get { return Directory.Exists(FullPath); }
        }



        public List<ProjectItem> SubItems
        {
            get { return _subItems; }
            set { _subItems = value; }
        }



        public IEnumerable<ProjectItem> Descendants
        {
            get { return _subItems.Union(_subItems.OfType<FolderItem>().SelectMany(x => x.Descendants)); }
        }



        public bool Expanded
        {
            get;
            set;
        }

        #endregion
    }
}