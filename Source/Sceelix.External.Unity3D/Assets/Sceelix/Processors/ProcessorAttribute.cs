using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.Sceelix.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcessorAttribute : Attribute
    {
        private readonly string _key;
        private readonly int _priority;
        
        private Type _type;


        public ProcessorAttribute(String key, int priority = 0)
        {
            _key = key;
            _priority = priority;
        }

        public string Key
        {
            get { return _key; }
        }

        public int Priority
        {
            get { return _priority; }
        }

        public Type Type
        {
            get { return _type; }
            private set { _type = value; }
        }

        /// <summary>
        /// Looks for FunctionProcessorAttributes defined in a set of assemblies.
        /// </summary>
        /// <typeparam name="T">Type of FunctionProcessorAttribute to look for.</typeparam>
        /// <param name="assemblies">(Optional)The assemblies where to look for the type. If none is defined, the current executing assembly is used.</param>
        /// <returns>Dictionary with the found FunctionProcessorAttribute instaces as values, as their Key as keys.</returns>
        public static Dictionary<String, T> GetClassesOfType<T>(params Assembly[] assemblies)
        {
            return GetClassesOfType<T>((IEnumerable<Assembly>)assemblies);
        }



        /// <summary>
        /// Looks for FunctionProcessorAttributes defined in a set of assemblies.
        /// </summary>
        /// <typeparam name="T">Type of FunctionProcessorAttribute to look for.</typeparam>
        /// <param name="assemblies">(Optional)The assemblies where to look for the type. If none is defined, the current executing assembly is used.</param>
        /// <returns>Dictionary with the found FunctionProcessorAttribute instaces as values, as their Key as keys.</returns>
        public static Dictionary<String, T> GetClassesOfType<T>(IEnumerable<Assembly> assemblies) 
        {
            //if the input is null, assume the executing assembly
            assemblies = assemblies.ToArray();
            if (!assemblies.Any())
                assemblies = new[] { Assembly.GetExecutingAssembly() };

            Dictionary<String, ProcessorAttribute> attributeDictionary = new Dictionary<string, ProcessorAttribute>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => typeof(T).IsAssignableFrom(x) ))
                {
                    try
                    {
                        var customAttribute = type.GetCustomAttributes(typeof (ProcessorAttribute), false).FirstOrDefault() as ProcessorAttribute;
                        if (customAttribute != null)
                        {
                            if (type.IsAbstract)
                            {
                                Debug.LogError(String.Format("Class '{0}' is marked with ProcessorAttribute, but cannot be instanced.",type.Name));
                                continue;
                            }

                            customAttribute.Type = type;

                            //check if any attribute with a higher priority exists
                            //if the one existing has a lower priority, replace it with this one
                            //if, for mistake, the priority is the same, warn the user
                            //otherwise let the previous one be
                            ProcessorAttribute existingAttribute;
                            if (attributeDictionary.TryGetValue(customAttribute.Key, out existingAttribute))
                            {
                                if (customAttribute.Priority == existingAttribute.Priority)
                                {
                                    Debug.LogWarning(String.Format("Did not register class '{0}' marked with ProcessorAttribute. A method with the same priority is already defined.", type.Name));
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
                        Debug.LogError(String.Format("Error while registering class '{0}' marked with ProcessorAttribute. Error {1}.", type.Name, ex));
                    }
                }
            }


            //now that we have determined which types should prevail, we create instances
            Dictionary<String, T> objectDictionary = new Dictionary<string, T>();
            foreach (KeyValuePair<string, ProcessorAttribute> processorAttribute in attributeDictionary)
                objectDictionary[processorAttribute.Key] = (T)Activator.CreateInstance(processorAttribute.Value.Type);


            return objectDictionary;
        }
    }
}
