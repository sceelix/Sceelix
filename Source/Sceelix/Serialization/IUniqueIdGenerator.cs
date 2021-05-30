namespace Sceelix.Serialization
{
    internal interface IUniqueIdGenerator
    {
        string GetId(object obj);
    }
}