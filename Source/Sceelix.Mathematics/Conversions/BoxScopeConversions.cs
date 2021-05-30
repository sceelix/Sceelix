using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Conversions
{
    [ConversionFunctions]
    public class BoxScopeConversions
    {
        public static SceeList BoxScopeToSceelist(BoxScope boxScope)
        {
            return new SceeList(
                new KeyValuePair<string, object>("XAxis", boxScope.XAxis),
                new KeyValuePair<string, object>("YAxis", boxScope.YAxis),
                new KeyValuePair<string, object>("ZAxis", boxScope.ZAxis),
                new KeyValuePair<string, object>("Translation", boxScope.Translation),
                new KeyValuePair<string, object>("Sizes", boxScope.Sizes));
        }
    }
}