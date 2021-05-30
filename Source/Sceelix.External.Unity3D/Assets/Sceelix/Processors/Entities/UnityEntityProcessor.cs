using System;
using System.Collections.Generic;
using System.IO;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Components;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Entities
{
    [Processor("UnityEntity")]
    public class UnityEntityProcessor : EntityProcessor
    {
        Dictionary<string, ComponentProcessor> _componentProcessors = ProcessorAttribute.GetClassesOfType<ComponentProcessor>();

        public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
        {
            //first of all, let's see if we are loading a prefab
            var prefabPath = entityToken["Prefab"].ToTypeOrDefault<String>();
            var scaleMode = entityToken["ScaleMode"].ToTypeOrDefault<String>();
            var positioning = entityToken["Positioning"].ToTypeOrDefault<String>();

            GameObject gameObject;

            //if a prefab instruction is passed, load it
            if (!String.IsNullOrEmpty(prefabPath))
            {
                if (!prefabPath.StartsWith("Assets/"))
                    prefabPath = "Assets/" + prefabPath;

                //make sure the extension is set
                prefabPath = Path.ChangeExtension(prefabPath, ".prefab");

                gameObject = context.InstantiatePrefab(prefabPath);

                if (gameObject == null)
                {
                    gameObject = new GameObject();
                    Debug.LogWarning(String.Format("Could not create instance of prefab {0}. Please verify that it exists in the requested location.", prefabPath));
                    prefabPath = String.Empty;
                }
            }
            else
            {
                gameObject = new GameObject("Unity Entity");
            }

            //fill in the name, static, enabled, tag and layer fields
            ProcessCommonUnityAttributes(context, gameObject, entityToken);
            
            gameObject.transform.rotation *= Quaternion.LookRotation(entityToken["ForwardVector"].ToVector3(), entityToken["UpVector"].ToVector3());

            //if this is a prefab, we need to make its size and position match the same size
            if (!String.IsNullOrEmpty(prefabPath))
            {
                //try to get the bounds of the object
                var objectBounds = CalculateLocalBounds(gameObject);

                if (objectBounds.HasValue)
                {
                    var objectSize = new Vector3(objectBounds.Value.size.x / gameObject.transform.localScale.x, objectBounds.Value.size.y / gameObject.transform.localScale.y, objectBounds.Value.size.z / gameObject.transform.localScale.z);
                    var objectMin = new Vector3(objectBounds.Value.min.x / gameObject.transform.localScale.x, objectBounds.Value.min.y / gameObject.transform.localScale.y, objectBounds.Value.min.z / gameObject.transform.localScale.z); 
                    var intendedSize = entityToken["Size"].ToVector3();

                    if (scaleMode == "Stretch To Fill")
                    {
                        var scale = new Vector3(1 / objectSize.x, 1 / objectSize.y, 1 / objectSize.z);

                        gameObject.transform.localScale = Vector3.Scale(intendedSize, scale);

                        if (positioning == "Minimum")
                            gameObject.transform.Translate(-Vector3.Scale(objectMin, gameObject.transform.localScale));
                    }
                    else if (scaleMode == "Scale To Fit")
                    {
                        var scale = new Vector3(intendedSize.x / objectSize.x, intendedSize.y / objectSize.y, intendedSize.z / objectSize.z);
                        var minCoordinate = Math.Min(Math.Min(scale.x, scale.y), scale.z);
                        var newScale = new Vector3(minCoordinate, minCoordinate, minCoordinate);

                        gameObject.transform.localScale = newScale;

                        if (positioning == "Minimum")
                            gameObject.transform.Translate(-Vector3.Scale(objectMin, newScale));
                    }
                    else
                    {
                        if (positioning == "Minimum")
                            gameObject.transform.Translate(-Vector3.Scale(objectMin, gameObject.transform.localScale));
                    }
                }
            }
            else
            {
                var intendedSize = entityToken["Scale"].ToVector3();
                gameObject.transform.localScale = intendedSize;
            }


            gameObject.transform.position += entityToken["Position"].ToVector3();

            //now, iterate over the components
            //and look for the matching component processor
            foreach (JToken jToken in entityToken["Components"].Children())
            {
                ComponentProcessor componentProcessorAttribute;

                if (_componentProcessors.TryGetValue(jToken["ComponentType"].ToObject<String>(), out componentProcessorAttribute))
                    componentProcessorAttribute.Process(context, gameObject, jToken);
                else
                {
                    Debug.LogWarning(String.Format("There is no defined processor for component type {0}.", jToken["ComponentType"]));
                }
            }

            yield return gameObject;
        }
        

        private Bounds? CalculateLocalBounds(GameObject gameObject)
        {
            Quaternion currentRotation = gameObject.transform.rotation;
            gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            //Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
            Bounds? bounds = null;

            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                if (bounds.HasValue)
                    bounds.Value.Encapsulate(renderer.bounds);
                else
                    bounds = renderer.bounds;
            }

            if (!bounds.HasValue)
                return null;

            Vector3 localCenter = bounds.Value.center - gameObject.transform.position;
            //bounds.Value.center = localCenter;

            gameObject.transform.rotation = currentRotation;

            return new Bounds(localCenter, bounds.Value.size);
        }
    }
}