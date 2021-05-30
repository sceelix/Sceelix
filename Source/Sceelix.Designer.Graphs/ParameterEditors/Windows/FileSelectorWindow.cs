using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Utils;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.ParameterEditors.Windows
{
    public class FileSelectorWindow : DialogWindow
    {
        private readonly FileExtensionInfo[] _extensionFilter;
        private readonly Project _project;
        private readonly TreeView _treeView;
        private IServiceLocator _services;


        public FileSelectorWindow(IServiceLocator services, Project project, FileExtensionInfo[] extensionFilter)

        {
            _services = services;
            _project = project;
            _extensionFilter = extensionFilter;

            Width = 300;
            Height = 500;
            Title = "Choose Project File";

            var windowStackPanel = new FlexibleStackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            _treeView = new TreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            windowStackPanel.Children.Add(new ScrollViewer
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = _treeView
            });

            var dropdown = new DropDownButton()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 25
            };
            if (extensionFilter.Any())
            {
                dropdown.Items.Add("Supported Files (" + String.Join(",", extensionFilter.SelectMany(x => x.Extensions)) + ")");
                
                //add each of the extensions
                dropdown.Items.AddRange(extensionFilter.Select(x => x.Title + " (" + String.Join(",", x.Extensions) + ")"));
            }
            
            //add the "all" selector"
            dropdown.Items.Add("All files (*.*)");
            dropdown.SelectedIndex = 0;
            var selectedIndexProperty = dropdown.Properties.Get<int>("SelectedIndex");
            selectedIndexProperty.Changed += delegate(object sender, GamePropertyEventArgs<int> args)
            {
                RefreshTreeList(args.NewValue);
            };
            windowStackPanel.Children.Add(dropdown);

            

            var buttonPanel = new StackPanel() {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 30, Margin = new Vector4F(10)};
            {
                TextButton okButton, cancelButton;
                StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};

                panel.Children.Add(okButton = new TextButton() {Width = 95, Height = 25, Text = "OK", Margin = new Vector4F(5)});
                okButton.Click += delegate
                {
                    var userData = _treeView.SelectedItem.UserData as FileItem;
                    if (userData != null)
                    {
                        SelectedPath = userData.ProjectRelativePath;
                        Accept();
                    }
                };

                panel.Children.Add(cancelButton = new TextButton() {Width = 95, Height = 25, Text = "Cancel", Margin = new Vector4F(5)});
                cancelButton.Click += delegate { Cancel(); };

                buttonPanel.Children.Add(panel);
            }

            windowStackPanel.Children.Add(buttonPanel);

            Content = windowStackPanel;
        }



        public String SelectedPath
        {
            get;
            set;
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            RefreshTreeList(0);
        }



        private void RefreshTreeList(int selectedIndex)
        {
            //our list has 2 more items than the number of items in _extensionFilter
            var relativeIndex = selectedIndex - 1;

            //if there are no filters or
            //if the last item is chosen (we suppose we have no filters), show everything
            if (_extensionFilter.Length == 0 || relativeIndex == _extensionFilter.Length)
                RefreshTreeList(null);

            //if the first item is chosen, consider ALL the possible filters
            else if(relativeIndex < 0)
                RefreshTreeList(_extensionFilter);
            
            //otherwise show only the selected filter
            else
                RefreshTreeList(new [] { _extensionFilter[relativeIndex] });
        }


        private void RefreshTreeList(FileExtensionInfo[] extensionFilter)
        {
            _treeView.Items.Clear();

            LoadNodesToTree(_project.BaseFolder, _treeView, extensionFilter);

            //expand the root node
            _treeView.GetTreeViewItemByUserData(_project.BaseFolder).IsExpanded = true;

            if (!String.IsNullOrWhiteSpace(SelectedPath))
            {
                var subFileItem = _project.GetSubFileItem(SelectedPath);

                var treeViewItem = (SelectableTreeViewItem) _treeView.GetTreeViewItemByUserData(subFileItem);
                if (treeViewItem != null)
                {
                    _treeView.ExpandParents(treeViewItem);
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }
            }
        }



        private void LoadNodesToTree(ProjectItem projectItem, ITreeViewControl parent, FileExtensionInfo[] extensionFilter)
        {
            //if the item is a folder, we have to go over its subitems
            FolderItem folderItem = projectItem as FolderItem;
            if (folderItem != null)
            {
                SelectableTreeViewItem treeViewItem = new SelectableTreeViewItem {Text = projectItem.FileName, Texture = EmbeddedResources.Load<Texture2D>("Resources/folder3.png"), UserData = projectItem};
                parent.Items.Add(treeViewItem);

                //on double click, change the expanded state
                treeViewItem.DoubleClick += (sender, args) => treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                

                foreach (ProjectItem subItem in folderItem.SubItems)
                    LoadNodesToTree(subItem, treeViewItem, extensionFilter);

                treeViewItem.IsExpanded = folderItem.Expanded;
            }
            else
            {
                var fileItem = (FileItem) projectItem;

                //if the extension has any items _extensionFilter.Any() && 
                //if the extension is not within the requested list, ignore this fileitem
                if (extensionFilter != null && !_extensionFilter.Any(x => x.FitsExtension(fileItem.ProjectRelativePath)))
                    return;

                IFileHandler fileHandler = _services.Get<FileHandlerManager>().GetFileHandler(fileItem);

                Texture2D icon = fileHandler != null ? fileHandler.Icon16x16 : EmbeddedResources.Load<Texture2D>("Resources/document.png");

                SelectableTreeViewItem fileTreeViewItem = new SelectableTreeViewItem {Text = projectItem.FileName, Texture = icon, UserData = projectItem};
                parent.Items.Add(fileTreeViewItem);

                fileTreeViewItem.DoubleClick += delegate
                {
                    SelectedPath = fileItem.ProjectRelativePath;
                    Accept();
                };
            }
        }
    }
}