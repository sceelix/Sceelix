using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Mesh Collider")]
    public class MeshColliderProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

            //if a meshCollider already exists, this will be null, so don't overwrite it
            if (meshCollider == null)
                return;

            meshCollider.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.convex = jtoken["Properties"]["IsConvex"].ToObject<bool>();
            meshCollider.isTrigger = jtoken["Properties"]["IsTrigger"].ToObject<bool>();
        }
    }
}