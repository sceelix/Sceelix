using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Collections;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;
using Sceelix.Mathematics.Data;

namespace Sceelix.Designer.Actors.Components
{
    public class ScopeHighlightComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {
        private readonly BoxScope _boxScope;
        private DebugRenderer _debugRenderer;
        private bool _isSelected;
        private Pose _pose;

        public ScopeHighlightComponent(BoxScope boxScope)
        {
            _boxScope = boxScope;
        }

        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
        }


        public override void OnLoad()
        {
            _pose = BoxScopeUtils.ToPoseWithoutScale(_boxScope);
            
            var selectableEntityComponent = GetComponent<SelectableEntityComponent>();
            selectableEntityComponent.IsSelected.Changed += (field, value, newValue) => _isSelected = newValue;
        }


        public void Draw(RenderContext context)
        {
            if (_isSelected)
                _debugRenderer.DrawAxes(_pose, 3, true);
        }
    }
}