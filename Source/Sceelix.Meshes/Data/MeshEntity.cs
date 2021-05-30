using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;

namespace Sceelix.Meshes.Data
{
    [Entity("Mesh")]
    public class MeshEntity : Entity, IEnumerable<Face>, IActor
    {
        //scope that encapsulates all the previous geometries
        private BoxScope _boxScope;

        /// <summary>
        /// List of faces that define this mesh.
        /// </summary>
        private List<Face> _faces = new List<Face>();



        protected MeshEntity()
        {
        }



        /// <summary>
        /// Creates a mesh entity from a set of faces and a given scope.
        /// </summary>
        /// <param name="faces">Face enumeration with the faces</param>
        /// <param name="scope">(Optional) Explicit boxscope. If not defined, the standard scope calculation will be performed.</param>
        public MeshEntity(IEnumerable<Face> faces, BoxScope? scope = null)
        {
            _faces = new List<Face>(faces);
            _boxScope = scope ?? Face.GetAlignedScope(_faces);
        }



        /// <summary>
        /// Creates a mesh entity from one (or many) face(s).
        /// </summary>
        /// <param name="faces"></param>
        public MeshEntity(params Face[] faces)
            : this(faces, Face.GetAlignedScope(faces))
        {
        }



        /// <summary>
        /// Adds the face to the mesh and updates the boxscope.
        /// </summary>
        /// <param name="face"></param>
        public void Add(Face face)
        {
            _faces.Add(face);
            _boxScope = _boxScope.Grow(face.Vertices.Select(x => x.Position));
        }



        /// <summary>
        /// Adds the faces to the mesh and updates the boxscope.
        /// </summary>
        /// <param name="faces"></param>
        public void AddRange(IEnumerable<Face> faces)
        {
            var faceList = faces as IList<Face> ?? faces.ToList();

            _faces.AddRange(faceList);
            _boxScope = _boxScope.Grow(faceList.SelectMany(x => x.Vertices).Select(x => x.Position));
        }



        /// <summary>
        /// Maintains the orientation and recalculates the translation and sizes so that it encompasses the MeshEntity.
        /// </summary>
        public void AdjustScope()
        {
            _boxScope = _boxScope.Adjust(FaceVertices.Select(x => x.Position));
        }



        /// <summary>
        /// Maintains the orientation of the parent and recalculates the translation and sizes so that it encompasses the current MeshEntity.
        /// </summary>
        public void AdjustScope(BoxScope parentScope)
        {
            _boxScope = parentScope.Adjust(FaceVertices.Select(x => x.Position));
        }



        /// <summary>
        /// Removes possible links/references to faces/vertices that may not be part of this mesh.
        /// </summary>
        public void CleanFaceConnections()
        {
            HashSet<Face> faces = new HashSet<Face>(_faces);
            foreach (Vertex faceVertex in FaceVertices) faceVertex.CleanFaceConnections(faces);
        }



        public MeshEntity CreateDerived(MeshEntity meshEntity)
        {
            var newMeshEntity = new MeshEntity();
            Attributes.SetAttributesTo(newMeshEntity.Attributes);

            return newMeshEntity;
        }



        public virtual MeshEntity CreateDerived(IEnumerable<Face> collection, bool deriveScope = true)
        {
            MeshEntity newMeshEntity = (MeshEntity) new MeshEntity(collection).DeepClone();

            if (deriveScope)
                newMeshEntity.AdjustScope(BoxScope);

            Attributes.SetAttributesTo(newMeshEntity.Attributes);

            return newMeshEntity;
        }



        public virtual MeshEntity CreateDerived(Face face, bool deriveScope = true)
        {
            MeshEntity newMeshEntity = (MeshEntity) new MeshEntity(face).DeepClone();

            if (deriveScope)
                newMeshEntity.AdjustScope(BoxScope);

            Attributes.SetAttributesTo(newMeshEntity.Attributes);

            return newMeshEntity;
        }



        public override IEntity DeepClone()
        {
            MeshEntity clonedMeshEntity = (MeshEntity) base.DeepClone();

            clonedMeshEntity._boxScope = _boxScope;
            clonedMeshEntity._faces = new List<Face>();

            if (_faces != null)
            {
                Dictionary<Vertex, Vertex> oldToNewVertexMapping = new Dictionary<Vertex, Vertex>();

                foreach (Face face in Faces)
                {
                    List<Vertex> newVertices = new List<Vertex>(face.Vertices.Count());

                    //go 
                    foreach (Vertex vertex in face.Vertices)
                        newVertices.Add(GetNewVertex(vertex, oldToNewVertexMapping));

                    Face newFace = new Face(face, newVertices);

                    //copy the holes as well
                    if (face.HasHoles)
                        foreach (CircularList<Vertex> circularList in face.Holes)
                        {
                            List<Vertex> newHoleVertices = new List<Vertex>(circularList.Count());

                            foreach (Vertex vertex in circularList)
                                newHoleVertices.Add(GetNewVertex(vertex, oldToNewVertexMapping));

                            newFace.AddHole(newHoleVertices);
                        }


                    foreach (Vertex oldVertex in face.Vertices)
                    {
                        HalfVertex oldHv = oldVertex[face];
                        HalfVertex newHv = oldToNewVertexMapping[oldVertex][newFace];
                        //HalfVertex newHv = oldVertex.GetGlobalAttribute<Vertex>("NewVertex")[newFace];//  oldToNewVertexMapping[oldVertex][newFace];

                        //newHv.GeometryProperties = oldHv.GeometryProperties.ToList();
                        oldHv.Attributes.SetAttributesTo(newHv.Attributes);
                    }


                    clonedMeshEntity._faces.Add(newFace);
                }
            }

            //yes, we need to do this twice
            //clonedMeshEntity.BoxScope = _boxScope.DeepClone();

            return clonedMeshEntity;
        }



        public static BoundingBox GetBoundingBox(IEnumerable<Face> faces)
        {
            BoundingBox boundingBox = new BoundingBox();
            IEnumerable<Vertex> vertices = faces.SelectMany(val => val.Vertices);

            foreach (Vertex vertex in vertices)
                if (!vertex.Position.IsNaN && !vertex.Position.IsInfinity)
                    boundingBox.AddPoint(vertex.Position);

            return boundingBox;
        }



        public IEnumerator<Face> GetEnumerator()
        {
            return _faces.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        private Vertex GetNewVertex(Vertex vertex, Dictionary<Vertex, Vertex> oldToNewVertexMapping)
        {
            Vertex newVertex;

            //if((newVertex = vertex.GetGlobalAttribute<Vertex>("NewVertex")) == null)
            if (!oldToNewVertexMapping.TryGetValue(vertex, out newVertex))
            {
                oldToNewVertexMapping.Add(vertex, newVertex = new Vertex(vertex));
                //newVertex = new Vertex(vertex);
                vertex.Attributes.SetAttributesTo(newVertex.Attributes);
                //vertex.SetAttribute("NewVertex", newVertex);
            }

            return newVertex;
        }



        public void InsertInto(BoxScope target)
        {
            foreach (var faceVertex in FaceVertices)
            {
                var scopePosition = _boxScope.ToRelativeScopePosition(faceVertex.Position);
                faceVertex.Position = target.ToRelativeWorldPosition(scopePosition);

                foreach (HalfVertex halfVertex in faceVertex.HalfVertices)
                {
                    Vector3D normal = halfVertex.Normal;
                    Vector3D scopeDirection = _boxScope.ToScopeDirection(normal);
                    halfVertex.Normal = target.ToWorldDirection(scopeDirection);
                }
            }

            foreach (var face in Faces)
            {
                Vector3D normal = face.Normal;
                Vector3D scopeDirection = _boxScope.ToScopeDirection(normal);

                face.Normal = target.ToWorldDirection(scopeDirection);
            }

            _boxScope = target;

            if (target.XAxis.Cross(target.SizedYAxis).Dot(target.ZAxis) < 0)
            {
                this.ForEach(x => x.Flip());
                _boxScope = new BoxScope(_boxScope.XAxis, _boxScope.YAxis, _boxScope.XAxis.Cross(_boxScope.YAxis), _boxScope.Translation, _boxScope.Sizes);
                AdjustScope();
            }
        }



        /// <summary>
        /// Removes the face, detaches it from associated vertices and adjusts the scope.
        /// </summary>
        /// <param name="face"></param>
        public void RemoveAndDetach(Face face, bool adjustScope = false)
        {
            _faces.Remove(face);
            face.Detach();

            if (adjustScope)
                AdjustScope();
        }



        #region Properties

        /// <summary>
        /// Average position of all mesh vertices, i.e. the mesh centroid.
        /// </summary>
        public Vector3D Centroid
        {
            get
            {
                if (_faces.Count == 0)
                    return Vector3D.Zero;

                return _faces.Select(x => x.Centroid).Aggregate((sum, val) => sum + val) / _faces.Count;
            }
        }



        /// <summary>
        /// The current boxscope that encloses the mesh. When set, will call the AdjustScope() function to make sure it completely encloses the mesh.
        /// 
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



        [SubEntity("Faces")]
        public ReadOnlyCollection<Face> Faces => new ReadOnlyCollection<Face>(_faces);



        [SubEntity("Vertices")]
        public IEnumerable<Vertex> FaceVertices
        {
            get { return _faces.SelectMany(val => val.AllVertices).Distinct(); }
        }



        public IEnumerable<Vertex> FaceVerticesWithHoles
        {
            get { return _faces.SelectMany(val => val.AllVertices).Distinct(); }
        }



        [EntityProperty]
        public int VertexCount => FaceVertices.Count();



        [EntityProperty]
        public float Area
        {
            get
            {
                float surfaceArea = 0;

                foreach (Face face in this) surfaceArea += face.Area;

                return surfaceArea;
            }
        }



        [EntityProperty]
        public bool IsConvex
        {
            get { return Faces.All(f => f.IsConvex); }
        }



        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox boundingBox = new BoundingBox();

                //foreach (Vertex vertex in AllVertices)
                foreach (Vertex vertex in FaceVertices)
                    if (!vertex.Position.IsNaN && !vertex.Position.IsInfinity)
                        boundingBox.AddPoint(vertex.Position);

                return boundingBox;
            }
        }



        public IEnumerable<Edge> FaceEdges
        {
            get { return _faces.SelectMany(val => val.AllEdges).Distinct(new Edge.CoincidencyEqualityComparer()); }
        }



        [EntityProperty]
        public bool HasFaces => _faces != null;



        /// <summary>
        /// A listing of all subentites that this entity encompasses.
        /// Order of type or hierarchy is not guaranteed.
        /// </summary>
        public override IEnumerable<IEntity> SubEntityTree
        {
            get
            {
                foreach (var face in Faces)
                {
                    yield return face;

                    foreach (var halfVertex in face.HalfVertices)
                        yield return halfVertex;
                }

                foreach (var faceVertex in FaceVertices)
                    yield return faceVertex;
            }
        }



        public Material Material
        {
            set
            {
                foreach (Face face in _faces)
                    face.Material = value;
            }
        }



        /// <summary>
        /// The sum of the perimeters of all faces in the mesh.
        /// </summary>
        [EntityProperty]
        public float Perimeter
        {
            get { return Faces.Sum(x => x.Perimeter); }
        }



        public BoundingRectangle BoundingRectangle
        {
            get
            {
                BoundingRectangle boundingRectangle = new BoundingRectangle();
                IEnumerable<Vertex> vertices = this.SelectMany(val => val.Vertices);

                foreach (Vertex vertex in vertices)
                    if (!vertex.Position.IsNaN && !vertex.Position.IsInfinity)
                        boundingRectangle.AddPoint(vertex.Position.ToVector2D());

                return boundingRectangle;
            }
        }



        /// <summary>
        /// The scope of the whole mesh, indicating base position, orientation and sizes.
        /// </summary>
        [EntityProperty("Scope")]
        public SceeList BoxScopeList => ConvertHelper.Convert<SceeList>(_boxScope);



        public Face this[int index]
        {
            get { return _faces[index]; }
            set { _faces[index] = value; }
        }

        #endregion
    }
}