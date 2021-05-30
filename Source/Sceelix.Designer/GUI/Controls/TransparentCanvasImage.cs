using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI.Controls
{
    public class TransparentCanvasImage : RenderTargetControl
    {
        //private Texture2D _texture;

        public static readonly int TexturePropertyId = CreateProperty<Texture2D>(typeof(Image), "Texture", GamePropertyCategories.Appearance, null, null, UIPropertyOptions.AffectsMeasure);
        private readonly SpriteBatch _spritebatch;
        private readonly Texture2D _transparentSquares;



        public TransparentCanvasImage(IGraphicsService graphicsService)
            : base(graphicsService)
        {
            _spritebatch = new SpriteBatch(graphicsService.GraphicsDevice);

            _transparentSquares = EmbeddedResources.Load<Texture2D>("Resources/TransparentSquares.png");
        }



        /// <summary>
        /// Gets or sets the texture with the image that should be displayed. 
        /// This is a game object property.
        /// </summary>
        /// <value>The texture with the image that should be displayed.</value>
        public Texture2D Texture
        {
            get { return GetValue<Texture2D>(TexturePropertyId); }
            set
            {
                SetValue(TexturePropertyId, value);
                ShouldRender = true;
            }
        }



        /// <inheritdoc/>
        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            // If nothing else is set, the desired size is determined by the SourceRectangle or 
            // the whole texture.
            Vector2F result = base.OnMeasure(availableSize);

            if (Texture == null)
                return result;

            float width = Width;
            float height = Height;
            Vector4F padding = Padding;
            Vector2F desiredSize = Vector2F.Zero;

            if (Numeric.IsPositiveFinite(width))
            {
                desiredSize.X = width;
            }
            else
            {
                int imageWidth = Texture.Width;
                desiredSize.X = padding.X + padding.Z + imageWidth;
            }

            if (Numeric.IsPositiveFinite(height))
            {
                desiredSize.Y = height;
            }
            else
            {
                int imageHeight = Texture.Height;
                desiredSize.Y = padding.Y + padding.W + imageHeight;
            }

            return desiredSize;
        }



        public override void Render(RenderContext renderContext)
        {
            renderContext.GraphicsService.GraphicsDevice.Clear(Color.Transparent);

            if (Texture != null)
            {
                _spritebatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
                _spritebatch.Draw(_transparentSquares, new Rectangle(0, 0, (int) ActualWidth, (int) ActualHeight), new Rectangle(0, 0, (int) ActualWidth, (int) ActualHeight), Color.White);
                _spritebatch.End();

                _spritebatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                _spritebatch.Draw(Texture, new Rectangle(0, 0, (int) ActualWidth, (int) ActualHeight), Color.White);
                _spritebatch.End();

                ShouldRender = false;
            }
        }
    }
}