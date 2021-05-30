using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public class ShadowHandler
    {
        private const int ShadowOpacity = 50;
        private readonly GraphControl _control;
        private Effect _graphShadowEffect;

        private RenderTarget2D _shadowTexture;



        public ShadowHandler(GraphControl control)
        {
            _control = control;
        }



        public GraphicsDevice GraphicsDevice
        {
            get { return _control.GraphicsDevice; }
        }



        public VisualGraph VisualGraph
        {
            get { return _control.VisualGraph; }
        }



        public void LoadContent(ContentManager manager)
        {
            _graphShadowEffect = manager.Load<Effect>("Effects/GraphShadow");
        }



        public void ProcessShadowTexture(SpriteBatch spritebatch, RenderContext context)
        {
            if (_shadowTexture != null)
            {
                //change the rendertarget
                GraphicsDevice.SetRenderTarget(_shadowTexture);

                GraphicsDevice.Clear(Color.Transparent);

                //draw the whole graph in black and with a small offset
                _graphShadowEffect.CurrentTechnique = _graphShadowEffect.Techniques["Black"];
                //_graphShadowEffect.CurrentTechnique.Passes[0].Apply();
                spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, _graphShadowEffect, Matrix.CreateTranslation(20, 20, 0)*_control.Camera.MatrixTransform);

                //_camera.GetTransformation(GraphicsDevice)
                VisualGraph.Draw(spritebatch, _control.Camera.MatrixTransform);
                spritebatch.End();

                //put back the old render target
                GraphicsDevice.SetRenderTarget(context.RenderTarget);
            }
        }



        public void DrawShadowTexture(SpriteBatch spritebatch)
        {
            if (_shadowTexture != null)
            {
                spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                spritebatch.Draw(_shadowTexture, Vector2.Zero, Color.FromNonPremultiplied(255, 255, 255, ShadowOpacity));
                spritebatch.End();
            }
        }



        public void Resize(Point size)
        {
            if (size.X > 0 && size.Y > 0)
                _shadowTexture = new RenderTarget2D(GraphicsDevice, (int) size.X, (int) size.Y, true, GraphicsDevice.DisplayMode.Format, DepthFormat.None);
        }
    }
}