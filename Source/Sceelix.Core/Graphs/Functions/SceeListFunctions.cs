using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("List")]
    public class SceeListFunctions
    {
        public static object Avg(dynamic sceelist)
        {
            IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>) sceelist;

            var sum = enumerable.Aggregate((result, val) => result + val);

            return sum / enumerable.Count();
        }



        public static dynamic Empty(dynamic sceelist)
        {
            return sceelist.Count == 0;
        }



        public static dynamic Last(dynamic sceelist)
        {
            return sceelist.Last();
        }



        public static object Max(dynamic sceelist)
        {
            IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>) sceelist;

            return enumerable.Max(x => x.Value);
        }



        public static object Min(dynamic sceelist)
        {
            IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>) sceelist;

            return enumerable.Min(x => x.Value);
        }



        public static object OrderAsc(dynamic sceelist)
        {
            IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>) sceelist;

            return enumerable.OrderBy(x => x.Value);
        }



        public static object OrderDesc(dynamic sceelist)
        {
            IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>) sceelist;

            return enumerable.OrderByDescending(x => x.Value);
        }



        public static dynamic Reverse(dynamic sceelist)
        {
            return sceelist.Reverse();
        }



        public static dynamic Sceelist(dynamic data)
        {
            return ConvertHelper.Convert(data, typeof(SceeList));
        }



        public static dynamic Sceelist(dynamic data, dynamic recursive)
        {
            var sceelist = ConvertHelper.Convert(data, typeof(SceeList));
            if (recursive)
                SceelistRecusiveCast(sceelist);

            return sceelist;
        }



        private static void SceelistRecusiveCast(dynamic sceelist)
        {
            for (int i = 0; i < sceelist.Count; i++)
                if (ConvertHelper.CanConvert(sceelist[i].GetType(), typeof(SceeList)))
                {
                    var subSceelist = ConvertHelper.Convert(sceelist[i], typeof(SceeList));
                    sceelist[i] = subSceelist;

                    SceelistRecusiveCast(subSceelist);
                }
        }



        public static dynamic Size(dynamic sceelist)
        {
            return sceelist.Count;
        }



        public static object Sum(dynamic sceelist)
        {
            IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>) sceelist;

            return enumerable.Aggregate((result, val) => result + val);
        }
    }
}