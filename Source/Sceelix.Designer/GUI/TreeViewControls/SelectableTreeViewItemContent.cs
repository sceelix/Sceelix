using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    internal class SelectableTreeViewItemContent : StackPanel
    {
        private readonly Image _image;
        private readonly TextBlock _textBlock;
        private bool _isSelected;



        public SelectableTreeViewItemContent()
        {
            Orientation = Orientation.Horizontal;
            Style = "StandardTreeViewItem";

            Children.Add(_image = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Vector4F(2),
                Foreground = Color.White
                //Foreground = Microsoft.Xna.Framework.Color.White
            });

            Children.Add(_textBlock = new TextBlock
            {
                //HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Vector4F(2)
            });
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



        public Color TextColor
        {
            get { return _textBlock.Foreground; }
            set { _textBlock.Foreground = value; }
        }



        public override string VisualState
        {
            get
            {
                if (_isSelected)
                    return "Selected";

                return "Default";
            }
        }



        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                InvalidateVisual();
            }
        }
    }
}