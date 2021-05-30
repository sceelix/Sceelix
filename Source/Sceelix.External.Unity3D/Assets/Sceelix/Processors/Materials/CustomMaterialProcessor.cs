using System;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("CustomMaterial")]
    public class CustomMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            var shaderName = jtoken["Shader"].ToObject<String>();

            Material customMaterial = new Material(Shader.Find(shaderName));


            foreach (JToken propertyToken in jtoken["Properties"].Children())
            {
                var propertyName = propertyToken["Name"].ToObject<String>();
                var propertyType = propertyToken["Type"].ToObject<String>();
                switch (propertyType)
                {
                    case "TextureSlot":
                        var textureType = propertyToken["Value"]["Type"].ToObject<String>();
                        bool isNormal = textureType == "Normal";
                        customMaterial.SetTexture(propertyName, Texture2DExtensions.CreateOrGetTexture(context, propertyToken["Value"], isNormal));
                        break;
                    case "Boolean":
                        var status = propertyToken["Value"].ToObject<bool>();
                        if (status)
                            customMaterial.EnableKeyword(propertyName);
                        else
                            customMaterial.DisableKeyword(propertyName);
                        break;
                    case "Color":
                        customMaterial.SetColor(propertyName, propertyToken["Value"].ToColor());
                        break;
                    case "Int32":
                        customMaterial.SetInt(propertyName, propertyToken["Value"].ToObject<int>());
                        break;
                    case "Single":
                        customMaterial.SetFloat(propertyName, propertyToken["Value"].ToObject<float>());
                        break;
                    case "Vector4":
                        customMaterial.SetVector(propertyName, propertyToken["Value"].ToVector4());
                        break;
                    case "String":
                        customMaterial.SetOverrideTag(propertyName, propertyToken["Value"].ToObject<String>());
                        break;
                }
            }

            return customMaterial;
        }
    }
}