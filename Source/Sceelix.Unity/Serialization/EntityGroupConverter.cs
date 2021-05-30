using System;
using Newtonsoft.Json;
using Sceelix.Core.Data;
using Sceelix.Extensions;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(IEntityGroup))]
    public class EntityGroupConverter : JsonConverter
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
            IEntityGroup entityGroup = (IEntityGroup) value;

            writer.WriteStartObject();
            {
                writer.WritePropertyName("EntityType");
                writer.WriteValue("EntityGroup");

                writer.WritePropertyName("GroupType");
                writer.WriteValue(entityGroup.GetType().Name.ToSplitCamelCase());

                EntityConverter.SerializeCommonUnityAttributes(writer, entityGroup, serializer);

                /*var nameValue = entityGroup.GetAttributeWithMeta<UnityMeta>(_nameAttribute);
                if (nameValue != null)
                {
                    writer.WritePropertyName("Name");
                    writer.WriteValue(nameValue.ToString());
                }


                var entry = entityGroup.Attributes.TryGetEntry<AttributeKey>(_nameAttribute);
                if (entry.HasValue && entry.Value.Key.HasMeta<UnityMeta>())
                {
                    writer.WritePropertyName("Name");
                    writer.WriteValue(entry.Value.Value.ToString());
                }*/

                //entityGroup.GetAttributeWithMeta("Name", _nameAttribute, typeof(UnityMeta));
                //writer.WritePropertyName("Name");

                writer.WritePropertyName("SubEntities");
                writer.WriteStartArray();
                foreach (IEntity subEntity in entityGroup.SubEntities)
                    serializer.Serialize(writer, subEntity);

                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}