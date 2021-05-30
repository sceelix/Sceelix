namespace Sceelix.Annotations
{
    public class JsonContractResolver : StringKeyAttribute
    {
        public JsonContractResolver(string key)
            : base(key)
        {
        }
    }
}