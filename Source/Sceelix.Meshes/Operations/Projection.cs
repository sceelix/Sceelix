using System.Collections.Generic;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Operations
{
    public static class Projection
    {
        public static Projection2DInfo ProjectTo2D(this Face face)
        {
            return ProjectTo2D(face, Vector3D.ZVector);
        }



        public static Projection2DInfo ProjectTo2D(this Face face, Vector3D planeNormal)
        {
            List<Vector3D> projectedPoints = new List<Vector3D>();
            float angle = face.Normal.AngleTo(planeNormal);

            Vector3D axis = Vector3D.Cross(face.Normal, planeNormal);
            foreach (Vertex vertex in face.Vertices) projectedPoints.Add(vertex.Position.Rotate(axis, angle));

            return new Projection2DInfo(projectedPoints, angle, axis);
        }



        public struct Projection2DInfo
        {
            public List<Vector3D> ProjectedPoints
            {
                get;
            }


            public float Angle
            {
                get;
            }


            public Vector3D Axis
            {
                get;
            }



            public Projection2DInfo(List<Vector3D> projectedPoints, float angle, Vector3D axis)
                : this()
            {
                ProjectedPoints = projectedPoints;
                Angle = angle;
                Axis = axis;
            }
        }
    }
}