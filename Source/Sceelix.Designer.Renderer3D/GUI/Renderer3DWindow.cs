using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using DigitalRune;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;

namespace Sceelix.Designer.Renderer3D.GUI
{
    // A resizable window with a ScrollViewer.
    [DesignerWindow("3D Viewer")]
    public class Renderer3DWindow : AnimatedWindow, IServiceable
    {
        private TextBlock _coordinateLabel;
        private StackPanel _progressPanel;
        private TextBlock _statusText;

        private IServiceLocator _services;
        //private BarMenuEntry _menuEntry;
        

        public void Initialize(IServiceLocator services)
        {
            _services = services;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "3D Viewer";
            Width = 500;
            Height = 500;
            CanResize = true;

            var verticalStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            BarMenu barMenuContent = new BarMenu();
            verticalStackPanel.Children.Add(barMenuContent);

            var cameraMenuItemControl = new MenuChild() { Text = "Camera" };
            barMenuContent.MenuChildren.Add(cameraMenuItemControl);


            var windowItemControl = new MenuChild() { Text = "Window" };
            barMenuContent.MenuChildren.Add(windowItemControl);

            windowItemControl.MenuChildren.Add(new MenuChild(ShowPreferences) { Text = "Preferences", BeginGroup = true });


            var barMenuService = new BarMenuService(barMenuContent);

            verticalStackPanel.Children.Add(new Renderer3DControl(_services, barMenuService, this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            });

            verticalStackPanel.Children.Add(AddBottomPanel());

            Content = verticalStackPanel;

            _services.Get<BarMenuManager>().RegisterMenu(barMenuContent, "3D Viewer");
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
                Content = _coordinateLabel = new TextBlock()
                {
                    Margin = new Vector4F(30, 4, 4, 4),
                    //Height = 20,
                    Text = "0,0",
                    HorizontalAlignment = HorizontalAlignment.Right
                },
                HorizontalAlignment = HorizontalAlignment.Stretch
            });

            return new ContentControl() {Content = bottomStackPanel, ClipContent = true};
            //return bottomStackPanel;
        }
        


        private void ShowPreferences(MenuChild obj)
        {
            _services.Get<SettingsManager>().OpenSettingsWindow("Renderer 3D");
        }


        public void SetProgressVisibility(bool state)
        {
            _progressPanel.IsVisible = state;
        }


        public void SetStatusBarText(string text)
        {
            _statusText.Text = text;
        }



        public void SetCornerCoordinateText(string text)
        {
            _coordinateLabel.Text = text;
        }


        
    }
}