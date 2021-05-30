using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Actors.Data
{
    [Entity("Actor")]
    public interface IActor : IEntity
    {
        BoxScope BoxScope
        {
            get;
            set;
        } //and set, but do not allow it to be changed in some entities (such as Surface)

        /*void Translate(Vector3D direction, bool scopeRelative);

        void Scale(Vector3D scaling, Vector3D pivot, bool scopeRelative);*/

        void InsertInto(BoxScope target);
    }
}