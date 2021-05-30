using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;

namespace Sceelix.Unity.Attributes
{
    [MetaManager("Unity")]
    public class UnityMetaParser : IMetaParser
    {
        public object Parse(string metaToken, string[] args)
        {
            return new UnityMeta();
        }
    }
}