using System;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;

namespace Sceelix.Paths.Data
{
    /// <summary>
    /// Define an edge connecting two street vertices.
    /// It is called "static", because it precalculates a couple of measurements, assuming that the vertex positions are not changed.
    /// This optimizes the process for many procedures.
    /// </summary>
    internal class StaticPathEdge
    {
        //precalculated data



        public StaticPathEdge(PathEdge edge)
        {
            Edge = edge;

            Direction = Target.Position - Source.Position;
            NormalizedDirection = Direction.Normalize();
            CenterPoint = Source.Position + NormalizedDirection * Length;
            Length = Direction.Length;
        }



        public Vector3D CenterPoint
        {
            get;
        }


        public Vector3D Direction
        {
            get;
        }


        public PathEdge Edge
        {
            get;
        }


        public float Length
        {
            get;
        }


        public LineSegment3D LineSegment => new LineSegment3D(Source.Position, Target.Position);


        public Vector3D NormalizedDirection
        {
            get;
        }


        public PathVertex Source => Edge.Source;


        public PathVertex Target => Edge.Target;



        /// <summary>
        /// Introduces a new vertex into the street and returns the 2 new static street edges
        /// </summary>
        /// <param name="vNew"></param>
        /// <returns></returns>
        /*public StaticStreetEdge[] IntroduceVertex(StreetVertex vNew)
        {
            float distanceFromV0 = (vNew.Position - V0.Position).Length;

            int v0Index = _v0[_street].StreetIndex;
            int v1Index = _v1[_street].StreetIndex;

            //in the returning/last connecting vertex
            if (v0Index > v1Index)
                v1Index = _street.Vertices.Count;

            for (int i = v0Index + 1; i <= v1Index; i++)
            {
                if (distanceFromV0 > (_street[i - 1].Position - V0.Position).Length &&
                    distanceFromV0 < (_street[i].Position - V0.Position).Length)
                {
                    _street.Vertices.Insert(i, vNew);
                    vNew.CreateHalfStreetVertex(_street, i);
                    break;
                }
            }

            _street.ResetHalfStreetVertices();

            return new []{new StaticStreetEdge(_street,_v0,vNew),new StaticStreetEdge(_street,vNew,_v1)};
        }*/
        public Vector3D CalculateIntersection(StaticPathEdge otherEdge, bool includingEnds)
        {
            return LineSegment.Intersection(otherEdge.LineSegment, includingEnds);
        }



        /*public float MiddleIntersectionValue(StaticPathEdge otherEdge, bool includingEnds)
        {
            Vector3D p1 = Source.Position;
            Vector3D p2 = otherEdge.Source.Position;
            Vector3D v1 = Target.Position - p1;
            Vector3D v2 = otherEdge.Target.Position - p2;

            Vector3D v1v2 = v1.Cross(v2);

            if (v1v2.ExactlyEquals(Vector3D.Zero))
            {
                return float.PositiveInfinity;
            }


            Vector3D p1p2 = p2 - p1;
            Vector3D p1p2v2 = p1p2.Cross(v2);

            if (!v1v2.IsCollinear(p1p2v2))
                return float.NaN;

            //REFACTORING NEEDED!!!
            float a = Vector3D.GetCommonMultiplier(p1p2v2, v1v2);

            Vector3D aV1p1p2 = v1*a - p1p2;

            double b = Vector3D.GetCommonMultiplier(aV1p1p2, v2);

            if (includingEnds)
            {
                if (a >= 0 && a <= 1 && b >= 0 && b <= 1)
                    return a;

                return float.NaN;
            }

            if (a > 0 && a < 1 && b > 0 && b < 1)
                return a;

            return float.NaN;
        }*/


        /*public PathEdge[] IntroduceVertex(PathVertex vNew)
        {
            return _edge.IntroduceVertex(vNew);
        }*/



        public bool CoincidentWith(StaticPathEdge edge)
        {
            return Source == edge.Source && Target == edge.Target
                   || Source == edge.Target && Target == edge.Source;
        }



        public bool HasPointInBetween(Vector3D position)
        {
            return Math.Abs((position - Source.Position).Normalize().Dot((position - Target.Position).Normalize()) - -1) < Vector3D.Precision;
        }



        public bool HasPointInBetween(Vector3D position, float precision)
        {
            return Math.Abs((position - Source.Position).Normalize().Dot((position - Target.Position).Normalize()) - -1) < precision;
        }



        public bool IsAtCollidableRange(StaticPathEdge se1)
        {
            return (se1.CenterPoint - CenterPoint).Length < se1.Length + Length;
        }



        /*public bool IsAtSameLine(StaticStreetEdge second)
        {
            float dotDirection = this.NormalizedDirection.Dot(second.NormalizedDirection);
            float dotStartingPoints = ( - second.V0.Position).Normalize().Dot(NormalizedDirection);

            this.V0.Position.MultipleOf(second.V0.Position);


            return  == 1 && 

        }*/



        public bool IsConnectedTo(PathVertex pathVertex)
        {
            return Source == pathVertex || Target == pathVertex;
        }
    }
}