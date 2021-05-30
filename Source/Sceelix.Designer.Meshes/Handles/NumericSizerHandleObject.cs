using System;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;
using Sceelix.Meshes.Handles;


namespace Sceelix.Designer.Meshes.Handles
{
    public class NumericSizerHandleObject : GameObject, IDrawableElement
    {
        private readonly IServiceLocator _services;
        private readonly NumericSizerHandle _handle;
        //private Pose _arrowPose;

        private readonly float _arrowSize = 1;
        private readonly float _baseRadius = 0.1f;
        private readonly CameraObject _cameraObject;
        private readonly Color _color = Color.Black;
        private readonly float _coneHeight = 0.5f;
        private readonly float _coneRadius = 0.2f;

        private float _currentValue;
        private readonly DebugRenderer _debugRenderer;

        private readonly Vector3F _direction;

        private readonly Color _highlightedColor = Color.White;

        private IInputService _inputService;
        //private FigureNode _arrowFigureNode;
        private bool _isHighlighted;
        private readonly MessageManager _messageManager;
        private Vector3 _offset;


        private readonly Matrix33F _orientation;
        private Vector3F _position;
        private readonly Renderer3DControl _render3DControl;
        private Vector3 _screenPosition;



        public NumericSizerHandleObject(IServiceLocator services, NumericSizerHandle handle)
        {
            _services = services;
            _handle = handle;

            _arrowSize *= handle.Scale;
            _baseRadius *= handle.Scale;
            _coneRadius *= handle.Scale;
            _coneHeight *= handle.Scale;

            _color = handle.Color.ToXnaColor();
            _highlightedColor = _color.Adjust(0.3f);

            //_direction = visualVisualHandle.Direction.Normalize().ToVector3F();

            //var point1 = visualVisualHandle.BasePosition.ToVector3F();
            //var point2 = (visualVisualHandle.BasePosition + visualVisualHandle.Direction * 2).ToVector3F();
            _currentValue = Convert.ToSingle(handle.Value);

            // Red arrow
            /*var arrow = new PathFigure2F();
            arrow.Segments.Add(new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(1, 0) });
            arrow.Segments.Add(new LineSegment2F { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, 0.1f) });
            arrow.Segments.Add(new LineSegment2F { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, -0.1f) });
            var scene = _services.Get<IScene>();*/
            /*scene.Children.AddSync(_arrowFigureNode = new FigureNode(arrow)
            {
                StrokeThickness = 4,
                StrokeColor = new Vector3F(1, 0, 0),
                PoseLocal = GetPoseOrientedTo(visualVisualHandle.BasePosition.ToVector3F(), visualVisualHandle.Direction.ToVector3F()),
                UserData = this
            });*/

            _position = handle.BasePosition.ToVector3F();
            _direction = handle.Direction.ToVector3F();
            var perpendicularDirection = handle.Direction.PerpendicularVector.ToVector3F();
            var target = _position + _direction;

            //Pose.FromMatrix(Matrix44F.CreateTranslation(position) * 
            //Pose.FromMatrix(* 

            //var axis = m.Right;

            //Matrix m = Matrix.CreateWorld(Vector3.Zero, _direction.ToXna(), perpendicularDirection.ToXna());
            //var axis = Pose.FromMatrix(m).ToWorldDirection(Vector3F.UnitX);
            //_orientation = Pose.FromMatrix(Matrix.CreateFromAxisAngle(axis.ToXna(), MathHelper.ToRadians(-90)) * m).Orientation;

            var pos = Pose.FromMatrix(Matrix.CreateWorld(Vector3.Zero, _direction.ToXna(), perpendicularDirection.ToXna()));
            var axis = pos.ToWorldDirection(Vector3F.UnitX);
            //_orientation = Pose.FromMatrix(Matrix44F.CreateRotation(axis, MathHelper.ToRadians(-90)) * pos.ToMatrix44F()).Orientation;

            _orientation = Pose.FromMatrix(Matrix44F.FromXna(Matrix.CreateFromAxisAngle(axis.ToXna(), MathHelper.ToRadians(-90)))*pos).Orientation;


            //_orientation = Pose.FromMatrix(Matrix.CreateWorld(Vector3.Zero, _direction.ToXna(), perpendicularDirection.ToXna())).Orientation;
            //_orientation = Pose.Identity.Orientation;
            //_orientation = Pose.FromMatrix(Matrix44F.CreateLookAt(Vector3F.Zero, _direction,perpendicularDirection).Inverse).Orientation; //* ;

            //_arrowPose = GetPoseOrientedTo(visualVisualHandle.BasePosition.ToVector3F(), visualVisualHandle.Direction.ToVector3F());


            _inputService = services.Get<IInputService>();
            _cameraObject = services.Get<CameraObject>();
            _messageManager = services.Get<MessageManager>();
            _render3DControl = services.Get<Renderer3DControl>();
            _debugRenderer = services.Get<DebugRenderer>();

            _render3DControl.ShouldRender = true;
        }



        /*public FigureNode ArrowFigureNode
        {
            get { return _arrowFigureNode; }
        }*/



        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;

                //_arrowFigureNode.StrokeColor = _isHighlighted ? new Vector3F(1,1,1) : new Vector3F(0, 0, 1);

                _render3DControl.ShouldRender = true;
            }
        }



        public CollisionObject CollisionObject
        {
            get
            {
                var basePosition = _position + _direction*_arrowSize/2;

                return new CollisionObject(new CustomGeometricObject(new CylinderShape(_coneRadius, _arrowSize + _coneHeight), new Pose(basePosition, _orientation)) {UserData = this});
            }
        }



        public bool IsGrabbed
        {
            get;
            set;
        }



        public void Draw(RenderContext context)
        {
            var basePosition = _position + _direction*_arrowSize/2;
            var topPosition = _position + _direction*_arrowSize;


            _debugRenderer.DrawCylinder(_baseRadius, _arrowSize, new Pose(basePosition, _orientation), _isHighlighted ? _highlightedColor : _color, false, false);

            //_debugRenderer.DrawCylinder(0.1f, 1, Pose.Identity, Color.Red, false, false);
            //_debugRenderer.DrawCylinder(0.1f, _arrowSize, new Pose(basePosition, _orientation), Color.Red, false, false);


            _debugRenderer.DrawCone(_coneRadius, _coneHeight, new Pose(topPosition, _orientation), _isHighlighted ? _highlightedColor : _color, false, false);


            //_debugRenderer.DrawCylinder(0.1f,1, _arrowPose, Color.Red,false,false);
            //_debugRenderer.DrawCone(0.1f, 1, _arrowPose, Color.Red, false, false);
            //_debugRenderer.DrawCone(0.1f, 0.5f, new Pose(_arrowPose.Position + (_visualVisualHandle.Direction).ToVector3F(), _arrowPose.Orientation), Color.Red, false, false);

            //_debugRenderer.DrawArrow(_visualVisualHandle.BasePosition.ToVector3F(), ((_visualVisualHandle.BasePosition + _visualVisualHandle.Direction * 3)).ToVector3F(), _isHighlighted ? Color.Red : Color.Black,false);
        }



        public Pose GetPoseOrientedTo(Vector3F origin, Vector3F direction)
        {
            var axis = Vector3F.Cross(direction, Vector3F.UnitX);
            var angle = Vector3F.GetAngle(direction, Vector3F.UnitX);

            if (axis.IsNumericallyZero && Math.Abs(angle) < MathHelper.PiOver2)
                return new Pose() {Position = origin, Orientation = Matrix33F.Identity};

            if (axis.IsNumericallyZero)
                return new Pose() {Position = origin, Orientation = -Matrix33F.Identity};

            return new Pose()
            {
                Position = origin, Orientation = Matrix33F.CreateRotation(axis,
                    -Vector3F.GetAngle(direction, Vector3F.UnitX))
            };
        }
        


        public void StartGrab()
        {
            _screenPosition = _cameraObject.WorldToScreen(_position.ToXna());
            _offset = _position.ToXna() - _cameraObject.ScreenToWorld(new Vector3(_cameraObject.MousePosition.X, _cameraObject.MousePosition.Y, _screenPosition.Z));

            IsGrabbed = true;
        }



        public void UpdateGrab(IInputService inputService, CameraObject cameraObject)
        {
            if (inputService.IsReleased(MouseButtons.Left))
                IsGrabbed = false;
            else
            {
                Vector3 cursorPosition = _cameraObject.ScreenToWorld(new Vector3(_cameraObject.MousePosition.X, _cameraObject.MousePosition.Y, _screenPosition.Z)) + _offset;

                var directionDelta = Vector3F.FromXna(cursorPosition) - _position;

                if (!directionDelta.IsNumericallyZero)
                {
                    var angle = Vector3F.GetAngle(directionDelta, _direction);
                    var fraction = directionDelta.Length*(float) Math.Cos(angle);

                    //Console.WriteLine(fraction);

                    var vectorToMove = _direction*fraction;

                    //var poseWorld = ArrowFigureNode.PoseWorld;
                    //poseWorld.Position += vectorToMove;
                    _position += vectorToMove;
                    //_arrowFigureNode.PoseWorld = poseWorld;

                    if (Math.Abs(fraction) > float.Epsilon)
                    {
                        _currentValue += fraction;

                        _messageManager.Publish(new ChangeParameterValue(_handle.FullName, _currentValue));
                    }

                    _render3DControl.ShouldRender = true;
                }
            }
        }
    }
}