using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Meshes.Data
{
    public enum VertexBoundaryType
    {
        NonBoundary,
        SemiBoundary,
        FullBoundary
    }

    /// <summary>
    /// 
    /// </summary>
    [Entity("Vertex", TypeBrowsable = false)]
    public class Vertex : Entity
    {
        private static readonly FieldKey NormalKey = new FieldKey("Normal");
        private static readonly FieldKey ColorKey = new FieldKey("Color");
        private static readonly FieldKey UV0Key = new FieldKey("UV0");

        //private List<Edge> _edges;

        /// <summary>
        /// 3D Location of the vertex
        /// </summary>
        private Vector3D _position;



        public Vertex()
        {
            _position = new Vector3D();
            HalfVertices = new List<HalfVertex>();
        }



        public Vertex(Vector3D position)
        {
            _position = position;
            //_position = position.Round();
            HalfVertices = new List<HalfVertex>();
        }



        public Vertex(Vertex vertex)
            : this(vertex.Position)
        {
        }



        /// <summary>
        /// Gets all half vertices that start or end from this vertex.
        /// </summary>
        public IEnumerable<HalfVertex> AllHalfVertices
        {
            get { return HalfVertices.Union(HalfVertices.Select(x => x.Previous[x.Face])); }
        }



        public VertexBoundaryType BoundaryType
        {
            get
            {
                bool? hasBoundary = null;

                foreach (Edge emanatingEdge in AllEdges)
                {
                    var isBoundary = emanatingEdge.IsBoundary;

                    if (hasBoundary.HasValue && hasBoundary.Value != isBoundary)
                        return VertexBoundaryType.SemiBoundary;

                    hasBoundary = isBoundary;
                }

                if (hasBoundary.HasValue && hasBoundary.Value)
                    return VertexBoundaryType.FullBoundary;


                return VertexBoundaryType.NonBoundary;
            }
        }



        public Color? Color
        {
            get
            {
                var color = (Color?) Attributes.TryGet(ColorKey);
                return color;
            }
            set { Attributes.TrySet(ColorKey, value, true); }
        }



        /// <summary>
        /// Gets half vertices that start from this vertex.
        /// </summary>
        public List<HalfVertex> HalfVertices
        {
            get;
        }
        //set { _halfVertices = value; }



        public HalfVertex this[Face face]
        {
            //According to dotnetpearls, using list for this kind of search is faster than using dictionary...
            get { return HalfVertices.Find(x => x.Face == face); }
        }



        public Vector3D? Normal
        {
            get
            {
                var normal = (Vector3D?) Attributes.TryGet(NormalKey);
                return normal;
            }
            set { Attributes.TrySet(NormalKey, value, true); }
        }



        /// <summary>
        /// 3D Location of the vertex
        /// </summary>
        /// [NaturalAttribute]
        public Vector3D Position
        {
            get { return _position; }
            set { _position = value; }
        }



        public Vector2D? UV0
        {
            get { return Attributes.TryGet(UV0Key) as Vector2D?; }
            set { Attributes.TrySet(UV0Key, value, true); }
        }



        public VertexUVCollection UVs => new VertexUVCollection(this);



        public void CleanFaceConnections(HashSet<Face> faces)
        {
            HalfVertices.RemoveAll(x => !faces.Contains(x.Face));
        }



        internal HalfVertex CreateHalfVertex(Face face, int index)
        {
            HalfVertex halfVertex = new HalfVertex(face, index);
            HalfVertices.Add(halfVertex);

            return halfVertex;
        }



        /// <summary>
        /// This is a hacked function, not working properly for normal faces. Created for the sake of the triangulation of self-intersecting faces.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="position"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public Vertex CreateInterpolatedVertex(Face face, Vector3D position, Vertex v1)
        {
            float distance = (v1.Position - Position).Length;

            float relativeDistance0 = (Position - position).Length / distance;
            float relativeDistance1 = (v1.Position - position).Length / distance;

            HalfVertex halfVertex0 = this[face];
            HalfVertex halfVertex1 = v1[face];

            HalfVertex halfVertex = new HalfVertex(face, Math.Min(halfVertex0.FaceIndex, halfVertex1.FaceIndex) + 1);

            halfVertex.Normal = halfVertex0.Normal * relativeDistance0 + halfVertex1.Normal * relativeDistance1;
            halfVertex.UV0 = halfVertex0.UV0 * relativeDistance0 + halfVertex1.UV0 * relativeDistance1;
            halfVertex.Color = halfVertex0.Color * relativeDistance0 + halfVertex1.Color * relativeDistance1;

            Vertex newVertex = new Vertex(position);
            newVertex.HalfVertices.Add(halfVertex);

            return newVertex;
        }



        public override IEntity DeepClone()
        {
            return new Vertex(this);
        }



        public void DeleteFromFaces()
        {
            foreach (HalfVertex halfVertex in HalfVertices) halfVertex.Face.RemoveVertex(this);
        }



        public void MergeInto(Vertex existingVertex)
        {
            //move the half-vertices of this vertex to the one of the existing vertex
            existingVertex.HalfVertices.AddRange(HalfVertices);

            //existingVertex.GeometryProperties.AddRange(GeometryProperties);

            //tell the face of  of the face that we are cha
            foreach (HalfVertex halfvertice in HalfVertices) halfvertice.Face.SetIndex(halfvertice.FaceIndex, existingVertex);
        }



        internal HalfVertex RemoveHalfVertices(Face face)
        {
            HalfVertex halfVertex = this[face];
            HalfVertices.RemoveAll(x => x.Face == face);
            return halfVertex;
        }



        #region Object Overriden Functions

        public override string ToString()
        {
            return "Vertex - Position:" + _position;
        }

        #endregion


        /*public override object this[string attributeName]
            {
                get
                {
                    if (String.IsNullOrWhiteSpace(attributeName))
                    {

                    }
                    if (attributeName == "Position")
                        return new SceeList(new KeyValuePair<string, object>("X", _position.X), new KeyValuePair<string, object>("Y", _position.Y), new KeyValuePair<string, object>("Z", _position.Z));

                    return base[attributeName];
                }
                set
                {
                    if (attributeName == "Position")
                    {
                        var sceeList = value.CastTo<SceeList>();
                        _position = new Vector3D(sceeList[0].CastTo<float>(), sceeList[1].CastTo<float>(), sceeList[2].CastTo<float>());
                    }
                    else
                    {
                        base[attributeName] = value;
                    }
                }
            }*/
        /*public Edge GetEdgeTo(Vertex vertex)
        {
            throw new NotImplementedException();
        }*/


        public class VertexUVCollection
        {
            public VertexUVCollection(Vertex vertex)
            {
                Vertex = vertex;
            }



            public Vector2D? this[int index]
            {
                get
                {
                    var geometryTextureCoordinate = Vertex.Attributes.TryGet(new FieldKey("UV" + index)) as Vector2D?;
                    if (geometryTextureCoordinate.HasValue)
                        return geometryTextureCoordinate.Value;

                    return Vector2D.Zero;
                }
                set { Vertex.Attributes.TrySet(new FieldKey("UV" + index), value, true); }
            }



            private Vertex Vertex
            {
                get;
            }
        }

        #region Enumerators

        /// <summary>
        /// An inumerable allowing to iterate over the vertices of the face
        /// </summary>
        //[JsonIgnore]
        public IEnumerable<Face> AdjacentFaces
        {
            get
            {
                //starts with the edge stored on the vertex
                foreach (HalfVertex halfvertice in HalfVertices) yield return halfvertice.Face;
            }
        }



        /// <summary>
        /// An inumerable allowing to iterate over the vertices of the face
        /// </summary>
        //[JsonIgnore]
        public IEnumerable<Vertex> AdjacentVertices
        {
            get
            {
                HashSet<Vertex> returnedVertices = new HashSet<Vertex>();

                foreach (HalfVertex halfvertex in HalfVertices)
                {
                    if (!returnedVertices.Contains(halfvertex.Next))
                    {
                        returnedVertices.Add(halfvertex.Next);

                        yield return halfvertex.Next;
                    }

                    if (!returnedVertices.Contains(halfvertex.Previous))
                    {
                        returnedVertices.Add(halfvertex.Previous);

                        yield return halfvertex.Previous;
                    }
                }
            }
        }



        /// <summary>
        /// Enumerates all the edges that start with this vertex.
        /// </summary>
        public IEnumerable<Edge> OutgoingEdges
        {
            get
            {
                foreach (HalfVertex halfVertex in HalfVertices) yield return new Edge(this, halfVertex.Next);
            }
        }



        /// <summary>
        /// Enumerates all the edges that end in this vertex.
        /// </summary>
        public IEnumerable<Edge> IncomingEdges
        {
            get
            {
                foreach (HalfVertex halfVertex in HalfVertices) yield return new Edge(halfVertex.Previous, this);
            }
        }



        /// <summary>
        /// Enumerates all the edges that either start or end in this vertex
        /// </summary>
        public IEnumerable<Edge> AllEdges
        {
            get
            {
                foreach (Edge edge in IncomingEdges)
                    yield return edge;

                foreach (Edge edge in OutgoingEdges)
                    yield return edge;
            }
        }

        #endregion
    }
}