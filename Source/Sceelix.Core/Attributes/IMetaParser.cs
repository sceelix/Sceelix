namespace Sceelix.Core.Attributes
{
    public interface IMetaParser
    {
        object Parse(string metaToken, string[] args);
    }
}