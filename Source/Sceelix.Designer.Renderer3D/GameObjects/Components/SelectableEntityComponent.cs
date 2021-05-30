using DigitalRune.Geometry.Collisions;
using Sceelix.Collections;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Messages;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    public class SelectableEntityComponent : EntityObjectComponent, IServiceable
    {
        private static readonly SystemKey NodeSequenceKey = new SystemKey("Node Sequence");

        private readonly IEntity _entity;
        private readonly NotifyProperty<bool> _isSelected = new NotifyProperty<bool>(false);
        private CollisionComponent _collisionComponent;

        private RenderTargetControl _renderTargetControl;
        


        public SelectableEntityComponent(IEntity entity)
        {
            _entity = entity;
        }


        public void Initialize(IServiceLocator services)
        {
            _renderTargetControl = services.Get<RenderTargetControl>();
        }


        public override void OnLoad()
        {
            _collisionComponent = GetComponentOrDefault<CollisionComponent>();
            if (_collisionComponent != null && !_collisionComponent.Aabb.Minimum.IsNaN && !_collisionComponent.Aabb.Maximum.IsNaN)
                EntityObjectDomain.CollisionObjects.Add(new CollisionObject(_collisionComponent));

            _isSelected.Changed += OnIsSelectedOnChanged;
            
            EntityObjectDomain.MessageManager.Register<NodeClicked>(OnNodeClicked); //Node.Guid
            EntityObjectDomain.MessageManager.Register<GeometricObjectClick>(OnGeometricObjectClick);
            EntityObjectDomain.MessageManager.Register<EntitySelected>(OnEntitySelected);
            EntityObjectDomain.MessageManager.Register<EntityFocused>(OnEntityFocused);
        }



        private void OnEntityFocused(EntityFocused obj)
        {
            if(obj.Entity == _entity && _collisionComponent != null)
                EntityObjectDomain.CameraObject.Frame(_collisionComponent.Aabb);
        }



        private void OnEntitySelected(EntitySelected obj)
        {
            if(obj.Location != "3D Renderer")
                IsSelected.Value = _entity == obj.Entity;
        }



        private void OnGeometricObjectClick(GeometricObjectClick obj)
        {
            var newSelectedValue = _collisionComponent != null && obj.GeometricObject == _collisionComponent;

            //if the item wasn't selected and now it is, notify
            if(newSelectedValue && !IsSelected.Value)
                EntityObjectDomain.MessageManager.Publish(new EntitySelected(_entity, "3D Renderer"));

            //if the item was selected and now it isn't, notify
            else if (!newSelectedValue && IsSelected.Value)
                EntityObjectDomain.MessageManager.Publish(new EntityDeselected(_entity, "3D Renderer"));
            
            IsSelected.Value = newSelectedValue;
        }



        private void OnNodeClicked(NodeClicked obj)
        {
            var attributeValue = _entity.Attributes.TryGet(NodeSequenceKey);
            if (attributeValue != null)
            {
                var sequence = (GraphTrail) attributeValue;
                IsSelected.Value = sequence.Objects.Contains(obj.Node);
            }
        }



        private void OnIsSelectedOnChanged(NotifyProperty<bool> field, bool value, bool newValue)
        {
            //the value was changed, we need to update the rendering
            _renderTargetControl.ShouldRender = true;
        }
        

        public IEntity Entity
        {
            get { return _entity; }
        }



        public NotifyProperty<bool> IsSelected
        {
            get { return _isSelected; }
        }
    }
}