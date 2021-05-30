using System;

namespace Sceelix.Mathematics.Noise
{
    /// <summary>
    /// From the pseudocode of http://freespace.virgin.net/hugo.elias/models/m_perlin.htm
    /// </summary>
    public class PerlinNoise2D
    {
        private readonly double _amplitude;
        private readonly double _frequency;
        private readonly int _octaves;
        private readonly double _persistence;
        private readonly int _seed;



        public PerlinNoise2D(int seed, int octaves, double persistence, double frequency, double amplitude)
        {
            _seed = seed;
            _octaves = octaves;
            _persistence = persistence;
            _frequency = frequency;
            _amplitude = amplitude;
        }



        /*private double Noise(double x, double y)
        {
            return Noise((int))
        }


        private double SmoothNoise(double x, double y)
        {
            var corners = (Noise(x - 1, y - 1) + Noise(x + 1, y - 1) + Noise(x - 1, y + 1) + Noise(x + 1, y + 1)) / 16;
            var sides = (Noise(x - 1, y) + Noise(x + 1, y) + Noise(x, y - 1) + Noise(x, y + 1)) / 8;
            var center = Noise(x, y) / 4;

            return corners + sides + center;
        }*/



        private double CosineInterpolate(double a, double b, double x)
        {
            var ft = x * Math.PI;
            var f = (1 - Math.Cos(ft)) * 0.5;

            return a * (1 - f) + b * f;
        }



        private double InterpolatedNoise(double x, double y)
        {
            //if (x > 100)
            //    Console.WriteLine(x + "," + y);

            x = x % 100;
            y = y % 100;

            var integerX = (int) x;
            var fractionalX = x - integerX;

            var integerY = (int) y;
            //integerY = integerY % 25;
            var fractionalY = y - integerY;

            var v1 = Noise(integerX, integerY);
            var v2 = Noise(integerX + 1, integerY);
            var v3 = Noise(integerX, integerY + 1);
            var v4 = Noise(integerX + 1, integerY + 1);


            var i1 = CosineInterpolate(v1, v2, fractionalX);
            var i2 = CosineInterpolate(v3, v4, fractionalX);

            return CosineInterpolate(i1, i2, fractionalY);
        }



        private double Noise(int x, int y)
        {
            //x = x%100;
            //y = y % 100;
            var n = x + y * 57;
            n = (n << 13) ^ n;
            var val = 1.0 - ((n * (n * n * 15731 + 789221) + 1376312589 * _seed) & 0x7fffffff) / 1073741824.0;
            //Console.WriteLine(val);

            return val;
        }



        public double Noise2D(float x, float y)
        {
            double total = 0;

            var currentFrequency = _frequency;
            var currentAmplitude = _amplitude;

            for (int j = 0; j < _octaves; j++)
            {
                total = total + InterpolatedNoise(x * currentFrequency, y * currentFrequency) * currentAmplitude;

                currentFrequency *= 2;
                currentAmplitude *= _persistence;
            }

            return total;
        }
    }
}