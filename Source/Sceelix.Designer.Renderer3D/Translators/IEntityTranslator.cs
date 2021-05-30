using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Annotations;
using Sceelix.Core.Data;
using Sceelix.Designer.Renderer3D.Data;


namespace Sceelix.Designer.Renderer3D.Translators
{
    public class EntityTranslatorAttribute : TypeKeyAttribute
    {
        public EntityTranslatorAttribute(Type typeKey)
            : base(typeKey)
        {
        }
    }

    public interface IEntityTranslator
    {
        void Process(List<IEntity> entities, EntityObjectDomain entityObjectDomain);
    }


    public abstract class EntityTranslator<T> : IEntityTranslator where T : IEntity
    {
        public void Process(List<IEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            Process(entities.Cast<T>().ToList(), entityObjectDomain);
        }

        public abstract void Process(List<T> entities, EntityObjectDomain entityObjectDomain);
    }
}