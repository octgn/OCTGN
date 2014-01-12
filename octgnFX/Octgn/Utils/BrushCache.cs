namespace Octgn.Utils
{
    using System.Collections.Generic;
    using System.Windows.Media;

    public static class BrushCache
    {
        private static readonly Dictionary<Color, Brush> Cache = new Dictionary<Color, Brush>();

        public static Brush CacheToBrush(this Color color)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(color))
                {
                    return Cache[color];
                }
                var brush = new SolidColorBrush(color);
				Cache.Add(color,brush);
                return brush;
            }
        }
    }
}