using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Utils;
using Sceelix.Designer.Graphs.GUI.Execution;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.GUI
{
    public class GraphDocumentControl : DocumentControl, IServiceable
    {
        //master settings of this type of control
        private GraphEditorSettings _editorSettings;


        //the graph control canvas
        private GraphControl _graphControl;

        private MessageManager _messageManager;

        private StackPanel _progressPanel;

        //private StackPanel _bottomStackPanel;
        private TextBlock _statusTextControl;

        //
        private bool _shouldReloadGraphOnActivation;
        private WindowAnimator _windowAnimator;
        private SettingsManager _settingsManager;
        private IServiceLocator _services;


        public void Initialize(IServiceLocator services)
        {
            _services = services;

            _editorSettings = services.Get<SettingsManager>().Get<GraphEditorSettings>();

            //_graphControl.FrameAll(false);
            _messageManager = services.Get<MessageManager>();
            _messageManager.Register<GraphContentChanged>(OnGraphChangedMessage, val => val.Item == FileItem);
            _messageManager.Register<ProjectItemMoved>(OnProjectItemMoved);

            _windowAnimator = services.Get<WindowAnimator>();
            _settingsManager = services.Get<SettingsManager>();
        }


        protected override void OnFirstLoad()
        {
            var verticalStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            BarMenu barMenuContent = new BarMenu();

            var graphMenuItemControl = new MenuChild() {Text = "Graph"};
            barMenuContent.MenuChildren.Add(graphMenuItemControl);

            graphMenuItemControl.MenuChildren.Add(new MenuChild(OnExecuteMenuClick) {Text = "Execute"});
            //graphMenuItemControl.MenuChildren.Add(new MenuChild(OnStepByStepExecuteMenuClick) { Text = "Execute (Step-by-Step)" });
            graphMenuItemControl.MenuChildren.Add(new MenuChild(ClearExecutionColorsMenuClick) {Text = "Clear Colors"});
            graphMenuItemControl.MenuChildren.Add(new MenuChild(ClearCache) {Text = "Clear Cache"});
            graphMenuItemControl.MenuChildren.Add(new MenuChild(SaveGraph) {Text = "Save", BeginGroup = true});
            graphMenuItemControl.MenuChildren.Add(new MenuChild(ShowGraphPropertiesMenuClick) {Text = "Properties", BeginGroup = true});

            var viewMenuItemControl = new MenuChild() {Text = "View"};
            barMenuContent.MenuChildren.Add(viewMenuItemControl);

            viewMenuItemControl.MenuChildren.Add(new MenuChild(OnFrameSelectionMenuClick) {Text = "Frame"});
            //viewMenuItemControl.MenuChildren.Add(new MenuChild(OnFrameAllMenuClick) { Text = "Frame All" });
            //viewMenuItemControl.MenuChildren.Add(new MenuChild(PrintScreen) {Text = "Printscreen"});

            viewMenuItemControl.MenuChildren.Add(new MenuChild(ShowSettings) {Text = "Settings"});


            /*var optionsMenuItemControl = new MenuChild() { Text = "Options" };
            barMenuContent.MenuChildren.Add(optionsMenuItemControl);

            optionsMenuItemControl.MenuChildren.Add(new CheckMenuChild(OnLiveExecutionMenuClick) { Text = "Live Execution", IsChecked = _editorSettings.LiveExecution.Value });
            optionsMenuItemControl.MenuChildren.Add(new CheckMenuChild(OnSaveOnExecutionMenuClick) { Text = "Save On Execution", IsChecked = _editorSettings.SaveOnExecution.Value });
            */
            verticalStackPanel.Children.Add(barMenuContent);

            _graphControl = new GraphControl(_services, FileItem, this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            verticalStackPanel.Children.Add(_graphControl);

            verticalStackPanel.Children.Add(AddBottomPanel());

            Content = verticalStackPanel;

            //_graphControl.ReloadGraph(false);
        }


        protected override void OnActivate()
        {
            if (_shouldReloadGraphOnActivation)
            {
                _graphControl.ReloadGraph(false);
                _shouldReloadGraphOnActivation = false;
            }
            else
            {
                _graphControl.ReloadGraph(false);
            }
        }


        protected override void OnClose(bool shouldSave)
        {
            if (shouldSave)
                _graphControl.Save();

            _graphControl.Dispose();

            //close any content in the inspector window that this tab may own
            _messageManager.Publish(new OwnerClosed(this));

            _messageManager.Unregister(this);
            //_messageManager.UnRegister<GraphContentChanged>(OnGraphChangedMessage);
        }


        private void OnGraphChangedMessage(GraphContentChanged obj)
        {
            AlertFileChange();
        }


        private UIControl AddBottomPanel()
        {
            var bottomStackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 22,
                Orientation = Orientation.Horizontal,
            };

            bottomStackPanel.Children.Add(_statusTextControl
                = new TextBlock()
                {
                    Margin = new Vector4F(4),
                    //Height = 20,
                    Text = "Done."
                });

            _progressPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                IsVisible = false
            };

            _progressPanel.Children.Add(new ProgressBar
            {
                IsIndeterminate = true,
                Margin = new Vector4F(4),
                Width = 200,
            });

            var stopImageButton = new ImageButton
            {
                Margin = new Vector4F(3),
                Width = 16,
                Height = 16,
                Texture = EmbeddedResources.Load<Texture2D>("Resources/Player Stop.png"),
            };
            stopImageButton.Image.Margin = new Vector4F(1);
            stopImageButton.Click += StopButtonOnClick;
            _progressPanel.Children.Add(stopImageButton);

            bottomStackPanel.Children.Add(_progressPanel);

            return new ContentControl() {Content = bottomStackPanel, ClipContent = true};
        }


        private void OnProjectItemMoved(ProjectItemMoved obj)
        {
            _shouldReloadGraphOnActivation = true;
        }


        private void ClearCache(MenuChild obj)
        {
            _graphControl.ClearCache();

            MessageWindow window = new MessageWindow();
            window.Title = "Clear Cache";
            window.Text = "The procedure cache has been successfully cleared.";
            window.Buttons = new[] {"OK"};
            window.MessageIcon = MessageWindowIcon.Information;

            window.Show(_windowAnimator);
        }


        private void StopButtonOnClick(object sender, EventArgs eventArgs)
        {
            _graphControl.GraphExecutionManager.Abort();
        }


        private void ShowSettings(MenuChild obj)
        {
            _settingsManager.OpenSettingsWindow("Editors/Graph");
        }


        private void OnExecuteMenuClick(MenuChild obj)
        {
            _graphControl.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true));

            //GuiExecuteRequest(new ExecutionOptions(true));
        }


        /*private void OnStepByStepExecuteMenuClick(MenuChild obj)
        {
            GuiExecuteRequest(new ExecutionOptions(true));
        }*/


        /*public void GuiExecuteRequest(ExecutionOptions options = null)
        {
            if (_editorSettings.SaveOnExecution.Value)
            {
                _graphControl.Save();

                AlertFileSave();
            }

            _graphControl.GraphExecutionManager.ExecuteGraphAsync(options);
        }*/


        private void ClearExecutionColorsMenuClick(MenuChild obj)
        {
            _messageManager.Publish(new MarkNode(null));
            _messageManager.Publish(new MarkPort(null));
            _messageManager.Publish(new MarkEdge(null));
        }


        private void SaveGraph(MenuChild obj)
        {
            _graphControl.Save();
            AlertFileSave();
        }


        private void ShowGraphPropertiesMenuClick(MenuChild obj)
        {
            _graphControl.ShowGraphProperties();
        }


        private void OnFrameSelectionMenuClick(MenuChild obj)
        {
            _graphControl.FrameSelection(true);
        }


        /*protected override void OnLoad(bool firstTimeLoad)
        {
            base.OnLoad(firstTimeLoad);

            if (firstTimeLoad)
                FirstTimeLoad();

            _graphControl.ReloadGraph(firstTimeLoad);
        }*/


        public void InformProcessStarted(string statusText, bool enableProgressBar)
        {
            _statusTextControl.Text = statusText;
            _progressPanel.IsVisible = enableProgressBar;
        }


        public void InformProcessStopped(String message = "Done.")
        {
            _progressPanel.IsVisible = false;
            _statusTextControl.Text = message;
        }


        public GraphEditorSettings EditorSettings
        {
            get { return _editorSettings; }
        }


        public string StatusText
        {
            get { return _statusTextControl.Text; }
            set
            {
                _statusTextControl.Text = value;
            }
        }


        /*public bool ProgressbarVisibility
        {
            get { _}
        }*/
    }
}