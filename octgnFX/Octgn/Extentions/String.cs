using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Octgn.Utils;
using Octgn.Data;

namespace Octgn.Extentions
{
    using log4net;

    using Octgn.Core;

    public static partial class StringExtensionMethods
    {
        public static string Decrypt(this string text)
        {
            RIPEMD160 hash = RIPEMD160.Create();
            var un = (Prefs.Username ?? "").Clone() as string;
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(un));
            text = Cryptor.Decrypt(text, BitConverter.ToString(hasher));
            return text;
        }

        public static string Encrypt(this string text)
        {
            // Create a hash of current nickname to use as the Cryptographic Key
            RIPEMD160 hash = RIPEMD160.Create();
            var un = (Prefs.Username ?? "").Clone() as string;
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(un));
            return Cryptor.Encrypt(text, BitConverter.ToString(hasher));
        }

		/// <summary>
		/// Provides a cleaner method of string concatenation. (i.e. "Name {0}".With(firstName)
		/// </summary>
		public static string With(this string input, params object[] args)
		{
			return string.Format(input, args);
		}

        public static string Sha1(this string text)
        {
            var buffer = Encoding.Default.GetBytes(text);
            var cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
        }

        public static void SetLastPythonFunction(this ILog log, string function)
        {
            GlobalContext.Properties["lastpythonfunction"] = function;
        }

        public static void SetUserName(this ILog log, string username)
        {
            GlobalContext.Properties["username"] = username;
        }

        public static void SetRunningGame(this ILog log, string gameName, Guid gameId, Version gameVersion)
        {
            GlobalContext.Properties["gameName"] = gameName;
            GlobalContext.Properties["gameId"] = gameId;
            GlobalContext.Properties["gameVersion"] = gameVersion;
        }

        public static int ToInt(this Guid guid)
        {
            return guid.ToByteArray().Aggregate(0, (current, b) => current + b*2);
        }
    }
}
