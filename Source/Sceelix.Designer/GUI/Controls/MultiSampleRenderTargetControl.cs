using System;
using System.Drawing;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Utils;
using Color = Microsoft.Xna.Framework.Color;
using Image = DigitalRune.Game.UI.Controls.Image;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Sceelix.Designer.GUI.Controls
{
    public class MultiSampleRenderTargetControl : ContentControl
    {
        private readonly IGraphicsService _graphicsService;
        private readonly Image _image;

        private readonly SpriteBatch _spriteBatch;
        private RenderTarget2D _multiSampledTarget;

        private float _multiSamplingFactor = 4f;

        private RenderTarget2D _renderTarget;

        private bool _shouldRender = true;



        public MultiSampleRenderTargetControl(IGraphicsService graphicsService)
        {
            _graphicsService = graphicsService;
            Content = _image = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            //create kind of a temporary rendertarget
            _renderTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, 100, 100, false, SurfaceFormat.Color, DepthFormat.Depth24);

            _spriteBatch = new SpriteBatch(graphicsService.GraphicsDevice);
        }



        public float MultiSamplingFactor
        {
            get { return _multiSamplingFactor; }
            set { _multiSamplingFactor = value; }
        }



        /// <summary>
        /// This should be overriden in the subclasses.
        /// </summary>
        public virtual bool ShouldRender
        {
            get { return _shouldRender; }
            set { _shouldRender = value; }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (ActualWidth > 0 && ActualHeight > 0)
            {
                if (_renderTarget.Width != (int) ActualWidth || _renderTarget.Height != (int) ActualHeight)
                {
                    _multiSampledTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, (int) (ActualWidth*MultiSamplingFactor), (int) (ActualHeight*MultiSamplingFactor), false, SurfaceFormat.Color, DepthFormat.Depth24);

                    _image.Texture = _renderTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, (int) ActualWidth, (int) ActualHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
                    Resized(new Point((int) (_renderTarget.Width*MultiSamplingFactor), (int) (_renderTarget.Height*MultiSamplingFactor)));
                }
            }
        }



        protected sealed override void OnRender(UIRenderContext context)
        {
            if (ActualWidth > 0 && ActualHeight > 0 && _multiSampledTarget != null)
            {
                RenderContext renderContext = new RenderContext(_graphicsService);

                renderContext.GraphicsService.GraphicsDevice.SetRenderTarget(_multiSampledTarget);
                renderContext.RenderTarget = _multiSampledTarget;
                renderContext.Viewport = new Viewport(0, 0, (int) (ActualWidth*MultiSamplingFactor), (int) (ActualHeight*MultiSamplingFactor));

                Render(renderContext);

                //now that the content has been drawn to the auxiliary rendertarget, render it to the actual image texture
                renderContext.GraphicsService.GraphicsDevice.SetRenderTarget(_renderTarget);
                _spriteBatch.Begin();
                _spriteBatch.Draw(_multiSampledTarget, new Rectangle(0, 0, (int) ActualWidth, (int) ActualHeight), Color.White);
                _spriteBatch.End();

                renderContext.GraphicsService.GraphicsDevice.SetRenderTarget(null);
            }

            base.OnRender(context);
        }



        public void PrintScreen(String path)
        {
            if (_renderTarget != null)
            {
                Bitmap bitmap = _renderTarget.ToBitmap();
                bitmap.Save(path);
            }
        }



        protected virtual void Resized(Point point)
        {
        }



        public virtual void Render(RenderContext renderContext)
        {
        }
    }
}