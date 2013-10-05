namespace Octgn.Test
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using NUnit.Framework;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.Util;
    using Octgn.Play.State;

    public class PlayGround
    {
        [Test]
        public void Spaces()
        {
            var g = Guid.NewGuid();
            Console.WriteLine(g);
            var enc = this.EncryptGuid(g, 2);
            Console.WriteLine("Length: {0} {1}", enc.Length, enc.Length * 8);
            var dec = this.DecryptGuid(enc, 2);
            Console.WriteLine(dec);

            ulong a = 123;

            var b = Crypto.ModExp(a);

            var c = Crypto.ModExp(a);

            Assert.AreEqual(b, c);
        }

        public byte[] EncryptGuid(Guid guid, ulong key)
        {
            var keybytes = BitConverter.GetBytes(key);
            Console.WriteLine(keybytes.Length);
            var crypt = new DESCryptoServiceProvider
                        {
                            Key = BitConverter.GetBytes(key),
                            IV = BitConverter.GetBytes(key)
                        };

            using (var ms = new MemoryStream())
            using (var st = new CryptoStream(ms, crypt.CreateEncryptor(), CryptoStreamMode.Write))
            {
                var gb = guid.ToByteArray();
                st.Write(gb, 0, gb.Length);
                st.Flush();
                st.FlushFinalBlock();
                ms.Flush();
                ms.Position = 0;
                return ms.ToArray();
            }

        }
        public Guid DecryptGuid(byte[] bytes, ulong key)
        {
            var crypt = new DESCryptoServiceProvider
            {
                Key = BitConverter.GetBytes(key),
                IV = BitConverter.GetBytes(key)
            };

            var bt = new byte[16];

            using (var ms = new MemoryStream(bytes))
            using (var st = new CryptoStream(ms, crypt.CreateDecryptor(), CryptoStreamMode.Read))
            {
                ms.Position = 0;
                st.Read(bt, 0, 16);
                return new Guid(bt);
            }

        }
    }
}