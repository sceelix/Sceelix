using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Graphics;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Interfaces;

namespace Sceelix.Designer.Renderer3D.Data
{
    public class GameObjectGroup : GameObject, IDrawableElement
    {
        private readonly List<GameObject> _gameObjects = new List<GameObject>();



        public List<GameObject> GameObjects
        {
            get { return _gameObjects; }
        }



        public void Draw(RenderContext context)
        {
            foreach (IDrawableElement drawableGameObject in _gameObjects.OfType<IDrawableElement>())
                drawableGameObject.Draw(context);
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Load();
            }
        }



        protected override void OnUnload()
        {
            base.OnUnload();

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Unload();
            }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Update(deltaTime);
            }
        }
    }
}