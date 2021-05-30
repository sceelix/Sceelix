using System;
using Assets.Sceelix.Components;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Billboard")]
    public class BillboardProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            if (gameObject.GetComponent<BillboardComponent>()
                || gameObject.GetComponent<MeshFilter>() != null
                || gameObject.GetComponent<MeshRenderer>() != null)
                return;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = BillboardComponent.GetMesh();

            var imageToken = jtoken["Image"];
            var name = imageToken["Name"].ToObject<String>();

            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = context.CreateOrGetAssetOrResource<Material>("Material_" + name + ".mat", delegate ()
            {
                var billboardMaterial = new Material(Shader.Find("Standard"))
                {
                    mainTexture = context.CreateOrGetAssetOrResource(name + ".asset", () => imageToken["Content"].ToTexture())
                };

                billboardMaterial.SetFloat("_Glossiness", 0);
                billboardMaterial.SetFloat("_Mode", 1);
                billboardMaterial.DisableKeyword("_ALPHABLEND_ON");
                billboardMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                billboardMaterial.EnableKeyword("_ALPHATEST_ON");

                return billboardMaterial;
            });

            //don't forget about the billboardcomponent!
            gameObject.AddComponent<BillboardComponent>();
        }
    }
}