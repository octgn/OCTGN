using System;
using System.Security.Cryptography;
using System.Text;

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

        public static string CreateSHAHash(string Password)
        {
            using(System.Security.Cryptography.SHA512Managed HashTool = new System.Security.Cryptography.SHA512Managed())
            {
                Byte[] PasswordAsByte = System.Text.Encoding.ASCII.GetBytes(Password);
                Byte[] EncryptedBytes = HashTool.ComputeHash(PasswordAsByte);
                HashTool.Clear();
                return BitConverter.ToString(EncryptedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string HashEmailAddress(string address)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();

                var hasedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(address));

                var sb = new StringBuilder();

                for(var i = 0; i < hasedBytes.Length; i++)
                {
                    sb.Append(hasedBytes[i].ToString("X2"));
                }

                return sb.ToString().ToLower();
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        public static string MD5(string str)
        {
            byte[] textBytes = System.Text.Encoding.Default.GetBytes(str);
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler;
                cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash = cryptHandler.ComputeHash(textBytes);
                string ret = "";
                foreach(byte a in hash)
                {
                    if(a < 16)
                        ret += "0" + a.ToString("x");
                    else
                        ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }
    }
}