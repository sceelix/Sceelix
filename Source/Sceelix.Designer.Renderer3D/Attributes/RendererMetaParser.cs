using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;

namespace Sceelix.Designer.Renderer3D.Attributes
{
    [MetaManager("Renderer")]
    public class RendererMetaParser : IMetaParser
    {
        public object Parse(string metaToken, string[] args)
        {
            return new RendererMeta();
        }
    }
}
