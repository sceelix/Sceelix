using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.FileHandlers.Text;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Extensions;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    [DesignerWindow("Document Area")]
    public class DocumentAreaWindow : AnimatedWindow, IServiceable, IUpdateableElement
    {
        private TabControl _tabControl;
        private MessageManager _messageManager;
        private FileHandlerManager _fileHandlerManager;
        private GraphicsWindowManager _graphicsWindowManager;
        private WindowAnimator _windowAnimator;

        private bool _previouslyActive = false;
        


        public void Initialize(IServiceLocator services)
        {
            _messageManager = services.Get<MessageManager>();
            _fileHandlerManager = services.Get<FileHandlerManager>();
            _graphicsWindowManager = services.Get<GraphicsWindowManager>();
            _windowAnimator = services.Get<WindowAnimator>();

            _messageManager.Register<OpenFile>(OnOpenFile);
            _messageManager.Register<FileDeleted>(OnFileDeleted);
            _messageManager.Register<FileRenamed>(OnFileRenamed);
            _messageManager.Register<ProjectLoaded>(OnProjectLoaded);
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "Document Area";
            CanResize = true;
            Width = 500;
            Height = 300;
            ClipContent = true;

            InitializeTabControl();

            //Commented out for now, need more attention in the future
            //having this on resulted in having the graph control refresh every time we would switch windows
            //including from inspector to the graph control, which caused problems when changing parameter values in the inspector
            //(it would reset on the graph).
            //var isActiveProperty = Properties.Get<bool>("IsActive");
            //isActiveProperty.Changed += IsActivePropertyOnChanged;
        }

        /// <summary>
        /// When the document area window loses "focus" (i.e. other window is clicked) and gets it again, the current tab should be notified.
        /// Useful for the graph document window, for instance (for reloading its graph after file moves).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gamePropertyEventArgs"></param>
        private void IsActivePropertyOnChanged(object sender, GamePropertyEventArgs<bool> gamePropertyEventArgs)
        {
            if (_tabControl.SelectedIndex >= 0)
            {
                DocumentTabItem tabItem = _tabControl.Items[_tabControl.SelectedIndex] as DocumentTabItem;
                if (tabItem != null && gamePropertyEventArgs.NewValue)
                {
                    tabItem.DocumentControl.Activate();
                }
            }
        }


        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_graphicsWindowManager.IsReallyActive && _previouslyActive != _graphicsWindowManager.IsReallyActive)
                CheckAllForExternalFileChange();

            _previouslyActive = _graphicsWindowManager.IsReallyActive;
        }


        private void CheckAllForExternalFileChange()
        {
            Queue<DocumentTabItem> tabItemQueue = new Queue<DocumentTabItem>(_tabControl.Items.OfType<DocumentTabItem>());
            CheckForExternalFileChange(tabItemQueue, false, false);
        }


        private void CheckForExternalFileChange(Queue<DocumentTabItem> tabItemQueue, bool yesToAll, bool noToAll)
        {
            if (tabItemQueue.Count > 0)
            {
                var documentTabItem = tabItemQueue.Dequeue();
                var documentControl = documentTabItem.DocumentControl;
                var fileInfo = new FileInfo(documentControl.FileItem.FullPath);
                if (fileInfo.LastWriteTime > documentControl.LastLoadTime)
                {
                    documentControl.LastLoadTime = DateTime.Now;

                    if (yesToAll)
                    {
                        documentControl.FileContentUpdate();
                        CheckForExternalFileChange(tabItemQueue, true, false);
                    }
                    else if (noToAll)
                    {
                        CheckForExternalFileChange(tabItemQueue, false, true);
                    }
                    else
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Title = "External changes detected";
                        messageWindow.Text = documentControl.FileItem.FullPath + "\n\n";

                        if (documentControl.HasUnsavedChanges)
                            messageWindow.Text += "The file has been changed externally but still has changes inside this editor.\nDo you want to reload it and lose the changes made in the editor?";
                        else
                            messageWindow.Text += "The file has been changed externally.\nDo you want to reload it?";

                        messageWindow.Buttons = new[] { "Yes", "Yes To All", "No", "No To All" };
                        messageWindow.Click += delegate (object sender, EventArgs args)
                        {
                            if (messageWindow.Selection.Contains("Yes"))
                            {
                                documentControl.FileContentUpdate();
                                CheckForExternalFileChange(tabItemQueue, messageWindow.Selection == "Yes To All", false);
                            }
                            else
                            {
                                CheckForExternalFileChange(tabItemQueue, false, messageWindow.Selection == "No To All");
                            }
                        };
                        messageWindow.Show(_windowAnimator);
                    }
                }
                else
                {
                    CheckForExternalFileChange(tabItemQueue, yesToAll, noToAll);
                }
            }
        }
        

        /// <summary>
        /// If a new project is loaded, close all existing tabs.
        /// </summary>
        /// <param name="obj"></param>
        private void OnProjectLoaded(ProjectLoaded obj)
        {
            CloseAllTabs();
        }


        private void InitializeTabControl()
        {
            _tabControl = new TabControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(4),
            };
            _tabControl.Items.CollectionChanged += ItemsOnCollectionChanged;

            Content = _tabControl;
        }


        private void ItemsOnCollectionChanged(object sender, CollectionChangedEventArgs<TabItem> collectionChangedEventArgs)
        {
            //because of an issue with the tabcontrol, we need to reset the tabcontrol if all the tabs are removed.
            if (_tabControl.Items.Count == 0)
                InitializeTabControl();

            //if we are adding new tabs, we want to focus on the most recent one
            if (collectionChangedEventArgs.Action == CollectionChangedAction.Add)
                _tabControl.Select(collectionChangedEventArgs.NewItems.First());
        }


        private void OnOpenFile(OpenFile openFile)
        {
            TabItem tabItem = _tabControl.Items.FirstOrDefault(val => val.TabPage.CastTo<DocumentControl>().FileItem == openFile.Item);

            if (tabItem != null)
                _tabControl.Select(tabItem);
            else
            {
                DocumentControl documentControl = _fileHandlerManager.GetDocumentControlForFile(openFile.Item, new TextEditorDocumentControl());
                documentControl.Activated += () => _messageManager.Publish(new DocumentActivated(openFile.Item));

                documentControl.VerticalAlignment = VerticalAlignment.Stretch;
                documentControl.HorizontalAlignment = HorizontalAlignment.Stretch;

                tabItem = new DocumentTabItem(this, documentControl);
                _tabControl.Items.Add(tabItem);
            }
        }


        private void OnFileDeleted(FileDeleted obj)
        {
            //close the tab - no asking, just close
            TabItem tabItem = _tabControl.Items.FirstOrDefault(val => val.TabPage.CastTo<DocumentControl>().FileItem == obj.Item);
            if (tabItem != null)
                _tabControl.Items.Remove(tabItem);
        }


        private void OnFileRenamed(FileRenamed obj)
        {
            DocumentTabItem tabItem = _tabControl.Items.OfType<DocumentTabItem>().FirstOrDefault(val => val.TabPage.CastTo<DocumentControl>().FileItem == obj.Item);
            if (tabItem != null)
                tabItem.DocumentControl.ReviewFormName();
        }


        public void CloseTab(DocumentTabItem documentTabItem)
        {
            CloseTabs(new List<DocumentTabItem>() {documentTabItem});
        }


        public void CloseAllTabs()
        {
            CloseTabs(_tabControl.Items.OfType<DocumentTabItem>().ToList());
        }


        public void CloseAllTabsBut(DocumentTabItem documentTabItem)
        {
            CloseTabs(_tabControl.Items.OfType<DocumentTabItem>().Where(x => x != documentTabItem).ToList());
        }


        internal void CloseTabs(List<DocumentTabItem> documentTabList)
        {
            //we're going to lose the reference in the process once this tab is closed, so keep it
            var tabControl = _tabControl;

            int numUnsavedWindows = documentTabList.Count(x => x.DocumentControl.HasUnsavedChanges);
            if (numUnsavedWindows > 0)
            {
                MessageWindow messageWindow = new MessageWindow()
                {
                    Title = "Unsaved Changes",
                    Text = "You have unsaved changes in " + numUnsavedWindows + " document(s). Do you with to save the changes before closing?",
                    Buttons = new[] {"Yes", "No", "Cancel"},
                    MessageIcon = MessageWindowIcon.Question
                };
                messageWindow.Click += delegate
                {
                    if (messageWindow.Selection != "Cancel")
                        CloseWindowsAux(tabControl, documentTabList, (x) => messageWindow.Selection == "Yes" && x.DocumentControl.HasUnsavedChanges);
                };
                messageWindow.Show(this);
            }
            else
            {
                CloseWindowsAux(tabControl, documentTabList, (x) => false);
            }
        }


        private void CloseWindowsAux(TabControl tabControl, List<DocumentTabItem> documentTabList, Func<DocumentTabItem, bool> shouldCloseFunc)
        {
            if (documentTabList.Count == 1)
            {
                var documentTabItem = documentTabList.First();
                var index = tabControl.Items.IndexOf(documentTabItem);
                if (index == tabControl.SelectedIndex && index > 0)
                    tabControl.Select(tabControl.Items[index - 1]);
            }

            //tell the documents that we are closing so that they don't trigger any events by just being loaded
            foreach (var documentTabItem in documentTabList)
                documentTabItem.DocumentControl.IsClosing = true;

            //if the user chooses yes, save the contents first
            //it is up to the control to implement the Close function
            foreach (var documentTabItem in documentTabList)
                documentTabItem.DocumentControl.Close(shouldCloseFunc(documentTabItem));

            //now we can close the windows
            foreach (var documentTabItem in documentTabList)
                tabControl.Items.Remove(documentTabItem);
        }
    }
}