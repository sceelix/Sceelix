using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Materials;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("MeshRenderer")]
    public class MeshRendererProcessor : ComponentProcessor
    {
        Dictionary<string, MaterialProcessor> _materialProcessors = ProcessorAttribute.GetClassesOfType<MaterialProcessor>();

        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            GameObject realGameObject = gameObject;

            //if a MeshRenderer already exists, don't overwrite it
            //this may be the case if a prefab is loaded first
            //still, we need to run the rest of the function because we may be reading
            //important mesh and material information into the asset manager
            //so run this into a fake game object and then discard it
            if (gameObject.GetComponent<MeshRenderer>() != null)
                gameObject = new GameObject();

            //var sceelixObjectComponent = gameObject.GetComponent<SceelixObjectComponent>();

            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

            //GenericData[] genericMaterials = genericGameComponent.Get<GenericData[]>("Materials");
            var materialTokens = jtoken["Materials"].Children().ToList();

            Material[] sharedMaterials = new Material[materialTokens.Count];
            for (int index = 0; index < materialTokens.Count; index++)
            {
                var materialToken = materialTokens[index];
                var materialName = materialToken["Name"].ToObject<String>();

                var material = context.CreateOrGetAssetOrResource<Material>(materialName + ".mat", delegate ()
                {
                    MaterialProcessor materialProcessorAttribute;

                    if (materialToken["Type"] == null)
                    {
                        Debug.LogWarning("Could not load material. It was expected to have been loaded before. This could have been caused by a failure in a previous load.");
                        return null;
                    }

                    //if there is a type field, use it to find its processor
                    var materialType = materialToken["Type"].ToObject<String>();
                    if (_materialProcessors.TryGetValue(materialType, out materialProcessorAttribute))
                        return materialProcessorAttribute.Process(context, materialToken);


                    Debug.LogWarning(String.Format("There is no defined processor for material type {0}.", materialType));
                    return null;

                });

                sharedMaterials[index] = material;
            }

            renderer.sharedMaterials = sharedMaterials;

            if(realGameObject != gameObject)
                Object.DestroyImmediate(gameObject);
        }
    }
}