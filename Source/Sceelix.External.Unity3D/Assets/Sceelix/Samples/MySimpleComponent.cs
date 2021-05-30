using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Components;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Samples
{
    /// <summary>
    /// This class demonstrates how you can instantiate any kind of component from Sceelix.
    /// 
    /// If you choose the "Custom" option at the "Unity Component" node in Sceelix, you can indicate names of components
    /// and properties, and the Sceelix Plugin will try to instantiate them through reflection. This works for simple
    /// types and requires no extra coding at all.
    /// 
    /// </summary>
    public class MySimpleComponent : MonoBehaviour
    {
        public String Name;
        public bool IsEnemy;
        public float Strength;
        public int Level;
        public double Experience;

        //because this is a more complex data type, you need a custom processor to set its value. See below.
        public List<String> MinionNames;

    }

    /// <summary>
    /// Alternatively, you can also easily create your own component processor using C#. Basically, you will be doing the interpretation of the json tree data structure.
    /// Writing your own processor allows for more complex component creation and is also faster to run (since it does not use reflection).
    /// 
    /// All you need to do is create a function with the signature:
    /// public static void YourFunctionName(IGenerationContext context, GameObject gameObject, JToken jtoken)
    /// 
    /// And it should have the [ComponentProcessor("YourComponentName")] attribute before. 
    /// 
    /// If you choose the "Other" option at the "Unity Component" node in Sceelix, you can indicate your own component name, which should match the string
    /// define in the attribute.
    /// 
    /// Doing this will allow the Sceelix Plugin to automatically read the defined functions.
    /// 
    /// You can also use this strategy to overwrite the default component handlers without messing with existing plugin code. All you need to do is define a 
    /// higher priority value in the attribute. We could override the default light component processor function (see the DefaultComponentManager) if you wish, 
    /// without needing to change the plugin code (making it safer to update the Sceelix plugin later).
    /// 
    /// You should look at the "DefaultComponentManager.cs" file for more examples on how to do this interpretation. 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="gameObject"></param>
    /// <param name="jtoken"></param>
    [Processor("MySimpleComponent", priority: 0)]
    public class MySimpleComponentProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            var simpleComponent = gameObject.AddComponent<MySimpleComponent>();

            //we can introduce some other type of logic
            if (jtoken["Properties"]["IsBoss?"].ToObject<bool>())
            {
                simpleComponent.Name = "Boss";
                simpleComponent.IsEnemy = true;
            }
            else
            {
                simpleComponent.Name = "Other";
                simpleComponent.IsEnemy = false;
            }

            simpleComponent.MinionNames = jtoken["Properties"]["Minions"].ToObject<String>().Split(',').ToList();
        }
    }

}