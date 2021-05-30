using Sceelix.Annotations;

namespace Sceelix.Core.Resolution
{
    public class NodeResolverAttribute : StringKeyAttribute
    {
        private string _guid;



        public NodeResolverAttribute(string guid)
            : base(guid)
        {
            _guid = guid;
        }
    }
}