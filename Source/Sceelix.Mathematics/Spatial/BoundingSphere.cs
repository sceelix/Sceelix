using System;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Spatial
{
    public struct BoundingSphere
    {
        private float _radius;
        private float _radiusSqr;
        public Vector3D Center;



        public BoundingSphere(Vector3D center) : this(center, 0)
        {
        }



        public BoundingSphere(Vector3D center, float radius)
        {
            _radius = radius;
            _radiusSqr = radius * radius;
            Center = center;
        }



        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _radiusSqr = _radius * _radius;
            }
        }



        public float RadiusSqr
        {
            get { return _radiusSqr; }
            set
            {
                _radiusSqr = value;
                _radius = (float) Math.Sqrt(_radiusSqr);
            }
        }



        /// <summary>
        /// Checks if this sphere intersects the target sphere
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Intersects(BoundingSphere target)
        {
            double radiusSum = target.Radius + Radius;
            Vector3D centerDifference = target.Center - Center;

            //fast check
            if (centerDifference.X > radiusSum || centerDifference.Y > radiusSum || centerDifference.Z > radiusSum)
                return false;

            //slow check
            if (centerDifference.Length > radiusSum)
                return false;

            return true;
        }
    }
}