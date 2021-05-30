using System;
using System.Drawing;
using System.IO;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Utils;
using Image = DigitalRune.Game.UI.Controls.Image;
using Point = Microsoft.Xna.Framework.Point;

namespace Sceelix.Designer.GUI.Controls
{
    public class RenderTargetControl : ContentControl
    {
        private readonly IGraphicsService _graphicsService;
        private readonly Image _image;
        private int _frame;

        private string _printScreenPath;

        private RenderTarget2D _renderTarget;
        private bool _shouldRender = true;

        private TimeSpan _time = TimeSpan.Zero;



        public RenderTargetControl(IGraphicsService graphicsService)
        {
            _graphicsService = graphicsService;
            Content = _image = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Foreground = Microsoft.Xna.Framework.Color.White
            };

            _renderTarget = _graphicsService.RenderTargetPool.Obtain2D(new RenderTargetFormat(2, 2, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8));
            //_renderTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, 100, 100, false, SurfaceFormat.Color, DepthFormat.Depth24, 1, RenderTargetUsage.DiscardContents);//RenderTargetUsage.PreserveContents
        }



        /// <summary>
        /// This should be overriden in the subclasses.
        /// </summary>
        public virtual bool ShouldRender
        {
            get { return _shouldRender; }
            set
            {
                _shouldRender = value;
                //if(_shouldRender)
                //    InvalidateVisual();
            }
        }



        public event Action<Point> Resized;



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (ActualWidth > 0 && ActualHeight > 0)
            {
                if (_renderTarget.Width != (int) ActualWidth || _renderTarget.Height != (int) ActualHeight)
                {
                    //keep a reference for later disposing it
                    var oldRenderTarget = _renderTarget;

                    _image.Texture = _renderTarget = _graphicsService.RenderTargetPool.Obtain2D(new RenderTargetFormat((int) ActualWidth, (int) ActualHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8));
                    //_image.Texture = _renderTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, (int)ActualWidth, (int)ActualHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 1, RenderTargetUsage.DiscardContents);

                    if (Resized != null)
                        Resized.Invoke(new Point(_renderTarget.Width, _renderTarget.Height));

                    //oldRenderTarget.Dispose();
                    _graphicsService.RenderTargetPool.Recycle(oldRenderTarget);

                    ShouldRender = true;
                }
            }

            if (ShouldRender)
                InvalidateVisual();
        }



        protected sealed override void OnRender(UIRenderContext context)
        {
            base.OnRender(context);

            if (ActualWidth > 0 && ActualHeight > 0 && ShouldRender)
            {
                var deltaTime = new TimeSpan(context.DeltaTime.Ticks); //2f
                //keep our time counter
                _time += deltaTime;
                _frame += 1;

                var rendertargetBindings = _graphicsService.GraphicsDevice.GetRenderTargets();
                var previousRenderTarget = rendertargetBindings.Any() ? rendertargetBindings[0].RenderTarget as RenderTarget2D : null;


                //var previousRenderTarget = rendertargetBindings[0] != null ? (rendertargetBindings[0].RenderTarget as RenderTarget2D) : null;

                RenderContext renderContext;
                RenderTarget2D customRenderTarget = null;
                if (!String.IsNullOrWhiteSpace(_printScreenPath))
                {
                    customRenderTarget = _graphicsService.RenderTargetPool.Obtain2D(new RenderTargetFormat((int) ActualWidth*2, (int) ActualHeight*2, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8));
                    //customRenderTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, (int)ActualWidth * 2, (int)ActualHeight * 2, false, SurfaceFormat.Color, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
                    renderContext = SetupRenderContext(context, customRenderTarget);
                    //customRenderTarget.Dispose();
                }
                else
                {
                    renderContext = SetupRenderContext(context, _renderTarget);
                }

                //RenderContext renderContext = SetupRenderContext(context, _renderTarget);
                //var previousRenderTarget = Screen.RenderData as RenderTarget2D;


                Render(renderContext);

                renderContext.GraphicsService.GraphicsDevice.SetRenderTarget(previousRenderTarget);
                //renderContext.GraphicsService.GraphicsDevice.SetRenderTarget(previousRenderTarget);

                if (!String.IsNullOrWhiteSpace(_printScreenPath))
                {
                    Bitmap bitmap = customRenderTarget.ToBitmap();
                    bitmap.Save(_printScreenPath);

                    _printScreenPath = null;

                    _graphicsService.RenderTargetPool.Recycle(customRenderTarget);
                }
            }
        }



        private RenderContext SetupRenderContext(UIRenderContext context, RenderTarget2D renderTarget)
        {
            RenderContext renderContext = new RenderContext(_graphicsService);
            renderContext.RenderTarget = renderTarget;
            renderContext.Viewport = new Viewport(0, 0, renderTarget.Width, renderTarget.Height);
            renderContext.DeltaTime = context.DeltaTime;
            renderContext.Time = _time;
            renderContext.Frame = _frame;

            renderContext.GraphicsService.GraphicsDevice.SetRenderTarget(renderTarget);

            //renderContext.GraphicsService.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Microsoft.Xna.Framework.Color.Transparent, renderContext.GraphicsService.GraphicsDevice.Viewport.MaxDepth, 0);
            renderContext.GraphicsService.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.White);

            return renderContext;
        }



        public void PrintScreen(String path)
        {
            if (_renderTarget != null)
            {
                _printScreenPath = path;
                ShouldRender = true;
            }
        }



        public void PrintScreen()
        {
            PrintScreen(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Printscreen " + DateTime.Now.ToFileTimeUtc() + ".png"));
        }



        public Bitmap GetPrintScreen()
        {
            if (_renderTarget != null)
                return _renderTarget.ToBitmap();

            return null;
        }
        


        public virtual void Render(RenderContext renderContext)
        {
        }
    }
}