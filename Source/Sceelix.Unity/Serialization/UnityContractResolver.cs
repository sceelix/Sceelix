using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sceelix.Annotations;
using Sceelix.Unity.Annotations;

namespace Sceelix.Unity.Serialization
{
    [JsonContractResolver("Unity")]
    public class UnityContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// The type mapping for unity types
        /// </summary>
        private static readonly Dictionary<Type, JsonConverter> UnityTypeMapping = AttributeReader.OfTypeKeyAttribute<UnityJsonConverterAttribute>().GetInstancesOfType<JsonConverter>();

        /// <summary>
        /// The alternative type mapping.
        /// </summary>
        private static readonly Dictionary<Type, JsonConverter> DefaultTypeMapping = AttributeReader.OfTypeKeyAttribute<StandardJsonConverterAttribute>().GetInstancesOfType<JsonConverter>();



        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (UnityTypeMapping.ContainsKey(objectType))
                contract.Converter = UnityTypeMapping[objectType];
            else if (DefaultTypeMapping.ContainsKey(objectType))
                contract.Converter = DefaultTypeMapping[objectType];

            return contract;
        }
    }
}