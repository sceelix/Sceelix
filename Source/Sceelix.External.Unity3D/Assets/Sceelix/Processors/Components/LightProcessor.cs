using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Light")]
    public class LightProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            Light light = gameObject.AddComponent<Light>();

            light.type = jtoken["Properties"]["LightType"].ToEnum<LightType>();
            light.range = jtoken["Properties"]["Range"].ToObject<float>();
            light.color = jtoken["Properties"]["Color"].ToColor();
            light.intensity = jtoken["Properties"]["Intensity"].ToObject<float>();
            light.bounceIntensity = jtoken["Properties"]["Bounce Intensity"].ToObject<float>();
            light.renderMode = jtoken["Properties"]["Render Mode"].ToEnum<LightRenderMode>();
            light.shadows = jtoken["Properties"]["Shadow Type"].ToEnum<LightShadows>();
        }
    }
}