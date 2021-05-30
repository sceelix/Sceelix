using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.Interfaces;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    public class EntityObject
    {
        private readonly EntityObjectDomain _entityObjectDomain;

        private readonly List<EntityObjectComponent> _components = new List<EntityObjectComponent>();



        public EntityObject(EntityObjectDomain entityObjectDomain)
        {
            _entityObjectDomain = entityObjectDomain;
        }



        public void AddComponent(EntityObjectComponent component)
        {
            component.Parent = this;

            if(component is IServiceable)
                ((IServiceable)component).Initialize(_entityObjectDomain.Services);

            if (component is IUpdateableElement)
                _entityObjectDomain.Updated += deltaTime => ((IUpdateableElement)component).Update(deltaTime);

            if (component is IDrawableElement)
                _entityObjectDomain.Drawn += context => ((IDrawableElement)component).Draw(context);

            if (component is IDisposable)
                _entityObjectDomain.Unloaded += () => ((IDisposable)component).Dispose();


            component.OnLoad();

            _components.Add(component);
        }



        public void AddComponentRange(IEnumerable<EntityObjectComponent> components)
        {
            foreach (var component in components)
                AddComponent(component);
        }



        public T GetComponentOrDefault<T>() where T : EntityObjectComponent
        {
            return (T) Components.FirstOrDefault(x => x is T);
        }



        public T GetComponent<T>() where T : EntityObjectComponent
        {
            T component = GetComponentOrDefault<T>();
            if (component == null)
                throw new ArgumentException("The requested component " + typeof(T).Name + " was not found.");

            return component;
        }



        public bool HasComponent<T>()
        {
            return Components.Any(x => x is T);
        }



        public ReadOnlyCollection<EntityObjectComponent> Components
        {
            get { return new ReadOnlyCollection<EntityObjectComponent>(_components); }
        }



        internal EntityObjectDomain EntityObjectDomain
        {
            get { return _entityObjectDomain; }
        }
    }
}