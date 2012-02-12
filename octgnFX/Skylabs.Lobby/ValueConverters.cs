using System;
using System.Security.Cryptography;
using System.Text;

namespace Skylabs.Lobby
{
    public static class ValueConverters
    {
        /// <summary>
        ///   Converts a DateTime to a long of seconds since the epoch.
        /// </summary>
        /// <param name="time"> Time to convert. </param>
        /// <returns> Seconds since the epoch. </returns>
        public static long ToPhpTime(DateTime time)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var span = time.Subtract(unixEpoch);

            return (long) span.TotalSeconds;
        }

        /// <summary>
        ///   Returns a DateTime from a long representing seconds elapsed since the epoch
        /// </summary>
        /// <param name="secondsSinceEpoch"> Seconds </param>
        /// <returns> DateTime representation of the seconds. </returns>
        public static DateTime FromPhpTime(long secondsSinceEpoch)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            return unixEpoch.Add(new TimeSpan(0, 0, (int) secondsSinceEpoch));
        }

        public static string CreateShaHash(string password)
        {
            using (var hashTool = new SHA512Managed())
            {
                var passwordAsByte = Encoding.ASCII.GetBytes(password);
                var encryptedBytes = hashTool.ComputeHash(passwordAsByte);
                hashTool.Clear();
                return BitConverter.ToString(encryptedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string HashEmailAddress(string address)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();

                var hasedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(address));

                var sb = new StringBuilder();

                for (var i = 0; i < hasedBytes.Length; i++)
                {
                    sb.Append(hasedBytes[i].ToString("X2"));
                }

                return sb.ToString().ToLower();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string Md5(string str)
        {
            var textBytes = Encoding.Default.GetBytes(str);
            var cryptHandler = new MD5CryptoServiceProvider();
            var hash = cryptHandler.ComputeHash(textBytes);
            var ret = "";
            foreach (var a in hash)
            {
                if (a < 16)
                    ret += "0" + a.ToString("x");
                else
                    ret += a.ToString("x");
            }
            return ret;
        }
    }
}