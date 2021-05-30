using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    public class CollisionDrawComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {
        private CollisionComponent _collisionComponent;
        private DebugRenderer _debugRenderer;


        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
        }


        public override void OnLoad()
        {
            
            _collisionComponent = GetComponentOrDefault<CollisionComponent>();
        }



        public void Draw(RenderContext context)
        {
            if (_collisionComponent != null)
                _debugRenderer.DrawShape(_collisionComponent.Shape, _collisionComponent.Pose, _collisionComponent.Scale, Color.Yellow, true, true);
        }
    }
}