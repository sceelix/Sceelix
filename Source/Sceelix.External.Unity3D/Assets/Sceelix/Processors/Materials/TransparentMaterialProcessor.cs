using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("TransparentMaterial")]
    public class TransparentMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material transparentMaterial = new Material(Shader.Find("Standard"));

            transparentMaterial.SetFloat("_Mode", 3);
            transparentMaterial.SetTexture("_MainTex", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["Texture"]));
            transparentMaterial.color = new Color(1, 1, 1, jtoken["Properties"]["Transparency"].ToObject<float>());
            transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMaterial.SetInt("_ZWrite", 0);
            transparentMaterial.DisableKeyword("_ALPHATEST_ON");
            transparentMaterial.DisableKeyword("_ALPHABLEND_ON");
            transparentMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMaterial.renderQueue = 3000;

            return transparentMaterial;
        }
    }
}