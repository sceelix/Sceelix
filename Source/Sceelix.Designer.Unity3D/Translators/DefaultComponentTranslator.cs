using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Annotations;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Services;
using Sceelix.Logging;
using Sceelix.Unity.Data;

namespace Sceelix.Designer.Unity3D.Translators
{
    [TypeKey(typeof(UnityComponent))]
    public class DefaultComponentTranslator : ComponentTranslator
    {
        public static String[] NonImplementedSceelixComponents = new[] {"Light", "Camera", "Mesh Collider", "Custom", "Surface", "Other"};

        public static Dictionary<String, ComponentTranslator> ComponentActuators = AttributeReader.OfStringKeyAttribute().GetInstancesOfType<ComponentTranslator>();


        public override IEnumerable<EntityObjectComponent> Process(UnityEntity unityEntity, UnityComponent unityComponent, EntityObjectDomain entityObjectDomain)
        {
            //last attempt, try to get a translator by type string
            ComponentTranslator componentTranslator;
            if (ComponentActuators.TryGetValue(unityComponent.Type, out componentTranslator))
            {
                //and forward the request
                foreach (var entityObjectComponent in componentTranslator.Process(unityEntity, unityComponent, entityObjectDomain))
                    yield return entityObjectComponent;
            }
            else if (NonImplementedSceelixComponents.Contains(unityComponent.Type))
            {
                //we'll ignore those components whose translators we won't be implementing for now
            }
            else
            {
                entityObjectDomain.Logger.Log("No translator has been defined for the component '" + unityComponent.Type + "'.", LogType.Warning);
            }
        }


        public override void PostProcess(IEnumerable<EntityObject> entityObjects, EntityObjectDomain contentLoader)
        {
        }


        public override bool IsDrawable
        {
            get { return false; }
        }
    }
}