using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("ImportedMaterial")]
    public class ImportedMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material importedMaterial = new Material(Shader.Find("Standard"));

            if (jtoken["Properties"]["ColorDiffuse"] != null)
                importedMaterial.color = jtoken["Properties"]["ColorDiffuse"].ToColor();

            if (jtoken["Properties"]["Shininess"] != null)
                importedMaterial.SetFloat("_Glossiness", jtoken["Properties"]["Shininess"].ToObject<float>());

            if (jtoken["Properties"]["Shininess"] != null)
                importedMaterial.SetFloat("_Glossiness", jtoken["Properties"]["Shininess"].ToObject<float>());

            if (jtoken["Properties"]["DiffuseTexture"] != null)
                importedMaterial.SetTexture("_MainTex", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["DiffuseTexture"]));

            if (jtoken["Properties"]["SpecularTexture"] != null)
                importedMaterial.SetTexture("_MetallicGlossMap", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["SpecularTexture"]));

            if (jtoken["Properties"]["NormalTexture"] != null)
                importedMaterial.SetTexture("_BumpMap", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["NormalTexture"], true));

            if (jtoken["Properties"]["HeightTexture"] != null)
                importedMaterial.SetTexture("_ParallaxMap", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["HeightTexture"]));


            return importedMaterial;
        }
    }
}