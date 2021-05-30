using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Unity3D.GameObjects.Components
{
    public class TextComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {
        private readonly string _text;
        private readonly Vector3F _position;
        private DebugRenderer _debugRenderer;



        public TextComponent(String text, Vector3F position)
        {
            _text = text;
            _position = position;
        }



        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
        }



        public void Draw(RenderContext obj)
        {
            _debugRenderer.DrawText(_text, _position, Color.Black, true);
        }
    }
}
