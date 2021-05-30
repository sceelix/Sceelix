using System.Collections.Generic;
using System.Linq;
using Sceelix.Annotations;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Attributes
{
    public class MetaParserManager
    {
        private static readonly Dictionary<string, IMetaParser> MetaManagers = AttributeReader.OfStringKeyAttribute<MetaManagerAttribute>().GetInstancesOfType<IMetaParser>().ToDictionary(x => x.Key.ToLower(), x => x.Value);



        public static IEnumerable<string> MetaParsers
        {
            get { return MetaManagers.Keys.Select(x => x.ToLower()); }
        }



        public static object Parse(string metaStringCommand, params string[] args)
        {
            IMetaParser metaParser;
            if (MetaManagers.TryGetValue(metaStringCommand, out metaParser))
                return metaParser.Parse(metaStringCommand, args);

            return null;
        }
    }
}