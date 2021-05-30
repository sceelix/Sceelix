using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Meshes.GameObjects
{
    public class OrbitCameraObject : GameObject
    {
        private const float FieldOfView = ConstantsF.PiOver4;
        private readonly Vector3F _defaultPosition = new Vector3F(25, 10, -25);
        private readonly int _farDistance = 10000;

        private CameraNode _cameraNode;
        private Pose _cameraPose;
        private RenderTargetControl _control;
        private readonly IScene _scene;
        private IServiceLocator _services;

        private bool _leftMouseMoving = false;
        private bool _middleMouseMoving = false;

        //to allow the mouse to be moved and not suffer from the effects of the big position offset,
        //we use this flag to skip an update cycle
        private bool _mousePositionForced;
        private bool _rightMouseMoving = false;
        private IInputService _inputService;

        private Vector3F _target = Vector3F.Zero;


        public OrbitCameraObject(IServiceLocator services, RenderTargetControl control, IScene scene)
        {
            _control = control;
            _scene = scene;

            _inputService = services.Get<IInputService>();

            Name = "OrbitCamera";

            _services = services;
        }



        protected override void OnLoad()
        {
            // Create a camera node.
            CameraNode = new CameraNode(new Camera(new PerspectiveProjection())) { Name = "OrbitCameraNode" };

            // Add to scene.
            // (This is usually optional. Since cameras do not have a visual representation,
            // it  makes no difference if the camera is actually part of the scene graph or
            // not. - Except when other scene nodes are attached to the camera. In this case
            // the camera needs to be in the scene.)
            if (_scene != null)
                _scene.Children.Add(CameraNode);

            var graphicsService = _services.Get<IGraphicsService>();

            ResetPose();
            ResetProjection(new Point(graphicsService.GraphicsDevice.Viewport.Width, graphicsService.GraphicsDevice.Viewport.Height));
        }


        protected override void OnUnload()
        {
            if (CameraNode.Parent != null)
                CameraNode.Parent.Children.Remove(CameraNode);

            CameraNode.Dispose(false);
            CameraNode = null;
        }


        public void ResetPose()
        {
            _cameraPose = Pose.FromMatrix(Matrix44F.CreateLookAt(_defaultPosition, Vector3F.Zero, Vector3F.Up).Inverse);

            UpdateView();
        }


        public void ResetProjection(Point point)
        {
            if (IsLoaded)
            {
                var graphicsService = _services.Get<IGraphicsService>();
                var projection = (PerspectiveProjection)CameraNode.Camera.Projection;
                projection.SetFieldOfView(
                    FieldOfView,
                    (float)point.X / (float)point.Y,
                    1f,
                    _farDistance);
                /*CameraNode.View = Matrix44F.CreatePerspectiveFieldOfView(ConstantsF.PiOver4,
                    (float) point.X/(float) point.Y,
                    0.1f,
                    _farDistance);*/
            }
        }


        private void UpdateView()
        {
            //CameraNode.PoseWorld = Pose.FromMatrix(Matrix44F.CreateLookAt(_position, _target, _upVector).Inverse);
            CameraNode.PoseWorld = _cameraPose;

            //and refresh the screen, of course
            _control.ShouldRender = true;
        }


        // This property is null while the CameraObject is not added to the game
        // object service.
        public CameraNode CameraNode
        {
            get { return _cameraNode; }
            private set { _cameraNode = value; }
        }


        public Viewport ViewPort
        {
            get { return new Viewport(0, 0, (int)_control.ActualWidth, (int)_control.ActualHeight); }
        }



        public void OnHandleInput(InputContext context)
        {
            HandleMouse();
        }


        private void HandleMouse()
        {
            if (!_inputService.IsMouseOrTouchHandled && !_mousePositionForced)
            {
                bool wasMouseInteracted = false;

                _leftMouseMoving = _inputService.IsDown(MouseButtons.Left)
                                   && (_control.IsMouseOver || _leftMouseMoving);

                _middleMouseMoving = (_inputService.IsDown(MouseButtons.Middle) || (_inputService.IsDown(MouseButtons.Left) && _inputService.ModifierKeys.HasFlag(ModifierKeys.Shift)))
                                     && (_control.IsMouseOver || _middleMouseMoving);

                _rightMouseMoving = _inputService.IsDown(MouseButtons.Right)
                                    && (_control.IsMouseOver || _rightMouseMoving);


                //get the vector indicating the direction left to the one we are looking at (normal)
                //Vector3F crossDirection = Vector3F.Cross(_upVector, Direction);
                Vector3F crossDirection = _cameraPose.ToWorldDirection(Vector3F.Left);
                var invertLookFactor = 1;

                if (_rightMouseMoving)
                {
                    var movement = crossDirection * MouseSpeed * _inputService.MousePositionDelta.X * 2f
                                   + _cameraPose.ToWorldDirection(Vector3F.Up) * MouseSpeed * _inputService.MousePositionDelta.Y * 2f;

                    _cameraPose.Position += movement;
                    RotateMouseCursorAroundCorners();

                    wasMouseInteracted = true;
                }

                if (_leftMouseMoving)
                {
                    //Rotation around the target
                    //calculate the transform to be applied over the target
                    var transform = Matrix44F.CreateTranslation(_target)
                                    * Matrix44F.CreateRotationY(MouseSpeed * -_inputService.MousePositionDelta.X / 10f)
                                    * Matrix44F.CreateRotation(crossDirection, invertLookFactor * -MouseSpeed * -_inputService.MousePositionDelta.Y / 10f)
                                    * Matrix44F.CreateTranslation(-_target) * _cameraPose.ToMatrix44F();
                    
                    _cameraPose = Pose.FromMatrix(transform);

                    RotateMouseCursorAroundCorners();

                    wasMouseInteracted = true;
                }

                if (_middleMouseMoving)
                {
                    //window.Activate();

                    _cameraPose.Position += Direction * (MouseSpeed * -_inputService.MousePositionDelta.Y);

                    RotateMouseCursorAroundCorners();

                    wasMouseInteracted = true;
                }

                //zoom in and out with the scrollwheel
                if (_control.IsMouseOver)
                {
                    _cameraPose.Position += Direction * (_inputService.MouseWheelDelta * MouseSpeed);

                    if (Math.Abs(_inputService.MouseWheelDelta) > float.Epsilon)
                        wasMouseInteracted = true;
                }

                if (wasMouseInteracted)
                {
                    UpdateView();
                    _inputService.IsMouseOrTouchHandled = true;
                }
            }
            else
            {
                _mousePositionForced = false;
            }
        }


        private void RotateMouseCursorAroundCorners()
        {
            var screen = _control.UIService.Screens.First();

            var maxWidth = screen.ActualWidth - 1;
            var maxHeight = screen.ActualHeight - 1;

            var currentPosition = _inputService.MousePosition;

            if (_inputService.MousePosition.X <= 0)
            {
                currentPosition.X = maxWidth;
                _mousePositionForced = true;
            }
            else if (_inputService.MousePosition.X >= maxWidth)
            {
                currentPosition.X = 0;
                _mousePositionForced = true;
            }

            if (_inputService.MousePosition.Y <= 0)
            {
                currentPosition.Y = maxHeight;
                _mousePositionForced = true;
            }
            else if (_inputService.MousePosition.Y >= maxHeight)
            {
                currentPosition.Y = 0;
                _mousePositionForced = true;
            }


            MouseHelper.SetPosition((int)currentPosition.X, (int)currentPosition.Y);
        }


        public Vector3F Direction
        {
            get
            {
                return _cameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
                //return (_target - _position).Normalized; }
            }
        }


        public float MouseSpeed
        {
            //get { return _settings.MouseSpeed.Value / 100f; 
            get { return 1 / 100f; }
        }



        public void Frame(float distance)
        {
            Vector3F cameraDirection = CameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
            
            //we can already assign the position and target now, if we don't call the UpdateView
            _cameraPose.Position = - cameraDirection.Normalized * distance;

            UpdateView();
        }
    }
}
