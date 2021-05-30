using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.ParameterEditors.Windows
{
    public class FolderSelectorWindow : DialogWindow
    {
        private readonly Project _project;
        private readonly TreeView _treeView;



        public FolderSelectorWindow(Project project)
        {
            _project = project;

            Width = 300;
            Height = 500;
            Title = "Choose Project Folder";

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
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, //at least for now, this will avoid the Digitalrune Scrollviewer issue
                Content = _treeView
            });

            var buttonPanel = new StackPanel() {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 30, Margin = new Vector4F(10)};
            {
                TextButton okButton, cancelButton;
                StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};

                panel.Children.Add(okButton = new TextButton() {Width = 95, Height = 25, Text = "OK", Margin = new Vector4F(5)});
                okButton.Click += delegate
                {
                    var userData = _treeView.SelectedItem.UserData as FolderItem;
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

            RefreshTreeList();
        }



        private void RefreshTreeList()
        {
            foreach (var projectItem in _project.BaseFolder.SubItems)
                LoadNodesToTree(projectItem, _treeView);


            if (!String.IsNullOrWhiteSpace(SelectedPath))
            {
                var subFileItem = _project.GetSubFolderItem(SelectedPath);

                var treeViewItem = (SelectableTreeViewItem) _treeView.GetTreeViewItemByUserData(subFileItem);
                if (treeViewItem != null)
                {
                    _treeView.ExpandParents(treeViewItem);
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }
            }
        }



        private void LoadNodesToTree(ProjectItem projectItem, ITreeViewControl parent)
        {
            //if the item is a folder, we have to go over its subitems
            FolderItem folderItem = projectItem as FolderItem;
            if (folderItem != null)
            {
                SelectableTreeViewItem treeViewItem = new SelectableTreeViewItem {Text = projectItem.FileName, Texture = EmbeddedResources.Load<Texture2D>("Resources/folder3.png"), UserData = projectItem};
                parent.Items.Add(treeViewItem);

                treeViewItem.DoubleClick += delegate
                {
                    SelectedPath = folderItem.ProjectRelativePath;
                    Close();
                };

                foreach (ProjectItem subItem in folderItem.SubItems)
                    LoadNodesToTree(subItem, treeViewItem);

                treeViewItem.IsExpanded = folderItem.Expanded;
            }
        }
    }
}