using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Graphs.GUI.Navigation;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using MathHelper = DigitalRune.Mathematics.MathHelper;
using Ray = DigitalRune.Geometry.Shapes.Ray;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    // Creates and controls a 3D camera node. (The camera node is not automatically
    // set as the active camera in any graphics screen. This needs to be done by the
    // sample.)
    public class CameraObject : GameObject
    {
        private const float FieldOfView = ConstantsF.PiOver4;
        private const float SpeedBoost = 10;
        private readonly RenderTargetControl _control;
        private readonly IInputService _inputService;

        private readonly IServiceLocator _services;

        private readonly Renderer3DSettings _settings;

        private CameraAnimator _cameraAnimator;
        private CameraNode _cameraNode;
        private Pose _cameraPose;
        private DebugRenderer _debugRenderer;

        // Position and Orientation of camera.
        private readonly Vector3F _defaultPosition = new Vector3F(25, 10, -25);

        private readonly float _farDistance;
        private bool _isEnabled;

        private bool _leftMouseMoving = false;
        private bool _middleMouseMoving = false;

        //to allow the mouse to be moved and not suffer from the effects of the big position offset,
        //we use this flag to skip an update cycle
        private bool _mousePositionForced;
        private bool _rightMouseMoving = false;

        //Major properties, like the position and the target
        //private Vector3F _upVector = Vector3F.UnitY;
        //private Vector3F _position = new Vector3F(1, 1, 1);
        private Vector3F _target = Vector3F.Zero;
        private BarMenuService _barMenuService;
        private Renderer3DWindow _window;


        //on the plugin load, grab all the classes that have a particular tag
        //
        //private static ConfigurableInputCommand Mouse = new ConfigurableInputCommand(){PrimaryMapping = };


        public CameraObject(IServiceLocator services, RenderTargetControl control)
        {
            _control = control;

            Name = "Camera";

            _services = services;
            _inputService = services.Get<IInputService>();
            _settings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
            _debugRenderer = services.Get<DebugRenderer>();
            _window = services.Get<Renderer3DWindow>();
            _barMenuService = services.Get<BarMenuService>();

            //this could be assigned a settings value
            _farDistance = 10000;

            //enabled by default
            IsEnabled = true;

            _barMenuService.RegisterMenuEntry("Camera/Reset View", ResetPose, EmbeddedResources.Load<Texture2D>("Resources/Arrow1DownRight_16x16.png"));
        }
        


        // This property is null while the CameraObject is not added to the game
        // object service.
        public CameraNode CameraNode
        {
            get { return _cameraNode; }
            private set { _cameraNode = value; }
        }



        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }



        public Viewport ViewPort
        {
            get { return new Viewport(0, 0, (int) _control.ActualWidth, (int) _control.ActualHeight); }
        }



        public Vector2F MousePosition
        {
            get { return _control.GetMouseRelativePosition(_inputService.MousePosition); }
        }



        public float CameraSpeed
        {
            get { return _settings.WalkingSpeed.Value/10f; }
        }



        public float MouseSpeed
        {
            get { return _settings.MouseSpeed.Value/100f; }
        }



        public Vector3F Direction
        {
            get
            {
                return _cameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
                //return (_target - _position).Normalized; }
            }
        }



        // OnLoad() is called when the GameObject is added to the IGameObjectService.
        protected override void OnLoad()
        {
            // Create a camera node.
            CameraNode = new CameraNode(new Camera(new PerspectiveProjection())) {Name = "PlayerCamera"};

            // Add to scene.
            // (This is usually optional. Since cameras do not have a visual representation,
            // it  makes no difference if the camera is actually part of the scene graph or
            // not. - Except when other scene nodes are attached to the camera. In this case
            // the camera needs to be in the scene.)
            var scene = _services.Get<IScene>();
            if (scene != null)
                scene.Children.Add(CameraNode);

            var graphicsService = _services.Get<IGraphicsService>();

            ResetPose();
            ResetProjection(new Point(graphicsService.GraphicsDevice.Viewport.Width, graphicsService.GraphicsDevice.Viewport.Height));
        }



        // OnUnload() is called when the GameObject is removed from the IGameObjectService.
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



        private void UpdateView()
        {
            //CameraNode.PoseWorld = Pose.FromMatrix(Matrix44F.CreateLookAt(_position, _target, _upVector).Inverse);
            CameraNode.PoseWorld = _cameraPose;

            //and refresh the screen, of course
            _control.ShouldRender = true;
        }



        private void UpdateView(Matrix matrix)
        {
            CameraNode.PoseWorld = Pose.FromMatrix(matrix);

            //and refresh the screen, of course
            _control.ShouldRender = true;
        }



        public void ResetProjection(Point point)
        {
            if (IsLoaded)
            {
                var graphicsService = _services.Get<IGraphicsService>();
                var projection = (PerspectiveProjection) CameraNode.Camera.Projection;
                projection.SetFieldOfView(
                    FieldOfView,
                    (float) point.X/(float) point.Y,
                    1f,
                    _farDistance);
                /*CameraNode.View = Matrix44F.CreatePerspectiveFieldOfView(ConstantsF.PiOver4,
                    (float) point.X/(float) point.Y,
                    0.1f,
                    _farDistance);*/
            }
        }



        public void OnHandleInput(InputContext context)
        {
            //do not allow updates while the camera is moving
            if (!IsEnabled || _cameraAnimator != null)
                return;

            HandleMouse(_window);

            HandleKeyboard(_window);
        }



        private void HandleMouse(Window window)
        {
            /*Pose p = new Pose();
            var frontDir = p.ToWorldDirection(Vector3F.Forward);
            p.Position += frontDir;
            Quaternion*/

            if (!_inputService.IsMouseOrTouchHandled && !_mousePositionForced)
            {
                //if (!_mousePositionForced)
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
                var invertLookFactor = _settings.InvertLook.Value ? -1 : 1;

                if (_rightMouseMoving)
                {
                    if (_inputService.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    {
                        //zoom in fast
                        _cameraPose.Position += Direction*(MouseSpeed*-_inputService.MousePositionDelta.Y);
                        /*var movement = Direction * (MouseSpeed * -_inputService.MousePositionDelta.Y);
                            
                        _position += movement;
                        _target += movement;*/
                    }
                    else
                    {
                        //rotation around the position (FPS look)
                        //calculate the transform to be applied over the target
                        var transform = Matrix44F.CreateTranslation(_cameraPose.Position)
                                        *Matrix44F.CreateRotationY(MouseSpeed*-_inputService.MousePositionDelta.X/10f)
                                        *Matrix44F.CreateRotation(crossDirection, invertLookFactor*MouseSpeed*_inputService.MousePositionDelta.Y/10f)
                                        *Matrix44F.CreateTranslation(-_cameraPose.Position)*_cameraPose.ToMatrix44F();

                        _cameraPose = Pose.FromMatrix(transform);
                    }

                    RotateMouseCursorAroundCorners();

                    wasMouseInteracted = true;
                }

                if (_leftMouseMoving && _inputService.ModifierKeys.HasFlag(ModifierKeys.Alt))
                {
                    //Rotation around the target
                    //calculate the transform to be applied over the target
                    var transform = Matrix44F.CreateTranslation(_target)
                                    *Matrix44F.CreateRotationY(MouseSpeed*-_inputService.MousePositionDelta.X/10f)
                                    *Matrix44F.CreateRotation(crossDirection, invertLookFactor*-MouseSpeed*-_inputService.MousePositionDelta.Y/10f)
                                    *Matrix44F.CreateTranslation(-_target)*_cameraPose.ToMatrix44F();

                    //_position = transform.TransformPosition(_position);
                    _cameraPose = Pose.FromMatrix(transform);

                    RotateMouseCursorAroundCorners();

                    wasMouseInteracted = true;
                }

                if (_middleMouseMoving)
                {
                    window.Activate();

                    var movement = crossDirection*MouseSpeed*_inputService.MousePositionDelta.X*2f
                                   + _cameraPose.ToWorldDirection(Vector3F.Up)*MouseSpeed*_inputService.MousePositionDelta.Y*2f;

                    _cameraPose.Position += movement;

                    RotateMouseCursorAroundCorners();

                    wasMouseInteracted = true;
                }

                //zoom in and out with the scrollwheel
                if (_control.IsMouseOver)
                {
                    _cameraPose.Position += Direction*(_inputService.MouseWheelDelta*MouseSpeed);
                    /*_position += Direction * (_inputService.MouseWheelDelta * MouseSpeed);
                    _target += Direction * (_inputService.MouseWheelDelta * MouseSpeed);*/

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


            MouseHelper.SetPosition((int) currentPosition.X, (int) currentPosition.Y);
        }



        private void HandleKeyboard(Window window)
        {
            //check for keyboard input
            if (!_inputService.IsKeyboardHandled && window.IsActive)
            {
                var wasKeyInteracted = false;

                //get the vector indicating the direction left to the one we are looking at (normal)
                Vector3F crossDirection = _cameraPose.ToWorldDirection(Vector3F.Left);

                var actualCameraSpeed = _inputService.ModifierKeys.HasFlag(ModifierKeys.Shift) ? CameraSpeed*SpeedBoost : CameraSpeed;

                //if (_inputService.IsDown(Keys.LeftShift))
                var movement = Direction*actualCameraSpeed;
                var crossMovement = crossDirection*actualCameraSpeed;
                var verticalMovement = _cameraPose.ToWorldDirection(Vector3F.Up)*actualCameraSpeed;

                if (_inputService.IsPressed(Keys.Home, false))
                {
                    ResetPose();
                    wasKeyInteracted = true;
                }
                if (_inputService.IsDown(Keys.W))
                {
                    _cameraPose.Position += movement;

                    wasKeyInteracted = true;
                }
                if (_inputService.IsDown(Keys.S))
                {
                    _cameraPose.Position -= movement;

                    wasKeyInteracted = true;
                }
                if (_inputService.IsDown(Keys.A))
                {
                    _cameraPose.Position += crossMovement;

                    wasKeyInteracted = true;
                }
                if (_inputService.IsDown(Keys.D))
                {
                    _cameraPose.Position -= crossMovement;

                    wasKeyInteracted = true;
                }
                if (_inputService.IsDown(Keys.Space))
                {
                    _cameraPose.Position += verticalMovement;

                    wasKeyInteracted = true;
                }
                if (_inputService.IsDown(Keys.C))
                {
                    _cameraPose.Position -= verticalMovement;

                    wasKeyInteracted = true;
                }

                /*if (_inputService.IsPressed(Keys.P, false))
                {
                    _inputService.Settings.MouseCenter = new Vector2F(_control.ActualX + _control.ActualWidth / 2f, _control.ActualY + _control.ActualHeight / 2f);
                    _inputService.EnableMouseCentering = !_inputService.EnableMouseCentering;
                    //_control.ShouldRender = true;
                    //_middleMouseMoving = _inputService.EnableMouseCentering;
                }*/
                if (wasKeyInteracted)
                {
                    UpdateView();
                    _inputService.IsKeyboardHandled = true;
                }
            }
        }



        // OnUpdate() is called once per frame.
        protected override void OnUpdate(TimeSpan deltaTime)
        {
            if (_cameraAnimator != null)
            {
                var matrixTransform = _cameraAnimator.Update(deltaTime);

                if (_cameraAnimator.IsOver)
                {
                    _cameraAnimator = null;

                    //do a last update, fixing minor accuracy errors
                    UpdateView();
                }
                else
                {
                    //where we're animating, we should render every frame
                    UpdateView(matrixTransform);
                }
            }
        }



        public RayShape GetRay()
        {
            if (!_control.IsMouseOver)
                return null;

            Vector2F position = _control.GetMouseRelativePosition(_inputService.MousePosition);
            Viewport viewPort = ViewPort;

            var rayStart = viewPort.Unproject(new Vector3F(position.X, position.Y, 0), CameraNode.Camera.Projection.ToMatrix44F(), CameraNode.View);
            var rayEnd = viewPort.Unproject(new Vector3F(position.X, position.Y, 1), CameraNode.Camera.Projection.ToMatrix44F(), CameraNode.View);
            var direction = rayEnd - rayStart;

            return new RayShape(rayStart, direction.Normalized, direction.Length);
        }



        public Vector3F UnProject(Vector2F vector2)
        {
            Viewport viewPort = new Viewport(0, 0, (int) _control.ActualWidth, (int) _control.ActualHeight);

            Vector3F rayStart = Vector3F.FromXna(viewPort.Unproject(new Vector3(0, 0, 0),
                CameraNode.Camera.Projection.ToXna(), CameraNode.View.ToXna(), Matrix.Identity));

            Vector3F rayEnd = Vector3F.FromXna(viewPort.Unproject(new Vector3(vector2.X, vector2.Y, 0),
                CameraNode.Camera.Projection.ToXna(), CameraNode.View.ToXna(), Matrix.Identity));

            return rayEnd - rayStart;
        }



        public Vector3F Get3DOffset()
        {
            Viewport viewPort = new Viewport(0, 0, (int) _control.ActualWidth, (int) _control.ActualHeight);

            Vector3F rayStart = Vector3F.FromXna(viewPort.Unproject(new Vector3(_inputService.PreviousMouseState.X, _inputService.PreviousMouseState.Y, 1),
                CameraNode.Camera.Projection.ToXna(), CameraNode.View.ToXna(), Matrix.Identity));

            Vector3F rayEnd = Vector3F.FromXna(viewPort.Unproject(new Vector3(_inputService.MouseState.X, _inputService.MouseState.Y, 1),
                CameraNode.Camera.Projection.ToXna(), CameraNode.View.ToXna(), Matrix.Identity));

            return rayEnd - rayStart;
        }



        public Vector3 WorldToScreen(Vector3 worldPosition)
        {
            return ViewPort.Project(worldPosition, CameraNode.Camera.Projection.ToXna(), CameraNode.View.ToXna(),
                Matrix.Identity);
        }



        public Vector3 ScreenToWorld(Vector3 screenPosition)
        {
            return ViewPort.Unproject(screenPosition, CameraNode.Camera.Projection.ToXna(), CameraNode.View.ToXna(), Matrix.Identity);
        }



        public void Frame(Aabb aabb)
        {
            if (aabb.Center.IsNaN)
                return;

            Vector3F cameraDirection = CameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);

            /*var planeNormal = Vector3F.Cross(cameraDirection, Vector3F.Up).Normalized;
            
                
            var distances = GetPoints(aabb).Select(x => DistanceToPoint(x, aabb.Center, planeNormal));
            var size = distances.Max();


            var points = GetPoints(aabb).Select(x => WorldToScreen(x.ToXna())).ToList();
            var minX = points.Min(x => x.X);
            var minY = points.Min(x => x.Y);
            var maxX = points.Max(x => x.X);
            var maxY = points.Max(x => x.Y);
            var averageZ = points.Average(x => x.Z);

            var leftPoint = ScreenToWorld(new Vector3(minX, (maxY - minY)/2f, averageZ));
            var rightPoint = ScreenToWorld(new Vector3(maxX, (maxY - minY) / 2f, averageZ));
            float size = (leftPoint - rightPoint).Length() / 2f;*/


            float size = (aabb.Center - aabb.Minimum).Length;
            float distance = size/(float) Math.Tan(FieldOfView / 2f);

            

            //we can already assign the position and target now, if we don't call the UpdateView
            _cameraPose.Position = aabb.Center - cameraDirection.Normalized*distance;
            _target = aabb.Center;

            //_cameraAnimator = new CameraObjectAnimator(CameraNode.PoseWorld.ToXna(), newPosition, newTarget, _upVector, 500);
            _cameraAnimator = new CameraAnimator(CameraNode.PoseWorld.ToXna(), Matrix44F.CreateLookAt(_cameraPose.Position, _target, _cameraPose.ToWorldDirection(Vector3F.Up)).Inverse.ToXna(), 500);
        }


        public float DistanceToPoint(Vector3F point, Vector3F point0, Vector3F normal)
        {
            //should be divided by the size of the normal, but it is 1...
            return Math.Abs(Vector3F.Dot(point - point0,normal));
        }


        private IEnumerable<Vector3F> GetPoints(Aabb aabb)
        {
            var extent = aabb.Extent;

            yield return aabb.Minimum;
            yield return aabb.Minimum + new Vector3F(extent.X, 0,0);
            yield return aabb.Minimum + new Vector3F(extent.X, extent.Y, 0);
            yield return aabb.Minimum + new Vector3F(extent.X, 0, extent.Z);
            yield return aabb.Minimum + new Vector3F(0, extent.Y, 0);
            yield return aabb.Minimum + new Vector3F(0, extent.Y, extent.Z);
            yield return aabb.Minimum + new Vector3F(0, 0, extent.Z);
            yield return aabb.Maximum;
        }



        public Shape GetConePickingShape()
        {
            if (!_control.IsMouseOver)
                return null;

            Vector2F position = _control.GetMouseRelativePosition(_inputService.MousePosition);
            Viewport viewPort = new Viewport(0, 0, (int) _control.ActualWidth, (int) _control.ActualHeight);

            //first, we determine the direction of our mouse click
            Vector3F mouseClosePosition = viewPort.Unproject(new Vector3F(position.X, position.Y, 0), CameraNode.Camera.Projection.ToMatrix44F());
            Vector3F mouseFarPosition = viewPort.Unproject(new Vector3F(position.X, position.Y, 1), CameraNode.Camera.Projection.ToMatrix44F());

            var direction = mouseFarPosition - mouseClosePosition;

            //then, we determine the direction from the camera center
            Vector3F centerClosePosition = viewPort.Unproject(new Vector3F(viewPort.Width/2.0f, viewPort.Height/2.0f, 0), CameraNode.Camera.Projection.ToMatrix44F());
            Vector3F centerFarPosition = viewPort.Unproject(new Vector3F(viewPort.Width/2.0f, viewPort.Height/2.0f, 1), CameraNode.Camera.Projection.ToMatrix44F());

            var offsetDirection = centerFarPosition - centerClosePosition;

            //we calculate the angles and pose of this offset rotation
            var angle = Vector3F.GetAngle(direction, offsetDirection);
            var axis = Vector3F.Cross(offsetDirection, direction);

            var rotationPose = new Pose(Vector3F.Zero, Matrix33F.CreateRotation(axis, angle));

            //determine a picking radius for the cone
            var pickingRadius = 5;
            float radius = viewPort.Unproject(new Vector3(viewPort.Width/2.0f + pickingRadius, viewPort.Height/2.0f, 1), //viewPort.Width / 2.0f + pickingRadius, viewPort.Height / 2.0f
                (Matrix) CameraNode.Camera.Projection.ToMatrix44F(),
                Matrix.Identity,
                Matrix.Identity).X;

            var thePose = new Pose(new Vector3F(0, 0, -CameraNode.Camera.Projection.Far), Matrix33F.CreateRotationX(ConstantsF.PiOver2));

            // A transformed shape is used to rotate and translate the cone.
            return new TransformedShape(
                new GeometricObject(
                    new ConeShape(radius, CameraNode.Camera.Projection.Far),
                    CameraNode.PoseWorld*rotationPose*thePose));
        }

        

        /// <summary>
        /// Starts a camera animation to a certain matrix tranformation
        /// </summary>
        /// <param name="targetTransform"></param>
        public void AnimateCameraTo(Matrix targetTransform)
        {
        }
    }
}