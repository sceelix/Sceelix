using System;
using Newtonsoft.Json;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Helpers;

namespace Sceelix.Unity.Serialization.SurfaceMaterials
{
    [UnityJsonConverter(typeof(ResourceContent))]
    public class ResourceContentConverter : JsonConverter
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
            var resourceContent = (ResourceContent) value;
            var name = SerializationHelper.ToUnityPath(resourceContent.Name);

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(name);

            var resourceContentReference = "ResourceContent_" + resourceContent.Name;

            if (!serializer.HasReference(resourceContentReference))
            {
                //write byte array
                writer.WritePropertyName("Content");
                writer.WriteValue(resourceContent.Content);

                //add the reference now
                serializer.AddObject(resourceContentReference, resourceContent);
            }

            writer.WriteEndObject();
        }
    }
}