using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Sceelix.Conversion;

namespace Sceelix.Designer.Utils
{
    /// <summary>
    /// Provides the means to load embedded resources from assemblies.
    /// Converts the loaded stream to the requested types through the conversion helper.
    /// </summary>
    /// <seealso cref="Sceelix.Conversion.ConvertHelper" />
    public static class EmbeddedResources
    {
        private static readonly Dictionary<String, Object> LoadedResources = new Dictionary<String, Object>();



        /// <summary>
        /// Loads the specified resource from the calling assembly and converts it to the indicated type.
        /// The resource should have been marked as "Embedded Resource" in the compilation options.
        /// A conversion from "Stream" to T should have been registered at the ConvertHelper.
        /// </summary>
        /// <param name="type">Type to convert the resource to.</param>
        /// <param name="resourceName">Path to the resource inside the assembly (e.g. MyResources/MyImage.png).</param>
        /// <param name="allowCache">If set to <c>true</c>, it will attempt to load a cached resource. If false, a new object will be loaded.</param>
        /// <returns>The loaded resource.</returns>
        /// <exception cref="ArgumentException">If no conversion from stream to the requested type has been defined. </exception>
        /// <exception cref="FileNotFoundException">If the requested resource location does not exist. </exception>
        public static Object Load(Type type, String resourceName, bool allowCache = true)
        {
            return Load(type, resourceName, Assembly.GetCallingAssembly(), allowCache);
        }



        /// <summary>
        /// Loads the specified resource from the calling assembly and converts it to the indicated type.
        /// The resource should have been marked as "Embedded Resource" in the compilation options.
        /// A conversion from "Stream" to T should have been registered at the ConvertHelper.
        /// </summary>
        /// <typeparam name="T">Type to convert the resource to.</typeparam>
        /// <param name="resourceName">Path to the resource inside the assembly (e.g. MyResources/MyImage.png).</param>
        /// <param name="allowCache">If set to <c>true</c>, it will attempt to load a cached resource. If false, a new object will be loaded.</param>
        /// <returns>The loaded resource.</returns>
        /// <exception cref="ArgumentException">If no conversion from stream to T has been defined. </exception>
        /// <exception cref="FileNotFoundException">If the requested resource location does not exist. </exception>
        public static T Load<T>(String resourceName, bool allowCache = true)
        {
            return (T) Load(typeof(T), resourceName, Assembly.GetCallingAssembly(), allowCache);
        }



        /// <summary>
        /// Loads the specified resource from the indicated assembly and converts it to the indicated type.
        /// The resource should have been marked as "Embedded Resource" in the compilation options.
        /// A conversion from "Stream" to T should have been registered at the ConvertHelper.
        /// </summary>
        /// <typeparam name="T">Type to convert the resource to.</typeparam>
        /// <param name="resourceName">Path to the resource inside the assembly (e.g. MyResources/MyImage.png).</param>
        /// <param name="allowCache">If set to <c>true</c>, it will attempt to load a cached resource. If false, a new object will be loaded.</param>
        /// <returns>The resource </returns>
        /// <exception cref="ArgumentException">If no conversion from stream to T has been defined. </exception>
        /// <exception cref="FileNotFoundException">If the requested resource location does not exist. </exception>
        public static T Load<T>(String resourceName, Assembly assembly, bool allowCache = true)
        {
            return (T) Load(typeof(T), resourceName, assembly, allowCache);
        }



        /// <summary>
        /// Loads the specified resource from the indicated assembly and converts it to the indicated type.
        /// The resource should have been marked as "Embedded Resource" in the compilation options.
        /// A conversion from "Stream" to the given type should have been registered at the ConvertHelper.
        /// </summary>
        /// <param name="type">Type to convert the resource to.</param>
        /// <param name="resourceName">Path to the resource inside the assembly (e.g. MyResources/MyImage.png).</param>
        /// <param name="allowCache">If set to <c>true</c>, it will attempt to load a cached resource. If false, a new object will be loaded.</param>
        /// <returns>The resource </returns>
        /// <exception cref="ArgumentException">If no conversion from stream to the requested type has been defined. </exception>
        /// <exception cref="FileNotFoundException">If the requested resource location does not exist. </exception>
        public static Object Load(Type type, String resourceName, Assembly assembly, bool allowCache = true)
        {
            String resourcePath = assembly.GetName().Name + "." + resourceName.Replace("/", ".");

            Object value;

            if (!allowCache || !LoadedResources.TryGetValue(resourcePath, out value))
            {
                if (ConvertHelper.CanConvert(typeof(Stream), type))
                {
                    //load the stream, if possible
                    //convert it to the intended type, as registered in the ConvertHelper
                    Stream stream = assembly.GetManifestResourceStream(resourcePath);
                    if (stream == null)
                        throw new FileNotFoundException("Resource '" + resourceName + "' was not found. Please make sure that you have included the file in the correct assembly and it is marked as 'Embedded Resource'.");

                    value = ConvertHelper.Convert(stream, type);

                    //if the type we want to return is not a stream, then we have converted our value and therefore we can dispose the original one
                    if (!(value is Stream || value is Bitmap))
                        stream.Dispose();

                    //if this value type is cacheable, store it to the dictionary
                    if (allowCache)
                        LoadedResources.Add(resourcePath, value);
                }
                else
                {
                    throw new ArgumentException("There is no registered conversion from stream to this kind of content type.");
                }
            }

            return value;
        }
    }
}