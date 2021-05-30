using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Annotations;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.ProjectExplorer.Services
{
    [ProjectExplorerService]
    public class FileFolderMenuManager : IServiceable
    {
        private MessageManager _messageManager;
        private ProjectExplorerWindow _projectExplorerWindow;



        public void Initialize(IServiceLocator services)
        {
            _messageManager = services.Get<MessageManager>();
            _projectExplorerWindow = services.Get<ProjectExplorerWindow>();

            var fileContextMenuManager = services.Get<ContextMenuManager>("FileProject");
            
            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Open", MenuItemOpenFileClick) {Icon = EmbeddedResources.Load<Texture2D>("Resources/document.png") });
            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Open Folder in Explorer", MenuItemOpenFolderExplorerClick){Icon = EmbeddedResources.Load<Texture2D>( "Resources/computer_16x16.png") });
            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Copy Path", MenuItemCopyPathClick) { BeginGroup = true, Icon = EmbeddedResources.Load<Texture2D>("Resources/Wizard_16x16.png") });


            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Rename", MenuItemRenameFileClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/rename16x16.png") });
            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Duplicate", MenuItemDuplicateFileClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardCopy_16x16.png") });
            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Exclude", MenuItemExcludeClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/cancel.png") });
            fileContextMenuManager.RegisterEntry(new ContextMenuEntry("Delete", MenuItemDeleteClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/Trash_16x16.png") });


            var folderContextMenuManager = services.Get<ContextMenuManager>("FolderProject");

            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Add"){Icon = EmbeddedResources.Load<Texture2D>("Resources/Plus_16x16.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Add/New Item...", MenuItemCreateClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/document.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Add/Existing Item...", MenuItemImportClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/import16x16.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Add/New Folder", MenuItemNewFolderClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/folder.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Add/Existing Folder and Contents...", MenuItemImportFolderClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/import16x16.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Add/Reload Folder...", MenuItemReloadFolderClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/import16x16.png") });

            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Open in Explorer", MenuItemOpenFolderExplorerClick) { Icon = EmbeddedResources.Load<Texture2D>("Resources/computer_16x16.png") });
            
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Copy Path", MenuItemCopyPathClick, HasParentFolder) { BeginGroup = true, Icon = EmbeddedResources.Load<Texture2D>("Resources/Wizard_16x16.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Rename", MenuItemRenameFileClick, HasParentFolder) { Icon = EmbeddedResources.Load<Texture2D>("Resources/rename16x16.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Exclude", MenuItemExcludeClick, HasParentFolder) { Icon = EmbeddedResources.Load<Texture2D>("Resources/cancel.png") });
            folderContextMenuManager.RegisterEntry(new ContextMenuEntry("Delete", MenuItemDeleteClick, HasParentFolder) { Icon = EmbeddedResources.Load<Texture2D>("Resources/Trash_16x16.png") });
        }


        private bool HasParentFolder(Object content)
        {
            var item = (SelectableTreeViewItem) content;
            FolderItem folderItem = (FolderItem)item.UserData;

            return folderItem.ParentFolder != null;
        }


        private void MenuItemCopyPathClick(Object content)
        {
            var treeViewItem = (SelectableTreeViewItem)content;
            var projectItem = (ProjectItem)treeViewItem.UserData;

            ClipboardHelper.Copy(projectItem.FullPath);
        }


        private void MenuItemOpenFileClick(Object content)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem)content;
            var fileItem = item.UserData as FileItem;
            if (fileItem != null)
            {
                _messageManager.Publish(new OpenFile(fileItem));
            }
        }

        private void MenuItemOpenFolderExplorerClick(Object content)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem)content;
            var projectItem = (ProjectItem)item.UserData;

            if (projectItem is FolderItem)
            {
                UrlHelper.OpenUrlInBrowser(projectItem.FullPath);
            }
            else
            {
                UrlHelper.OpenUrlInBrowser(projectItem.FullContainerPath);
            }
        }


        private void MenuItemRenameFileClick(Object content)
        {
            var treeViewItem = (SelectableTreeViewItem)content;

            _projectExplorerWindow.RenameProjectItem(treeViewItem);
        }


        private void MenuItemDuplicateFileClick(Object content)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem)content;

            _projectExplorerWindow.Duplicate(item);
        }


        private void MenuItemDeleteClick(Object content)
        {
            _projectExplorerWindow.DeleteFiles();
        }


        private void MenuItemExcludeClick(Object content)
        {
            _projectExplorerWindow.ExcludeFiles();
        }


        private void MenuItemCreateClick(Object content)
        {
            SelectableTreeViewItem folderTreeItem = (SelectableTreeViewItem)content;

            _projectExplorerWindow.StartCreateItem(folderTreeItem);
        }


        private void MenuItemImportClick(Object content)
        {
            var treeViewItem = (SelectableTreeViewItem)content;

            _projectExplorerWindow.StartImport(treeViewItem);
        }


        private void MenuItemNewFolderClick(Object content)
        {
            _projectExplorerWindow.StartNewFolder((SelectableTreeViewItem)content);
        }



        private void MenuItemImportFolderClick(Object content)
        {
            _projectExplorerWindow.StartImportFolder((SelectableTreeViewItem)content);
        }


        private void MenuItemReloadFolderClick(Object content)
        {
            _projectExplorerWindow.StartReloadFolder((SelectableTreeViewItem)content);
        }
    }
}
