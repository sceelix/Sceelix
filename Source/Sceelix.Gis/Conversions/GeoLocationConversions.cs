using System.Collections.Generic;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Gis.Data;

namespace Sceelix.Gis.Conversions
{
    [ConversionFunctions]
    public class GeoLocationConversions
    {
        public static SceeList ToSceeList(GeoLocation geolocation)
        {
            return new SceeList(new KeyValuePair<string, object>("Latitude", geolocation.Latitude), new KeyValuePair<string, object>("Longitude", geolocation.Longitude));
        }



        public static string ToString(GeoLocation geolocation)
        {
            return string.Format("{0},{1}", ConvertHelper.Convert<string>(geolocation.Latitude), ConvertHelper.Convert<string>(geolocation.Longitude));
        }
    }
}