using System;
using DigitalRune.Physics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Services;

//using Sceelix.GameEntities.Data;

namespace Sceelix.Designer.Unity3D.GameObjects.Components
{
    public class PhysicsComponent : EntityObjectComponent, IUpdateableElement, IServiceable
    {
        //private readonly Sceelix.GameEntities.Data.PhysicsComponent _component;
        private RigidBody _body;

        private CollisionComponent _collisionComponent;
        private RenderTargetControl _renderTargetControl;

        

        public PhysicsComponent(RigidBody body)
        {
            _body = body;   //new RigidBody() {Material = new UniformMaterial()};
        }


        public void Initialize(IServiceLocator services)
        {
            _renderTargetControl = services.Get<RenderTargetControl>();
        }


        public override void OnLoad()
        {
            
            _collisionComponent = GetComponent<CollisionComponent>();
            
            _body.Shape = _collisionComponent.Shape;
            _body.Pose = _collisionComponent.Pose;
            _body.Scale = _collisionComponent.Scale;

            /*_body = new RigidBody(_collisionComponent.Shape, null, new UniformMaterial()
            {
                Restitution = Bounciness
            }) {Pose = _collisionComponent.Pose,
                Scale = _collisionComponent.Scale};*/ //, poseWorld, null  Pose.Identity   Pose = new Pose(DigitalRuneConvert.ZUpRotation)          

            /*_body.MotionType = (MotionType) Enum.Parse(typeof(MotionType), _component.MotionType);
            _body.LinearVelocity = _component.LinearVelocity.ToVector3F();*/

            EntityObjectDomain.RigidBodies.Add(_body);
        }



        /*public float Bounciness
        {
            get { return (UniformMaterial)_body.Material.Restitution; };
            set;
        }*/



        public void Update(TimeSpan timeSpan)
        {
            if (_body.MotionType == MotionType.Dynamic)
            {
                if (_collisionComponent.Pose != _body.Pose)
                {
                    _collisionComponent.Pose = _body.Pose;
                    _renderTargetControl.ShouldRender = true;
                }
            }
            else if (_body.MotionType == MotionType.Kinematic)
            {
                if (_collisionComponent.Pose != _body.Pose)
                {
                    _body.Pose = _collisionComponent.Pose;
                    _renderTargetControl.ShouldRender = true;
                }
            }
        }


    }
}