using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Annotations;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Annotations;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Helpers;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButtons = DigitalRune.Game.Input.MouseButtons;
using Orientation = DigitalRune.Game.UI.Orientation;
using TreeView = Sceelix.Designer.GUI.TreeViewControls.TreeView;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    // A resizable window with a ScrollViewer.
    [DesignerWindow("Project Explorer")]
    public class ProjectExplorerWindow : AnimatedWindow, IServiceable
    {
        //private TreeViewItem _lastSelectedItem;
        private FileHandlerManager _fileHandlerManager;
        private ProjectHistorySettings _projectHistorySettings;
        private Project _activeProject;
        private ProjectExplorerSettings _projectExplorerSettings;

        /// <summary>
        /// A useful tip for fighting the nasty OpenFileDialog hanging behavior: 
        /// http://wishmesh.com/2011/06/call-to-openfiledialog-or-savefiledialog-hangs-or-freezes/
        /// - For OpenFileDialog the ShowHelp property must be explicitly set.
        /// - For SaveFileDialog the ShowHelp, CreatePrompt, and OverwritePrompt properties must be explicitly set.
        /// </summary>
        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog() {Multiselect = true, ShowHelp = true};

        //for dragging and dropping
        private TreeViewItem _previouslyHoveredTreeItem;

        /// <summary>
        /// Menu item that has all recent project references below
        /// </summary>
        private MenuChild _recentMenuItem;

        private readonly Synchronizer _synchronizer = new Synchronizer();

        private TreeView _treeView;
        private IServiceLocator _services;
        
        private List<object> _projectExploreServices;

        private ContextMenuManager _folderContextMenuManager;
        private ContextMenuManager _fileContextMenuManager;


        public void Initialize(IServiceLocator services)
        {
            _services = services;
            _fileHandlerManager = services.Get<FileHandlerManager>();
            _projectHistorySettings = new ProjectHistorySettings();
            _projectExplorerSettings = services.Get<SettingsManager>().Get<ProjectExplorerSettings>();
            
            InitializeImport();

            services.Get<MessageManager>().Register<FileCreated>(OnFileCreated);
            services.Get<MessageManager>().Register<DocumentActivated>(OnDocumentActivated);
            services.Get<MessageManager>().Register<FileDragEnter>(OnFileDragEnter);
            services.Get<MessageManager>().Register<FileDragDrop>(OnFileDragDrop);

            ServiceManager serviceManager = new ServiceManager(services);
            serviceManager.Register(this);
            serviceManager.Register(_folderContextMenuManager = new ContextMenuManager(), "FolderProject");
            serviceManager.Register(_fileContextMenuManager = new ContextMenuManager(), "FileProject");
            
            _projectExploreServices = AttributeReader.OfAttribute<ProjectExplorerServiceAttribute>().GetInstancesOfType<Object>();
            foreach (var projectExplorerService in _projectExploreServices)
                serviceManager.Register(projectExplorerService.GetType(), projectExplorerService);
            
            foreach (var projectExplorerService in _projectExploreServices.OfType<IServiceable>())
                projectExplorerService.Initialize(serviceManager);
        }



        private void OnFileDragEnter(FileDragEnter obj)
        {
            obj.DragEventArgs.Effect = IsMouseOver ? DragDropEffects.Copy : DragDropEffects.None;

            var targetTreeItem = _treeView.GetMouseHoveredItem() as ProjectTreeItem;
            if (targetTreeItem != null)
            {
                if (_previouslyHoveredTreeItem != null)
                    _previouslyHoveredTreeItem.Header.Background = Color.Transparent;

                var targetFolderItem = targetTreeItem.UserData as FolderItem;
                if (targetFolderItem != null)
                {
                    targetTreeItem.Header.Background = Color.DarkBlue;

                    _previouslyHoveredTreeItem = targetTreeItem;
                }
            }
        }


        private void OnFileDragDrop(FileDragDrop fileDragDrop)
        {
            if (_previouslyHoveredTreeItem != null)
            {
                _previouslyHoveredTreeItem.Header.Background = Color.Transparent;

                var folderItem =_previouslyHoveredTreeItem.UserData as FolderItem;
                if (folderItem != null)
                {
                    var folderTreeItem = _previouslyHoveredTreeItem;
                    
                    ImportFilesWindow filesWindow = new ImportFilesWindow(_services, folderItem, fileDragDrop.Paths.ToArray());
                    filesWindow.Show(Screen);
                    filesWindow.Closed += delegate
                    {
                        //sort the items
                        folderItem.SortItems();

                        //refresh the treeviewnode
                        RefreshTreeItemChildren(folderTreeItem);

                        folderTreeItem.IsExpanded = true;

                        ActiveProject.Save();
                    };
                }

                _previouslyHoveredTreeItem = null;
            }
        }



        private void OnDocumentActivated(DocumentActivated obj)
        {
            if (_projectExplorerSettings.TrackDocumentArea.Value)
            {
                var treeViewItem = _treeView.GetTreeViewItemByUserData(obj.FileItem) as SelectableTreeViewItem;
                if (treeViewItem != null)
                {
                    //expand items until this one
                    foreach (var parentItem in _treeView.GetParentItems(treeViewItem))
                        parentItem.IsExpanded = true;

                    //deselect all other files
                    foreach (var item in _treeView.GetFlatTreeItems().OfType<SelectableTreeViewItem>())
                        item.IsSelected = false;

                    //select this one and focus on it, so that it is visible in the window
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }
            }
        }



        public Project ActiveProject
        {
            get { return _activeProject; }
        }



        private void OnFileCreated(FileCreated obj)
        {
            CreateFileNode(obj.Item, _treeView.GetTreeViewItemByUserData(obj.Item.ParentFolder));
            ActiveProject.Save();
        }



        public void InitializeImport()
        {
            //group the importers by the filePath. For instance, all images all belong to the same "Image" group
            IEnumerable<IGrouping<string, IFileHandler>> groups = _services.Get<FileHandlerManager>().FileHandlers.GroupBy(val => val.ItemName);

            String filterString = String.Empty;
            String allfilterString = String.Empty;
            foreach (IGrouping<string, IFileHandler> grouping in groups)
            {
                if (filterString != String.Empty)
                    filterString += "|";

                //filterString += grouping.Key + " Files|";

                //now list all the extensions. For instance, *.bmp;*.jpg;*.jpeg;*.png;*.tif;
                String extensions = String.Empty;
                foreach (String extension in grouping.SelectMany(x => x.Extensions))
                {
                    if (extensions != String.Empty)
                        extensions += ";";

                    if (allfilterString != String.Empty)
                        allfilterString += ";";

                    extensions += "*" + extension;
                    allfilterString += "*" + extension;
                }

                filterString += grouping.Key + " Files (" + extensions + ") |" + extensions;
            }

            if (!String.IsNullOrWhiteSpace(allfilterString))
                _openFileDialog.Filter = "All Supported Files (" + allfilterString + ")|" + allfilterString + "|" + filterString + "|All Files (*.*)|*.*";
        }



        protected override void OnLoad()
        {
            Title = "Project Explorer";
            Width = 300;
            Height = 500;
            CanResize = true;
            
            var windowStackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            BarMenu barMenu = new BarMenu();

            barMenu.MenuChildren.Add(new MenuChild(CreateNewProject) {Text = "New..", Icon = EmbeddedResources.Load<Texture2D>("Resources/Luggage_16x16.png"), ToolTip = new ToolTipControl("Creates a new project.")});
            
            var openProjectMenu = new MenuChild() {Text = "Open..", Icon = EmbeddedResources.Load<Texture2D>("Resources/folder.png")};
            barMenu.MenuChildren.Add(openProjectMenu);

            openProjectMenu.MenuChildren.Add(new MenuChild(OpenExistingProject) { Text = "Existing Project", Icon = EmbeddedResources.Load<Texture2D>("Resources/folder.png") });
            openProjectMenu.MenuChildren.Add(_recentMenuItem = new MenuChild() {Text = "Recent..", Icon = EmbeddedResources.Load<Texture2D>("Resources/folder.png")});
            openProjectMenu.MenuChildren.Add(new MenuChild((obj) => LoadTutorialProject()) { Text = "Tutorial Samples", Icon = EmbeddedResources.Load<Texture2D>("Resources/folder.png"),BeginGroup = true});
            openProjectMenu.MenuChildren.Add(new MenuChild((obj) => CloseProject()) { Text = "Close", Icon = EmbeddedResources.Load<Texture2D>("Resources/folder.png"), BeginGroup = true });

            UpdateRecentMenuItemEntries();

            _treeView = new TreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _treeView.InputProcessed += TreeViewOnInputProcessed;

            windowStackPanel.Children.Add(barMenu);
            windowStackPanel.Children.Add(new VerticalScrollViewer()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, //at least for now, this will avoid the Digitalrune Scrollviewer issue
                Content = _treeView
            });

            Content = windowStackPanel;

            _synchronizer.Enqueue(() => LoadLastOrTutorialProject(_projectHistorySettings));

            base.OnLoad();
        }
        


        private void UpdateRecentMenuItemEntries()
        {
            _recentMenuItem.MenuChildren.Clear();

            foreach (string projectName in _projectHistorySettings.ProjectNames)
            {
                //discard the projects that don't exist anymore
                if (File.Exists(projectName))
                {
                    var name = projectName;

                    //create an entry that will load the corresponding project
                    _recentMenuItem.MenuChildren.Add(new MenuChild(
                        (child) =>
                        {
                            LoadProject(name);
                            UpdateRecentMenuItemEntries();
                        })
                    {
                        Text = projectName
                    });
                }
            }
        }



        private void LoadLastOrTutorialProject(ProjectHistorySettings projectHistorySettings)
        {
            //if we have a new version, we have new tutorial samples
            if (projectHistorySettings.IsNewVersion)
            {
                //if a previous project was loaded, ask first
                var lastProject = _projectHistorySettings.GetLastProject();
                if (!String.IsNullOrWhiteSpace(lastProject))
                {
                    MessageWindow messageWindow = new MessageWindow()
                    {
                        MessageIcon = MessageWindowIcon.Question,
                        Text = "It is the first time you are running this Sceelix version.\nWould you like to load the tutorial sample project?",
                        Title = "Load Tutorials",
                        Buttons = new[] {"Yes", "No"}
                    };
                    messageWindow.Click += delegate
                    {
                        if (messageWindow.Selection == "Yes")
                        {
                            LoadTutorialProject();
                        }
                        else
                        {
                            RestoreLastProject();
                        }
                    };
                    messageWindow.Show(Screen);
                }
                else
                {
                    //don't ask, just load
                    LoadTutorialProject();
                }
            }
            else
            {
                //otherwise, try to load the last project
                RestoreLastProject();
            }
        }



        private void LoadTutorialProject()
        {
            if (File.Exists(Project.DefaultTutorialFile))
                LoadProject(Project.DefaultTutorialFile);
            else
            {
                ExtractFolderWindow filesWindow = new ExtractFolderWindow(Project.DefaultTutorialZip, SceelixApplicationInfo.ExtrasFolder);
                filesWindow.Show(Screen);
                filesWindow.Accepted += delegate { LoadProject(Project.DefaultTutorialFile); };
            }
        }



        private void RestoreLastProject()
        {
            var lastProject = _projectHistorySettings.GetLastProject();
            if (!String.IsNullOrWhiteSpace(lastProject))
                LoadProject(lastProject);
        }



        private void TreeViewOnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            ProcessInternalDragAndDrop();
            ProcessKeyboardInteractions();
        }



        private void ProcessKeyboardInteractions()
        {
            if (!InputService.IsKeyboardHandled && IsActive)
            {
                if (InputService.IsPressed(Keys.Delete, false))
                {
                    InputService.IsKeyboardHandled = true;

                    DeleteFiles();
                }
                else if (InputService.IsPressed(Keys.Enter, false))
                {
                    var messageManager = _services.Get<MessageManager>();
                    foreach (var fileItem in GetOrderedSelectedItems().Select(x => (ProjectItem) x.UserData).OfType<FileItem>())
                    {
                        messageManager.Publish(new OpenFile(fileItem));
                    }

                    InputService.IsKeyboardHandled = true;
                }
                else if (InputService.IsPressed(Keys.F2, false))
                {
                    var projectTreeItems = GetOrderedSelectedItems().ToList();
                    if (projectTreeItems.Count == 1)
                        RenameProjectItem(projectTreeItems.First());

                    InputService.IsKeyboardHandled = true;
                }
            }
        }



        public void DeleteFiles()
        {
            var projectTreeItems = GetOrderedSelectedItems().ToList();
            if (projectTreeItems.Any())
            {
                String text, title;

                if (projectTreeItems.Count == 1)
                {
                    var item = (ProjectItem) projectTreeItems[0].UserData;

                    text = "Are you sure you want to delete \"" + item.Name + "\"?";
                    title = "Delete " + item.Nature;
                }

                else
                {
                    text = "Are you sure you want to delete all these items?";
                    title = "Delete Multiple Items";
                }

                MessageWindow messageWindow = new MessageWindow()
                {
                    MessageIcon = MessageWindowIcon.Question,
                    Text = text,
                    //Text = "Again, we set the axis parameter to specify the scope axis on which\n the handle should lie. The depth attribute of the model behaves differently than the width or height attributes. It scales the depth of the cube around a central point. To accommodate this, we use the parameter skin=diameterArrow. This creates a handle with different drag behavior, and two orange arrows that can both be used to change the depth of the cube model.",
                    Title = title,
                    Buttons = new[] {"Yes", "No"}
                };
                messageWindow.Click += delegate
                {
                    if (messageWindow.Selection == "Yes")
                    {
                        foreach (var projectTreeItem in projectTreeItems)
                        {
                            projectTreeItem.Parent.Items.Remove(projectTreeItem);
                            projectTreeItem.UserData.CastTo<ProjectItem>().DeleteFromProject();
                        }
                    }
                };
                messageWindow.Show(this);
            }
        }



        private void ProcessInternalDragAndDrop()
        {
            var draggedData = _treeView.DraggedData as ProjectTreeItem[];
            if (draggedData != null)
            {
                var targetTreeItem = _treeView.GetMouseHoveredItem() as ProjectTreeItem;
                if (targetTreeItem != null)
                {
                    if (_previouslyHoveredTreeItem != null)
                        _previouslyHoveredTreeItem.Header.Background = Color.Transparent;

                    var targetFolderItem = targetTreeItem.UserData as FolderItem;
                    //if the target
                    if (!draggedData.Contains(targetTreeItem) && targetFolderItem != null)
                    {
                        targetTreeItem.Header.Background = Color.DarkBlue;

                        if (InputService.IsReleased(MouseButtons.Left))
                        {
                            var orderedSelectedItems = FilterSelectedItems(draggedData).ToList();
                            bool isError = false;

                            foreach (var selectedItem in orderedSelectedItems)
                            {
                                var folderItem = selectedItem.UserData as FolderItem;
                                if (folderItem != null && folderItem.IsAncestorOf(targetFolderItem))
                                {
                                    MessageWindow messageWindow = new MessageWindow()
                                    {
                                        MessageIcon = MessageWindowIcon.Error,
                                        Text = "Cannot move '" + folderItem.Name + "'. The destination folder is a subfolder of the source folder.",
                                        Title = "Error moving folder.",
                                        Buttons = new[] {"OK"}
                                    };
                                    messageWindow.Show(Screen);

                                    isError = true;
                                }
                            }

                            if (!isError)
                            {
                                try
                                {
                                    
                                    foreach (var orderedSelectedItem in orderedSelectedItems)
                                    {
                                        var projectItem = (ProjectItem) orderedSelectedItem.UserData;
                                        projectItem.MoveTo(targetFolderItem);
                                    }

                                    targetFolderItem.SortItems();

                                    RefreshTreeList();
                                }
                                catch (Exception ex)
                                {
                                    MessageWindow messageWindow = new MessageWindow()
                                    {
                                        MessageIcon = MessageWindowIcon.Error,
                                        Text = "Cannot move files:" + ex.Message,
                                        Title = "Error moving folder.",
                                        Buttons = new[] {"OK"}
                                    };
                                    messageWindow.Show(Screen);
                                }
                            }
                        }

                        _previouslyHoveredTreeItem = targetTreeItem;
                    }
                }

                if (InputService.IsReleased(MouseButtons.Left))
                {
                    if (_previouslyHoveredTreeItem != null)
                        _previouslyHoveredTreeItem.Header.Background = Color.Transparent;

                    _services.Get<MessageManager>().Publish(new ProjectItemsDropped(draggedData.Select(x => (ProjectItem)x.UserData).ToArray()));

                    _treeView.DraggedData = null;
                    Screen.UIService.Cursor = Screen.Renderer.GetCursor("Arrow");
                }
            }
            else
            {
                //Start dragging here
                if (InputService.IsDown(MouseButtons.Left) && !InputService.MousePositionDelta.IsNumericallyZero)
                {
                    var treeViewItems = GetOrderedSelectedItems().ToArray();

                    if (treeViewItems.Any() && treeViewItems.Any(x => x.IsMouseOver))
                    {
                        _treeView.DraggedData = treeViewItems;
                        Screen.UIService.Cursor = Screen.Renderer.GetCursor("ArrowCarry");
                    }
                }
            }
        }



        /*private IEnumerable<ProjectTreeItem> GetSelectedItems()
        {
            return UIHelper.GetDescendants(_treeView).OfType<ProjectTreeItem>().Where(x => x.IsSelected);
        }*/



        /// <summary>
        /// This function purposely stops when an item is selected.
        /// This means that if a folder is selected and a subitem of that folder is too, the subitem will be ignored.
        /// 
        /// This is useful for drag and dropping and deleting files/folders.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private IEnumerable<ProjectTreeItem> GetOrderedSelectedItems(ProjectTreeItem control = null)
        {
            if (control == null)
            {
                foreach (var treeViewItem in _treeView.Items)
                    foreach (var items in GetOrderedSelectedItems((ProjectTreeItem) treeViewItem))
                        yield return items;
            }
            else
            {
                if (control.IsSelected)
                    yield return control;
                else
                    foreach (var treeViewItem in control.Items)
                        foreach (var items in GetOrderedSelectedItems((ProjectTreeItem) treeViewItem))
                            yield return items;
            }
        }



        /// <summary>
        /// This function purposely stops when an item is selected.
        /// This means that if a folder is selected and a subitem of that folder is too, the subitem will be ignored.
        /// 
        /// This is useful for drag and dropping and deleting files/folders.
        /// </summary>
        /// <param name="selections"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        private IEnumerable<ProjectTreeItem> FilterSelectedItems(IEnumerable<ProjectTreeItem> selections, ProjectTreeItem control = null)
        {
            if (control == null)
            {
                foreach (var treeViewItem in _treeView.Items)
                    foreach (var items in FilterSelectedItems(selections, (ProjectTreeItem) treeViewItem))
                        yield return items;
            }
            else
            {
                if (selections.Contains(control))
                    yield return control;
                else
                    foreach (var treeViewItem in control.Items)
                        foreach (var items in FilterSelectedItems(selections, (ProjectTreeItem) treeViewItem))
                            yield return items;
            }
        }



        /// <summary>
        /// Opens the "New Project" window and creates a new project.
        /// </summary>
        /// <param name="menuChild"></param>
        private void CreateNewProject(MenuChild menuChild)
        {
            NewProjectWindow windowNewProject = new NewProjectWindow(_services);
            windowNewProject.Accepted += delegate { LoadProject(windowNewProject.CreatedProject); };
            windowNewProject.Show(Screen);
        }



        private void OpenExistingProject(MenuChild menuChild)
        {
            //var graphicsService = (GraphicsManager)Services.Get<IGraphicsService>();
            //var form = (Form) graphicsService.GameForm;

            OpenFileDialog openFileDialog = new OpenFileDialog {Filter = "Sceelix Project Files|*." + Project.FileExtension,};
            if (_activeProject != null)
                openFileDialog.InitialDirectory = _activeProject.BaseFolder.FullPath;

            if (openFileDialog.ShowCrossDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadProject(openFileDialog.FileName);
            }
        }



        private bool LoadProject(string projectFilePath)
        {
            try
            {
                _activeProject = ProjectIO.Load(_services, projectFilePath);
                LoadProject(_activeProject);
            }
            catch (Exception ex)
            {
                _services.Get<MessageManager>().Publish(new ExceptionThrown(new Exception("Could not load the project file '" + projectFilePath + "': "  + ex.Message, ex)));
                return false;
            }

            return true;
        }



        public void CloseProject()
        {
            _activeProject = null;

            _treeView.Items.Clear();
        }


        /// <summary>
        /// Loads the project by instance, adds the items to the tree viewer and warns anyone that may be interested.
        /// </summary>
        public void LoadProject(Project project)
        {
            _activeProject = project;

            _projectHistorySettings.ReorganizeProjects(project.ProjectFilePath);

            //UpdateRecentMenuItemEntries();

            RefreshTreeList();

            _services.Get<MessageManager>().Publish(new ProjectLoaded(_activeProject));
        }



        private void RefreshTreeList()
        {
            _treeView.Items.Clear();

            LoadNodesToTree(_activeProject.BaseFolder, _treeView);

            //expand the root node
            var baseTreeItem = _treeView.GetTreeViewItemByUserData(_activeProject.BaseFolder);
            //baseTreeItem.ToolTip = _activeProject.BaseFolder.FullPath;
            baseTreeItem.IsExpanded = true;
        }



        private void RefreshTreeItemChildren(TreeViewItem treeViewItem)
        {
            treeViewItem.Items.Clear();

            FolderItem folderItem = (FolderItem) treeViewItem.UserData;

            foreach (ProjectItem subItem in folderItem.SubItems)
                LoadNodesToTree(subItem, treeViewItem);

            treeViewItem.IsExpanded = true;
        }



        private void LoadNodesToTree(ProjectItem projectItem, ITreeViewControl parent)
        {
            //TreeViewItem node = CreateNode(projectItem, parent);

            //if the item is a folder, we have to go over its subitems
            FolderItem folderItem = projectItem as FolderItem;
            if (folderItem != null)
            {
                CreateFolderNode(folderItem, parent);
            }
            else
            {
                CreateFileNode(projectItem, parent);
            }
        }



        private void CreateFolderNode(FolderItem folderItem, ITreeViewControl parent)
        {
            ProjectTreeItem treeViewItem = new ProjectTreeItem {Text = folderItem.FileName, Texture = EmbeddedResources.Load<Texture2D>("Resources/folder3.png"), UserData = folderItem};
            parent.Items.Add(treeViewItem);

            //treeViewItem.DoubleClick += FolderOnDoubleClick;
            treeViewItem.DoubleClick += (sender, args) => treeViewItem.IsExpanded = !treeViewItem.IsExpanded;

            foreach (ProjectItem subItem in folderItem.SubItems)
                LoadNodesToTree(subItem, treeViewItem);

            treeViewItem.IsExpanded = folderItem.Expanded;

            treeViewItem.RightClick += FolderOnRightClick;

            treeViewItem.PropertyChanged += TreeViewItemOnPropertyChanged;
        }



        private void TreeViewItemOnPropertyChanged(object sender, GamePropertyEventArgs gamePropertyEventArgs)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem) sender;
            var folderItem = (FolderItem) selectableTreeViewItem.UserData;

            folderItem.Expanded = selectableTreeViewItem.IsExpanded;

            ActiveProject.Save();
        }



        private void CreateFileNode(ProjectItem projectItem, ITreeViewControl parent)
        {
            IFileHandler fileHandler = _fileHandlerManager.GetFileHandler((FileItem) projectItem);

            Texture2D icon = fileHandler != null ? fileHandler.Icon16x16 : EmbeddedResources.Load<Texture2D>("Resources/document.png");

            if (!projectItem.ExistsPhysically)
                icon = EmbeddedResources.Load<Texture2D>("Resources/Exclamation_16x16.png");

            ProjectTreeItem fileTreeViewItem = new ProjectTreeItem {Text = projectItem.FileName, Texture = icon, UserData = projectItem};
            parent.Items.Add(fileTreeViewItem);

            fileTreeViewItem.DoubleClick += FileOnDoubleClick;
            fileTreeViewItem.RightClick += FileOnRightClick;

            if (!projectItem.ExistsPhysically)
                fileTreeViewItem.ToolTip = new ToolTipControl("Could not find path '" + projectItem.FullPath + "'.");
        }



        /// <summary>
        /// Context menu on folder right-click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderOnRightClick(object sender, EventArgs e)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem) sender;
            FolderItem folderItem = (FolderItem) item.UserData;

            MultiContextMenu multiContextMenu = new MultiContextMenu();



            _folderContextMenuManager.Initialize(multiContextMenu, item);

            multiContextMenu.Open(Screen, InputService.MousePosition);
        }



        /// <summary>
        /// Context menu on file right-click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileOnRightClick(object sender, EventArgs e)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem) sender;

            MultiContextMenu multiContextMenu = new MultiContextMenu();
            
            _fileContextMenuManager.Initialize(multiContextMenu, item);

            multiContextMenu.Open(Screen, InputService.MousePosition);
        }


        public void StartImportFolder(SelectableTreeViewItem treeViewItem)
        {
            var folderItem = (FolderItem)treeViewItem.UserData;

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = _activeProject.BaseFolder.FullPath,
                Description = "Please select the folder to import. Select the same project folder if you simply wish to refresh its contents from disk."
            };

            DialogResult dialogResult = folderBrowserDialog.ShowCrossDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var selectedPath = PathHelper.ToUniversalPath(folderBrowserDialog.SelectedPath);

                if (selectedPath != folderItem.FullPath
                    && folderItem.FullPath.Contains(selectedPath))
                {
                    //the selected folder is a parent, so it can't be chosen
                    MessageWindow window = new MessageWindow();
                    window.Title = "Invalid Folder";
                    window.Text = "The selected folder cannot be a parent of the destination folder.";
                    window.Buttons = new[] { "OK" };
                    window.MessageIcon = MessageWindowIcon.Error;

                    window.Show(Screen);
                }
                else
                {
                    //otherwise, it's not an issue, so proceed
                    ImportFilesWindow filesWindow = new ImportFilesWindow(_services, folderItem, selectedPath);
                    filesWindow.Show(Screen);
                    filesWindow.Closed += delegate
                    {
                        //sort the items
                        folderItem.SortItems();

                        //refresh the treeviewnode
                        RefreshTreeItemChildren(treeViewItem);

                        treeViewItem.IsExpanded = true;

                        ActiveProject.Save();
                    };
                }
            }
        }



        public void StartReloadFolder(SelectableTreeViewItem treeViewItem)
        {
            var folderItem = (FolderItem)treeViewItem.UserData;

            //import the same folder
            ImportFilesWindow filesWindow = new ImportFilesWindow(_services, folderItem, folderItem.FullPath);
            filesWindow.Show(Screen);
            filesWindow.Closed += delegate
            {
                //sort the items
                folderItem.SortItems();

                //refresh the treeviewnode
                RefreshTreeItemChildren(treeViewItem);

                treeViewItem.IsExpanded = true;

                ActiveProject.Save();
            };
        }



        public void StartNewFolder(SelectableTreeViewItem treeViewItem)
        {
            var folderItem = (FolderItem)treeViewItem.UserData;

            InputWindow inputWindow = new InputWindow()
            {
                InputText = "New Folder",
                Title = "New Folder",
                LabelText = "Folder Name:",
                Width = 400,
                MessageIcon = EmbeddedResources.Load<Texture2D>("Resources/Folder48x48.png"),
                UserData = folderItem,
                Check = OnCheckNewFolder
            };
            inputWindow.Accepted += delegate
            {
                FolderItem item = new FolderItem(inputWindow.InputText, folderItem);

                item.CreatePhysicalDirectory();

                //add the reference to the containing folder
                folderItem.AddItem(item);

                //sort the items
                folderItem.SortItems();

                RefreshTreeItemChildren(treeViewItem);

                //saves the project
                ActiveProject.Save();
            };
            inputWindow.Show(Screen);
        }



        private string OnCheckNewFolder(string text, Object userData)
        {
            FolderItem folderItem = (FolderItem) userData;

            if (String.IsNullOrWhiteSpace(text))
                return "The name cannot be empty. Please introduce a valid name.";
            if (FileCreationHelper.InvalidFileNameCharsRegex.IsMatch(text))
                return "A folder name cannot contain any of the following characters:\\ / : * ? \" < > |";
            if (folderItem.ContainsFolderWithName(text))
                return "A folder with that name already exists. Please choose another.";

            return String.Empty;
        }



        public void StartImport(SelectableTreeViewItem treeViewItem)
        {
            var folderItem = (FolderItem)treeViewItem.UserData;

            DialogResult dialogResult = _openFileDialog.ShowCrossDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                ImportFilesWindow filesWindow = new ImportFilesWindow(_services, folderItem, _openFileDialog.FileNames);
                filesWindow.Show(Screen);
                filesWindow.Closed += delegate
                {
                    //sort the items
                    folderItem.SortItems();

                    //refresh the treeviewnode
                    RefreshTreeItemChildren(treeViewItem);

                    treeViewItem.IsExpanded = true;

                    ActiveProject.Save();
                };
            }
        }



        public void StartCreateItem(SelectableTreeViewItem folderTreeItem)
        {
            var folderItem = (FolderItem)folderTreeItem.UserData;

            CreateFileWindow windowNewProject = new CreateFileWindow(_services, folderItem);
            windowNewProject.Accepted += delegate
            {
                CreateFileNode(windowNewProject.CreatedItem, folderTreeItem);

                folderTreeItem.IsExpanded = true;

                _services.Get<MessageManager>().Publish(new OpenFile(windowNewProject.CreatedItem));

                ActiveProject.Save();
            };

            windowNewProject.Show(Screen);
        }



        public void RenameProjectItem(SelectableTreeViewItem treeViewItem)
        {
            var projectItem = (ProjectItem)treeViewItem.UserData;

            Func<String, Object, String> checkFunction = projectItem is FolderItem ? OnCheckFolder : (Func<String, Object, String>)OnCheckFile;

            InputWindow inputWindow = new InputWindow()
            {
                InputText = projectItem.Name,
                Title = "Rename " + projectItem.Nature,
                LabelText = "New Name:",
                Width = 400,
                MessageIcon = EmbeddedResources.Load<Texture2D>("Resources/Rename48x48.png"),
                UserData = projectItem,
                Check = checkFunction
            };
            inputWindow.Accepted += delegate
            {
                //renames the item
                projectItem.Rename(inputWindow.InputText);

                //sorts the items of the parent by name
                projectItem.ParentFolder.SortItems();

                //changes the name of the treeview node
                treeViewItem.Text = projectItem.FileName;

                //we assume that 
                RefreshTreeItemChildren((TreeViewItem)treeViewItem.Parent);

                //saves the project
                ActiveProject.Save();
            };
            inputWindow.Show(Screen);
        }


        private string OnCheckFolder(string text, Object userData)
        {
            FolderItem folderItem = (FolderItem)userData;

            if (String.IsNullOrWhiteSpace(text))
                return "The name cannot be empty. Please introduce a valid name.";
            if (FileCreationHelper.InvalidFileNameCharsRegex.IsMatch(text))
                return "A folder name cannot contain any of the following characters:\\ / : * ? \" < > |";
            if (folderItem.ParentFolder.ContainsFolderWithName(text))
                return "A folder with that name already exists. Please choose another.";

            return String.Empty;
        }



        private string OnCheckFile(string text, Object userData)
        {
            FileItem fileItem = (FileItem)userData;

            if (String.IsNullOrWhiteSpace(text))
                return "The name cannot be empty. Please introduce a valid name.";
            if (FileCreationHelper.InvalidFileNameCharsRegex.IsMatch(text))
                return "A file name cannot contain any of the following characters:\\ / : * ? \" < > |";
            if (fileItem.ParentFolder.ContainsFileWithName(text + fileItem.Extension))
                return "A file with that name already exists. Please choose another.";

            return String.Empty;
        }




        private void FileOnDoubleClick(object sender, EventArgs eventArgs)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem) sender;

            var fileItem = item.UserData as FileItem;
            if (fileItem != null)
            {
                _services.Get<MessageManager>().Publish(new OpenFile(fileItem));
            }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }



        public void Duplicate(SelectableTreeViewItem item)
        {
            var originalFileItem = (FileItem)item.UserData;

            FileItem duplicate = originalFileItem.Duplicate();
            
            //gets parent tree node
            CreateFileNode(duplicate, _treeView.GetTreeViewItemByUserData(originalFileItem.ParentFolder));

            ActiveProject.Save();
        }



        public void ExcludeFiles()
        {

            var projectTreeItems = GetOrderedSelectedItems().ToList();
            if (projectTreeItems.Any())
            {
                String text, title;

                if (projectTreeItems.Count == 1)
                {
                    var item = (ProjectItem)projectTreeItems[0].UserData;

                    text = "Are you sure you want to exclude \"" + item.Name + "\" from the project?";
                    title = "Exclude " + item.Nature;
                }

                else
                {
                    text = "Are you sure you want to exclude all these items from the project?";
                    title = "Exclude Multiple Items";
                }

                MessageWindow messageWindow = new MessageWindow()
                {
                    MessageIcon = MessageWindowIcon.Question,
                    Text = text,
                    Title = title,
                    Buttons = new[] { "Yes", "No" }
                };
                messageWindow.Click += delegate
                {
                    if (messageWindow.Selection == "Yes")
                    {
                        foreach (var projectTreeItem in projectTreeItems)
                        {
                            projectTreeItem.Parent.Items.Remove(projectTreeItem);
                            projectTreeItem.UserData.CastTo<ProjectItem>().ExcludeFromProject();
                        }
                    }
                };
                messageWindow.Show(this);
            }
        }
    }
}