﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Octgn
{
    internal class Cryptor
    {
        /// <summary>
        ///   Encrypts an array of bytes using a Key and an Initialization Vector
        /// </summary>
        /// <param name="clearData"> The data to be encrypted </param>
        /// <param name="Key"> The key to encrypt it with </param>
        /// <param name="IV"> The Initialization Vector </param>
        /// <returns> An encrypted byte array </returns>
        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            var ms = new MemoryStream();
            TripleDES alg = TripleDES.Create();
            alg.Key = Key;
            alg.IV = IV;
            var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }


        public static string Encrypt(string Text, string Key)
        {
            byte[] Bytes = Encoding.Unicode.GetBytes(Text);
            var pdb = new Rfc2898DeriveBytes(Key,
                                              new byte[]
                                                  {
                                                      0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                                                      0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                                                  });
            
            byte[] encryptedData = Encrypt(Bytes, pdb.GetBytes(16), pdb.GetBytes(8));
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        ///   Decrypts an array of data that has been encrypted using the Encrypt command.
        /// </summary>
        /// <param name="cipherData"> The data to decrypt </param>
        /// <param name="Key"> The Key to decrypt it with </param>
        /// <param name="IV"> An Initialization Vector </param>
        /// <returns> A decrypted data byte array </returns>
        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            var ms = new MemoryStream();
            TripleDES alg = TripleDES.Create();
            alg.Key = Key;
            alg.IV = IV;
            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            try
            {
                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();
                byte[] decryptedData = ms.ToArray();
                return decryptedData;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }


        public static string Decrypt(string EncryptedText, string key)
        {
            byte[] cryptedBytes = Convert.FromBase64String(EncryptedText);
            var pdb = new Rfc2898DeriveBytes(key,
                                              new byte[]
                                                  {
                                                      0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65,
                                                      0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                                                  });
            byte[] decryptedData = Decrypt(cryptedBytes, pdb.GetBytes(16), pdb.GetBytes(8));
            return Encoding.Unicode.GetString(decryptedData ?? new byte[] {});
        }
    }
}