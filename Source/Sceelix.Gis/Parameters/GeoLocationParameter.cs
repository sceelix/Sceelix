using Sceelix.Core.Parameters;
using Sceelix.Gis.Data;

namespace Sceelix.Gis.Parameters
{
    public class GeoLocationParameter : CompoundParameter
    {
        private readonly DoubleParameter _parameterLat = new DoubleParameter("Latitude", 0); //{ DecimalDigits = 6 };
        private readonly DoubleParameter _parameterLong = new DoubleParameter("Longitude", 0); //{ DecimalDigits = 6 };



        public GeoLocationParameter(string label) : base(label)
        {
        }



        public GeoLocation Value => new GeoLocation(_parameterLat.Value, _parameterLong.Value);
    }
}