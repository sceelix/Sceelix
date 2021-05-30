using System;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    public class DynamicSceneComponent : EntityObjectComponent
    {
        private readonly SceneNode _sceneNode;
        private CollisionComponent _collisionComponent;

        private Pose _relativePose;
        private Vector3F _relativeScale;

        public DynamicSceneComponent(SceneNode sceneNode)
        {
            _sceneNode = sceneNode;
        }



        public override void OnLoad()
        {
            EntityObjectDomain.SceneNodes.Add(_sceneNode);

            _collisionComponent = GetComponent<CollisionComponent>();
            
            _relativePose =  _collisionComponent.Pose.Inverse * _sceneNode.PoseWorld;
            _relativeScale = new Vector3F(1f / _collisionComponent.Scale.X, 1f / _collisionComponent.Scale.Y, 1f / _collisionComponent.Scale.Z) * _sceneNode.ScaleLocal;

            _collisionComponent.PoseChanged += CollisionComponentOnPoseChanged;

            UpdatePose();
        }



        private void CollisionComponentOnPoseChanged(object sender, EventArgs eventArgs)
        {
            UpdatePose();
        }



        private void UpdatePose()
        {
            _sceneNode.PoseWorld = _collisionComponent.Pose * _relativePose;
            _sceneNode.ScaleLocal = _collisionComponent.Scale * _relativeScale;
        }
    }
}