using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core;
using Sceelix.Core.Environments;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    [DesignerWindow("Log")]
    public class LogWindow : AnimatedWindow, IServiceable
    {
        //overall control of 
        private readonly List<LogWindowEntry> _logMessages = new List<LogWindowEntry>();
        private ObjectTreeView _objectTreeView;

        private readonly Synchronizer _synchronizer = new Synchronizer();
        private ObjectTreeView.Column _contentColumn;
        private ObjectTreeView.Column _idColumn;
        private ObjectTreeView.Column _countColumn;
        private bool _refreshMessages;

        private ObjectTreeView.Column _typeColumn;
        private LogSettings _logSettings;
        private IServiceLocator _services;


        public void Initialize(IServiceLocator services)
        {
           _services = services;
            
            _logSettings = services.Get<SettingsManager>().Get<LogSettings>();

            var messageManager = services.Get<MessageManager>();
            messageManager.Register<ExceptionThrown>(OnExceptionThrown);
            messageManager.Register<LogMessageSent>(OnLogMessageSent);
            messageManager.Register<LogMessageClear>(OnLogMessageClear);
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "Log";
            Width = 500;
            Height = 500;
            CanResize = true;

            var windowStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            BarMenu barMenu = new BarMenu();
            barMenu.MenuChildren.Add(new MenuChild(ClearTreeView) { Text = "Clear", Icon = EmbeddedResources.Load<Texture2D>("Resources/Trash_16x16.png") });
            windowStackPanel.Children.Add(barMenu);


            windowStackPanel.Children.Add(_objectTreeView = new ObjectTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = obj => false,
                ViewType = ObjectTreeView.ListViewType.List,
                HasItemToolTip = true,
                GetItemTooltip = GetItemTooltip,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                GetColumnValue = delegate (object obj, ObjectTreeView.Column column)
                {
                    var message = (LogWindowEntry)obj;
                    if (column == _idColumn)
                        return message.Id;
                    if (column == _contentColumn)
                        return message.Content;
                    if (column == _countColumn)
                        return message.Count;

                    return message.LogType;
                },
                GetCellControl = delegate (ObjectTreeView.Column column, object obj)
                {
                    if (column == _typeColumn)
                    {
                        var logType = (LogType)obj;

                        switch (logType)
                        {
                            case LogType.Information:
                                return GetImage("LogInfo_16x16");
                            case LogType.Warning:
                                return GetImage("LogWarning_16x16");
                            case LogType.Error:
                                return GetImage("LogError_16x16");
                            case LogType.Debug:
                                return GetImage("LogDebug_16x16");
                            default:
                                return GetImage("LogInfo_16x16");
                        }
                    }
                    else if (column == _idColumn)
                    {
                        return new ContentControl() { Content = new TextBlock() { Text = obj.ToString(), HorizontalAlignment = HorizontalAlignment.Center } };
                    }

                    //return new ContentControl() {Content = new LineContainer() { Content = new TextBlock() { Text = obj.ToString() }, VerticalAlignment = VerticalAlignment.Stretch}  };

                    return new TextBlock() { Text = obj.SafeToString() };
                    //return new ContentControl() {Content = new TextBlock() { Text = obj.ToString(),VerticalAlignment = VerticalAlignment.Center}};//, WrapText = true
                },
                GetContextMenu = delegate (object o, ObjectTreeView.Column column)
                {
                    var windowEntry = (LogWindowEntry)o;

                    MultiContextMenu multiContext = new MultiContextMenu();
                    multiContext.MenuChildren.Add(new MenuChild(CopyAll) { Text = "Copy", UserData = o });

                    if (windowEntry.Detail != null)
                    {
                        multiContext.MenuChildren.Add(new MenuChild(CopyMessage) { Text = "Copy Message", UserData = o });
                        multiContext.MenuChildren.Add(new MenuChild(CopyDetailsContent) { Text = "Copy Details", UserData = o });
                    }

                    if (windowEntry.ResponseMessage != null)
                    {
                        multiContext.MenuChildren.Add(new MenuChild(ShowOrigin) { Text = "Show Origin", UserData = o, BeginGroup = true });
                    }

                    return multiContext;
                },
                ItemHeight = 22,
                ItemPadding = new Vector4F(0, 5, 0, 5)
            }); //);

            _objectTreeView.ItemEntered += ObjectTreeViewOnItemEntered;


            _objectTreeView.Columns.Add(_idColumn = new ObjectTreeView.Column("#", 50, false));
            _objectTreeView.Columns.Add(_typeColumn = new ObjectTreeView.Column("", 30, false));
            _objectTreeView.Columns.Add(_contentColumn = new ObjectTreeView.Column("Content", 100, true));
            _objectTreeView.Columns.Add(_countColumn = new ObjectTreeView.Column(" ", 50, false));

            Content = windowStackPanel;

            _objectTreeView.Items = _logMessages;
        }


        private void ShowOrigin(MenuChild obj)
        {
            var windowEntry = (LogWindowEntry)obj.UserData;
            _services.Get<MessageManager>().Publish(windowEntry.ResponseMessage);
        }



        private void CopyAll(MenuChild obj)
        {
            var windowEntry = (LogWindowEntry)obj.UserData;

            var content = windowEntry.Content;
            if (windowEntry.Detail != null)
                content += "\n\n" + windowEntry.Detail;

            ClipboardHelper.Copy(content);
        }


        private void CopyMessage(MenuChild obj)
        {
            var windowEntry = (LogWindowEntry) obj.UserData;
            
            ClipboardHelper.Copy(windowEntry.Content);
        }


        private void CopyDetailsContent(MenuChild obj)
        {
            var windowEntry = (LogWindowEntry)obj.UserData;

            ClipboardHelper.Copy(windowEntry.Detail);
        }



        private void OnObject(object obj)
        {
            AddLogEntry(new LogWindowEntry() { Content = obj.ToString() });

        }



        private object GetItemTooltip(object obj)
        {
            var entry = ((LogWindowEntry) obj);
            if (entry.Detail != null)
                return new ToolTipControl(entry.Content, entry.Detail) { MaxWidth = 700 };

            return null;
        }



        private void ObjectTreeViewOnItemEntered(object item)
        {
            var logMessage = item as LogWindowEntry;
            if (logMessage != null && logMessage.ResponseMessage != null)
            {
                _services.Get<MessageManager>().Publish(logMessage.ResponseMessage);
            }
        }



        private void OnLogMessageClear(LogMessageClear obj)
        {
            _synchronizer.Enqueue(() => ClearTreeView(null));
        }



        private void OnLogMessageSent(LogMessageSent obj)
        {
            AddLogEntry(new LogWindowEntry() { Content = obj.Message, LogType = obj.LogType, ResponseMessage = obj.ResponseMessage });
        }


        /// <summary>
        /// Add the message to the textbox, but do it in the main thread.
        /// </summary>
        /// <param name="obj"></param>
        private void OnExceptionThrown(ExceptionThrown obj)
        {
            AddLogEntry(new LogWindowEntry() { Content = obj.Exception.Message, LogType = LogType.Error, Detail = obj.Exception.ToString(), ResponseMessage = obj.ResponseMessage });
        }



        private void AddLogEntry(LogWindowEntry logWindowEntry)
        {
            //run in the main thread.
            _synchronizer.Enqueue(() =>
            {
                var aggregateValues = _logSettings.AggregateMessages.Value;

                //a hash-based approach would be better...
                var existingEntry = aggregateValues ? _logMessages.FirstOrDefault(x => x.Equals(logWindowEntry)) : null;
                if (existingEntry != null)
                {
                    existingEntry.Count++;
                }
                else
                {
                    _logMessages.Add(logWindowEntry);
                    logWindowEntry.Id = _logMessages.Count;
                }

                _refreshMessages = true;
            });
        }



       private void ClearTreeView(MenuChild obj)
        {
            _logMessages.Clear();
            _objectTreeView.Clear();

            GC.Collect();
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();

            if (_refreshMessages)
            {
                _objectTreeView.Items = _logMessages;
                _objectTreeView.FocusOnIndex(_logMessages.Count);
                _refreshMessages = false;
            }
        }



        private ContentControl GetImage(string text)
        {
            return new ContentControl()
            {
                Content = new Image()
                {
                    Texture = EmbeddedResources.Load<Texture2D>("Resources/" + text + ".png"),
                    Width = 16,
                    Height = 16,
                    Margin = new Vector4F(0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Color.White
                }
            };
        }



        
    }
}