using System;
using System.Collections.Generic;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Entities
{
    [Processor("EntityGroup")]
    public class EntityGroupProcessor : EntityProcessor
    {
        //in order to avoid infinite loops, we have to define this field statically
        private static readonly Dictionary<String, EntityProcessor> _entityProcessors = ProcessorAttribute.GetClassesOfType<EntityProcessor>();

        public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
        {
            var groupType = entityToken["GroupType"].ToObject<String>();

            GameObject gameObject = new GameObject(groupType);

            //fill in the name, static, enabled, tag and layer fields
            ProcessCommonUnityAttributes(context, gameObject, entityToken);

            foreach (JToken subEntityToken in entityToken["SubEntities"].Children())
            {
                EntityProcessor entityProcessor;

                //if there is a processor for this entity Type, call it
                if (_entityProcessors.TryGetValue(subEntityToken["EntityType"].ToObject<String>(), out entityProcessor))
                {
                    var childGameObjects = entityProcessor.Process(context, subEntityToken);
                    foreach (GameObject childGameObject in childGameObjects)
                    {
                        childGameObject.transform.parent = gameObject.transform;
                    }
                }
                else
                {
                    Debug.LogWarning(String.Format("There is no defined processor for entity type {0}.", entityToken["EntityType"]));
                }
            }

            yield return gameObject;
        }
    }
}