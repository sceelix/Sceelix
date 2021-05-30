using Sceelix.Core.Annotations;
using Sceelix.Core.Data;

namespace Sceelix.Unity.Data
{
    [Entity("Unity Component", TypeBrowsable = false)]
    public class UnityComponent : Entity
    {
        public UnityComponent()
        {
            Type = GetType().Name;
        }



        public UnityComponent(string type)
        {
            Type = type;
        }



        public string Type
        {
            get;
            protected set;
        }
    }
}