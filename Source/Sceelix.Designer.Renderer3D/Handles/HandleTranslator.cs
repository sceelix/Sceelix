using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Handles;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.Handles
{
    public abstract class HandleTranslator
    {
        //public abstract IEnumerable<Object> Process(List<Entity> entities, Environment loadEnvironment);

        public abstract Type GuideType
        {
            get;
        }
        

        public abstract void Clear();

        internal abstract void Add(IEnumerable<VisualHandle> visualVisualHandle);


        public virtual void Update(TimeSpan deltaTime)
        {
        }
        
        public abstract void HandleInput(InputContext context);
    }


    public abstract class HandleTranslator<T> : HandleTranslator where T : VisualHandle
    {
        protected readonly IServiceLocator Services;



        protected HandleTranslator(IServiceLocator services)
        {
            Services = services;
        }



        public override Type GuideType
        {
            get { return typeof(T); }
        }



        internal override void Add(IEnumerable<VisualHandle> visualVisualHandle)
        {
            Add(visualVisualHandle.OfType<T>());
        }



        public abstract void Add(IEnumerable<T> visualHandles);
    }
}