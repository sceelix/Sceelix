using Sceelix.Helpers;

namespace Sceelix.Unity.Helpers
{
    public class SerializationHelper
    {
        public static string ToUnityPath(string path)
        {
            return PathHelper.ToUniversalPath(path).Replace("/", ".").Replace(":", "");
        }
    }
}