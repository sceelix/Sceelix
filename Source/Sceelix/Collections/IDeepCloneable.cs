namespace Sceelix.Collections
{
    public interface IDeepCloneable<out T>
    {
        T DeepClone();
    }
}