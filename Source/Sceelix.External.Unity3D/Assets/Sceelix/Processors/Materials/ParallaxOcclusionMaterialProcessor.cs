using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("ParallaxOcclusionMaterial")]
    public class ParallaxOcclusionMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            //for some reason, looking for the StandardSpecular doesn't return anything...
            Material parallaxOcclusionMaterial = new Material(Shader.Find("Standard"));

            parallaxOcclusionMaterial.SetTexture("_MainTex", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["DiffuseTexture"]));
            parallaxOcclusionMaterial.SetTexture("_MetallicGlossMap", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["SpecularTexture"]));
            parallaxOcclusionMaterial.SetTexture("_BumpMap", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["NormalTexture"], true));
            parallaxOcclusionMaterial.SetTexture("_ParallaxMap", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["HeightTexture"]));
            parallaxOcclusionMaterial.EnableKeyword("_NORMALMAP");
            parallaxOcclusionMaterial.EnableKeyword("_PARALLAXMAP");

            return parallaxOcclusionMaterial;
        }
    }
}