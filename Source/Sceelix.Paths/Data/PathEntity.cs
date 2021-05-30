using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;

namespace Sceelix.Paths.Data
{
    [Entity("Path")]
    public class PathEntity : Entity, IEnumerable<PathEdge>, IActor
    {
        private BoxScope _boxScope;
        private List<PathEdge> _pathEdges = new List<PathEdge>();



        /// <summary>
        /// Creates a path from a given set of edges.
        /// </summary>
        /// <param name="vertices">Edges from which the path should be created.</param>
        public PathEntity(params PathEdge[] edges)
            : this((IEnumerable<PathEdge>) edges)
        {
            if (edges.Length == 0)
                throw new ArgumentException("A path must have at least one vertex");
        }



        /// <summary>
        /// Creates a path from a given set of edges.
        /// </summary>
        /// <param name="vertices">Edges from which the path should be created.</param>
        public PathEntity(IEnumerable<PathEdge> edges)
        {
            var edgeList = edges.ToList();

            foreach (PathEdge pathEdge in edgeList)
                _pathEdges.Add(pathEdge);

            _boxScope = new BoxScope(edgeList.SelectMany(x => x.Vertices).Select(x => x.Position));
        }



        /// <summary>
        /// Calculates and returns a boundingbox with this entity's extent.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox boundingBox = new BoundingBox();

                foreach (PathVertex vertex in Vertices.Where(x => !x.Position.IsNaN))
                    boundingBox.AddPoint(vertex.Position);

                return boundingBox;
            }
        }



        /// <summary>
        /// Box Scope that defines the size, orientation and 
        /// </summary>
        public BoxScope BoxScope
        {
            get { return _boxScope; }
            set
            {
                _boxScope = value;
                AdjustScope();
            }
        }



        /// <summary>
        /// Edges of this path.
        /// </summary>
        [SubEntity("Edges")]
        public IEnumerable<PathEdge> Edges => _pathEdges;



        public override IEnumerable<IEntity> SubEntityTree
        {
            get
            {
                foreach (var pathEdge in Edges)
                    yield return pathEdge;

                foreach (var pathVertex in Vertices)
                    yield return pathVertex;
            }
        }



        /// <summary>
        /// Enumeration of vertices in this path (obtained from traversing the edges).
        /// </summary>
        [SubEntity("Vertices")]
        public IEnumerable<PathVertex> Vertices
        {
            get { return _pathEdges.SelectMany(val => val.Vertices).Distinct(); }
        }



        /// <summary>
        /// Adds an edge to the path and updates the scope.
        /// </summary>
        /// <param name="edge">Edge to be added.</param>
        public void AddEdge(PathEdge edge)
        {
            _pathEdges.Add(edge);
            _boxScope = _boxScope.Grow(edge.Vertices.Select(x => x.Position));
        }



        /// <summary>
        /// Adds a set of edges to the path and updates the scope.
        /// </summary>
        /// <param name="edges">Edges to be added.</param>
        public void AddEdges(IEnumerable<PathEdge> edges)
        {
            var edgeList = edges.ToList();

            foreach (PathEdge pathEdge in edgeList)
            {
                _pathEdges.Add(pathEdge);
                if (!pathEdge.IsAttached)
                    pathEdge.AttachToVertices();
            }

            _boxScope = _boxScope.Grow(edgeList.SelectMany(x => x.Vertices).Select(x => x.Position));
        }



        /// <summary>
        /// Adds a set of edges to the path and updates the scope.
        /// </summary>
        /// <param name="edges">Edges to be added.</param>
        public void AddEdges(params PathEdge[] edges)
        {
            AddEdges((IEnumerable<PathEdge>) edges);
        }



        /// <summary>
        /// Maintains the orientation and recalculates the translation and sizes so that it encompasses the Entity.
        /// </summary>
        public void AdjustScope()
        {
            _boxScope = _boxScope.Adjust(Vertices.Select(x => x.Position));
        }



        /// <summary>
        /// Maintains the orientation of the parent and recalculates the translation and sizes so that it encompasses the current Entity.
        /// </summary>
        public void AdjustScope(BoxScope parentScope)
        {
            _boxScope = parentScope.Adjust(Vertices.Select(x => x.Position));
        }



        /// <summary>
        /// Removes possible links/references to edges that may not be part of this path.
        /// </summary>
        public void CleanConnections()
        {
            HashSet<PathEdge> edges = new HashSet<PathEdge>(_pathEdges);
            foreach (PathVertex pathVertex in Vertices.ToList())
                pathVertex.CleanConnections(edges);
        }



        /// <summary>
        /// Creates a path from a given set of positions, which are turned into vertices. Builds edges connecting them in the sequence that was given.
        /// </summary>
        /// <param name="vertices">Vertices from which the path should be created.</param>
        public static PathEntity CreateSequence(params Vector3D[] positions)
        {
            return CreateSequence((IEnumerable<Vector3D>) positions);
        }



        /// <summary>
        /// Creates a path from a given set of positions, which are turned into vertices. Builds edges connecting them in the sequence that was given.
        /// </summary>
        /// <param name="vertices">Vertices from which the path should be created.</param>
        public static PathEntity CreateSequence(IEnumerable<Vector3D> positions)
        {
            return CreateSequence(positions.Select(x => new PathVertex(x)));
        }



        /// <summary>
        /// Creates a path from a given set of vertices. Builds edges connecting them in the sequence that was given.
        /// </summary>
        /// <param name="vertices">Vertices from which the path should be created.</param>
        public static PathEntity CreateSequence(params PathVertex[] positions)
        {
            return CreateSequence((IEnumerable<PathVertex>) positions);
        }



        /// <summary>
        /// Creates a path from a given set of vertices. Builds edges connecting them in the sequence that was given.
        /// </summary>
        /// <param name="vertices">Vertices from which the path should be created.</param>
        public static PathEntity CreateSequence(IEnumerable<PathVertex> vertices)
        {
            var vertexList = vertices.ToList();
            if (vertexList.Count < 2)
                throw new ArgumentException("Paths must have at least 2 vertices.");

            List<PathEdge> pathEdges = new List<PathEdge>();
            for (int i = 1; i < vertexList.Count; i++)
            {
                PathEdge pathEdge = new PathEdge(vertexList[i - 1], vertexList[i]);
                pathEdges.Add(pathEdge);
            }

            var pathEntity = new PathEntity(pathEdges);
            //pathEntity.RemoveEdge(pathEntity.Edges.First());

            return pathEntity;
        }



        public override IEntity DeepClone()
        {
            PathEntity deepClone = (PathEntity) base.DeepClone();

            Dictionary<PathVertex, PathVertex> oldToNewVertexMapping = Vertices.ToDictionary(pathVertex => pathVertex,
                pathVertex =>
                {
                    //create a new vertex with the same attributes
                    var newVertex = new PathVertex(pathVertex);
                    pathVertex.Attributes.SetAttributesTo(newVertex.Attributes);
                    return newVertex;
                });


            List<PathEdge> newPathEdges = new List<PathEdge>();
            foreach (PathEdge oldPathEdge in Edges)
            {
                //create new paths, using the map to get the vertices corresponding to the new ones.
                PathEdge newPathEdge = new PathEdge(oldToNewVertexMapping[oldPathEdge.Source], oldToNewVertexMapping[oldPathEdge.Target]);
                oldPathEdge.Attributes.SetAttributesTo(newPathEdge.Attributes);

                newPathEdges.Add(newPathEdge);
            }

            deepClone.BoxScope = deepClone.BoxScope;
            deepClone._pathEdges = newPathEdges;

            return deepClone;
        }



        /// <summary>
        /// Gets the enumerator of path edges.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<PathEdge> GetEnumerator()
        {
            return _pathEdges.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        /// <summary>
        /// Transforms this entity to that it fits the given boxscope.
        /// </summary>
        /// <param name="target">Scope where to move the entity to.</param>
        public void InsertInto(BoxScope target)
        {
            foreach (var vertex in Vertices)
            {
                var scopePosition = _boxScope.ToRelativeScopePosition(vertex.Position);
                vertex.Position = target.ToRelativeWorldPosition(scopePosition);
            }

            _boxScope = target;
        }



        /// <summary>
        /// Creates a new vertex where an edge was standing. Removes that edge
        /// and connects its source and target to the new vertex with two new edges. Uses the IntroduceVertex function from the pathedge,
        /// but already adds and respective removes the edges internally.
        /// </summary>
        /// <param name="edge">Edges to be replaced.</param>
        /// <param name="newMidVertex">Vertex in the middle to connect to.</param>
        /// <returns>Two new edges: one connecting source to the new vertex, and another connecting the new vertex to the new vertex.</returns>
        public PathEdge[] IntroduceVertex(PathEdge edge, PathVertex newMidVertex)
        {
            /*PathEdge pathEdge0 = new PathEdge(edge.Source, newMidVertex);
            PathEdge pathEdge1 = new PathEdge(newMidVertex, edge.Target);

            edge.Attributes.SetAttributesTo(pathEdge0.Attributes);
            edge.Attributes.SetAttributesTo(pathEdge1.Attributes);

            //this edge ceases to exist and can be removed
            edge.DetachFromVertices();
            _pathEdges.Remove(edge);

            AddEdges(pathEdge0, pathEdge1);

            return new[] {pathEdge0, pathEdge1};*/
            var newEdges = edge.IntroduceVertex(newMidVertex);

            //remove this vertex and add the new ones
            _pathEdges.Remove(edge);

            AddEdges(newEdges);

            return newEdges;
        }



        /// <summary>
        /// Removes the given edge from the path (and detaches it from its vertices).
        /// </summary>
        /// <param name="vertices">Edge to be removed.</param>
        /// <param name="adjustScope">Indicates if the boxscope should be readjusted after the removal (see AdjustScope function).</param>
        public void RemoveEdge(PathEdge edge, bool adjustScope = false)
        {
            edge.DetachFromVertices();

            _pathEdges.Remove(edge);

            //adjust only in the end, if desired
            if (adjustScope)
                AdjustScope();
        }



        /// <summary>
        /// Removes the given edges from the path.
        /// </summary>
        /// <param name="vertices">Edges to be removed.</param>
        /// <param name="adjustScope">Indicates if the boxscope should be readjusted after the removal (see AdjustScope function).</param>
        public void RemoveEdges(IEnumerable<PathEdge> edges, bool adjustScope = false)
        {
            foreach (var edge in edges)
                RemoveEdge(edge);

            //adjust only in the end, if desired
            if (adjustScope)
                AdjustScope();
        }



        /// <summary>
        /// Removes the give vertex from the path.
        /// </summary>
        /// <param name="vertices">Vertex to be removed.</param>
        /// <param name="reconnect">Indicates if the previously connected vertices should be connected among them.</param>
        /// <param name="adjustScope">Indicates if the boxscope should be readjusted after the removal (see AdjustScope function).</param>
        public List<PathEdge> RemoveVertex(PathVertex vertex, bool reconnect = false, bool adjustScope = false)
        {
            List<PathEdge> newPathEdges = new List<PathEdge>();

            //get all the edges (from this pathentity) that this vertex is connected to
            var edgesToRemove = vertex.Edges.Where(x => _pathEdges.Contains(x)).ToList();

            //remove the edges (but do NOT perform the scope adjustment yet)
            RemoveEdges(edgesToRemove);

            if (reconnect)
            {
                var sourceVertices = edgesToRemove.Where(e => e.Target == vertex).Select(x => x.OtherVertex(vertex)).ToList();
                var targetVertices = edgesToRemove.Where(e => e.Source == vertex).Select(x => x.OtherVertex(vertex)).ToList();


                //var vertices = edgesToRemove.Select(x => x.OtherVertex(vertex)).ToList();
                for (int i = 0; i < sourceVertices.Count; i++)
                for (int j = 0; j < targetVertices.Count; j++)
                {
                    PathEdge edge = new PathEdge(sourceVertices[i], targetVertices[j]);

                    _pathEdges.Add(edge);
                    newPathEdges.Add(edge);
                }
            }

            //adjust only in the end, if desired
            if (adjustScope)
                AdjustScope();

            return newPathEdges;
        }



        /// <summary>
        /// Removes the given vertices from the path.
        /// </summary>
        /// <param name="vertices">Vertices to be removed.</param>
        /// <param name="reconnect">Indicates if the previously connected vertices should be connected among them.</param>
        /// <param name="adjustScope">Indicates if the boxscope should be readjusted after the removal (see AdjustScope function).</param>
        public List<PathEdge> RemoveVertices(List<PathVertex> vertices, bool reconnect = false, bool adjustScope = false)
        {
            List<PathEdge> newPathEdges = new List<PathEdge>();

            //remove the edges (but do NOT perform the scope adjustment yet)
            foreach (PathVertex pathVertex in vertices)
                newPathEdges.AddRange(RemoveVertex(pathVertex, reconnect));

            //adjust only in the end, if desired
            if (adjustScope)
                AdjustScope();

            return newPathEdges;
        }
    }
}