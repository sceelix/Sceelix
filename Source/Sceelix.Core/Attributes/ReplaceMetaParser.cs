using Sceelix.Core.Annotations;

namespace Sceelix.Core.Attributes
{
    [MetaManager("Replace")]
    public class ReplaceMetaParser : IMetaParser
    {
        public object Parse(string metaToken, string[] args)
        {
            return new ReplaceMeta();
        }
    }
}