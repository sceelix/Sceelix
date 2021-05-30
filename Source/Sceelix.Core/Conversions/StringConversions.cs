using System;
using System.Globalization;
using Sceelix.Conversion;
using Sceelix.Core.Graphs;
using Sceelix.Resolution;

namespace Sceelix.Core.Conversions
{
    [ConversionFunctions]
    public class StringConversions
    {
        public static Guid StringToGuid(string str)
        {
            return new Guid(str);
        }



        public static Point StringToPoint(string str)
        {
            return Point.Parse(str, CultureInfo.InvariantCulture);
        }



        public static Type StringToType(string str)
        {
            var type = Type.GetType(str, false);
            if (type == null)
                type = TypeResolverManager.Resolve(str);

            return type;
        }
    }
}