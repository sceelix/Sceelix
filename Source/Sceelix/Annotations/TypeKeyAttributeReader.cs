using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Extensions;
using Sceelix.Loading;
using Sceelix.Logging;

namespace Sceelix.Annotations
{
    public class TypeKeyAttributeReader<TAttribute> : AttributeReader where TAttribute : TypeKeyAttribute
    {
        public TypeKeyAttributeReader(IEnumerable<Assembly> assemblies)
            : base(assemblies)
        {
        }



        /// <summary>
        /// Looks for FunctionProcessorAttributes defined in a set of assemblies.
        /// </summary>
        /// <typeparam name="TObject">Type of FunctionProcessorAttribute to look for.</typeparam>
        /// <param name="context"></param>
        /// <returns>Dictionary with the found FunctionProcessorAttribute instances as values, as their Key as keys.</returns>
        public Dictionary<Type, KeyValuePair<TAttribute, TObject>> GetAttributeAndClassesOfType<TObject>() //String context = null
        {
            var allAssemblyTypes = Assemblies.SelectMany(x => x.GetTypes()).ToList();

            Dictionary<Type, TAttribute> attributeDictionary = new Dictionary<Type, TAttribute>();
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
                        if (attributeDictionary.TryGetValue(customAttribute.TypeKey, out existingAttribute))
                        {
                            if (customAttribute.Priority == existingAttribute.Priority)
                                SceelixDomain.Logger.Log(string.Format("Did not register class '{0}' marked with ProcessorAttribute. A method with the same priority is already defined.", type.Name), LogType.Error);
                            //Debug.LogWarning(String.Format("Did not register class '{0}' marked with ProcessorAttribute. A method with the same priority is already defined.", type.Name));
                            else if (customAttribute.Priority > existingAttribute.Priority) attributeDictionary[customAttribute.TypeKey] = customAttribute;
                        }
                        else
                        {
                            attributeDictionary.Add(customAttribute.TypeKey, customAttribute);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SceelixDomain.Logger.Log(new Exception(string.Format("Error while registering class '{0}' marked with TypeKeyAttribute.", type.Name), ex));
                    //Debug.Listeners  .LogError();
                }

            Dictionary<Type, KeyValuePair<TAttribute, TObject>> instanceDictionary = attributeDictionary.ToDictionary(x => x.Key, y => new KeyValuePair<TAttribute, TObject>(y.Value, (TObject) Activator.CreateInstance(y.Value.TargetType)));

            //for those attributes that want subclasses to be included
            foreach (var instanceKeyValue in instanceDictionary.Values.Where(x => x.Key.IncludeSubclasses).ToList())
            {
                TypeKeyAttribute typeKeyAttribute = instanceKeyValue.Key;

                foreach (var subType in allAssemblyTypes.Where(x => x != typeKeyAttribute.TypeKey && typeKeyAttribute.TypeKey.IsAssignableFrom(x)))
                {
                    var instanceKeyValueToApply = SearchIndirectHandlers(subType, instanceDictionary);
                    if (instanceKeyValueToApply.HasValue)
                        instanceDictionary[subType] = instanceKeyValueToApply.Value;

                    /*var currentType = subType;
                    
                    var instanceKeyValueToApply = instanceKeyValue;
                    
                    do
                    {
                        //first, check if the type has a direct handler
                        //if not, check its direct interfaces
                        //if not, check its parent classes
                        KeyValuePair<TAttribute, TObject> newInstance;
                        if (instanceDictionary.TryGetValue(currentType, out newInstance))
                        {
                            instanceKeyValueToApply = newInstance;
                            break;
                        }

                        //get only the direct interfaces
                        var interfaces = currentType.GetInterfaces(false);
                        var foundInterface = false;
                        foreach (var interfaceType in interfaces)
                        {
                            if (instanceDictionary.TryGetValue(interfaceType, out newInstance))
                            {
                                instanceKeyValueToApply = newInstance;
                                foundInterface = true;
                                break;
                            }
                        }

                        if (foundInterface)
                            break;

                    } while ((currentType = currentType.BaseType) != null);
                    
                    instanceDictionary[subType] = instanceKeyValueToApply;*/
                }
            }

            return instanceDictionary;
        }



        public Dictionary<Type, T> GetInstancesOfType<T>() //String context = null
        {
            return GetAttributeAndClassesOfType<T>().ToDictionary(x => x.Key, y => y.Value.Value); //context
        }



        private KeyValuePair<TAttribute, TObject>? SearchIndirectHandlers<TObject>(Type currentType, Dictionary<Type, KeyValuePair<TAttribute, TObject>> instanceDictionary)
        {
            do
            {
                //first, check if the type has a direct handler
                //if not, check its direct interfaces
                //if not, check its parent classes
                KeyValuePair<TAttribute, TObject> newInstance;
                if (instanceDictionary.TryGetValue(currentType, out newInstance)) return newInstance;

                //get only the direct interfaces
                var interfaces = currentType.GetInterfaces(false);
                foreach (var interfaceType in interfaces)
                    if (instanceDictionary.TryGetValue(interfaceType, out newInstance))
                        return newInstance;
            } while ((currentType = currentType.BaseType) != null);

            return null;
        }
    }
}