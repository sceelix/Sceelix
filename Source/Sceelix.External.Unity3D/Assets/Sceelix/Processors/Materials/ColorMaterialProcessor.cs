using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("ColorMaterial")]
    public class ColorMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material colorMaterial = new Material(Shader.Find("Standard"));
            colorMaterial.color = jtoken["Properties"]["DefaultColor"].ToColor();
            return colorMaterial;
        }
    }
}