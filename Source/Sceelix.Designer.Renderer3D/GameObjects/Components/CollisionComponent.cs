using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    public class CollisionComponent : EntityObjectComponent, IGeometricObject
    {
        private readonly Shape _shape;
        private Aabb _aabb;

        private bool _aabbIsValid;

        private Pose _pose = Pose.Identity;
        private Vector3F _scale = Vector3F.One;



        public CollisionComponent(Shape shape)
        {
            _shape = shape;
            _aabb = _shape.GetAabb();
        }



        public CollisionComponent(Shape shape, Pose pose)
        {
            _shape = shape;
            _aabb = _shape.GetAabb();
            _pose = pose;
        }

        public CollisionComponent(Shape shape, Pose pose, Vector3F scale)
        {
            _shape = shape;
            _aabb = _shape.GetAabb();
            _pose = pose;
            _scale = scale;
        }



        private CollisionComponent(EntityObject parent, Shape shape)
        {
            Parent = parent;
            _shape = shape;
        }



        public event EventHandler<EventArgs> PoseChanged; // = delegate { };

        public event EventHandler<ShapeChangedEventArgs> ShapeChanged; // = delegate { };



        public IGeometricObject Clone()
        {
            return new CollisionComponent(Parent, _shape) {Pose = _pose, Scale = _scale};
        }



        public Aabb Aabb
        {
            get
            {
                if (_aabbIsValid == false)
                {
                    _aabb = Shape.GetAabb(Scale, Pose);
                    _aabbIsValid = true;
                }

                return _aabb;
            }
        }



        public Pose Pose
        {
            get { return _pose; }
            set
            {
                if (_pose != value)
                {
                    _pose = value;

                    if (PoseChanged != null)
                        PoseChanged(this, EventArgs.Empty);

                    _aabbIsValid = false;
                }
            }
        }



        public Shape Shape
        {
            get { return _shape; }
        }



        public Vector3F Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    if (ShapeChanged != null)
                        ShapeChanged.Invoke(this, ShapeChangedEventArgs.Empty);

                    _aabbIsValid = false;
                }
            }
        }

    }
}