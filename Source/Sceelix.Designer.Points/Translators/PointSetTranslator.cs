using System.Collections.Generic;
using Sceelix.Designer.Points.Components;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;
using Sceelix.Points.Data;

namespace Sceelix.Designer.Points.Translators
{
    [EntityTranslator(typeof(PointSetEntity))]
    public class PointSetTranslator : EntityTranslator<PointSetEntity>
    {

        public override void Process(List<PointSetEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            foreach (PointSetEntity pointCloudEntity in entities)
            {
                EntityObject componentObject = new EntityObject(entityObjectDomain);
                //componentObject.AddComponent(new SelectableEntityComponent(pointCloudEntity));
                componentObject.AddComponent(new PointCloudDrawComponent(pointCloudEntity));

                entityObjectDomain.ComponentObjects.Add(componentObject);
            }
        }
    }
}