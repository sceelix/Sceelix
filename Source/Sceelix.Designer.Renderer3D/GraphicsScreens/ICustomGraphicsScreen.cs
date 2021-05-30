using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;

namespace Sceelix.Designer.Renderer3D.GraphicsScreens
{
    public interface ICustomGraphicsScreen
    {
        DebugRenderer DebugRenderer
        {
            get;
        }



        Scene Scene
        {
            get;
        }



        CameraNode ActiveCameraNode
        {
            get;
            set;
        }



        void Update(TimeSpan deltaTime);

        void Render(RenderContext context);
    }
}