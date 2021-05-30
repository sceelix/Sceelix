using System;

namespace Sceelix.Mathematics.Helpers
{
    /// <summary>
    /// Easing curve collection. All functions expect a value in range [0,1] and output a value in the same range.
    /// Originally found in: https://github.com/tweenjs/tween.js and converted to C#.
    /// </summary>
    public static class CurvesHelper
    {
        public static double Linear(double k)
        {
            return k;
        }



        public static class Quadratic
        {
            public static double In(double k)
            {
                return k * k;
            }



            public static double InOut(double k)
            {
                if ((k *= 2) < 1) return 0.5 * k * k;

                return -0.5 * (--k * (k - 2) - 1);
            }



            public static double Out(double k)
            {
                return k * (2 - k);
            }
        }


        public static class Cubic
        {
            public static double In(double k)
            {
                return k * k * k;
            }



            public static double InOut(double k)
            {
                if ((k *= 2) < 1) return 0.5 * k * k * k;

                return 0.5 * ((k -= 2) * k * k + 2);
            }



            public static double Out(double k)
            {
                return --k * k * k + 1;
            }
        }

        public static class Quartic
        {
            public static double In(double k)
            {
                return k * k * k * k;
            }



            public static double InOut(double k)
            {
                if ((k *= 2) < 1) return 0.5 * k * k * k * k;

                return -0.5 * ((k -= 2) * k * k * k - 2);
            }



            public static double Out(double k)
            {
                return 1 - --k * k * k * k;
            }
        }

        public static class Quintic
        {
            public static double In(double k)
            {
                return k * k * k * k * k;
            }



            public static double InOut(double k)
            {
                if ((k *= 2) < 1) return 0.5 * k * k * k * k * k;

                return 0.5 * ((k -= 2) * k * k * k * k + 2);
            }



            public static double Out(double k)
            {
                return --k * k * k * k * k + 1;
            }
        }

        public static class Sinusoidal
        {
            public static double In(double k)
            {
                return 1 - Math.Cos(k * Math.PI / 2);
            }



            public static double InOut(double k)
            {
                return 0.5 * (1 - Math.Cos(Math.PI * k));
            }



            public static double Out(double k)
            {
                return Math.Sin(k * Math.PI / 2);
            }
        }


        public static class Exponential
        {
            public static double In(double k)
            {
                return k == 0 ? 0 : Math.Pow(1024, k - 1);
            }



            public static double InOut(double k)
            {
                if (k == 0) return 0;

                if (k == 1) return 1;

                if ((k *= 2) < 1) return 0.5 * Math.Pow(1024, k - 1);

                return 0.5 * (-Math.Pow(2, -10 * (k - 1)) + 2);
            }



            public static double Out(double k)
            {
                return k == 1 ? 1 : 1 - Math.Pow(2, -10 * k);
            }
        }

        public static class Circular
        {
            public static double In(double k)
            {
                return 1 - Math.Sqrt(1 - k * k);
            }



            public static double InOut(double k)
            {
                if ((k *= 2) < 1) return -0.5 * (Math.Sqrt(1 - k * k) - 1);

                return 0.5 * (Math.Sqrt(1 - (k -= 2) * k) + 1);
            }



            public static double Out(double k)
            {
                return Math.Sqrt(1 - --k * k);
            }
        }


        public static class Elastic
        {
            public static double In(double k)
            {
                if (k == 0) return 0;

                if (k == 1) return 1;

                return -Math.Pow(2, 10 * (k - 1)) * Math.Sin((k - 1.1) * 5 * Math.PI);
            }



            public static double InOut(double k)
            {
                if (k == 0) return 0;

                if (k == 1) return 1;

                k *= 2;

                if (k < 1) return -0.5 * Math.Pow(2, 10 * (k - 1)) * Math.Sin((k - 1.1) * 5 * Math.PI);

                return 0.5 * Math.Pow(2, -10 * (k - 1)) * Math.Sin((k - 1.1) * 5 * Math.PI) + 1;
            }



            public static double Out(double k)
            {
                if (k == 0) return 0;

                if (k == 1) return 1;

                return Math.Pow(2, -10 * k) * Math.Sin((k - 0.1) * 5 * Math.PI) + 1;
            }
        }


        public static class Back
        {
            public static double In(double k)
            {
                var s = 1.70158;

                return k * k * ((s + 1) * k - s);
            }



            public static double InOut(double k)
            {
                var s = 1.70158 * 1.525;

                if ((k *= 2) < 1) return 0.5 * (k * k * ((s + 1) * k - s));

                return 0.5 * ((k -= 2) * k * ((s + 1) * k + s) + 2);
            }



            public static double Out(double k)
            {
                var s = 1.70158;

                return --k * k * ((s + 1) * k + s) + 1;
            }
        }


        public static class Bounce
        {
            public static double In(double k)
            {
                return 1 - Out(1 - k);
            }



            public static double InOut(double k)
            {
                if (k < 0.5) return In(k * 2) * 0.5;

                return Out(k * 2 - 1) * 0.5 + 0.5;
            }



            public static double Out(double k)
            {
                if (k < 1 / 2.75)
                    return 7.5625 * k * k;
                if (k < 2 / 2.75)
                    return 7.5625 * (k -= 1.5 / 2.75) * k + 0.75;
                if (k < 2.5 / 2.75)
                    return 7.5625 * (k -= 2.25 / 2.75) * k + 0.9375;
                return 7.5625 * (k -= 2.625 / 2.75) * k + 0.984375;
            }
        }
    }
}