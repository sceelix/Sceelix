namespace Sceelix.Gis.Data
{
    public struct GeoLocation
    {
        public GeoLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }



        public double Latitude
        {
            get;
        }


        public double Longitude
        {
            get;
        }



        public override string ToString()
        {
            return string.Format("Latitude: {0}, Longitude: {1}", Latitude, Longitude);
        }
    }
}