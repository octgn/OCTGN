using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Octgn.Play.Gui
{
    public struct HslColor
    {
        private byte alpha;
        private float hue;
        private float luminance;
        private float saturation;

        public HslColor(Color c)
        {
            hue = saturation = luminance = 0;
            alpha = 0;
            FromRgba(c.ScR, c.ScG, c.ScB, c.A);
        }

        public float Hue
        {
            get { return hue; }
            set { Clamp(value, out hue); }
        }

        public float Saturation
        {
            get { return saturation; }
            set { Clamp(value, out saturation); }
        }

        public float Luminance
        {
            get { return luminance; }
            set { Clamp(value, out luminance); }
        }

        public byte Alpha
        {
            get { return alpha; }
        }

        public static implicit operator Color(HslColor c)
        {
            double red, green, blue;

            if (Math.Abs(c.saturation - 0) < float.Epsilon)
                red = green = blue = c.luminance;
            else
            {
                double m2 = c.luminance <= 0.5f
                                ? c.luminance*(1 + c.saturation)
                                : c.luminance + c.saturation - c.luminance*c.saturation;

                double m1 = (2*c.luminance) - m2;

                red = HueToRgb(m1, m2, c.hue + (1/3f));
                green = HueToRgb(m1, m2, c.hue);
                blue = HueToRgb(m1, m2, c.hue - (1/3f));
            }

            return Color.FromArgb(
                c.alpha,
                (byte) Math.Round(red*255),
                (byte) Math.Round(green*255),
                (byte) Math.Round(blue*255)
                );
        }

        private static double HueToRgb(double m1, double m2, double hue)
        {
            if (hue < 0) hue += 1;
            if (hue > 1) hue -= 1;

            if (6*hue < 1)
                return m1 + (m2 - m1)*hue*6;
            if (2*hue < 1)
                return m2;
            if (3*hue < 2)
                return m1 + (m2 - m1)*(2/3f - hue)*6;

            return m1;
        }

        private void FromRgba(float red, float green, float blue, byte lAlpha)
        {
            alpha = lAlpha;

            // Compute Max, Min and Delta
            float max, min;
            if (red > green)
            {
                max = red > blue ? red : blue;
                min = green < blue ? green : blue;
            }
            else
            {
                max = green > blue ? green : blue;
                min = red < blue ? red : blue;
            }
            float delta = max - min;

            // Compute Luminance
            luminance = (max + min)/2f;

            // Compute Saturation
            if (Math.Abs(luminance - 0) < float.Epsilon || Math.Abs(delta - 0) < float.Epsilon)
                saturation = 0;
            else if (luminance <= 0.5f)
                saturation = delta/(2*luminance);
            else
                saturation = delta/(2 - 2*luminance);

            // Compute Hue
            if (Math.Abs(delta - 0) < float.Epsilon)
                hue = 0;
            else if (Math.Abs(max - red) < float.Epsilon)
            {
                hue = (green - blue)/delta;
                if (green < blue) hue += 6;
            }
            else if (Math.Abs(max - green) < float.Epsilon)
                hue = (blue - red)/delta + 2;
            else
                hue = (red - green)/delta + 4;
            hue /= 6f;
        }

        private void Clamp(float source, out float target)
        {
            if (source > 1) target = 1;
            else if (source < 0) target = 0;
            else target = source;
        }
    }

    [ValueConversion(typeof (Color), typeof (Color))]
    public class LuminanceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = new HslColor((Color) value);
            c.Luminance = System.Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
            return (Color) c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("LuminanceConverter.ConvertBack");
        }

        #endregion
    }
}