using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sceelix.Components;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Entities;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Messages
{
    [Processor("Graph Results")]
    public class GraphResultsMessageProcessor : MessageProcessor
    {
        Dictionary<String, EntityProcessor> _entityProcessors = ProcessorAttribute.GetClassesOfType<EntityProcessor>();


        public override void Process(IGenerationContext context, JToken data)
        {
            context.ReportStart();

            //first, clear all prevous Sceelix Scene Object marked with "Remove"
            foreach (GameObject existingGameObject in UnityEngine.Object.FindObjectsOfType<GameObject>().ToList())
            {
                if (existingGameObject != null)
                {
                    var existingSceneComponent = existingGameObject.GetComponent<SceelixDesignerComponent>();
                    if (existingSceneComponent != null && existingSceneComponent.RemoveOnRegeneration)
                        UnityEngine.Object.DestroyImmediate(existingGameObject);
                }
            }

            try
            {
                //then, add the new Scene Object
                GameObject sceneGameObject = new GameObject();
                sceneGameObject.name = data["Name"].ToObject<String>();

                var sceneComponent = sceneGameObject.AddComponent<SceelixDesignerComponent>();
                sceneComponent.RemoveOnRegeneration = context.RemoveOnRegeneration;



                var entityTokens = data["Entities"].Children().ToList();
                for (int index = 0; index < entityTokens.Count; index++)
                {
                    JToken entityToken = entityTokens[index];

                    context.ReportProgress(index / (float)entityTokens.Count);

                    EntityProcessor entityProcessor;

                    //if there is a processor for this entity Type, call it
                    if (_entityProcessors.TryGetValue(entityToken["EntityType"].ToObject<String>(), out entityProcessor))
                    {
                        var childGameObjects = entityProcessor.Process(context, entityToken);
                        foreach (GameObject childGameObject in childGameObjects)
                        {
                            childGameObject.transform.parent = sceneGameObject.transform;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(String.Format("There is no defined processor for entity type {0}.", entityToken["EntityType"]));
                    }
                }

                context.ReportObjectCreation(sceneGameObject);
            }
            catch (Exception ex)
            {
                //log the exception anyway
                UnityEngine.Debug.LogError(ex);
            }

            //do not forget report the end of the process, even
            //if an exception was thrown
            context.ReportEnd();
        }
    }
}
