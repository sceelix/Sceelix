using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Annotations;
using Sceelix.Designer.Actors.Components;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Unity.Data;

namespace Sceelix.Designer.Unity3D.Translators
{
    [EntityTranslator(typeof(UnityEntity))]
    public class UnityEntityTranslator : EntityTranslator<UnityEntity>, IServiceable
    {
        private static Dictionary<Type, ComponentTranslator> ComponentTranslators = AttributeReader.OfTypeKeyAttribute().GetInstancesOfType<ComponentTranslator>();
        

        public void Initialize(IServiceLocator services)
        {
            ComponentTranslators.Values.OfType<IServiceable>().ForEach(x => x.Initialize(services));
        }


        public override void Process(List<UnityEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            List<EntityObject> entityObjects = new List<EntityObject>();

            foreach (UnityEntity unityEntity in entities)
            {
                var isDrawable = false;

                EntityObject entityObject = new EntityObject(entityObjectDomain);

                //get the translators from each component and add them
                foreach (UnityComponent gameComponent in unityEntity.GameComponents)
                {
                    var componentTranslator = ComponentTranslators[gameComponent.GetType()];
                    entityObject.AddComponentRange(componentTranslator.Process(unityEntity, gameComponent, entityObjectDomain));

                    isDrawable |= componentTranslator.IsDrawable;
                }

                //if there is no collisioncomponent, add the default, box one
                if (!entityObject.HasComponent<CollisionComponent>())
                {
                    entityObject.AddComponent(new CollisionComponent(GetOffsettedBoxShape(Vector3F.One), unityEntity.BoxScope.ToPoseWithoutScale())
                    {
                        Scale = unityEntity.Scale.ToVector3F(false)
                    });
                }

                //add the means to select the entity and view its scope and collision shape
                entityObject.AddComponent(new SelectableEntityComponent(unityEntity));
                entityObject.AddComponent(new ScopeHighlightComponent(unityEntity.BoxScope));
                entityObject.AddComponent(new CollisionHighlightComponent(!isDrawable));
                
                //if(!String.IsNullOrWhiteSpace(unityEntity.Prefab))
                //    entityObject.AddComponent(new TextComponent(unityEntity.Prefab,unityEntity.BoxScope.Centroid.ToVector3F()));

                entityObjects.Add(entityObject);
                entityObjectDomain.ComponentObjects.Add(entityObject);
            }

            //if we need to run merging operations, do it in the postprocess function
            foreach (ComponentTranslator componentTranslator in ComponentTranslators.Values)
                componentTranslator.PostProcess(entityObjects, entityObjectDomain);
        }
        

        public static Shape GetOffsettedBoxShape(Vector3F sizes)
        {
            GeometricObject geometricObject = new GeometricObject(new BoxShape(sizes));
            geometricObject.Pose = new Pose(sizes / 2f, Matrix33F.Identity);

            return new TransformedShape(geometricObject);
        }
    }
}
