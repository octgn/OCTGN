using System;

namespace Skylabs
{
    public static class ValueConverters
    {
        /// <summary>
        /// Converts a DateTime to a long of seconds since the epoch.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <returns>Seconds since the epoch.</returns>
        public static long toPHPTime(DateTime time)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = time.Subtract(unixEpoch);

            return (long)span.TotalSeconds;
        }

        /// <summary>
        /// Returns a DateTime from a long representing seconds elapsed since the epoch
        /// </summary>
        /// <param name="SecondsSinceEpoch">Seconds</param>
        /// <returns>DateTime representation of the seconds.</returns>
        public static DateTime fromPHPTime(long SecondsSinceEpoch)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            return unixEpoch.Add(new TimeSpan(0, 0, (int)SecondsSinceEpoch));
        }
    }
}