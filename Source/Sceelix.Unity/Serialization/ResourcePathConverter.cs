using System;
using Newtonsoft.Json;
using Sceelix.Core.Environments;
using Sceelix.Core.Resources;
using Sceelix.Meshes.Materials;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Helpers;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(ResourcePath))]
    public class ResourcePathConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //not used
            throw new NotImplementedException();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //not used
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resourcePath = (ResourcePath) value;
            var name = SerializationHelper.ToUnityPath(resourcePath.Path);

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(name);

            var resourceReference = "ResourcePath_" + resourcePath.Path;

            if (!serializer.HasReference(resourceReference))
            {
                var procedureEnvironment = serializer.GetService<IProcedureEnvironment>();
                var resources = procedureEnvironment.GetService<IResourceManager>();

                //write byte array
                writer.WritePropertyName("Content");
                var bytes = resources.LoadBinary(resourcePath.Path);
                writer.WriteValue(bytes);

                //add the reference now
                serializer.AddObject(resourceReference, resourcePath);
            }

            writer.WriteEndObject();
        }
    }
}