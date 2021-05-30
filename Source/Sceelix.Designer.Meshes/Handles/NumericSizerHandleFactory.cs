using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Handles;
using Sceelix.Designer.Services;
using Sceelix.Meshes.Handles;

namespace Sceelix.Designer.Meshes.Handles
{
    public class NumericSizerHandleFactory : HandleTranslator<NumericSizerHandle>
    {
        private readonly CameraObject _cameraObject;
        private readonly CollisionDomain _collisionDomain = new CollisionDomain();

        private readonly IGameObjectService _gameObjectManager;
        private readonly IInputService _inputService;
        //private List<NumericSizerGuideObject> _numericSizerGuideObjects = new List<NumericSizerGuideObject>();
        private GameObjectGroup _numericSizerGroup;



        public NumericSizerHandleFactory(IServiceLocator services)
            : base(services)
        {
            _gameObjectManager = services.Get<IGameObjectService>();
            _inputService = services.Get<IInputService>();
            _cameraObject = services.Get<CameraObject>();
        }



        public override void Clear()
        {
            if (_numericSizerGroup != null)
            {
                foreach (var gameObject in _numericSizerGroup.GameObjects)
                    gameObject.Unload();

                _gameObjectManager.Objects.Remove(_numericSizerGroup);
                _numericSizerGroup = null;
            }

            _collisionDomain.CollisionObjects.Clear();
        }



        public override void Add(IEnumerable<NumericSizerHandle> visualHandles)
        {
            foreach (var handle in visualHandles)
            {
                var numericSizerGuideObject = new NumericSizerHandleObject(Services, handle);
                _collisionDomain.CollisionObjects.Add(numericSizerGuideObject.CollisionObject);

                if (_numericSizerGroup == null)
                {
                    _numericSizerGroup = new GameObjectGroup();
                    _gameObjectManager.Objects.Add(_numericSizerGroup);
                }

                _numericSizerGroup.GameObjects.Add(numericSizerGuideObject);
            }
        }



        public override void HandleInput(InputContext context)
        {
        }



        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);

            if (_numericSizerGroup != null)
            {
                foreach (var numericSizerGuideObject in _numericSizerGroup.GameObjects.OfType<NumericSizerHandleObject>())
                {
                    if (numericSizerGuideObject.IsGrabbed)
                    {
                        numericSizerGuideObject.UpdateGrab(_inputService, _cameraObject);
                    }
                    else
                        numericSizerGuideObject.IsHighlighted = false;
                }


                var rayShape = _cameraObject.GetConePickingShape();
                //var cone = _cameraObject.GetConePickingShape();
                if (rayShape != null)
                {
                    if (_inputService.IsPressed(MouseButtons.Left, false))
                    {
                        //var arrow = new PathFigure3F();
                        //arrow.Segments.Add(new LineSegment3F { Point1 = rayShape.Origin, Point2 = rayShape.Origin + rayShape.Direction*1000 });

                        //Services.Get<IScene>().Children.Add(new FigureNode(arrow){StrokeColor = new Vector3F(1,0,0)});
                        //Services.Get<IScene>().Children.Add(new MeshNode(_cameraObject.GetConePickingShape()));
                    }

                    var rayCollisionObject = new CollisionObject(new GeometricObject(rayShape, Pose.Identity));
                    //var coneCollisionObject = new CollisionObject(new GeometricObject(cone, _cameraObject.CameraNode.PoseWorld));

                    foreach (var collisionObject in _collisionDomain.GetContactObjects(rayCollisionObject))
                    {
                        CustomGeometricObject customGeometricObject = (CustomGeometricObject) collisionObject.GeometricObject;
                        var numericSizerGuideObject = (NumericSizerHandleObject) customGeometricObject.UserData;
                        numericSizerGuideObject.IsHighlighted = true;


                        if (_inputService.IsPressed(MouseButtons.Left, false))
                        {
                            numericSizerGuideObject.StartGrab();
                        }
                    }
                }
            }
        }
    }
}