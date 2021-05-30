using System;
using System.Reflection;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Custom")]
    public class CustomProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            var componentName = jtoken["ComponentName"].ToObject<String>();

            //if the optimized processor does not exist, simply go for the slower, but very generic Reflection approach
            var componentType = GetType().Assembly.GetType(componentName);

            if (componentType == null)
                Debug.LogWarning(String.Format("Component '{0}' is not defined.", componentName));
            else
            {
                Component customComponent = gameObject.AddComponent(componentType);

                var properties = jtoken["Properties"];

                foreach (var genericProperty in properties.Children())
                {
                    var propertyFieldName = genericProperty["Name"].ToObject<String>();
                    var propertyFieldType = Type.GetType(genericProperty["Type"].ToObject<String>());

                    //the indicated value can be field or property - try field first
                    FieldInfo fieldInfo = componentType.GetField(propertyFieldName);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(customComponent, genericProperty["Value"].ToObject(propertyFieldType));
                    }
                    else
                    {
                        //otherwise, try property and let the user know if it failed
                        PropertyInfo propertyInfo = componentType.GetProperty(propertyFieldName);
                        if (propertyInfo != null)
                            propertyInfo.SetValue(customComponent, genericProperty["Value"], null);
                        else
                            Debug.LogWarning(String.Format("Property/Field '{0}' for component '{1}' is not defined.", propertyFieldName, componentName));
                    }
                }
            }
        }
    }
}