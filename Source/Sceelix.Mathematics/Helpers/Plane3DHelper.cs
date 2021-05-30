using System;
using System.Collections.Generic;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;

namespace Sceelix.Mathematics.Helpers
{
    public class Plane3DHelper
    {
        /// <summary>
        /// Accepts a location in 3D space a
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float MovePlane(ref Plane3D plane, IEnumerable<Vector3D> points)
        {
            float size = 0;

            foreach (var point in points)
            {
                float distance = plane.DistanceToPoint(point);

                PointToPlaneLocation pointToPlaneLocation = plane.LocationToPlane(point);
                if (pointToPlaneLocation == PointToPlaneLocation.Below)
                {
                    plane = new Plane3D(plane.Normal, point);
                    size += Math.Abs(distance);
                }
                else
                {
                    size = size > distance ? size : distance;
                }
            }

            return size;
        }



        /// <summary>
        /// Moves the given plane so that it stays behind the given point. Returns the 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float MovePlane(ref Plane3D plane, Vector3D point, float size)
        {
            //TODO: Review this code. Seems overly complex.
            float distance = plane.DistanceToPoint(point);

            PointToPlaneLocation pointToPlaneLocation = plane.LocationToPlane(point);
            if (pointToPlaneLocation == PointToPlaneLocation.Below)
            {
                plane = new Plane3D(plane.Normal, point); //.Point0 = point;

                return size + Math.Abs(distance);
            }

            return size > distance ? size : distance;
        }
    }
}