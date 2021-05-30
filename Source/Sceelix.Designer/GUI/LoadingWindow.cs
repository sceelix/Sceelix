using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.GUI.Windows;

namespace Sceelix.Designer.GUI
{
    public class LoadingWindow : AnimatedWindow
    {
        private readonly TextBlock _messageTextBlock;



        public LoadingWindow()
        {
            //Width = 300;
            //Height = 200;
            IsModal = true;
            CanResize = false;
            CanDrag = false;
            CloseButtonStyle = "";
            KeepCentered = true;
            Title = "Starting Sceelix Designer...";


            var verticalStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            verticalStackPanel.Children.Add(new Image()
            {
                Style = "LoadingWindowLogo",
                Width = 300,
                Height = 300,
                Margin = new Vector4F(20, 20, 20, 20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Foreground = Color.White
            });

            verticalStackPanel.Children.Add(_messageTextBlock = new TextBlock()
            {
                Text = "",
                Margin = new Vector4F(20, 20, 20, 15),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });

            Content = verticalStackPanel;
        }



        public string Text
        {
            get { return _messageTextBlock.Text; }
            set { _messageTextBlock.Text = value; }
        }
    }
}