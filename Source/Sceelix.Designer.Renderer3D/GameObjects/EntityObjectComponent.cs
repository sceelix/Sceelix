using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    public class EntityObjectComponent
    {
        protected internal EntityObject Parent
        {
            get;
            internal set;
        }



        protected EntityObjectDomain EntityObjectDomain
        {
            get { return Parent.EntityObjectDomain; }
        }


        public T GetComponentOrDefault<T>() where T : EntityObjectComponent
        {
            return Parent.GetComponentOrDefault<T>();
        }



        public T GetComponent<T>() where T : EntityObjectComponent
        {
            return Parent.GetComponent<T>();
        }



        public virtual void OnLoad()
        {
        }
    }
}