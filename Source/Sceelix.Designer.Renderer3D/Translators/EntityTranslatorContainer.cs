using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Linq;
using Sceelix.Annotations;
using Sceelix.Core.Data;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.Translators
{
    [Renderer3DService]
    public class EntityTranslatorContainer : IServiceable
    {
        private readonly Dictionary<Type, IEntityTranslator> _newEntityObjectTranslators = AttributeReader.OfTypeKeyAttribute<EntityTranslatorAttribute>().GetInstancesOfType<IEntityTranslator>();


        public void Initialize(IServiceLocator services)
        {
            _newEntityObjectTranslators.Values.OfType<IServiceable>().ForEach(x => x.Initialize(services));
        }

        /// <summary>
        /// Get the entityobjectfactory that can handle this type of entity.
        /// This is a robust approach, as it allows subclasses of a certain entity to be processed as well.
        /// For instance, a subclass of "MeshEntity" will be handled by the "MeshEntityObjectFactory", if a more specific
        /// factory is not available.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEntityTranslator GetTranslator(IEntity entity)
        {
            return _newEntityObjectTranslators[entity.GetType()];
        }





        public void ProcessEntityTranslation(IEnumerable<IEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            Dictionary<IEntityTranslator, List<IEntity>> entityData = new Dictionary<IEntityTranslator, List<IEntity>>();

            foreach (IEntity entity in entities)
            {
                var translator = GetTranslator(entity);

                if (translator != null)
                {
                    if (!entityData.ContainsKey(translator))
                        entityData.Add(translator, new List<IEntity>());

                    entityData[translator].Add(entity);
                }
            }

            foreach (var entityDataItem in entityData)
            {
                if (entityData.Values.Any())
                    entityDataItem.Key.Process(entityDataItem.Value, entityObjectDomain);
            }
        }


        public Dictionary<Type, IEntityTranslator> EntityObjectTranslators
        {
            get { return _newEntityObjectTranslators; }
        }
    }
}
