using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Annotations
{
    public class StringKeyAttributeReader<TAttribute> : AttributeReader where TAttribute : StringKeyAttribute
    {
        public StringKeyAttributeReader(IEnumerable<Assembly> assemblies)
            : base(assemblies)
        {
        }



        /// <summary>
        /// Looks for FunctionProcessorAttributes defined in a set of assemblies.
        /// </summary>
        /// <typeparam name="TObject">Type of FunctionProcessorAttribute to look for.</typeparam>
        /// <param name="context"></param>
        /// <returns>Dictionary with the found FunctionProcessorAttribute instances as values, as their Key as keys.</returns>
        public Dictionary<string, KeyValuePair<TAttribute, TObject>> GetAttributeAndClassesOfType<TObject>() //String context = null
        {
            var allAssemblyTypes = Assemblies.SelectMany(x => x.GetTypes()).ToList();

            Dictionary<string, TAttribute> attributeDictionary = new Dictionary<string, TAttribute>();
            foreach (var type in allAssemblyTypes.Where(x => typeof(TObject).IsAssignableFrom(x)))
                try
                {
                    var customAttribute = type.GetCustomAttribute<TAttribute>();
                    if (customAttribute != null)
                    {
                        if (type.IsAbstract) //|| customAttribute.Context != 
                            //Debug.LogError(String.Format("Class '{0}' is marked with ProcessorAttribute, but cannot be instanced.", type.Name));
                            continue;

                        //if there are context tags defined, our selection of types must have at least one
                        /*if (!String.IsNullOrWhiteSpace(context) && customAttribute.Context != context)
                            continue;*/

                        customAttribute.TargetType = type;

                        //check if any attribute with a higher priority exists
                        //if the one existing has a lower priority, replace it with this one
                        //if, for mistake, the priority is the same, warn the user
                        //otherwise let the previous one be
                        TAttribute existingAttribute;
                        if (attributeDictionary.TryGetValue(customAttribute.Key, out existingAttribute))
                        {
                            if (customAttribute.Priority == existingAttribute.Priority)
                            {
                                //Debug.LogWarning(String.Format("Did not register class '{0}' marked with ProcessorAttribute. A method with the same priority is already defined.", type.Name));
                            }
                            else if (customAttribute.Priority > existingAttribute.Priority)
                            {
                                attributeDictionary[customAttribute.Key] = customAttribute;
                            }
                        }
                        else
                        {
                            attributeDictionary.Add(customAttribute.Key, customAttribute);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SceelixDomain.Logger.Log(string.Format("Error while registering class '{0}' marked with ProcessorAttribute. Error {1}.", type.Name, ex));
                    //Debug.LogError();
                }

            return attributeDictionary.ToDictionary(x => x.Key, y => new KeyValuePair<TAttribute, TObject>(y.Value, (TObject) Activator.CreateInstance(y.Value.TargetType)));
        }



        public Dictionary<string, T> GetInstancesOfType<T>() //String context = null
        {
            return GetAttributeAndClassesOfType<T>().ToDictionary(x => x.Key, y => y.Value.Value); //context
        }
    }
}