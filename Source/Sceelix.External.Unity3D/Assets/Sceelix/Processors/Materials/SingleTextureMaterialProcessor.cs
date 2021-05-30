using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("SingleTextureMaterial")]
    public class SingleTextureMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material singletextureMaterial = new Material(Shader.Find("Standard"));

            singletextureMaterial.mainTexture = Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["Texture"]);
            singletextureMaterial.SetFloat("_Glossiness", 0);
            singletextureMaterial.SetFloat("_Mode", 1);
            singletextureMaterial.DisableKeyword("_ALPHABLEND_ON");
            singletextureMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            singletextureMaterial.EnableKeyword("_ALPHATEST_ON");

            return singletextureMaterial;
        }
    }
}