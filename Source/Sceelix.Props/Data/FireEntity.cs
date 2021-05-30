using Sceelix.Actors.Data;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Props.Data
{
    public class FireEntity : Entity, IActor
    {
        public BoxScope BoxScope
        {
            get;
            set;
        } = BoxScope.Identity;



        public void InsertInto(BoxScope target)
        {
            BoxScope = target;
        }
    }
}