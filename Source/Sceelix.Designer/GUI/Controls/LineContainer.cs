using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Extensions;

namespace Sceelix.Designer.GUI.Controls
{
    /// <summary>
    /// A control that shows an underlining effect on the passed textblock, when hovered.
    /// </summary>
    /// <seealso cref="DigitalRune.Game.UI.Controls.StackPanel" />
    public class LineContainer : StackPanel
    {
        private readonly TextBlock _textBlock;
        private Image _underlineImage;
        private bool _isHovered;

        private Texture2D _whiteTexture;
        private Texture2D _transparentTexture;

        public Func<bool> CanShowLine = () => true;


        public LineContainer(TextBlock textBlock)
        {
            _textBlock = textBlock;
            Orientation = Orientation.Vertical;


            InputProcessed += OnInputProcessed;
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            Children.Clear();

            _whiteTexture = Texture2DExtender.CreateColorTexture(Screen.Renderer.GraphicsDevice, Color.LightGray);
            _transparentTexture = Texture2DExtender.CreateColorTexture(Screen.Renderer.GraphicsDevice, Color.Transparent);

            Children.Add(_textBlock);
            Children.Add(_underlineImage = new Image()
            {
                Texture = _transparentTexture,
                //IsVisible = false,
                Height = 1,
                Margin = new Vector4F(3,0,3,0)
            });

            var measureString = Screen.Renderer.GetFont(_textBlock.Font).MeasureString(_textBlock.Text);
            _underlineImage.Width = measureString.X-6;
        }



        public bool IsHovered
        {
            get { return _isHovered; }
            set
            {
                _isHovered = value;

                //_underlineImage.IsVisible = IsHovered;
                _underlineImage.Texture = IsHovered && CanShowLine() ? _whiteTexture : _transparentTexture;
            }
        }
        


        private void OnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            IsHovered = _textBlock.IsMouseOver;
        }
    }
}