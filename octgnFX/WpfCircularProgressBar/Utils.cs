using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScottLogic.Util
{
    public static class Extensions
    {
        // For whatever reason, this had a problem being extended with the same arguments
        // and only a different return type; just changed the method name
        public static Point OffsetEx(this Point point, double X, double Y)
        {
            return new Point(point.X + X, point.Y + Y);
        }
    }

    public static class Utils
    {
        /// <summary>
        /// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}
