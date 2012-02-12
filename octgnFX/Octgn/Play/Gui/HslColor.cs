using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Octgn.Play.Gui
{
    public struct HslColor
    {
        private float _hue;
        private float _luminance;
        private float _saturation;

        public HslColor(Color c) : this()
        {
            _hue = _saturation = _luminance = 0;
            Alpha = 0;
            FromRgba(c.ScR, c.ScG, c.ScB, c.A);
        }

        public float Hue
        {
            get { return _hue; }
            set { Clamp(value, out _hue); }
        }

        public float Saturation
        {
            get { return _saturation; }
            set { Clamp(value, out _saturation); }
        }

        public float Luminance
        {
            get { return _luminance; }
            set { Clamp(value, out _luminance); }
        }

        public byte Alpha { get; private set; }

        public static implicit operator Color(HslColor c)
        {
            double red, green, blue;

            if (Math.Abs(c._saturation - 0) < float.Epsilon)
                red = green = blue = c._luminance;
            else
            {
                double m2 = c._luminance <= 0.5f
                                ? c._luminance*(1 + c._saturation)
                                : c._luminance + c._saturation - c._luminance*c._saturation;

                double m1 = (2*c._luminance) - m2;

                red = HueToRgb(m1, m2, c._hue + (1/3f));
                green = HueToRgb(m1, m2, c._hue);
                blue = HueToRgb(m1, m2, c._hue - (1/3f));
            }

            return Color.FromArgb(
                c.Alpha,
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
            Alpha = lAlpha;

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
            _luminance = (max + min)/2f;

            // Compute Saturation
            if (Math.Abs(_luminance - 0) < float.Epsilon || Math.Abs(delta - 0) < float.Epsilon)
                _saturation = 0;
            else if (_luminance <= 0.5f)
                _saturation = delta/(2*_luminance);
            else
                _saturation = delta/(2 - 2*_luminance);

            // Compute Hue
            if (Math.Abs(delta - 0) < float.Epsilon)
                _hue = 0;
            else if (Math.Abs(max - red) < float.Epsilon)
            {
                _hue = (green - blue)/delta;
                if (green < blue) _hue += 6;
            }
            else if (Math.Abs(max - green) < float.Epsilon)
                _hue = (blue - red)/delta + 2;
            else
                _hue = (red - green)/delta + 4;
            _hue /= 6f;
        }

        private static void Clamp(float source, out float target)
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
            var c = new HslColor((Color) value)
                        {Luminance = System.Convert.ToSingle(parameter, CultureInfo.InvariantCulture)};
            return (Color) c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("LuminanceConverter.ConvertBack");
        }

        #endregion
    }
}