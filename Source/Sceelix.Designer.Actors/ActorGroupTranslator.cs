using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Data;
using Sceelix.Designer.Actors.Components;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Actors
{
    [EntityTranslator(typeof(ActorGroup))]
    public class ActorGroupTranslator : EntityTranslator<ActorGroup>, IServiceable
    {
        private EntityTranslatorContainer _translatorContainer;



        public void Initialize(IServiceLocator services)
        {
            _translatorContainer = services.Get<EntityTranslatorContainer>();
        }



        public override void Process(List<ActorGroup> entities, EntityObjectDomain entityObjectDomain)
        {
            _translatorContainer.ProcessEntityTranslation(entities.SelectMany(x => x.SubEntities), entityObjectDomain);

            foreach (ActorGroup actorGroup in entities)
            {
                EntityObject componentObject = new EntityObject(entityObjectDomain);
                componentObject.AddComponent(new SelectableEntityComponent(actorGroup));
                componentObject.AddComponent(new ScopeHighlightComponent(actorGroup.BoxScope));
                entityObjectDomain.ComponentObjects.Add(componentObject);
            }
        }
    }
}