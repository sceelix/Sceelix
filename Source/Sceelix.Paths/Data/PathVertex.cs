using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Paths.Data
{
    [Entity("Vertex", TypeBrowsable = false)]
    public class PathVertex : Entity
    {
        //in or out edges connected to this vertex



        public PathVertex(Vector3D position)
        {
            Position = position;
        }



        public PathVertex(PathVertex pathVertex)
            : this(pathVertex.Position)
        {
            pathVertex.Attributes.SetAttributesTo(Attributes);
        }



        /// <summary>
        /// Number of edges connected to this vertex.
        /// </summary>
        /// <value> The degree.</value>
        public int Degree => Edges.Count;


        public List<PathEdge> Edges
        {
            get;
            set;
        } = new List<PathEdge>();


        /// <summary>
        /// Number of edges that arrive at this vertex (i.e. their Target is this vertex)
        /// </summary>
        /// <value>
        /// Count of ingoing edges.
        /// </value>
        public int InDegree => IngoingEdges.Count();



        /// <summary>
        /// Edges that arrive at this vertex (i.e. their Target is this vertex).
        /// </summary>
        /// <value> The ingoing edges. </value>
        public IEnumerable<PathEdge> IngoingEdges
        {
            get { return Edges.Where(x => x.Target == this); }
        }



        /// <summary>
        /// Number of edges that depart from this vertex (i.e. their Source is this vertex)
        /// </summary>
        /// <value>Count of outgoing edges.
        /// </value>
        public int OutDegree => OutgoingEdges.Count();



        /// <summary>
        /// Edges that depart from this vertex (i.e. their Source is this vertex)
        /// </summary>
        /// <value>Outgoing edges.
        /// </value>
        public IEnumerable<PathEdge> OutgoingEdges
        {
            get { return Edges.Where(x => x.Source == this); }
        }



        /// <summary>
        /// 3D coordinate of the vertex
        /// </summary>
        public Vector3D Position
        {
            get;
            set;
        }



        /// <summary>
        /// Removes references to edges that are not in the indicated hashset of edges.
        /// </summary>
        /// <param name="edges">The edges.</param>
        public void CleanConnections(HashSet<PathEdge> edges)
        {
            Edges.RemoveAll(x => !edges.Contains(x));
        }



        private double GetAngle(PathEdge edge, Vector3D firstDirection, Vector3D normal)
        {
            var direction = edge.GetDirectionFrom(this).ProjectToPlane(normal);

            if (firstDirection.Cross(direction).Dot(normal) > 0)
                return Math.PI + direction.AngleTo(-firstDirection);

            return direction.AngleTo(firstDirection);
        }



        /// <summary>
        /// Gets the edges, ordered clockwise by angle, relative to the given firstEdge, around the given normal.
        /// </summary>
        /// <param name="firstEdge">The first edge. Must be connected to this vertex! </param>
        /// <param name="normal">The normal around which to rotate.</param>
        /// <returns></returns>
        public IEnumerable<PathEdge> GetOrderedEdges(PathEdge firstEdge, Vector3D normal)
        {
            var firstDirection = firstEdge.GetDirectionFrom(this).ProjectToPlane(normal);

            return Edges.OrderBy(edge => GetAngle(edge, firstDirection, normal));
        }



        public override string ToString()
        {
            return string.Format("Position: {0}", Position);
        }
    }
}