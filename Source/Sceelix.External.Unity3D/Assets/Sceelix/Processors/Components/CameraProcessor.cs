using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Camera")]
    public class CameraProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            Camera camera = gameObject.AddComponent<Camera>();

            camera.clearFlags = jtoken["Properties"]["Clear Flags"].ToEnum<CameraClearFlags>();
            camera.backgroundColor = jtoken["Properties"]["Background"].ToColor();
            camera.fieldOfView = jtoken["Properties"]["Field of View"].ToObject<float>();
            camera.nearClipPlane = jtoken["Properties"]["Clipping Plane (Near)"].ToObject<float>();
            camera.farClipPlane = jtoken["Properties"]["Clipping Plane (Far)"].ToObject<float>();
            camera.depth = jtoken["Properties"]["Depth"].ToObject<float>();
            camera.renderingPath = jtoken["Properties"]["Rendering Path"].ToEnum<RenderingPath>();
            camera.useOcclusionCulling = jtoken["Properties"]["Occlusion Culling"].ToObject<bool>();
            camera.allowHDR = jtoken["Properties"]["HDR"].ToObject<bool>();
        }
    }
}