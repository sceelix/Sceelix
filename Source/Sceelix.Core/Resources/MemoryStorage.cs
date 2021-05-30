using System.Collections.Generic;

namespace Sceelix.Core.Resources
{
    public class MemoryStorage
    {
        public static Dictionary<string, object> Data
        {
            get;
        } = new Dictionary<string, object>();
    }
}