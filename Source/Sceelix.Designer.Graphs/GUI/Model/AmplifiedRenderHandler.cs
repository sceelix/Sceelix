using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Graphs.GUI.Interactions;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public class AmplifiedRenderHandler
    {
        private RenderTarget2D _amplifiedTexture;
        private readonly GraphControl _control;
        private readonly float _factor = 2;
        private Rectangle _rectangle;



        public AmplifiedRenderHandler(GraphControl control)
        {
            _control = control;
        }


        public void ProcessAmplifiedTexture(SpriteBatch spritebatch, RenderContext context)
        {
            if (_amplifiedTexture != null)
            {
                //change the rendertarget
                GraphicsDevice.SetRenderTarget(_amplifiedTexture);

                GraphicsDevice.Clear(Color.Transparent);

                //draw the whole graph in black and with a small offset

                //_graphShadowEffect.CurrentTechnique.Passes[0].Apply();
                //spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _control.Camera.MatrixTransform * Matrix.CreateScale(_factor));// * Matrix.CreateScale(_factor)
                spritebatch.Begin(SpriteSortMode.Deferred, transformMatrix: _control.Camera.MatrixTransform*Matrix.CreateScale(_factor)); // * Matrix.CreateScale(_factor)

                //_camera.GetTransformation(GraphicsDevice)
                VisualGraph.Draw(spritebatch, _control.Camera.MatrixTransform*Matrix.CreateScale(_factor));
                InteractionHandler.Draw(spritebatch);

                spritebatch.End();

                //put back the old render target
                GraphicsDevice.SetRenderTarget(context.RenderTarget);
            }
        }



        public void DrawAmplifiedTexture(SpriteBatch spritebatch)
        {
            if (_amplifiedTexture != null)
            {
                spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spritebatch.Draw(_amplifiedTexture, _rectangle, Color.White);
                spritebatch.End();
            }
        }



        public void Resize(Point size)
        {
            if (size.X > 0 && size.Y > 0)
            {
                _amplifiedTexture = new RenderTarget2D(GraphicsDevice, (int) (size.X*_factor), (int) (size.Y*_factor), false, SurfaceFormat.Color, DepthFormat.None);
                _rectangle = new Rectangle(0, 0, (int) (size.X), (int) (size.Y));
            }
        }





        public GraphicsDevice GraphicsDevice
        {
            get { return _control.GraphicsDevice; }
        }



        public VisualGraph VisualGraph
        {
            get { return _control.VisualGraph; }
        }



        public InteractionHandler InteractionHandler
        {
            get { return _control.InteractionHandler; }
        }

    }
}