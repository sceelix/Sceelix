using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;

namespace Sceelix.Points.Data
{
    [Entity("Point Set", TypeBrowsable = false)]
    public class PointSetEntity : Entity, IActor
    {
        private BoxScope _boxScope;



        private PointSetEntity()
        {
        }



        public PointSetEntity(params Vertex[] vertices)
            : this((IEnumerable<Vertex>) vertices)
        {
        }



        public PointSetEntity(IEnumerable<Vertex> vertices)
        {
            Vertices = vertices.ToList();

            _boxScope = new BoxScope(Vertices.Select(val => val.Position));
        }



        public BoxScope BoxScope
        {
            get { return _boxScope; }
            set { _boxScope = value; }
        }



        public Vector3D Centroid
        {
            get { return Vertices.Select(x => x.Position).Aggregate((sum, val) => sum + val); }
        }



        public List<Vertex> Vertices
        {
            get;
            private set;
        }



        public override IEntity DeepClone()
        {
            var newPointCloud = new PointSetEntity
            {
                _boxScope = _boxScope,
                Vertices = Vertices.Select(x => (Vertex) x.DeepClone()).ToList()
            };

            Attributes.SetAttributesTo(newPointCloud.Attributes);

            return newPointCloud;
        }



        public void InsertInto(BoxScope target)
        {
            foreach (var vertex in Vertices)
            {
                var scopePosition = _boxScope.ToRelativeScopePosition(vertex.Position);
                vertex.Position = target.ToRelativeWorldPosition(scopePosition);
            }

            _boxScope = target;
        }
    }
}