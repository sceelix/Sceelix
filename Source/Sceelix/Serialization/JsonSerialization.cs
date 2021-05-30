using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Sceelix.Serialization
{
    public class JsonSerialization
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings {PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace, ContractResolver = new MyContractResolver()};



        /// <summary>
        /// Loads an object from a json file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="path">Path to the file.</param>
        /// <returns>Object of the indicated type.</returns>
        public static T LoadFromFile<T>(string path)
        {
            string jsonText = File.ReadAllText(path);

            //var settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = typeNameHandling };

            return JsonConvert.DeserializeObject<T>(jsonText, settings);
        }



        /// <summary>
        /// Loads an object from a json stream.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="stream">Data stream.</param>
        /// <returns>Object of the indicated type.</returns>
        public static T LoadFromStream<T>(Stream stream)
        {
            string jsonText = new StreamReader(stream).ReadToEnd();

            //var settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = typeNameHandling };

            return JsonConvert.DeserializeObject<T>(jsonText, settings);
        }



        /// <summary>
        /// Saves an object to a json file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="path">Path of the file.</param>
        /// <param name="obj">Object to be saved.</param>
        public static void SaveToFile<T>(string path, T obj)
        {
            //var settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = typeNameHandling };

            string json = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllText(path, json);
        }



        public class MyContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(f => base.CreateProperty(f, memberSerialization))
                    .ToList();
                props.ForEach(p =>
                {
                    p.Writable = true;
                    p.Readable = true;
                });
                return props;
            }
        }
    }
}