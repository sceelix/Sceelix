using System;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.GUI.Controls
{
    public class RenderTargetGraphicsScreen : GraphicsScreen
    {
        //public event Action<Texture2D> TextureRendered = delegate { };

        private readonly IGraphicsService _graphicsService;
        private readonly IRenderableComponent _renderableComponent;

        private RenderTarget2D _renderTarget;
        private Point _size;



        public RenderTargetGraphicsScreen(IGraphicsService graphicsService, IRenderableComponent renderableComponent)
            : base(graphicsService)
        {
            _graphicsService = graphicsService;
            _renderableComponent = renderableComponent;

            //_renderTarget = new RenderTarget2D(graphicsService.GraphicsDevice, size.X, size.Y);
        }



        public Point Size
        {
            get { return _size; }
            set
            {
                if (value.X != _size.X || value.Y != _size.Y)
                {
                    _size = value;

                    _renderTarget = new RenderTarget2D(_graphicsService.GraphicsDevice, _size.X, _size.Y);
                }
            }
        }



        public Texture2D RenderedTexture
        {
            get { return _renderTarget; }
        }



        public IRenderableComponent RenderableComponent
        {
            get { return _renderableComponent; }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            _renderableComponent.Update(deltaTime);
        }



        protected override void OnRender(RenderContext context)
        {
            if (_size.X > 0 && _size.Y > 0)
            {
                context.GraphicsService.GraphicsDevice.SetRenderTarget(_renderTarget);
                context.RenderTarget = _renderTarget;
                context.Viewport = new Viewport(0, 0, _size.X, _size.Y);
                _renderableComponent.Render(context);
                context.RenderTarget = null;
                context.GraphicsService.GraphicsDevice.SetRenderTarget(null);
                //TextureRendered(_renderTarget);
            }
        }
    }
}