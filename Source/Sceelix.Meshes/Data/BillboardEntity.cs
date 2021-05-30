using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Meshes.Data
{
    [Entity("Billboard")]
    public class BillboardEntity : Entity, IActor
    {
        public BillboardEntity(Vector2D value, Color color)
        {
            BoxScope = new BoxScope(sizes: new Vector3D(value.X, 0, value.Y));
            Color = color;
        }



        public BoxScope BoxScope
        {
            get;
            set;
        }


        public Color Color
        {
            get;
            set;
        }


        public string Image
        {
            get;
            set;
        }



        public override IEntity DeepClone()
        {
            var clone = (BillboardEntity) base.DeepClone();
            clone.BoxScope = BoxScope;

            return clone;
        }



        /*public void Translate(Vector3D direction, bool scopeRelative)
        {
            _boxScope.Translation += direction;
        }

        public void Scale(Vector3D scaling, Vector3D pivot, bool scopeRelative)
        {
            _boxScope.Scale(scaling,pivot,scopeRelative);
        }*/



        public void InsertInto(BoxScope target)
        {
            BoxScope = target;
        }
    }
}