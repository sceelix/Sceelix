using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Messages;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GUI
{

    public class CollisionObjectPicker
    {
        private readonly CameraObject _cameraObject;
        private readonly UIControl _control;
        private readonly IInputService _inputService;

        private readonly CollisionDomain _collisionDomain;
        private readonly MessageManager _messageManager;



        public CollisionObjectPicker(IServiceLocator services, UIControl control, CollisionDomain collisionDomain)
        {
            _control = control;
            _collisionDomain = collisionDomain;

            _inputService = services.Get<IInputService>();
            _cameraObject = services.Get<CameraObject>();
            _messageManager = services.Get<MessageManager>();
        }


        public void ProcessInput()
        {
            if (_collisionDomain != null &&
                !_inputService.IsMouseOrTouchHandled &&
                _control.IsMouseOver
                && _inputService.IsPressed(MouseButtons.Left, false))
            {
                //in theory, a ray would be far more efficient
                //however, it doesn't work for picking lines and points, which are too thin,
                //so we need the cone picking shape for this case (since we're only performing the picking on click)
                //we should look for faster solutions, though
                var rayShape = _cameraObject.GetConePickingShape(); //_cameraObject.GetRay();
                
                if (rayShape != null)
                {
                    var rayCollisionObject = new CollisionObject(new GeometricObject(rayShape, Pose.Identity));
                    var cameraPosition = _cameraObject.CameraNode.PoseWorld.Position;
                    
                    CollisionObject selectedCollisionObject = _collisionDomain.GetContactObjects(rayCollisionObject).OrderBy(x => (x.GeometricObject.Aabb.Center - cameraPosition).Length).FirstOrDefault();

                    _messageManager.Publish(new GeometricObjectClick(selectedCollisionObject != null ? selectedCollisionObject.GeometricObject : null));
                }

                _inputService.IsMouseOrTouchHandled = true;
            }
        }
    }
}