using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    public class CollisionHighlightComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {
        private CollisionComponent _collisionComponent;
        private DebugRenderer _debugRenderer;

        //for efficiency reasons, keep this value store here again
        private bool _isSelected;
        private SelectableEntityComponent _selectableComponent;
        private bool _alwaysVisible;



        public CollisionHighlightComponent(bool alwaysVisible = false)
        {
            _alwaysVisible = alwaysVisible;
        }


        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
        }


        public override void OnLoad()
        {
            
            _collisionComponent = GetComponentOrDefault<CollisionComponent>();
            _selectableComponent = GetComponentOrDefault<SelectableEntityComponent>();
            _selectableComponent.IsSelected.Changed += (field, value, newValue) => _isSelected = newValue;
        }



        public void Draw(RenderContext context)
        {
            if (_collisionComponent != null && (_isSelected || _alwaysVisible))
                _debugRenderer.DrawShape(_collisionComponent.Shape, _collisionComponent.Pose, _collisionComponent.Scale, Color.Yellow, true, true);
        }
    }
}