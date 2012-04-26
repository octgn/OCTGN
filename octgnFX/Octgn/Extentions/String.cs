using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Octgn.Utils;
using Octgn.Data;

namespace Octgn.Extentions
{
    public static partial class ExtensionMethods
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
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(Program.LobbyClient.Username));
            return Cryptor.Encrypt(text, BitConverter.ToString(hasher));
        }
    }
}
