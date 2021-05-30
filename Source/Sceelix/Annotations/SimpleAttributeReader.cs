using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Annotations
{
    public class SimpleAttributeReader<TAttribute> : AttributeReader where TAttribute : Attribute
    {
        public SimpleAttributeReader(IEnumerable<Assembly> assemblies)
            : base(assemblies)
        {
        }



        /// <summary>
        /// Looks for FunctionProcessorAttributes defined in a set of assemblies.
        /// </summary>
        /// <typeparam name="TObject">Type of FunctionProcessorAttribute to look for.</typeparam>
        /// <returns>Dictionary with the found FunctionProcessorAttribute instances as values, as their Key as keys.</returns>
        public List<KeyValuePair<TAttribute, TObject>> GetAttributeAndClassesOfType<TObject>()
        {
            var allAssemblyTypes = Assemblies.SelectMany(x => x.GetTypes()).ToList();

            List<KeyValuePair<TAttribute, TObject>> attributeList = new List<KeyValuePair<TAttribute, TObject>>();

            foreach (var type in allAssemblyTypes.Where(x => typeof(TObject).IsAssignableFrom(x)))
                try
                {
                    var customAttribute = type.GetCustomAttribute<TAttribute>();
                    if (customAttribute != null)
                    {
                        if (type.IsAbstract)
                            continue;

                        attributeList.Add(new KeyValuePair<TAttribute, TObject>(customAttribute, (TObject) Activator.CreateInstance(type)));
                    }
                }
                catch (Exception ex)
                {
                    SceelixDomain.Logger.Log(string.Format("Error while registering class '{0}' marked with ProcessorAttribute. Error {1}.", type.Name, ex));
                }

            return attributeList;
        }



        public List<T> GetInstancesOfType<T>()
        {
            return GetAttributeAndClassesOfType<T>().Select(y => y.Value).ToList();
        }
    }
}