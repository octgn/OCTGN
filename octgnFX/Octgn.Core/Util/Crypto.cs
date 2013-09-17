namespace Octgn.Core.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Crypto
    {
        private static readonly BigInteger B = new BigInteger(1125899906842628);
        private static readonly BigInteger M = new BigInteger(18446744073709550147);
        private static readonly Random Rnd = new Random();

        public static ulong ModExp(ulong e)
        {
            var exp = new BigInteger(e);
            return (ulong) B.ModPow(exp, M).LongValue();
        }

        public static uint Random()
        {
            return (uint) Rnd.Next();
        }

        public static uint PositiveRandom()
        {
            return (uint) Rnd.Next() & 0x7fffffff;
        }

        public static int Random(int maxValue)
        {
            return Rnd.Next(maxValue);
        }

        public static ulong[] Encrypt(ulong data, ulong pkey)
        {
            var k = new BigInteger(new[] {PositiveRandom(), PositiveRandom()});
            BigInteger c1 = B.ModPow(k, M), c2 = new BigInteger(pkey).ModPow(k, M);
            BigInteger encrypted = (c2*new BigInteger(data))%M;
            return new[] {(ulong) c1.LongValue(), (ulong) encrypted.LongValue()};
        }

        public static ulong[] EncryptGuid(Guid data, ulong pkey)
        {
            var k = new BigInteger(new[] {PositiveRandom(), PositiveRandom()});
            BigInteger c1 = B.ModPow(k, M), c2 = new BigInteger(pkey).ModPow(k, M);
            byte[] bytes = data.ToByteArray();
            BigInteger encrypted1 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 0)))%M;
            BigInteger encrypted2 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 4)))%M;
            BigInteger encrypted3 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 8)))%M;
            BigInteger encrypted4 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 12)))%M;
            return new[]
                       {
                           (ulong) c1.LongValue(),
                           (ulong) encrypted1.LongValue(),
                           (ulong) encrypted2.LongValue(),
                           (ulong) encrypted3.LongValue(),
                           (ulong) encrypted4.LongValue()
                       };
        }

        public static ulong Decrypt(ulong[] data)
        {
            if (data.Length != 2) throw new ArgumentException("data should have a length of 2");

            BigInteger c1 = new BigInteger(data[0]), c2 = new BigInteger(data[1]);
            BigInteger res = (c1.ModPow(Prefs.PrivateKey, M).modInverse(M)*c2)%M;
            return (ulong) res.LongValue();
        }

        public static Guid DecryptGuid(ulong[] data)
        {
            if (data.Length != 5) throw new ArgumentException("data should have a length of 5");

            var c1 = new BigInteger(data[0]);
            var res = new byte[16];
            for (int i = 1; i < 5; i++)
            {
                var c2 = new BigInteger(data[i]);
                BigInteger part = (c1.ModPow(Prefs.PrivateKey, M).modInverse(M) * c2) % M;
                byte[] partBytes = BitConverter.GetBytes((uint) part.LongValue());
                partBytes.CopyTo(res, (i - 1)*4);
            }
            return new Guid(res);
        }

        public static uint Condense(this Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            return
                BitConverter.ToUInt32(bytes, 0) ^
                BitConverter.ToUInt32(bytes, 4) ^
                BitConverter.ToUInt32(bytes, 8) ^
                BitConverter.ToUInt32(bytes, 12);
        }

        //*************************************************
        // Encrypt guid to and from guid
        // Allows for 2.09227898e+13 different combonations
        //     for a single guid
        //*************************************************

        public static Guid Encrypt(this Guid guid, ulong key)
        {
            var encBytes = new byte[16];
            var guidBytes = guid.ToByteArray();
            var cur = 0;
            foreach (var p in key.Positions())
            {
                encBytes[p] = guidBytes[cur];
                cur++;
            }
            var ret = new Guid(encBytes);
            return ret;
        }

        public static Guid Decrypt(this Guid encGuid, ulong key)
        {
            var decBytes = new byte[16];
            var guidBytes = encGuid.ToByteArray();
            var cur = 0;
            foreach (var p in key.Positions())
            {
                decBytes[cur] = guidBytes[p];
                cur++;
            }
            var ret = new Guid(decBytes);
            return ret;
        }
        public static IEnumerable<byte> Positions(this ulong l)
        {
            const int Total = 16;
            var positions = new byte[Total];
            for (var i = 0; i < Total; i++)
            {
                var nibblePos = i * 4;
                var res = (byte)((l >> nibblePos) & 0xF);
                if (positions.Contains(res))
                {
                    for (var p = 0; p < Total; p++)
                    {
                        if (positions.Contains((byte)p) == false)
                        {
                            positions[i] = (byte)p;
                            break;
                        }
                    }
                }
                else
                {
                    positions[i] = res;
                }
                yield return positions[i];
            }
        }
    }
}
