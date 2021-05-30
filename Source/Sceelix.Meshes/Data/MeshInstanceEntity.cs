using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Meshes.Data
{
    [Entity("Mesh Instance")]
    public class MeshInstanceEntity : Entity, IActor
    {
        private BoxScope _boxScope;



        public MeshInstanceEntity(MeshEntity meshEntity)
        {
            _boxScope = meshEntity.BoxScope;

            MeshEntity = meshEntity;
            RelativeScale = new Vector3D(1 / _boxScope.Sizes.X, 1 / _boxScope.Sizes.Y, 1 / _boxScope.Sizes.Z).MakeValid(1);

            //reset the orientation of the mesh
            meshEntity.InsertInto(new BoxScope(sizes: _boxScope.Sizes));

            //_meshEntity.InsertInto(BoxScope.Identity);
        }



        public BoxScope BoxScope
        {
            get { return _boxScope; }
            set { _boxScope = value; }
        }



        public MeshEntity MeshEntity
        {
            get;
        }


        public Vector3D RelativeScale
        {
            get;
            set;
        }


        public Vector3D Scale => RelativeScale * _boxScope.Sizes.ReplaceValue(0, 1);



        public override IEntity DeepClone()
        {
            var clone = (MeshInstanceEntity) base.DeepClone();
            clone._boxScope = _boxScope;

            return clone;
        }



        /*public void Translate(Vector3D direction, bool scopeRelative)
        {
            _boxScope.Translation += direction;
        }

        public void Scale(Vector3D scaling, Vector3D pivot, bool scopeRelative)
        {
            if (scopeRelative)
                pivot = BoxScope.ToWorldPosition(pivot);

            Matrix transformation = scopeRelative ?
                Matrix.CreateTranslation(pivot) * BoxScope.ToWorldDirectionMatrix() * Matrix.CreateScale(scaling) * BoxScope.ToScopeDirectionMatrix() * Matrix.CreateTranslation(-pivot)
                :
                Matrix.CreateTranslation(pivot) * Matrix.CreateScale(scaling) * Matrix.CreateTranslation(-pivot);

            _boxScope.Transform(transformation);
        }*/



        public void InsertInto(BoxScope target)
        {
            _boxScope = target;
        }
    }
}