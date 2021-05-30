using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sceelix.Extensions;

namespace Sceelix.Mathematics.Data
{
    public struct Color
    {
        private static readonly Dictionary<string, Color> ColorNames = new Dictionary<string, Color>();



        static Color()
        {
            foreach (var fieldInfo in typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = fieldInfo.GetValue(null);
                if (value is Color)
                    ColorNames.Add(fieldInfo.Name, (Color) value);
            }
        }



        public Color(byte color)
            : this()
        {
            R = G = B = A = color;
        }



        public Color(byte r, byte g, byte b) : this()
        {
            R = r;
            G = g;
            B = b;
            A = 255;
        }



        public Color(byte r, byte g, byte b, byte a)
            : this()
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }



        public Color(byte? r, byte? g, byte? b, byte? a)
            : this()
        {
            R = r ?? 255;
            G = g ?? 255;
            B = b ?? 255;
            A = a ?? 255;
        }



        /// <summary>
        /// Creates a color from 4-sized array of 0-1 values
        /// </summary>
        /// <param name="color"></param>
        public Color(float[] color)
        {
            R = (byte) (color[0] * 255);
            G = (byte) (color[1] * 255);
            B = (byte) (color[2] * 255);
            A = (byte) (color[3] * 255);
        }



        /// <summary>
        /// Red value (0 to 255)
        /// </summary>
        public byte R
        {
            get;
        }


        /// <summary>
        /// Green Value (0 to 255)
        /// </summary>
        public byte G
        {
            get;
        }


        /// <summary>
        /// Blue Value (0 to 255)
        /// </summary>
        public byte B
        {
            get;
        }


        /// <summary>
        /// Alpha Value (0 to 255)
        /// </summary>
        public byte A
        {
            get;
        }



        public override string ToString()
        {
            //return String.Format("Red: {0}, Green: {1}, Blue: {2}, Alpha: {3}", _r, _g, _b, _a);
            return string.Format("color({0}, {1}, {2}, {3})", R, G, B, A);
        }



        public static Color operator +(Color a, Color b)
        {
            return new Color((byte) (a.R + b.R), (byte) (a.G + b.G), (byte) (a.B + b.B), (byte) (a.A + b.A));
        }



        public static Color operator -(Color a, Color b)
        {
            return new Color((byte) (a.R - b.R), (byte) (a.G - b.G), (byte) (a.B - b.B), (byte) (a.A - b.A));
        }



        public static Color operator +(Color a, float scalar)
        {
            return new Color((byte) (a.R + scalar), (byte) (a.G + scalar), (byte) (a.B + scalar), (byte) (a.A + scalar));
        }



        public static Color operator -(Color a, float scalar)
        {
            return new Color((byte) (a.R - scalar), (byte) (a.G - scalar), (byte) (a.B - scalar), (byte) (a.A - scalar));
        }



        public static Color operator *(Color a, float scalar)
        {
            return new Color((byte) (a.R * scalar), (byte) (a.G * scalar), (byte) (a.B * scalar), (byte) (a.A * scalar));
        }



        public static Color operator /(Color a, float scalar)
        {
            return new Color((byte) (a.R / scalar), (byte) (a.G / scalar), (byte) (a.B / scalar), (byte) (a.A / scalar));
        }



        public static implicit operator System.Drawing.Color(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }



        public static implicit operator Color(System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }



        /// <summary>
        /// Converts the color to an array of 0-255 byte values.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return new[] {R, G, B, A};
        }



        /// <summary>
        /// Converts the color to an array of 0-1 float values.
        /// </summary>
        /// <returns></returns>
        public float[] ToFloatArray()
        {
            return new[] {R / 255f, G / 255f, B / 255f, A / 255f};
        }



        public bool Equals(Color other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Color && Equals((Color) obj);
        }



        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                return hashCode;
            }
        }



        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }



        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }



        public static Color HsvToRgb(double h, double s, double v)
        {
            int hi = (int) Math.Floor(h / 60.0) % 6;
            double f = h / 60.0 - Math.Floor(h / 60.0);

            double p = v * (1.0 - s);
            double q = v * (1.0 - f * s);
            double t = v * (1.0 - (1.0 - f) * s);

            Color ret;

            switch (hi)
            {
                case 0:
                    ret = FromDoubles(v, t, p);
                    break;
                case 1:
                    ret = FromDoubles(q, v, p);
                    break;
                case 2:
                    ret = FromDoubles(p, v, t);
                    break;
                case 3:
                    ret = FromDoubles(p, q, v);
                    break;
                case 4:
                    ret = FromDoubles(t, p, v);
                    break;
                case 5:
                    ret = FromDoubles(v, p, q);
                    break;
                default:
                    ret = new Color(0x00, 0x00, 0x00, 0xFF);
                    break;
            }

            return ret;
        }



        /// <summary>
        /// Creates a Color from a set of 0-1 double values (assuming 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static Color FromDoubles(double r, double g, double b)
        {
            return new Color((byte) (r * 255.0), (byte) (g * 255.0), (byte) (b * 255.0), 255);
        }



        /// <summary>
        /// Creates a color from a hexadecimal string, with or without the #(ex. #5972FF or 5972FF).
        /// </summary>
        /// <param name="hexString">Hexadecimal string, with or without the #</param>
        /// <returns>Color corresponding to the hexstring.</returns>
        public static Color FromHex(string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Replace("#", "");

            var components = hexString.SplitSize(2).ToArray();

            var red = (byte) int.Parse(components[0], NumberStyles.HexNumber);
            var green = (byte) int.Parse(components[1], NumberStyles.HexNumber);
            var blue = (byte) int.Parse(components[2], NumberStyles.HexNumber);

            return new Color(red, green, blue, 255);
        }



        //public static Color Invisible = new Color(0, 0, 0, 0);
        public static Color AliceBlue = new Color(240, 248, 255, 255);
        public static Color AntiqueWhite = new Color(250, 235, 215, 255);
        public static Color Aqua = new Color(0, 255, 255, 255);
        public static Color Aquamarine = new Color(127, 255, 212, 255);
        public static Color Azure = new Color(240, 255, 255, 255);
        public static Color Beige = new Color(245, 245, 220, 255);
        public static Color Bisque = new Color(255, 228, 196, 255);
        public static Color Black = new Color(0, 0, 0, 255);
        public static Color BlanchedAlmond = new Color(255, 255, 205, 255);
        public static Color Blue = new Color(0, 0, 255, 255);
        public static Color BlueViolet = new Color(138, 43, 226, 255);
        public static Color Brown = new Color(165, 42, 42, 255);
        public static Color BurlyWood = new Color(222, 184, 135, 255);
        public static Color CadetBlue = new Color(95, 158, 160, 255);
        public static Color Chartreuse = new Color(127, 255, 0, 255);
        public static Color Chocolate = new Color(210, 105, 30, 255);
        public static Color Coral = new Color(255, 127, 80, 255);
        public static Color CornflowerBlue = new Color(100, 149, 237, 255);
        public static Color Cornsilk = new Color(255, 248, 220, 255);
        public static Color Crimson = new Color(220, 20, 60, 255);
        public static Color Cyan = new Color(0, 255, 255, 255);
        public static Color DarkBlue = new Color(0, 0, 139, 255);
        public static Color DarkCyan = new Color(0, 139, 139, 255);
        public static Color DarkGoldenrod = new Color(184, 134, 11, 255);
        public static Color DarkGray = new Color(169, 169, 169, 255);
        public static Color DarkGreen = new Color(0, 100, 0, 255);
        public static Color DarkKhaki = new Color(189, 183, 107, 255);
        public static Color DarkMagena = new Color(139, 0, 139, 255);
        public static Color DarkOliveGreen = new Color(85, 107, 47, 255);
        public static Color DarkOrange = new Color(255, 140, 0, 255);
        public static Color DarkOrchid = new Color(153, 50, 204, 255);
        public static Color DarkRed = new Color(139, 0, 0, 255);
        public static Color DarkSalmon = new Color(233, 150, 122, 255);
        public static Color DarkSeaGreen = new Color(143, 188, 143, 255);
        public static Color DarkSlateBlue = new Color(72, 61, 139, 255);
        public static Color DarkSlateGray = new Color(40, 79, 79, 255);
        public static Color DarkTurquoise = new Color(0, 206, 209, 255);
        public static Color DarkViolet = new Color(148, 0, 211, 255);
        public static Color DeepPink = new Color(255, 20, 147, 255);
        public static Color DeepSkyBlue = new Color(0, 191, 255, 255);
        public static Color DimGray = new Color(105, 105, 105, 255);
        public static Color DodgerBlue = new Color(30, 144, 255, 255);
        public static Color Firebrick = new Color(178, 34, 34, 255);
        public static Color FloralWhite = new Color(255, 250, 240, 255);
        public static Color ForestGreen = new Color(34, 139, 34, 255);
        public static Color Fuschia = new Color(255, 0, 255, 255);
        public static Color Gainsboro = new Color(220, 220, 220, 255);
        public static Color GhostWhite = new Color(248, 248, 255, 255);
        public static Color Gold = new Color(255, 215, 0, 255);
        public static Color Goldenrod = new Color(218, 165, 32, 255);
        public static Color Gray = new Color(128, 128, 128, 255);
        public static Color Green = new Color(0, 128, 0, 255);
        public static Color GreenYellow = new Color(173, 255, 47, 255);
        public static Color Honeydew = new Color(240, 255, 240, 255);
        public static Color HotPink = new Color(255, 105, 180, 255);
        public static Color IndianRed = new Color(205, 92, 92, 255);
        public static Color Indigo = new Color(75, 0, 130, 255);
        public static Color Invisible = new Color(0, 0, 0, 255);
        public static Color Ivory = new Color(255, 240, 240, 255);
        public static Color Khaki = new Color(240, 230, 140, 255);
        public static Color Lavender = new Color(230, 230, 250, 255);
        public static Color LavenderBlush = new Color(255, 240, 245, 255);
        public static Color LawnGreen = new Color(124, 252, 0, 255);
        public static Color LemonChiffon = new Color(255, 250, 205, 255);
        public static Color LightBlue = new Color(173, 216, 230, 255);
        public static Color LightCoral = new Color(240, 128, 128, 255);
        public static Color LightCyan = new Color(224, 255, 255, 255);
        public static Color LightGoldenrodYellow = new Color(250, 250, 210, 255);
        public static Color LightGray = new Color(211, 211, 211, 255);
        public static Color LightGreen = new Color(144, 238, 144, 255);
        public static Color LightPink = new Color(255, 182, 193, 255);
        public static Color LightSalmon = new Color(255, 160, 122, 255);
        public static Color LightSeaGreen = new Color(32, 178, 170, 255);
        public static Color LightSkyBlue = new Color(135, 206, 250, 255);
        public static Color LightSlateGray = new Color(119, 136, 153, 255);
        public static Color LightSteelBlue = new Color(176, 196, 222, 255);
        public static Color LightYellow = new Color(255, 255, 224, 255);
        public static Color Lime = new Color(0, 255, 0, 255);
        public static Color LimeGreen = new Color(50, 205, 50, 255);
        public static Color Linen = new Color(250, 240, 230, 255);
        public static Color Magenta = new Color(255, 0, 255, 255);
        public static Color Maroon = new Color(128, 0, 0, 255);
        public static Color MediumAquamarine = new Color(102, 205, 170, 255);
        public static Color MediumBlue = new Color(0, 0, 205, 255);
        public static Color MediumOrchid = new Color(186, 85, 211, 255);
        public static Color MediumPurple = new Color(147, 112, 219, 255);
        public static Color MediumSeaGreen = new Color(60, 179, 113, 255);
        public static Color MediumSlateBlue = new Color(123, 104, 238, 255);
        public static Color MediumSpringGreen = new Color(0, 250, 154, 255);
        public static Color MediumTurquoise = new Color(72, 209, 204, 255);
        public static Color MediumVioletRed = new Color(199, 21, 112, 255);
        public static Color MidnightBlue = new Color(25, 25, 112, 255);
        public static Color MintCream = new Color(245, 255, 250, 255);
        public static Color MistyRose = new Color(255, 228, 225, 255);
        public static Color Moccasin = new Color(255, 228, 181, 255);
        public static Color NavajoWhite = new Color(255, 222, 173, 255);
        public static Color Navy = new Color(0, 0, 128, 255);
        public static Color OldLace = new Color(253, 245, 230, 255);
        public static Color Olive = new Color(128, 128, 0, 255);
        public static Color OliveDrab = new Color(107, 142, 45, 255);
        public static Color Orange = new Color(255, 165, 0, 255);
        public static Color OrangeRed = new Color(255, 69, 0, 255);
        public static Color Orchid = new Color(218, 112, 214, 255);
        public static Color PaleGoldenrod = new Color(238, 232, 170, 255);
        public static Color PaleGreen = new Color(152, 251, 152, 255);
        public static Color PaleTurquoise = new Color(175, 238, 238, 255);
        public static Color PaleVioletRed = new Color(219, 112, 147, 255);
        public static Color PapayaWhip = new Color(255, 239, 213, 255);
        public static Color PeachPuff = new Color(255, 218, 155, 255);
        public static Color Peru = new Color(205, 133, 63, 255);
        public static Color Pink = new Color(255, 192, 203, 255);
        public static Color Plum = new Color(221, 160, 221, 255);
        public static Color PowderBlue = new Color(176, 224, 230, 255);
        public static Color Purple = new Color(128, 0, 128, 255);
        public static Color Red = new Color(255, 0, 0, 255);
        public static Color RosyBrown = new Color(188, 143, 143, 255);
        public static Color RoyalBlue = new Color(65, 105, 225, 255);
        public static Color SaddleBrown = new Color(139, 69, 19, 255);
        public static Color Salmon = new Color(250, 128, 114, 255);
        public static Color SandyBrown = new Color(244, 164, 96, 255);
        public static Color SeaGreen = new Color(46, 139, 87, 255);
        public static Color Seashell = new Color(255, 245, 238, 255);
        public static Color Sienna = new Color(160, 82, 45, 255);
        public static Color Silver = new Color(192, 192, 192, 255);
        public static Color SkyBlue = new Color(135, 206, 235, 255);
        public static Color SlateBlue = new Color(106, 90, 205, 255);
        public static Color SlateGray = new Color(112, 128, 144, 255);
        public static Color Snow = new Color(255, 250, 250, 255);
        public static Color SpringGreen = new Color(0, 255, 127, 255);
        public static Color SteelBlue = new Color(70, 130, 180, 255);
        public static Color Tan = new Color(210, 180, 140, 255);
        public static Color Teal = new Color(0, 128, 128, 255);
        public static Color Thistle = new Color(216, 191, 216, 255);
        public static Color Tomato = new Color(253, 99, 71, 255);
        public static Color Transparent = new Color(0, 0, 0, 0);
        public static Color Turquoise = new Color(64, 224, 208, 255);
        public static Color Violet = new Color(238, 130, 238, 255);
        public static Color Wheat = new Color(245, 222, 179, 255);
        public static Color White = new Color(255, 255, 255, 255);
        public static Color WhiteSmoke = new Color(245, 245, 245, 255);
        public static Color Yellow = new Color(255, 255, 0, 255);
        public static Color YellowGreen = new Color(154, 205, 50, 255);



        public static Color? GetColorByPrettyName(string colorName)
        {
            Color value;
            if (ColorNames.TryGetValue(colorName, out value))
                return value;

            return null;
        }
    }
}