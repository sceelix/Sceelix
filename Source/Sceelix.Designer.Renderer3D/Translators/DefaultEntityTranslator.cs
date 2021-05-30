using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.Messages;
using Sceelix.Designer.Services;
using Sceelix.Logging;

namespace Sceelix.Designer.Renderer3D.Translators
{
    [EntityTranslator(typeof(Entity))]
    public class DefaultEntityTranslator : EntityTranslator<Entity>
    {

        public override void Process(List<Entity> entities, EntityObjectDomain entityObjectDomain)
        {
            var entityType = entities.First().GetType();

            entityObjectDomain.Logger.Log(String.Format("No translator has been defined to visualize the type '{0}' in the 3D Renderer.", entityType.Name), LogType.Warning);
        }
    }
}