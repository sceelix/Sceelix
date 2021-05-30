using System;
using System.Collections.Generic;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;

namespace Sceelix.Paths.Data
{
    /// <summary>
    /// Defines a straight connection between 2 street vertices
    /// </summary>
    [Entity("Edge", TypeBrowsable = false)]
    public class PathEdge : Entity
    {
        //First and second vertex of the edge. Direction is v0 -> v1



        public PathEdge(PathVertex source, PathVertex target)
        {
            Source = source;
            Target = target;

            AttachToVertices();
        }



        /// <summary>
        /// Vector indicating the direction of the pathedge, already normalized.
        /// </summary>
        public Vector3D Direction => (Target.Position - Source.Position).Normalize();


        public bool IsAttached =>
            //checking just one of the vertices is enough
            Source.Edges.Contains(this);


        public bool IsValid => Source.Position != Target.Position;


        public float Length => (Source.Position - Target.Position).Length;


        public Line3D Line3D => new Line3D(Target.Position - Source.Position, Source.Position);


        public LineSegment2D LineSegment2D => new LineSegment2D(Source.Position.ToVector2D(), Target.Position.ToVector2D());


        public LineSegment3D LineSegment3D => new LineSegment3D(Source.Position, Target.Position);


        /// <summary>
        /// Just a standard boolean flag that may help in several algorithms.
        /// </summary>
        [Obsolete("To be removed soon. Please switch to the use of attributes.")]
        public bool Marked
        {
            get;
            set;
        }


        public PathVertex Source
        {
            get;
            set;
        }


        public PathVertex Target
        {
            get;
            set;
        }



        /// <summary>
        /// An enumeration of the edge's two vertices: source and target.
        /// </summary>
        public IEnumerable<PathVertex> Vertices
        {
            get
            {
                yield return Source;
                yield return Target;
            }
        }



        public void AttachToVertices()
        {
            Source.Edges.Add(this);
            Target.Edges.Add(this);
        }



        /// <summary>
        /// Indicates if the given edge is coincident with this edge, i.e. if they somehow connect the same two vertices.
        /// </summary>
        /// <param name="edge">Edge to compare to.</param>
        /// <returns>True if the two edges connect the same two vertices, otherwise false.</returns>
        public bool CoincidentWith(PathEdge edge)
        {
            return Source.Position.Equals(edge.Source.Position) && Target.Position.Equals(edge.Target.Position)
                   || Source.Position.Equals(edge.Target.Position) && Target.Position.Equals(edge.Source.Position);
        }



        /// <summary>
        /// Indicates if this edge somehow connects the given two vertices (i.e. any of them being the source and the other the target).
        /// </summary>
        /// <param name="vertex1">The first vertex.</param>
        /// <param name="vertex2">The second vertex.</param>
        /// <returns>True if the two vertices are connected by this edge, otherwise false.</returns>
        public bool Connects(PathVertex vertex1, PathVertex vertex2)
        {
            return vertex1 == Source && vertex2 == Target || vertex1 == Target && vertex2 == Source;
        }



        /// <summary>
        /// Determines whether this edge contains the given vertex, either as a source or as a target.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns>
        ///   <c>true</c> if the edge contains the given vertex</c>.
        /// </returns>
        public bool Contains(PathVertex vertex)
        {
            return Source == vertex || Target == vertex;
        }



        public void DetachFromVertices()
        {
            Source.Edges.Remove(this);
            Target.Edges.Remove(this);
        }



        /// <summary>
        /// Determines the vector direction when starting from the given vertex. 
        /// </summary>
        /// <param name="pathVertex">Vertex where to start from (it assumes that the given vertex is one the edge's vertices).</param>
        /// <returns>Direction from the given vertex.</returns>
        public Vector3D GetDirectionFrom(PathVertex pathVertex)
        {
            return OtherVertex(pathVertex).Position - pathVertex.Position;
        }



        /// <summary>
        /// Indicates if the given position lies on this edge.
        /// </summary>
        /// <param name="position">Position in 3D world to verify.</param>
        /// <returns>True if the position lies on this edge, otherwise false.</returns>
        public bool HasPointInBetween(Vector3D position)
        {
            return Math.Abs((position - Source.Position).Normalize().Dot((position - Target.Position).Normalize()) - -1) < Vector3D.Precision;
        }



        /// <summary>
        /// Introduces the new vertex in the edge, i.e. connects the source and target of this edge to this new vertex
        /// and returns two new edges, with the same attributes. The edge detaches itself from the vertices it was attached to.
        /// Important: If this is used for edges existing within an already created PathEntity, this edge should be removed and the
        /// newly created ones added.
        /// </summary>
        /// <param name="newVertex">Vertex in the middle to connect to.</param>
        /// <returns>Two new edges: one connecting source to the new vertex, and another connecting the new vertex to the new vertex.</returns>
        public PathEdge[] IntroduceVertex(PathVertex newVertex)
        {
            PathEdge pathEdge0 = new PathEdge(Source, newVertex);
            PathEdge pathEdge1 = new PathEdge(newVertex, Target);

            Attributes.SetAttributesTo(pathEdge0.Attributes);
            Attributes.SetAttributesTo(pathEdge1.Attributes);

            //this edge ceases to exist and can be removed
            DetachFromVertices();

            return new[] {pathEdge0, pathEdge1};
        }



        public PathVertex OtherVertex(PathVertex pathVertex)
        {
            if (pathVertex == Source)
                return Target;
            if (pathVertex == Target)
                return Source;

            return null;
        }



        /// <summary>
        /// Replaced the given vertex with a second one.
        /// </summary>
        /// <param name="currentVertex">Vertex to be replaced. It should be either the source or target vertex of this edge. If not, nothing will happen.</param>
        /// <param name="replacementVertex">Vertex to replace the vertex with.</param>
        public void ReplaceVertex(PathVertex currentVertex, PathVertex replacementVertex)
        {
            DetachFromVertices();

            if (currentVertex == Source)
                Source = replacementVertex;

            if (currentVertex == Target)
                Target = replacementVertex;

            AttachToVertices();
        }
    }
}