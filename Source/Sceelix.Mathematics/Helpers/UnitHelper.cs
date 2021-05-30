namespace Sceelix.Mathematics.Helpers
{
    public class UnitHelper
    {
        public double CelsiusToFahrenheit(double celsius)
        {
            return celsius * (9 / 5f) + 32;
        }



        public double CentimetersToInches(double centimeters)
        {
            return centimeters * 0.39370;
        }



        public double FahrenheitToCelsius(double fahrenheit)
        {
            return (fahrenheit - 32) * (5 / 9f);
        }



        public double FeetToMeters(double feet)
        {
            return feet / 3.2808d;
        }



        public double GramsToOunces(double grams)
        {
            return grams * 0.035274d;
        }



        public double InchesToCentimeters(double inches)
        {
            return inches / 0.39370;
        }



        public double KilogramsToLbs(double kilograms)
        {
            return kilograms * 2.2046226218488d;
        }



        public double KilometersToMiles(double kilometers)
        {
            return kilometers * 0.62137;
        }



        public double LbsToKilograms(double lbs)
        {
            return lbs / 2.2046226218488d;
        }



        public double MetersToFeet(double meters)
        {
            return meters * 3.2808d;
        }



        public double MetersToYards(double meters)
        {
            return meters / 0.91444d;
        }



        public double MilesToKilometers(double miles)
        {
            return miles / 0.62137;
        }



        public double OuncesToGrams(double ounces)
        {
            return ounces / 0.035274d;
        }



        public double YardsToMeters(double yards)
        {
            return yards * 0.91444d;
        }
    }
}