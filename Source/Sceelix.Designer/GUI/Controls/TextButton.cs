using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.Controls
{
    public class TextButton : Button
    {
        private readonly TextBlock _textBlock;



        public TextButton()
        {
            Content = _textBlock = new TextBlock
            {
                //Margin = new Vector4F(4, 0, 0, 0),
                Text = "Text",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            /*_button = new Button
            {
                
                Margin = new Vector4F(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };*/

            //Content = _button;
        }



        /*public TextButton(String text, Texture2D icon)
        {
            var buttonContentPanel = new StackPanel { Orientation = Orientation.Horizontal };
            buttonContentPanel.Children.Add(_image = new Image
            {
                Texture = icon
            });

            buttonContentPanel.Children.Add(_textBlock = new TextBlock
            {
                Margin = new Vector4F(4, 0, 0, 0),
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
            });

            Content = buttonContentPanel;
        }*/



        public String Text
        {
            get { return _textBlock.Text; }
            set { _textBlock.Text = value; }
        }



        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return _textBlock.HorizontalAlignment; }
            set { _textBlock.HorizontalAlignment = value; }
        }
    }
}