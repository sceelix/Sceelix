using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Points.Components;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Points.Data;

namespace Sceelix.Designer.Points.Translators
{
    [EntityTranslator(typeof(PointEntity))]
    public class PointTranslator : EntityTranslator<PointEntity>
    {

        public override void Process(List<PointEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            var sphereShape = new SphereShape() {Radius = 1};
            foreach (PointEntity pointEntity in entities)
            {
                EntityObject componentObject = new EntityObject(entityObjectDomain);
                componentObject.AddComponent(new CollisionComponent(sphereShape,pointEntity.BoxScope.ToPose()));
                componentObject.AddComponent(new SelectableEntityComponent(pointEntity));
                componentObject.AddComponent(new CollisionHighlightComponent());
                componentObject.AddComponent(new Actors.Components.ScopeHighlightComponent(pointEntity.BoxScope));

                entityObjectDomain.ComponentObjects.Add(componentObject);
            }

            EntityObject totalObject = new EntityObject(entityObjectDomain);
            //componentObject.AddComponent(new SelectableEntityComponent(pointCloudEntity));
            totalObject.AddComponent(new PointsDrawComponent(entities));
            entityObjectDomain.ComponentObjects.Add(totalObject);
        }
    }
}