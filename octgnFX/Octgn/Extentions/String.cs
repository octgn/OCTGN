using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Octgn.Utils;
using Octgn.Data;

namespace Octgn.Extentions
{
    public static partial class StringExtensionMethods
    {
        public static string Decrypt(this string text)
        {
            RIPEMD160 hash = RIPEMD160.Create();
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(Prefs.Username));
            text = Cryptor.Decrypt(text, BitConverter.ToString(hasher));
            return text;
        }

        public static string Encrypt(this string text)
        {
            // Create a hash of current nickname to use as the Cryptographic Key
            RIPEMD160 hash = RIPEMD160.Create();
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(Prefs.Username));
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

    }
}
