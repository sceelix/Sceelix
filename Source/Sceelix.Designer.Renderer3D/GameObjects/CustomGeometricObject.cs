using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    public class CustomGeometricObject : GeometricObject
    {
        public CustomGeometricObject()
        {
        }



        public CustomGeometricObject(Shape shape) : base(shape)
        {
        }



        public CustomGeometricObject(Shape shape, Vector3F scale) : base(shape, scale)
        {
        }



        public CustomGeometricObject(Shape shape, Pose pose) : base(shape, pose)
        {
        }



        public CustomGeometricObject(Shape shape, Vector3F scale, Pose pose) : base(shape, scale, pose)
        {
        }



        public CustomGeometricObject(object userData)
        {
            UserData = userData;
        }



        public object UserData
        {
            get;
            set;
        }



        protected override void CloneCore(GeometricObject source)
        {
            base.CloneCore(source);

            UserData = ((CustomGeometricObject) source).UserData;
        }



        protected override GeometricObject CreateInstanceCore()
        {
            return new CustomGeometricObject();
        }
    }
}