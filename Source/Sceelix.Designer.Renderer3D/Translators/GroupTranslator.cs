using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.Translators
{
    [EntityTranslator(typeof(EntityGroup))]
    public class GroupTranslator : EntityTranslator<EntityGroup>, IServiceable
    {
        private EntityTranslatorContainer _translatorContainer;



        public void Initialize(IServiceLocator services)
        {
            _translatorContainer = services.Get<EntityTranslatorContainer>();
        }



        public override void Process(List<EntityGroup> entities, EntityObjectDomain entityObjectDomain)
        {
            _translatorContainer.ProcessEntityTranslation(entities.SelectMany(x => x.SubEntities), entityObjectDomain);
        }
    }
}