using System.Collections.Generic;
using Sceelix.Designer.Props.Components;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Utils;
using Sceelix.Props.Data;
using Sceelix.Logging;

namespace Sceelix.Designer.Props.Translators
{
    [EntityTranslator(typeof(Ocean))]
    public class OceanTranslator : EntityTranslator<Ocean>
    {

        
        public override void Process(List<Ocean> entities, EntityObjectDomain entityObjectDomain)
        {
            if (BuildPlatform.IsWindows)
            {
                foreach (Ocean ocean in entities)
                {
                    EntityObject componentObject = new EntityObject(entityObjectDomain);
                    componentObject.AddComponent(new OceanComponent(ocean));

                    entityObjectDomain.ComponentObjects.Add(componentObject);
                }
            }
            else
            {
                entityObjectDomain.Environment.GetService<ILogger>().Log("Ocean rendering is not yet possible for this platform.", LogType.Warning);
            }
        }
    }
}