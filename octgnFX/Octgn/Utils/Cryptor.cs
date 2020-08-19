using System;
using log4net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Octgn.Utils
{
    internal class Cryptor
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string Encrypt(string text, string key)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            try
            {
                byte[] encryptedData = ProtectedData.Protect(textBytes, keyBytes, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedData);
            }
            catch (CryptographicException e)
            {
                Log.Error("Data encryption failed.", e);
                return null;
            }
        }

        public static string Decrypt(string encryptedText, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            try
            {
                var textBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedData = ProtectedData.Unprotect(textBytes, keyBytes, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (CryptographicException e)
            {
                Log.Error("Data decryption failed.", e);
                return null;
            }
            catch (FormatException e)
            {
                Log.Error("Encrypted data corrupt, cannot decrypt.", e);
                return null;
            }
        }
    }
}
