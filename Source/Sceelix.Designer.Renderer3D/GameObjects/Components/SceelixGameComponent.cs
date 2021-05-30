using System.Reflection;
using DigitalRune.Game;
using DigitalRune.Graphics;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    public class SceelixGameComponent : GameObject
    {
        private readonly bool _hasUpdate;
        private readonly SceelixGameObject _parent;



        public SceelixGameComponent(SceelixGameObject parent)
        {
            _parent = parent;

            var method = this.GetType().GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            _hasUpdate = method.DeclaringType == this.GetType();
        }



        public SceelixGameObject Parent
        {
            get { return _parent; }
        }



        public IServiceLocator Services
        {
            get { return _parent.Services; }
        }



        internal bool HasUpdate
        {
            get { return _hasUpdate; }
        }



        public T GetComponentOrDefault<T>() where T : SceelixGameComponent
        {
            return Parent.GetComponentOrDefault<T>();
        }



        public T GetComponent<T>() where T : SceelixGameComponent
        {
            return Parent.GetComponent<T>();
        }



        public virtual void Draw(RenderContext context)
        {
        }
    }
}