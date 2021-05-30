using System;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.GUI.Controls
{
    public class ImageButton : ContentControl
    {
        private readonly StackPanel _buttonStackPanel;

        private readonly Image _image;
        private readonly TextBlock _textBlock;



        public ImageButton()
        {
            Style = "ImageButton";

            _buttonStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };

            _buttonStackPanel.Children.Add(_image = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Vector4F(3)
            });

            _buttonStackPanel.Children.Add(_textBlock = new TextBlock
            {
                //HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Vector4F(3)
            });

            Content = _buttonStackPanel;
        }



        public override string VisualState
        {
            get
            {
                if (IsMouseOver)
                    return "MouseOver";

                return "Default";
            }
        }



        public StackPanel Panel
        {
            get { return _buttonStackPanel; }
        }



        public Image Image
        {
            get { return _image; }
        }



        public Texture2D Texture
        {
            get { return _image.Texture; }
            set { _image.Texture = value; }
        }



        public string Text
        {
            get { return _textBlock.Text; }
            set { _textBlock.Text = value; }
        }



        public event EventHandler<EventArgs> Click = delegate { };



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (!InputService.IsMouseOrTouchHandled)
            {
                if (IsMouseOver && InputService.IsPressed(MouseButtons.Left, false))
                {
                    Click.Invoke(this, EventArgs.Empty);

                    //Screen.Children.Add(new Button(){X = this.ActualX, Y = this.ActualY + this.ActualHeight,Width = this.ActualWidth,Height = this.ActualHeight});
                    InputService.IsMouseOrTouchHandled = true;

                    this.InvalidateVisual();
                }
            }
        }
    }
}