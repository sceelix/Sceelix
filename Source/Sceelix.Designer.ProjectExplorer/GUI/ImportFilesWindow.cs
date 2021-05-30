using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Helpers;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class ImportFilesWindow : DialogWindow
    {
        //if a directory was selected, all its contents m
        //private readonly string _directoryPath;
        private readonly String[] _importExclusions;

        //files to process are first sent here
        private readonly Queue<KeyValuePair<String, FolderItem>> _itemsToImport = new Queue<KeyValuePair<String, FolderItem>>();

        //if the user has to be questioned about file overwriting, we need to interrupt the process
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        //use the synchronizer to execute GUI related tasks in the main thread
        private readonly Synchronizer _synchronizer = new Synchronizer();
        private readonly FolderItem _targetFolderItem;
        private Button _cancelButton;
        private readonly List<String> _fileOrFolderPaths = new List<string>();

        //if the user presses the cancel button, warn the thread
        private bool _processCancelled = false;

        //window gui controls that must be changed
        private ProgressBar _progressBar;

        //by default the window asks if the files should be overwritten.
        //if the user decides Yes to All, this variable becomes true
        //if the user decides No to all, this variable becomes false
        //of none of the decisions are made, this variable remains null
        private bool? _shouldOverwrite;
        private TextBlock _textBlock;
        private IServiceLocator _services;


        private ImportFilesWindow(IServiceLocator services, FolderItem targetFolderItem)
        {
            _services = services;
            _targetFolderItem = targetFolderItem;
            _importExclusions = services.Get<SettingsManager>().Get<DesignerSettings>().ImportExclusions.Value.Split(',');

            Width = 500;
            Title = "Importing Files";
        }



        /// <summary>
        /// Imports a list of files or folders.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="targetFolderItem"></param>
        /// <param name="fileOrFolderPaths"></param>
        public ImportFilesWindow(IServiceLocator services, FolderItem targetFolderItem, params string[] fileOrFolderPaths)
            : this(services, targetFolderItem)
        {
            _fileOrFolderPaths.AddRange(fileOrFolderPaths);
        }



        /// <summary>
        /// Drawing of the window with progressbar and such.
        /// </summary>
        protected override void OnLoad()
        {
            var stackPanelMain = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };


            var stackPanelIconText = new FlexibleStackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var image = new ContentControl()
            {
                Content = new Image()
                {
                    Texture = EmbeddedResources.Load<Texture2D>("Resources/Import48x48.png"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 48,
                    Height = 48,
                    Margin = new Vector4F(10)
                },
                VerticalAlignment = VerticalAlignment.Stretch
            };
            stackPanelIconText.Children.Add(image);

            _progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            _textBlock = new TextBlock()
            {
                Text = "Processing Folders",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            stackPanelIconText.Children.Add(new ContentControl()
            {
                Content = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Children =
                    {
                        _textBlock,
                        _progressBar
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ClipContent = true
            });


            stackPanelMain.Children.Add(stackPanelIconText);

            var stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };


            stackPanelButtons.Children.Add(_cancelButton = new Button
            {
                Content = new TextBlock()
                {
                    Text = "Cancel",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 85,
                Height = 25
            });
            _cancelButton.Click += CancelButtonOnClick;


            stackPanelMain.Children.Add(new ContentControl()
            {
                Content = stackPanelButtons,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
            });

            Content = stackPanelMain;

            Thread thread = new Thread(ImportFiles)
            {
                CurrentCulture = CultureInfo.InvariantCulture,
                CurrentUICulture = CultureInfo.InvariantCulture
            };
            thread.Start();

            base.OnLoad();
        }



        /// <summary>
        /// This function is executed in a second thread to not interrupt the main thread
        /// </summary>
        private void ImportFiles()
        {
            //if the request was to import a list of files
            foreach (var path in _fileOrFolderPaths)
            {
                var universalPath = PathHelper.ToUniversalPath(path);

                if (universalPath == _targetFolderItem.FullPath
                    || !_targetFolderItem.FullPath.Contains(universalPath))
                {
                    if (Directory.Exists(path))
                        ProcessFolders(path, _targetFolderItem);
                    else
                    {
                        _itemsToImport.Enqueue(new KeyValuePair<string, FolderItem>(path, _targetFolderItem));
                    }
                }
            }

            var totalFileCount = _itemsToImport.Count;
            for (int i = 0; i < totalFileCount; i++)
            {
                //if the cancel button was process on the GUI thread
                if (_processCancelled)
                    break;

                var item = _itemsToImport.Dequeue();

                String message = "Processing " + Path.GetFileName(item.Key) + " (" + i + " of " + totalFileCount + ").";

                _synchronizer.Enqueue(() => _textBlock.Text = message);

                ImportFile(item.Key, item.Value);
            }

            //one the process is finished or cancelled, close the window
            _synchronizer.Enqueue(Close);
        }



        private void ProcessFolders(string importFolderPath, FolderItem container)
        {
            var selectedPath = PathHelper.ToUniversalPath(importFolderPath);

            FolderItem folderItem;

            //first of all, if the directory that was selected in the same as the container, we are not going recursively, 
            if (selectedPath == container.FullPath)
            {
                folderItem = container;
            }
            else
            {
                //if the item already exists, don't add it again
                var existingContainer = container.GetFolderItemByName(Path.GetFileName(selectedPath));
                if (existingContainer != null)
                {
                    folderItem = existingContainer;
                }
                else
                {
                    folderItem = new FolderItem(Path.GetFileName(selectedPath), container);
                    folderItem.CreatePhysicalDirectory();
                    container.AddItem(folderItem);
                }
            }

            //go recursively for all folders, creating folderitems as necessary
            foreach (String folderPath in Directory.GetDirectories(selectedPath))
                ProcessFolders(folderPath, folderItem);

            //for each folder, take note of the files to import
            foreach (String filePath in Directory.GetFiles(selectedPath))
            {
                if (!MatchExclusion(Path.GetFileName(filePath)))
                    _itemsToImport.Enqueue(new KeyValuePair<String, FolderItem>(filePath, folderItem));
            }
        }



        private bool MatchExclusion(string filePath)
        {
            foreach (string importExclusion in _importExclusions)
            {
                if (FitsMask(filePath, importExclusion))
                    return true;
            }

            return false;
        }



        private bool FitsMask(string fileName, string fileMask)
        {
            string pattern =
                '^' +
                Regex.Escape(fileMask.Replace(".", "__DOT__")
                        .Replace("*", "__STAR__")
                        .Replace("?", "__QM__"))
                    .Replace("__DOT__", "[.]")
                    .Replace("__STAR__", ".*")
                    .Replace("__QM__", ".")
                + '$';
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileName);
        }



        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            _processCancelled = true;
            //Cancel();
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }



        private void ImportFile(string importFilePath, FolderItem targetContainer)
        {
            //if a fileitem with that path already exists, ignore this import (we are probably just refreshing the folder)
            //if(targetContainer.GetFileItemByFullPath(externalFilePath) != null)
            //    return;
            String externalFilePath = PathHelper.ToUniversalPath(importFilePath);

            FileItem fileItem = new FileItem(Path.GetFileNameWithoutExtension(externalFilePath), Path.GetExtension(externalFilePath), targetContainer);

            //if the file that we are importing is already there
            if (fileItem.FullPath == externalFilePath)
            {
                Finalize(externalFilePath, fileItem, targetContainer);
            }
            //if the file with the same name does not yet exist
            //if a file with the same name already exists, confirm with the user what to do
            else if (targetContainer.ContainsFileWithName(fileItem.FileName))
            {
                if (!_shouldOverwrite.HasValue)
                {
                    bool shouldOverwriteNext = false;

                    _manualResetEvent.Reset();

                    _synchronizer.Enqueue(delegate
                    {
                        MessageWindow messageWindow = new MessageWindow()
                        {
                            MessageIcon = MessageWindowIcon.Question,
                            Text = "A file named \"" + fileItem.FileName + "\" already exists. Would you like to replace the existing file?",
                            Title = "Import File",
                            Buttons = new[] {"Yes", "Yes To All", "No", "No To All", "Cancel"}
                        };
                        messageWindow.Click += delegate
                        {
                            if (messageWindow.Selection.Contains("Yes"))
                            {
                                if (messageWindow.Selection == "Yes To All")
                                    _shouldOverwrite = true;

                                shouldOverwriteNext = true;

                                _manualResetEvent.Set();
                            }
                            else if (messageWindow.Selection.Contains("No"))
                            {
                                if (messageWindow.Selection == "No To All")
                                    _shouldOverwrite = false;

                                shouldOverwriteNext = false;

                                _manualResetEvent.Set();
                            }
                            if (messageWindow.Selection == "Cancel")
                            {
                                shouldOverwriteNext = false;

                                _itemsToImport.Clear();

                                _manualResetEvent.Set();
                            }
                        };

                        messageWindow.Show(Screen);
                    });

                    _manualResetEvent.WaitOne();

                    if (shouldOverwriteNext)
                        Finalize(externalFilePath, fileItem, targetContainer);
                }
                //if true, just overwrite, otherwise ignore the file for import
                else if (_shouldOverwrite.Value)
                {
                    //just overwrite
                    Finalize(externalFilePath, fileItem, targetContainer);
                }
            }
            else
            {
                Finalize(externalFilePath, fileItem, targetContainer);
            }
        }



        private void Finalize(string filePath, FileItem fileItem, FolderItem targetContainer)
        {
            try
            {
                if (filePath != fileItem.FullPath)
                    File.Copy(filePath, fileItem.FullPath, true);

                LoadGuid(fileItem);

                _synchronizer.Enqueue(delegate
                {
                    if (!targetContainer.ContainsFileItemWithFileName(fileItem.FileName))
                        targetContainer.AddItem(fileItem);

                    _services.Get<MessageManager>().Publish(new FileImported(fileItem));
                });
            }
            catch (Exception ex)
            {
                _synchronizer.Enqueue(delegate
                {
                    var window = new MessageWindow()
                    {
                        MessageIcon = MessageWindowIcon.Error,
                        Text = ex.Message,
                        Title = "Error",
                        Buttons = new[] {"OK"}
                    };
                    window.Show(Screen);
                });
            }
        }



        private void LoadGuid(FileItem fileItem)
        {
            IFileHandler fileHandler = _services.Get<FileHandlerManager>().GetFileHandler(fileItem);
            if (fileHandler != null)
            {
                Guid? guid = fileHandler.GetGuid(fileItem);
                if (guid.HasValue)
                    fileItem.Guid = guid.Value;
            }
        }



        public bool? ShouldOverwrite
        {
            get { return _shouldOverwrite; }
            set { _shouldOverwrite = value; }
        }
    }
}