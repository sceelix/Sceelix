using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Extensions;

namespace Sceelix.Designer.Meshes.Controls
{
    public class ModelViewerDocumentControl : DocumentControl, IServiceable
    {
        private IServiceLocator _services;
        private StackPanel _progressPanel;
        private TextBlock _statusText;
        private ModelViewer3DControl _modelViewer3DControl;
        private TextBlock _statsLabel;

        private ModelViewer3DSettings _modelViewer3DSettings;
        private SettingsManager _settingsManager;



        public void Initialize(IServiceLocator services)
        {
            _services = services;

            _settingsManager = _services.Get<SettingsManager>();
            _modelViewer3DSettings = _services.Get<SettingsManager>().Get<ModelViewer3DSettings>();
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
            verticalStackPanel.Children.Add(barMenuContent);

            var modelMenuItemControl = new MenuChild() { Text = "Model" };
            barMenuContent.MenuChildren.Add(modelMenuItemControl);

            AddCheckOption(modelMenuItemControl, "Flip Faces");
            AddCheckOption(modelMenuItemControl, "Flip Texture U");
            AddCheckOption(modelMenuItemControl, "Flip Texture V");
            AddCheckOption(modelMenuItemControl, "Rotate Axis");

            var viewMenuItemControl = new MenuChild() { Text = "View" };
            barMenuContent.MenuChildren.Add(viewMenuItemControl);

            viewMenuItemControl.MenuChildren.Add(new MenuChild(ShowSettings) { Text = "Settings" });

            verticalStackPanel.Children.Add(_modelViewer3DControl = new ModelViewer3DControl(_services, _services.Get<IGraphicsService>(), FileItem, _modelViewer3DSettings)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            });

            _modelViewer3DControl.MeshViewerLoading += MeshViewerLoading;
            _modelViewer3DControl.MeshViewerLoadingFinished += MeshViewerLoadingFinished;

            verticalStackPanel.Children.Add(AddBottomPanel());

            Content = verticalStackPanel;
        }



        private void ShowSettings(MenuChild obj)
        {
            _settingsManager.OpenSettingsWindow("Model Viewer");
        }



        private void AddCheckOption(MenuChild modelMenuItemControl, string propertyName)
        {
            var checkMenuChild = new CheckMenuChild(OnPropertyChangeClick) { Text = propertyName, IsChecked = FileItem.Properties.GetAndConvert(propertyName, false) };
            modelMenuItemControl.MenuChildren.Add(checkMenuChild);
        }



        private void MeshViewerLoadingFinished(object sender, EventArgs e)
        {
            _progressPanel.IsVisible = false;
            
            _statsLabel.Text = String.Join(" | ", _modelViewer3DControl.CurrentModelStatistics.Select(x => x.Key + ": " + x.Value));
        }



        private void MeshViewerLoading(object sender, EventArgs e)
        {
            _progressPanel.IsVisible = true;

            _statsLabel.Text = "";
        }



        private void OnPropertyChangeClick(MenuChild obj)
        {
            var checkMenuChild = (CheckMenuChild) obj;
            
            FileItem.Properties[checkMenuChild.Text] = checkMenuChild.IsChecked.ToString();

            FileItem.Project.Save();

            _modelViewer3DControl.ReloadModel();
        }



        private UIControl AddBottomPanel()
        {
            var bottomStackPanel = new FlexibleStackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 22,
                Orientation = Orientation.Horizontal,
            };

            bottomStackPanel.Children.Add(_statusText
                = new TextBlock()
                {
                    Margin = new Vector4F(4),
                    //Height = 20,
                    Text = "Ready."
                });


            _progressPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                IsVisible = false,
            };

            _progressPanel.Children.Add(new ProgressBar
            {
                IsIndeterminate = true,
                Margin = new Vector4F(4),
                Width = 200,
                //Height = 20,
            });

            bottomStackPanel.Children.Add(_progressPanel);

            bottomStackPanel.Children.Add(new ContentControl()
            {
                Content = _statsLabel = new TextBlock()
                {
                    Margin = new Vector4F(30, 4, 4, 4),
                    //Height = 20,
                    Text = "0,0",
                    HorizontalAlignment = HorizontalAlignment.Right
                },
                HorizontalAlignment = HorizontalAlignment.Stretch
            });

            return bottomStackPanel;
        }
    }
}
