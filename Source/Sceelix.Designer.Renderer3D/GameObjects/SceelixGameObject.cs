using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Graphics;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public class SceelixGameObject : GameObject, IDrawableElement
    {
        private readonly IServiceLocator _services;

        private readonly List<String> _tags = new List<string>();

        private List<SceelixGameComponent> _components = new List<SceelixGameComponent>();
        private List<SceelixGameComponent> _updateableGameComponents = new List<SceelixGameComponent>();



        public SceelixGameObject(IServiceLocator services)
        {
            _services = services;
        }



        public List<SceelixGameComponent> Components
        {
            get { return _components; }
            set { _components = value; }
        }



        public IServiceLocator Services
        {
            get { return _services; }
        }



        public List<string> Tags
        {
            get { return _tags; }
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            foreach (var gameObject in _components)
                gameObject.Load();

            //this has a memory overhead, but can heavily improve performance for greater number of objects
            _updateableGameComponents = _components.Where(x => x.HasUpdate).ToList();
        }



        protected override void OnUnload()
        {
            base.OnUnload();

            foreach (var gameObject in _components)
                gameObject.Unload();
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            foreach (var gameComponent in _updateableGameComponents)
            {
                gameComponent.NewFrame();
                gameComponent.Update(deltaTime);
            }
        }



        public T GetComponentOrDefault<T>() where T : SceelixGameComponent
        {
            return (T) Components.FirstOrDefault(x => x is T);
        }



        public T GetComponent<T>() where T : SceelixGameComponent
        {
            T component = GetComponentOrDefault<T>();
            if (component == null)
                throw new ArgumentException("The requested component " + typeof(T).Name + " was not found.");

            return component;
        }



        public override string ToString()
        {
            return string.Format("Name: {0} | Tags: {1}", Name, String.Join(",", Tags));
        }



        public void Draw(RenderContext context)
        {
            foreach (var gameComponent in Components)
            {
                gameComponent.Draw(context);
            }
        }
    }
}