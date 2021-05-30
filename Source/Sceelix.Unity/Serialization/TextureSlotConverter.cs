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
    [UnityJsonConverter(typeof(TextureSlot))]
    public class TextureSlotConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var textureSlot = (TextureSlot) value;
            var name = SerializationHelper.ToUnityPath(textureSlot.Path);

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(name);

            writer.WritePropertyName("Type");
            writer.WriteValue(textureSlot.Type.ToString());

            if (!string.IsNullOrEmpty(textureSlot.Path))
            {
                var textureSlotReference = "TextureSlot_" + textureSlot.Path;

                if (!serializer.HasReference(textureSlotReference))
                {
                    var procedureEnvironment = serializer.GetService<IProcedureEnvironment>();
                    var resources = procedureEnvironment.GetService<IResourceManager>();

                    //write byte array
                    writer.WritePropertyName("Content");
                    var bytes = resources.LoadBinary(textureSlot.Path);
                    writer.WriteValue(bytes);

                    //add the reference now
                    serializer.AddObject(textureSlotReference, textureSlot);
                }
            }

            writer.WriteEndObject();
        }
    }
}