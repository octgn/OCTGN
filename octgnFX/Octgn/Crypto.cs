using System;

namespace Octgn
{
    internal static class Crypto
    {
        private static readonly BigInteger b = new BigInteger(1125899906842628);
        private static readonly BigInteger m = new BigInteger(18446744073709550147);
        private static readonly Random rnd = new Random();

        public static ulong ModExp(ulong e)
        {
            var exp = new BigInteger(e);
            return (ulong) b.ModPow(exp, m).LongValue();
        }

        public static uint Random()
        {
            return (uint) rnd.Next();
        }

        public static uint PositiveRandom()
        {
            return (uint) rnd.Next() & 0x7fffffff;
        }

        public static int Random(int maxValue)
        {
            return rnd.Next(maxValue);
        }

        public static ulong[] Encrypt(ulong data, ulong pkey)
        {
            var k = new BigInteger(new[] {PositiveRandom(), PositiveRandom()});
            BigInteger c1 = b.ModPow(k, m), c2 = new BigInteger(pkey).ModPow(k, m);
            BigInteger encrypted = (c2*new BigInteger(data))%m;
            return new[] {(ulong) c1.LongValue(), (ulong) encrypted.LongValue()};
        }

        public static ulong[] Encrypt(Guid data, ulong pkey)
        {
            var k = new BigInteger(new[] {PositiveRandom(), PositiveRandom()});
            BigInteger c1 = b.ModPow(k, m), c2 = new BigInteger(pkey).ModPow(k, m);
            byte[] bytes = data.ToByteArray();
            BigInteger encrypted1 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 0)))%m;
            BigInteger encrypted2 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 4)))%m;
            BigInteger encrypted3 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 8)))%m;
            BigInteger encrypted4 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 12)))%m;
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
            BigInteger res = (c1.ModPow(Program.PrivateKey, m).modInverse(m)*c2)%m;
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
                BigInteger part = (c1.ModPow(Program.PrivateKey, m).modInverse(m)*c2)%m;
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
    }
}