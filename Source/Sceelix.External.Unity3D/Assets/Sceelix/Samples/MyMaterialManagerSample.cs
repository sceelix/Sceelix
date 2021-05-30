using System;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Materials;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Samples
{
    /// <summary>
    /// This class demonstrates how you can instantiate your own custom materials defined in Sceelix.
    /// 
    /// You can create any new class in any location of your project to do this (preferably outside the Sceelix folder, so that you can update the plugin in the future). 
    /// 
    /// All you need to do is create a function with the signature:
    /// public static Material YourFunctionName(IGenerationContext context, JToken jtoken)
    /// 
    /// And it should have the [MaterialProcessor("YourMaterialName")] attribute before. 
    /// 
    /// Doing this will allow the Sceelix Plugin to automatically read the defined functions.
    /// </summary>
    [Processor("MySpecialMaterial")]
    public class MySpecialMaterialProcessor : MaterialProcessor
    {
        /// <summary>
        /// You can define your custom material in the "Mesh Material" node in Sceelix by choosing the "Other" option.
        /// The "Name" you indicate there should match the one you define here. Here we indicate that we are handling a "MySpecialMaterial".
        /// 
        /// When the Sceelix plugin starts decoding the material, it will call this function, which should return an instance of the Unity Material class.
        /// 
        /// You should check the "DefaultMaterialManager" class in the Sceelix Plugin classes if you need help in figuring out the decoding.
        /// </summary>
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material mySpecialMaterial = new Material(Shader.Find("Standard"));

            //make use of the "DefaultMaterialManager.CreateOrGetTexture" function to decode textures. It will certainly fit most of your needs
            //Here it is assumed that, in Sceelix, you defined a texture called MyFirstTexture
            mySpecialMaterial.mainTexture = Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["MyFirstTexture"]);

            return mySpecialMaterial;
        }
    }


    [Processor("MySpecialMaterial", priority: 10)]
    public class MyOverridingMaterialProcessor : MaterialProcessor
    {
        SingleTextureMaterialProcessor _singleTextureMaterialProcessor = new SingleTextureMaterialProcessor();

        /// <summary>
        /// You can also override existing materials without messing with existing functions. All you need to do is define a higher priority value in
        /// the attribute. For instance, here we override the previous function to handle the "MySpecialMaterial". This makes little sense here, 
        /// but you could do this to override the default Sceelix functions, if you wish, without needing to change the plugin code 
        /// (making it safer to update the Sceelix plugin later).
        /// 
        /// If you would set "SingleTextureMaterial" as the material name in the attribute, you would override the default Sceelix
        /// handler for the single texture material (whose default priority is 0).
        /// </summary>
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            var style = jtoken["Properties"]["Style"].ToObject<String>();

            //you could pass strings to define styles or specific behaviours
            if (style == "Simple")
            {
                Material mySpecialMaterial = new Material(Shader.Find("Diffuse"));

                mySpecialMaterial.mainTexture = Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["MyTexture"]);

                return mySpecialMaterial;
            }
            if (style == "Existing")
            {
                //or you could send this to an existing processor, too...
                return _singleTextureMaterialProcessor.Process(context, jtoken);
            }
            if (style == "From Disk")
            {
                //you could also read existing materials from disk
                var existingMaterial = new Material(context.GetExistingResource<Material>("Assets/MyMaterials/New Material.mat"));

                //and then read your own extra definitions
                existingMaterial.SetTexture("_MainTex", Texture2DExtensions.CreateOrGetTexture(context, jtoken["Properties"]["MyTexture"]));

                return existingMaterial;
            }

            //if you return null, the default pink look will appear
            return null;
        }
    }


    
}
