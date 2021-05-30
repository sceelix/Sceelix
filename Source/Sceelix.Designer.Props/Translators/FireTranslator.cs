using System.Collections.Generic;
using Sceelix.Designer.Actors.Components;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Utils;
using Sceelix.Props.Data;
using Sceelix.Logging;

namespace Sceelix.Designer.Props.Translators
{
    [EntityTranslator(typeof(FireEntity))]
    public class FireTranslator : EntityTranslator<FireEntity>
    {

        public override void Process(List<FireEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            if (BuildPlatform.IsWindows)
            {
                foreach (FireEntity fire in entities)
                {
                    EntityObject componentObject = new EntityObject(entityObjectDomain);
                    componentObject.AddComponent(new CampfireComponent(fire.BoxScope.ToPose()));
                    componentObject.AddComponent(new CollisionComponent(fire.BoxScope.ToShape(), fire.BoxScope.ToPose()));
                    componentObject.AddComponent(new SelectableEntityComponent(fire));
                    componentObject.AddComponent(new CollisionHighlightComponent());
                    componentObject.AddComponent(new ScopeHighlightComponent(fire.BoxScope));

                    entityObjectDomain.ComponentObjects.Add(componentObject);
                }
            }
            else
            {
                entityObjectDomain.Environment.GetService<ILogger>().Log("Fire rendering is not yet possible for this platform.", LogType.Warning);
            }
        }
    }
}