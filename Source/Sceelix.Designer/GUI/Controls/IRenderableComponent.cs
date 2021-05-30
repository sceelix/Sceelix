using System;
using DigitalRune.Graphics;

namespace Sceelix.Designer.GUI.Controls
{
    public interface IRenderableComponent
    {
        void Update(TimeSpan deltaTime);
        void Render(RenderContext context);
    }
}