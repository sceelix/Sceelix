using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sceelix.Annotations;

namespace Sceelix.Serialization
{
    [JsonContractResolver("Standard")]
    public class StandardContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// The default type mapping to use.
        /// </summary>
        private static readonly Dictionary<Type, JsonConverter> DefaultTypeMapping = AttributeReader.OfTypeKeyAttribute<StandardJsonConverterAttribute>().GetInstancesOfType<JsonConverter>();



        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (DefaultTypeMapping.ContainsKey(objectType))
                contract.Converter = DefaultTypeMapping[objectType];

            return contract;
        }
    }
}