using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;
using Sceelix.Mathematics.Spatial;
using Sceelix.Meshes.Exceptions;
using Sceelix.Meshes.Materials;
using Sceelix.Meshes.Operations;

namespace Sceelix.Meshes.Data
{
    /// <summary>
    /// Class Face.
    /// </summary>
    [Entity("Face", TypeBrowsable = false)]
    public class Face : Entity
    {
        /// <summary>
        /// The material key
        /// </summary>
        private static readonly FieldKey MaterialKey = new FieldKey("Material");

        //private Material _material;

        //private List<IGeometryProperty> _geometryProperties = new List<IGeometryProperty>(1);

        //private static readonly FieldKey IsConvexKey = new FieldKey("IsConvex");

        /*private HalfVertex _firstHalfVertex;
        private List<HalfVertex> _holeFirstHalfVertices;*/


        /*public IEnumerable<HalfVertex> TheHalfVertices
        {
            get
            {
                yield return _firstHalfVertex;

                var currentHalfVertex = _firstHalfVertex.NextHalfVertex;

                while (currentHalfVertex != _firstHalfVertex)
                {
                    yield return currentHalfVertex;
                    currentHalfVertex = currentHalfVertex.NextHalfVertex;
                }
            }
        }

        

        public IEnumerable<Vertex> TheVertices
        {
            get
            {
                yield return _firstHalfVertex.Vertex;

                var currentVertex = _firstHalfVertex.NextHalfVertex;

                while (currentVertex != _firstHalfVertex)
                {
                    yield return currentVertex.Vertex;
                    currentVertex = currentVertex.NextHalfVertex;
                }
            }
        }*/


        private CircularList<Vertex> _vertices;



        public void AddHole(IEnumerable<Vertex> vertexList)
        {
            if (Holes == null)
                Holes = new List<CircularList<Vertex>>();

            var hole = new CircularList<Vertex>(vertexList);
            Holes.Add(hole);
            for (int i = 0; i < hole.Count; i++)
                hole[i].CreateHalfVertex(this, i);
        }



        /// <summary>
        /// Joey: Converts the  vectorlist to vertex list and than adds it using regular function
        /// </summary>
        /// <param name="vectorList"></param>
        public void AddHole(IEnumerable<Vector3D> vectorList)
        {
            CircularList<Vertex> list = new CircularList<Vertex>();
            foreach (Vector3D vector in vectorList) list.Add(new Vertex(vector));
            AddHole(list);
        }



        public bool AllAtToPlaneLocation(Plane3D cuttingPlane, PointToPlaneLocation location)
        {
            return _vertices.Select(val => val.Position).All(val => cuttingPlane.LocationToPlane(val) == location);
        }



        public void CalculateTangentAndBinormal()
        {
            /// http://stackoverflow.com/questions/5255806/how-to-calculate-tangent-and-binormal
            var circularHalfVertexList = new CircularList<HalfVertex>(HalfVertices);

            for (int i = 0; i < circularHalfVertexList.Count; i++)
            {
                var previousHalfVertex = circularHalfVertexList[i - 1];
                var currentHalfVertex = circularHalfVertexList[i];
                var nextHalfVertex = circularHalfVertexList[i + 1];

                var p_dx = previousHalfVertex.Vertex.Position - currentHalfVertex.Vertex.Position;
                var p_dy = nextHalfVertex.Vertex.Position - currentHalfVertex.Vertex.Position;

                var tc_dx = previousHalfVertex.UV0 - currentHalfVertex.UV0;
                var tc_dy = nextHalfVertex.UV0 - currentHalfVertex.UV0;

                var t = (p_dx * tc_dy.Y - p_dy * tc_dx.Y).Normalize();
                var b = (p_dx * tc_dy.X - p_dy * tc_dx.X).Normalize();

                var n = currentHalfVertex.Normal;
                var x = n.Cross(t);
                t = x.Cross(n).Normalize();

                // get updated bi-tangent
                x = b.Cross(n);
                b = n.Cross(x);

                currentHalfVertex.Tangent = t;
                currentHalfVertex.Binormal = b;
            }

            // compute derivations of the world position
            /*vec3 p_dx = dFdx(pw_i);
            vec3 p_dy = dFdy(pw_i);
            // compute derivations of the texture coordinate
            vec2 tc_dx = dFdx(tc_i);
            vec2 tc_dy = dFdy(tc_i);
            // compute initial tangent and bi-tangent
            vec3 t = normalize(tc_dy.y * p_dx - tc_dx.y * p_dy);
            vec3 b = normalize(tc_dy.x * p_dx - tc_dx.x * p_dy); // sign inversion
                                                                 // get new tangent from a given mesh normal
            vec3 n = normalize(n_obj_i);
            vec3 x = cross(n, t);
            t = cross(x, n);
            t = normalize(t);
            // get updated bi-tangent
            x = cross(b, n);
            b = cross(n, x);
            b = normalize(b);
            mat3 tbn = mat3(t, b, n);*/
        }



        public static bool CanCreateValidFace(IEnumerable<Vector3D> vertexPositions)
        {
            //int vertexPositionsLength = vertexPositions.First() == vertexPositions.Last() ? count - 1 : vertexPositions.Count();
            //if the first equals the last position, do not repeat it
            IEnumerable<Vector3D> list = vertexPositions as IList<Vector3D> ?? vertexPositions.ToList();

            if (!list.Any())
                return false;

            int vertexPositionsLength = list.First() == list.Last()
                ? list.Count() - 1
                : list.Count();

            if (vertexPositionsLength < 3)
                return false;

            return true;
        }



        public static bool CanCreateValidFace(IEnumerable<Vertex> vertices)
        {
            //int vertexPositionsLength = vertexPositions.First() == vertexPositions.Last() ? count - 1 : vertexPositions.Count();
            //if the first equals the last position, do not repeat it
            IEnumerable<Vertex> list = vertices as IList<Vertex> ?? vertices.ToList();

            if (!list.Any())
                return false;

            int length = list.First() == list.Last() ? list.Count() - 1 : list.Count();

            if (length < 3)
                return false;

            return true;
        }



        private bool CheckIfConvex()
        {
            Vector3D? calculatedNormal = null;

            for (int i = 0; i < _vertices.Count; i++)
            {
                Vector3D v1 = _vertices[i].Position;
                Vector3D v2 = _vertices[i + 1].Position;
                Vector3D v3 = _vertices[i + 2].Position;

                Vector3D v = Vector3D.Cross(v1 - v2, v3 - v2).Normalize();

                if (v.IsInfinityOrNaN)
                    continue;

                if (!calculatedNormal.HasValue)
                    calculatedNormal = v;
                else if (calculatedNormal.Value.Dot(v) < 0) //else if (!calculatedNormal.Value.Equals(v))
                    return false;
                /*if (!v.Equals(Normal))
                {
                    return false;
                }*/
            }

            return true;
        }



        /// <summary>
        /// Checks if the face the 3D point within its boundary (in the 3D domain).
        /// Unfinished - works only for convex polygons.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3D point)
        {
            if (!IsConvex)
                return false;

            foreach (Edge edge in Edges)
                if ((point - edge.V0.Position).Cross(edge.Direction).Dot(Normal) < 0)
                    return false;

            return true;
        }



        /// <summary>
        /// Checks if the face boundary contains the given point (on the 2D domain, considering only X and Y)
        /// Algorithm: http://www.codeproject.com/Tips/84226/Is-a-Point-inside-a-Polygon
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2D point)
        {
            bool conclusion = false;

            for (int i = 0, j = _vertices.Count - 1; i < _vertices.Count; j = i++)
                if (_vertices[i].Position.Y > point.Y != _vertices[j].Position.Y > point.Y &&
                    point.X < (_vertices[j].Position.X - _vertices[i].Position.X) * (point.Y - _vertices[i].Position.Y) / (_vertices[j].Position.Y - _vertices[i].Position.Y) + _vertices[i].Position.X)
                    conclusion = !conclusion;
            return conclusion;
        }



        /// <summary>
        /// Performs a deep clone of the object.
        /// Superclass function performs a memberwiseclone.
        /// </summary>
        /// <returns>IEntity.</returns>
        public override IEntity DeepClone()
        {
            Dictionary<Vertex, Vertex> oldToNewVertexMapping = new Dictionary<Vertex, Vertex>();

            List<Vertex> newVertices = new List<Vertex>(Vertices.Count());

            //go 
            foreach (Vertex vertex in Vertices)
                newVertices.Add(GetNewVertex(vertex, oldToNewVertexMapping));


            Face newFace = new Face(newVertices);
            newFace.Material = Material;
            //newFace.Tag = Tag;
            Attributes.SetAttributesTo(newFace.Attributes);


            //copy the holes as well
            if (HasHoles)
                foreach (CircularList<Vertex> circularList in Holes)
                {
                    List<Vertex> newHoleVertices = new List<Vertex>(circularList.Count());

                    foreach (Vertex vertex in circularList)
                        newHoleVertices.Add(GetNewVertex(vertex, oldToNewVertexMapping));

                    newFace.AddHole(newHoleVertices);
                }


            foreach (Vertex oldVertex in Vertices)
            {
                HalfVertex oldHv = oldVertex[this];
                HalfVertex newHv = oldToNewVertexMapping[oldVertex][newFace];

                //newHv.GetEmanatingEdge()
                //new Edge(old)

                //newHv.GeometryProperties = oldHv.GeometryProperties.ToList();
                oldHv.Attributes.SetAttributesTo(newHv.Attributes);
            }


            return newFace;
        }



        /// <summary>
        /// Detaches the face from its vertices.
        /// </summary>
        public void Detach()
        {
            CircularList<Vertex> newVertices = new CircularList<Vertex>();

            foreach (Vertex vertex in Vertices)
            {
                HalfVertex halfVertex = vertex.RemoveHalfVertices(this);

                Vertex newVertex = new Vertex(vertex.Position);
                newVertex.HalfVertices.Add(halfVertex);

                newVertices.Add(newVertex);
            }

            _vertices = newVertices;
        }



        //JOEY: Translate to centroid test
        /*public void TranslateToCentroid(Vector3D targetCentroid)
        {
            Vector3D curCentroid = Centroid;
            foreach (Vertex vertex in _vertices)
            {
                vertex.Position += (targetCentroid - curCentroid);
            }
        }*/


        /*public override IEnumerable<IEntity> SubEntityTree
        {
            get
            {
                foreach (var vertex in AllVertices)
                    yield return vertex;
            }
        }*/

        /*public List<String> Labels
        {
            get { return _labels; }
        }*/


        /*public static bool operator&(Face face, String s)
        {
            foreach (string label in face.Labels)
            {
                if (label.Contains(s))
                    return true;
            }

            return false;
        }*/



        public void Flip()
        {
            _vertices.Reverse();
            Normal *= -1;

            for (int i = 0; i < _vertices.Count; i++)
                _vertices[i][this].FaceIndex = i;
        }



        public BoxScope GetAlignedScope()
        {
            return GetAlignedScope(Edges.First());
        }



        public BoxScope GetAlignedScope(int edgeIndex)
        {
            return GetAlignedScope(new CircularList<Edge>(Edges)[edgeIndex]);
        }



        public BoxScope GetAlignedScope(Edge edge)
        {
            //first of all, calculate the direction of the xAxis, yAxis and zAxis, normalized
            //since the direction is clockwise, we are inverting the direction
            var xAxis = -edge.Direction;
            var yAxis = Normal.Cross(xAxis).Normalize();
            var zAxis = Normal;

            //for now, the anchor point reference is the first position
            var translation = edge.V1.Position;

            var baseScope = new BoxScope(xAxis, yAxis, zAxis, translation, new Vector3D());

            /*Plane3D planeX = new Plane3D(xAxis, translation);
            Plane3D planeY = new Plane3D(yAxis, translation);
            Plane3D planeZ = new Plane3D(zAxis, translation);

            var sizes = new Vector3D();

            //the first two points have already been considered, so just take a look at the rest
            for (int i = 2; i < Vertices.Count(); i++)
            {
                Vector3D point = this[i].Position;

                float x = Plane3D.MovePlane(ref planeX, point, sizes.X);
                float y = Plane3D.MovePlane(ref planeY, point, sizes.Y);
                float z = Plane3D.MovePlane(ref planeZ, point, sizes.Z);

                sizes = new Vector3D(x, y, z);
            }

            xAxis = planeX.Normal;
            yAxis = planeY.Normal;
            zAxis = planeZ.Normal;

            translation = Plane3D.CalculateIntersection(planeX, planeY, planeZ);*/

            return baseScope.Grow(Vertices.Select(x => x.Position).Skip(2)); //new BoxScope(xAxis, yAxis, zAxis, translation, sizes);
        }



        public static BoxScope GetAlignedScope(params Face[] faces)
        {
            var boxscope = faces.First().GetAlignedScope();

            foreach (Face otherFace in faces.Skip(1))
                boxscope = boxscope.Grow(otherFace.Vertices.Select(x => x.Position));

            return boxscope;
        }



        public static BoxScope GetAlignedScope(IEnumerable<Face> faces)
        {
            return GetAlignedScope(faces.ToArray());
        }



        public BoxScope GetDerivedScope(BoxScope parentScope)
        {
            Vector3D xAxis, yAxis, zAxis;

            //first of all, the z is always the normal of the face
            //_zAxis = face.Normal;
            if (Normal.Equals(parentScope.ZAxis))
            {
                xAxis = parentScope.XAxis;
                yAxis = parentScope.YAxis;
                zAxis = parentScope.ZAxis;
            }
            else if (Normal.Equals(-parentScope.ZAxis))
            {
                xAxis = -parentScope.XAxis;
                yAxis = parentScope.YAxis;
                zAxis = -parentScope.ZAxis;
            }
            else
            {
                xAxis = parentScope.ZAxis.Cross(Normal);
                zAxis = Normal;
                yAxis = zAxis.Cross(xAxis);
            }

            var sizes = new Vector3D();
            var translation = this[0].Position;

            var boxScope = new BoxScope(xAxis, yAxis, zAxis, translation, sizes);

            return boxScope.Grow(Vertices.Select(x => x.Position));
            //AddFace(face);
        }



        private Vertex GetNewVertex(Vertex vertex, Dictionary<Vertex, Vertex> oldToNewVertexMapping)
        {
            Vertex newVertex;

            if (!oldToNewVertexMapping.TryGetValue(vertex, out newVertex))
            {
                oldToNewVertexMapping.Add(vertex, newVertex = new Vertex(vertex));
                vertex.Attributes.SetAttributesTo(newVertex.Attributes);
            }

            return newVertex;
        }



        /// <summary>
        /// Not quite accurate, but will do for now.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public bool IsCoincidentWith(Face face)
        {
            if (!face.Normal.IsCollinear(face.Normal))
                return false;

            if (face._vertices.Count() != face._vertices.Count)
                return false;

            for (int i = 0; i < _vertices.Count; i++)
            {
            }

            return false;
        }



        /*public void MakePlanar()
        {
            Plane3D plane3D = new Plane3D(Normal, Centroid);

            foreach (Vertex vertex in Vertices)
            {
                float distanceToPoint = plane3D.DistanceToPoint(vertex.Position);

                vertex.Position -= plane3D.Normal * distanceToPoint;
                vertex.GeometryProperties.Add(new GeometryOffset(plane3D.Normal * distanceToPoint));
            }

            _isPlanar = CheckIfPlanar();
        }

        public void RestoreNonPlanar()
        {
            foreach (Vertex vertex in Vertices)
            {
                IGeometryProperty geometryOffset = vertex.GeometryProperties.FirstOrDefault(val => val is GeometryOffset);
                if (geometryOffset != null)
                {
                    vertex.Position += ((GeometryOffset) geometryOffset).Offset;
                    vertex.GeometryProperties.Remove(geometryOffset);
                }
                else
                {
                    return;
                }
            }

            _isPlanar = CheckIfPlanar();
        }*/



        public bool IsSelfIntersecting()
        {
            var alignedScope = GetAlignedScope();

            if (IsSelfIntersecting(alignedScope, Edges))
                return true;

            if (HasHoles)
                foreach (IEnumerable<Edge> holeEdges in AllHoleEdges)
                    if (IsSelfIntersecting(alignedScope, holeEdges))
                        return true;


            //var positions2D = _vertices.Select(x => alignedScope.ToScopePosition(x.Position).ToVector2D());
            /*var edgeList = AllEdges.ToList();
            for (int i = 0; i < edgeList.Count; i++)
            {
                for (int j = i+1; j < edgeList.Count; j++)
                {
                    var neighbour = (j == i+1) || (j == edgeList.Count - 1);

                    var hasIntersection = edgeList[i].Intersects(edgeList[j], !neighbour);
                    if (hasIntersection)
                        return true;
                }
            }*/

            return false;
        }



        private bool IsSelfIntersecting(BoxScope alignedScope, IEnumerable<Edge> edges)
        {
            var lineSegments = edges.Select(x => new LineSegment2D(alignedScope.ToScopePosition(x.V0.Position).ToVector2D(), alignedScope.ToScopePosition(x.V1.Position).ToVector2D())).ToList();

            for (int i = 0; i < lineSegments.Count; i++)
            for (int j = i + 1; j < lineSegments.Count; j++)
            {
                var neighbour = j == i + 1 || i == 0 && j == lineSegments.Count - 1;

                var hasIntersection = lineSegments[i].Intersects(lineSegments[j], !neighbour);
                if (hasIntersection)
                    return true;
            }

            return false;
        }



        public void RemoveVertex(Vertex vertex)
        {
            _vertices.Remove(vertex);

            ResetHalfStreetVertices();
        }



        public void ResetHalfStreetVertices()
        {
            for (int index = 0; index < _vertices.Count; index++) _vertices[index][this].FaceIndex = index;
        }



        public void SetIndex(int faceIndex, Vertex existingVertex)
        {
            _vertices[faceIndex] = existingVertex;
        }



        #region Constructors

        private Face()
        {
        }



        /*public Face(params HalfVertex[] halfVertices) : this((IEnumerable<HalfVertex>)halfVertices)
        {
            
        }

        public Face(IEnumerable<HalfVertex> halfVertices)
        {

        }*/



        /// <summary>
        /// Creates a face from a set of 3D Vectors, in clockwise order
        /// </summary>
        /// <param name="vertexPositions">Set of 3D vectors</param>
        public Face(params Vector3D[] vertexPositions)
            : this((IEnumerable<Vector3D>) vertexPositions)
        {
        }



        /// <summary>
        /// Creates a face from a set of 3D Vectors, in clockwise order
        /// </summary>
        /// <param name="vertexPositions">Set of 3D vectors</param>
        public Face(IEnumerable<Vector3D> vertexPositions)
        {
            List<Vector3D> vertexPositionsList = vertexPositions.ToList();
            //int vertexPositionsLength = vertexPositions.First() == vertexPositions.Last() ? count - 1 : vertexPositions.Count();de
            //if the first equals the last position, do not repeat it
            int vertexPositionsLength = vertexPositionsList.First().Equals(vertexPositionsList.Last())
                ? vertexPositionsList.Count - 1
                : vertexPositionsList.Count;

            if (vertexPositionsLength < 3)
                throw new InvalidGeometryException("Faces must have at least 3 vertices.");

            //initialize the list with that exact amount of vertices
            _vertices = new CircularList<Vertex>(vertexPositionsLength);

            //now load the positions into vertices
            for (int i = 0; i < vertexPositionsLength; i++) _vertices.Add(new Vertex(vertexPositionsList[i]));

            ProcessFaceCreation();
        }



        /// <summary>
        /// Creates a face from a set of vertices, in clockwise order
        /// </summary>
        /// <param name="vertices">Set of vertices</param>
        public Face(params Vertex[] vertices)
            : this(new CircularList<Vertex>(vertices))
        {
        }



        /// <summary>
        /// Creates a face from an enumerable of vertices, in clockwise order
        /// </summary>
        /// <param name="vertices">Set of vertices</param>
        public Face(IEnumerable<Vertex> vertices)
            : this(new CircularList<Vertex>(vertices))
        {
        }



        /// <summary>
        /// Creates a face from an enumerable of vertices, in clockwise order
        /// </summary>
        /// <param name="vertices">Set of vertices</param>
        private Face(CircularList<Vertex> vertices)
        {
            if (vertices.First() == vertices.Last())
                vertices.Remove(vertices.Last());

            _vertices = vertices;

            if (_vertices.Count < 3)
                throw new InvalidGeometryException("Faces must have at least 3 vertices.");

            ProcessFaceCreation();
        }



        internal Face(Face face, List<Vertex> newVertices)
        {
            _vertices = new CircularList<Vertex>(newVertices);

            IsConvex = face.IsConvex;
            IsPlanar = face.IsPlanar;
            Normal = face.Normal;

            //now, for every vertex, set the connection to this face
            for (int index = 0; index < _vertices.Count; index++)
                _vertices[index].CreateHalfVertex(this, index);

            face.Attributes.SetAttributesTo(Attributes);
        }



        private void ProcessFaceCreation()
        {
            //check if convex or not
            IsConvex = CheckIfConvex();

            //first, calculate the normal only this once
            //_geometryProperties.Add(new GeometryNormal(CalculateNormal()));
            Normal = CalculateNormal();

            //now, for every vertex, set the connection to this face
            for (int index = 0; index < _vertices.Count; index++)
                _vertices[index].CreateHalfVertex(this, index);

            IsPlanar = CheckIfPlanar();
            //in the end, do not forget to calculate the scope
            //Scope = new PlaneScope(this);
        }



        private bool CheckIfPlanar()
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                Vector3D currentPosition = _vertices[i].Position;
                Vector3D pn = _vertices[i - 1].Position - currentPosition;
                Vector3D pn1 = _vertices[i + 1].Position - currentPosition;

                Vector3D v = pn.Cross(pn1).Normalize();
                if (!v.IsCollinear(Normal))
                    return false;
            }

            return true;
        }



        /// <summary>
        /// Updates the value of IsPlanar and returns that newly calculated value.
        /// </summary>
        /// <returns></returns>
        public bool RecalculateIsPlanar()
        {
            return IsPlanar = CheckIfPlanar();
        }



        /// <summary>
        /// Updates the value of IfConvex and returns that newly calculated value.
        /// </summary>
        /// <returns></returns>
        public bool RecalculateIfConvex()
        {
            return IsConvex = CheckIfConvex();
        }



        /// <summary>
        /// Newell's Method for calculating Planar polygon normals
        /// </summary>
        private Vector3D CalculateNormal()
        {
            return Vector3D.CalculateNormal(_vertices.Select(x => x.Position));
        }



        public Vector3D RecalculateNormal()
        {
            return Normal = CalculateNormal();
        }

        #endregion

        #region Properties

        public float MaxPlanarDistance
        {
            get
            {
                float maxDistance = -float.MaxValue;
                Plane3D plane3D = new Plane3D(Normal, Centroid);

                foreach (Vertex vertex in Vertices)
                {
                    float distanceToPoint = plane3D.DistanceToPoint(vertex.Position);
                    if (distanceToPoint > maxDistance)
                        maxDistance = distanceToPoint;
                }

                return maxDistance;
            }
        }



        /*public Vector3D Normal
        {
            get { return ((GeometryNormal)_geometryProperties.First(val => val is GeometryNormal)).Normal; }
            set
            {
                _geometryProperties.RemoveAll(val => val is GeometryNormal);
                _geometryProperties.Add(new GeometryNormal(value));
    }
    }*/


        public Vector3D Normal
        {
            get;
            set;
        }


        /*public Color Color
        {
            get
            {
                IGeometryProperty colorProperty = _geometryProperties.FirstOrDefault(val => val is GeometryColor);
                if (colorProperty != null)
                    return ((GeometryColor)colorProperty).Color;

                return Color.White;
            }
            set
            {
                _geometryProperties.RemoveAll(val => val is GeometryColor);
                _geometryProperties.Add(new GeometryColor(value));
            }
        }*/


        public bool IsPlanar
        {
            get;
            private set;
        }



        public IEnumerable<Vertex> AllVertices
        {
            get
            {
                foreach (Vertex vertex in Vertices)
                    yield return vertex;

                if (HasHoles)
                    foreach (CircularList<Vertex> circularList in Holes)
                    foreach (Vertex vertex in circularList)
                        yield return vertex;
            }
        }



        public float Perimeter
        {
            get
            {
                float perimeter = 0;

                //just add up the length of each edge
                foreach (Edge edge in Edges)
                    perimeter += edge.Length;

                return perimeter;
            }
        }



        public int NumConcave
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _vertices.Count; i++)
                {
                    Vector3D v1 = _vertices[i].Position;
                    Vector3D v2 = _vertices[i + 1].Position;
                    Vector3D v3 = _vertices[i + 2].Position;

                    Vector3D v = Vector3D.Cross(v1 - v2, v3 - v2).Normalize();

                    if (!v.Equals(Normal)) count++;
                }

                return count;
            }
        }



        public Vector3D Centroid
        {
            get
            {
                /*Vector3D centroid = new Vector3D();

                foreach (Vertex vertex in _vertices)
                    centroid += vertex.Position;

                centroid /= _vertices.Count();*/

                return Vector3D.Average(_vertices.Select(x => x.Position));

                //return _scope.Translation + (_scope.XAxis/2 + _scope.YAxis/2);
            }
        }



        public Plane3D Plane => new Plane3D(Normal, _vertices[0].Position);



        [EntityProperty(HandleType = typeof(SceeList))]
        public Material Material
        {
            get
            {
                var material = Attributes.TryGet(MaterialKey) as Material;
                if (material != null)
                    return material;

                return new ColorMaterial(Color.White);
            }
            set { Attributes.TrySet(MaterialKey, value, true); }
        }



        [EntityProperty]
        public bool HasHoles => Holes != null && Holes.Count > 0;



        /// <summary>
        /// Gets the Vertex located at the input index. Does a circular search, so both negative and oversized indices are accepted.
        /// </summary>
        /// <param name="index">Index of the vertex to be located</param>
        /// <returns>The vertex at the indicated location</returns>
        public Vertex this[int index] => _vertices[index];
        //set { _vertices[_vertices.GetNormalizedIndex(index)] = value; }



        public List<CircularList<Vertex>> Holes
        {
            get;
            set;
        }


        [EntityProperty]
        public bool IsConvex
        {
            get;
            private set;
        }
        //return isConvex.Value;

        /// <summary>
        /// An inumerable allowing to iterate over the vertices of the face
        /// </summary>
        public IEnumerable<Vertex> Vertices =>
            //starts with the half-edge stored on the face
            _vertices;



        /// <summary>
        /// An inumerable allowing to iterate over the half vertices of the face
        /// </summary>
        public IEnumerable<HalfVertex> HalfVertices
        {
            get
            {
                //starts with the half-edge stored on the face
                return _vertices.Select(val => val[this]);
            }
        }



        /// <summary>
        /// An inumerable allowing to iterate over the edges of the face
        /// </summary>
        public IEnumerable<Edge> Edges
        {
            get
            {
                //join each two vertices into and edge
                //for (int i = 0; i < _vertices.Count; i++)
                //    yield return _vertices[i].GetEdgeTo(_vertices[i + 1]);

                for (int i = 0; i < _vertices.Count; i++)
                    yield return new Edge(_vertices[i], _vertices[i + 1]);

                //in the end, join the last and the first
                //yield return new Edge(_vertices.Last(), _vertices.First());
            }
        }



        public BoundingBox BoundingBox
        {
            get { return new BoundingBox(Vertices.Select(x => x.Position)); }
        }



        public BoundingRectangle BoundingRectangle
        {
            get
            {
                return new BoundingRectangle(Vertices.Select(x => x.Position.ToVector2D()));
                ;
            }
        }



        public IEnumerable<Edge> AllEdges
        {
            get
            {
                //join each two vertices into and edge
                for (int i = 0; i < _vertices.Count; i++)
                    yield return new Edge(_vertices[i], _vertices[i + 1]);

                if (HasHoles)
                    foreach (CircularList<Vertex> circularList in Holes)
                        for (int i = 0; i < circularList.Count; i++)
                            yield return new Edge(circularList[i], circularList[i + 1]);
            }
        }



        public IEnumerable<IEnumerable<Edge>> AllHoleEdges
        {
            get
            {
                if (HasHoles)
                {
                    List<Edge> holeEdges = new List<Edge>();
                    foreach (CircularList<Vertex> circularList in Holes)
                        for (int i = 0; i < circularList.Count; i++)
                            holeEdges.Add(new Edge(circularList[i], circularList[i + 1]));
                    yield return holeEdges;
                }
            }
        }



        [EntityProperty]
        public float Area
        {
            get
            {
                float surfaceArea = 0;

                List<FaceTriangle> faceTriangles = this.Triangulate();

                foreach (FaceTriangle faceTriangle in faceTriangles)
                    surfaceArea += faceTriangle.CalculateArea();

                return surfaceArea;
            }
        }



        /*public IEnumerable<Line3D> Lines3D
        {
            get 
            {
                //join each two vertices into and edge
                for (int i = 0; i < _vertices.Count; i++)
                    yield return new Line3D(_vertices[i + 1].Position - _vertices[i].Position, _vertices[i].Position);

                if (HasHoles)
                    foreach (CircularList<Vertex> circularList in Holes)
                    {
                        for (int i = 0; i < circularList.Count; i++)
                            yield return new Line3D(circularList[i + 1].Position - circularList[i].Position,circularList[i].Position);
                    }
            }
        }*/

        #endregion
    }
}