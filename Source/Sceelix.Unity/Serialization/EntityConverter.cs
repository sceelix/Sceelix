using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.Extensions;
using Sceelix.Logging;
using Sceelix.Serialization;
using Sceelix.Unity.Annotations;
using Sceelix.Unity.Attributes;

namespace Sceelix.Unity.Serialization
{
    [UnityJsonConverter(typeof(Entity))]
    public class EntityConverter : JsonConverter
    {
        private static readonly AttributeKey[] _commonAttributeKeys =
        {
            new GlobalAttributeKey("Name"),
            new GlobalAttributeKey("Tag"),
            new GlobalAttributeKey("Layer"),
            new GlobalAttributeKey("Static"),
            new GlobalAttributeKey("Enabled")
        };



        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }



        public static void SerializeCommonUnityAttributes(JsonWriter writer, IEntity entity, JsonSerializer serializer)
        {
            WriteUnityAttributeKeys(writer, entity, _commonAttributeKeys);
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var entity = (Entity) value;

            //do not serialize those entities not expressively converted
            var environment = serializer.GetService<IProcedureEnvironment>();

            environment.GetService<ILogger>().Log(string.Format("The serialization of the entity '{0}' is not (yet) supported.", entity.GetType()), LogType.Warning);
        }



        public static void WriteUnityAttributeKeys(JsonWriter writer, IEntity entity, IEnumerable<AttributeKey> attributeKeys)
        {
            foreach (var commonAttributeKey in attributeKeys)
            {
                var attributeValue = entity.GetAttributeWithMeta<UnityMeta>(commonAttributeKey);
                if (attributeValue != null)
                {
                    writer.WritePropertyName(commonAttributeKey.Name);
                    writer.WriteValue(attributeValue);
                }
            }
        }
    }
}