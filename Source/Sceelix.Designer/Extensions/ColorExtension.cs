using System;
using Sceelix.Designer.Utils;
using Color = Microsoft.Xna.Framework.Color;

namespace Sceelix.Designer.Extensions
{
    /*public struct HSVA
    {
        private double _hue;
        private double _saturation;
        private double _value;
        private double _alpha;

        // r,g,b values are from 0 to 1
        // h = [0,360], s = [0,1], v = [0,1]
        //		if s == 0, then h = -1 (undefined)
        void RGBtoHSV(Color color)//float r, float g, float b, float h, float s, float v
        {
            float min, max, delta;
            min = Math.Min(color.R, Math.Min(color.G, b));
            max = Math.Max(color.R, Math.Max(color.G, b));
            v = max;				// v
            delta = max - min;
            if (max != 0)
                *s = delta / max;		// s
            else
            {
                // r = g = b = 0		// s = 0, v is undefined
                s = 0;
                h = -1;
                return;
            }
            if (color.R == max)
                h = (color.G - b) / delta;		// between yellow & magenta
            else if (color.G == max)
                h = 2 + (b - color.R) / delta;	// between cyan & yellow
            else
                h = 4 + (color.R - color.G) / delta;	// between magenta & cyan
            h *= 60;				// degrees
            if (h < 0)
                h += 360;
        }
        void HSVtoRGB(float* r, float* g, float* b, float h, float s, float v)
        {
            int i;
            float f, p, q, t;
            if (s == 0)
            {
                // achromatic (grey)
                *r = *g = *b = v;
                return;
            }
            h /= 60;			// sector 0 to 5
            i = floor(h);
            f = h - i;			// factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0:
                    *r = v;
                    *g = t;
                    *b = p;
                    break;
                case 1:
                    *r = q;
                    *g = v;
                    *b = p;
                    break;
                case 2:
                    *r = p;
                    *g = v;
                    *b = t;
                    break;
                case 3:
                    *r = p;
                    *g = q;
                    *b = v;
                    break;
                case 4:
                    *r = t;
                    *g = p;
                    *b = v;
                    break;
                default:		// case 5:
                    *r = v;
                    *g = p;
                    *b = q;
                    break;
            }
        }
    }*/

    public static class ColorExtension
    {
        public static Color Invert(this Color color)
        {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
        }



        public static Color AdjustBrightness(this Color color, float amount)
        {
            return new Color((int) (color.R*amount), (int) (color.G*amount), (int) (color.B*amount), color.A);
        }



        public static Color Adjust(this Color color, float amount)
        {
            float amountPositive = 1.5f;
            float amountNegative = 0.7f;

            int r = (int) (color.R < 128 ? color.R*amountPositive : color.R*amountNegative);
            int g = (int) (color.G < 128 ? color.G*amountPositive : color.G*amountNegative);
            int b = (int) (color.B < 128 ? color.B*amountPositive : color.B*amountNegative);

            return new Color(r, g, b, color.A);
        }



        public static Color Extreme(this Color color)
        {
            int total = color.R*color.G*color.B;

            if (color.R >= 192 || color.G >= 192 || color.B >= 192)
                return new Color(0, 0, 0, 255);

            return new Color(255, 255, 255, 255);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">Hue, 0 to 360</param>
        /// <param name="s">Saturation, 0 to 1</param>
        /// <param name="v">Value, 0 to 1</param>
        /// <returns></returns>
        public static Color HsvToRgb(double h, double s, double v)
        {
            int hi = (int) Math.Floor(h/60.0)%6;
            double f = (h/60.0) - Math.Floor(h/60.0);

            double p = v*(1.0 - s);
            double q = v*(1.0 - (f*s));
            double t = v*(1.0 - ((1.0 - f)*s));

            Color ret;

            switch (hi)
            {
                case 0:
                    ret = GetRgb(v, t, p);
                    break;
                case 1:
                    ret = GetRgb(q, v, p);
                    break;
                case 2:
                    ret = GetRgb(p, v, t);
                    break;
                case 3:
                    ret = GetRgb(p, q, v);
                    break;
                case 4:
                    ret = GetRgb(t, p, v);
                    break;
                case 5:
                    ret = GetRgb(v, p, q);
                    break;
                default:
                    ret = new Color(0x00, 0x00, 0x00, 0xFF);
                    break;
            }
            return ret;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">Hue, 0 to 360</param>
        /// <param name="s">Saturation, 0 to 1</param>
        /// <param name="v">Value, 0 to 1</param>
        /// <returns></returns>
        public static Color HsvToRgb(double[] hsv)
        {
            return HsvToRgb(hsv[0], hsv[1], hsv[2]);
        }



        private static Color GetRgb(double r, double g, double b)
        {
            return new Color((byte) (r*255.0), (byte) (g*255.0), (byte) (b*255.0), 255);
        }



        public static double[] ColorToHSV(this Color color)
        {
            var xnaColor = color.ToGDIColor();

            double[] hsv = new double[3];

            int max = Math.Max(xnaColor.R, Math.Max(xnaColor.G, xnaColor.B));
            int min = Math.Min(xnaColor.R, Math.Min(xnaColor.G, xnaColor.B));

            hsv[0] = xnaColor.GetHue();
            hsv[1] = (max == 0) ? 0 : 1d - (1d*min/max);
            hsv[2] = max/255d;

            return hsv;
        }
    }
}