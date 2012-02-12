using System;

namespace Octgn
{
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
            var encrypted = (c2*new BigInteger(data))%M;
            return new[] {(ulong) c1.LongValue(), (ulong) encrypted.LongValue()};
        }

        public static ulong[] Encrypt(Guid data, ulong pkey)
        {
            var k = new BigInteger(new[] {PositiveRandom(), PositiveRandom()});
            BigInteger c1 = B.ModPow(k, M), c2 = new BigInteger(pkey).ModPow(k, M);
            var bytes = data.ToByteArray();
            var encrypted1 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 0)))%M;
            var encrypted2 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 4)))%M;
            var encrypted3 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 8)))%M;
            var encrypted4 = (c2*new BigInteger(BitConverter.ToUInt32(bytes, 12)))%M;
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
            var res = (c1.ModPow(Program.PrivateKey, M).modInverse(M)*c2)%M;
            return (ulong) res.LongValue();
        }

        public static Guid DecryptGuid(ulong[] data)
        {
            if (data.Length != 5) throw new ArgumentException("data should have a length of 5");

            var c1 = new BigInteger(data[0]);
            var res = new byte[16];
            for (var i = 1; i < 5; i++)
            {
                var c2 = new BigInteger(data[i]);
                var part = (c1.ModPow(Program.PrivateKey, M).modInverse(M)*c2)%M;
                var partBytes = BitConverter.GetBytes((uint) part.LongValue());
                partBytes.CopyTo(res, (i - 1)*4);
            }
            return new Guid(res);
        }

        public static uint Condense(this Guid guid)
        {
            var bytes = guid.ToByteArray();
            return
                BitConverter.ToUInt32(bytes, 0) ^
                BitConverter.ToUInt32(bytes, 4) ^
                BitConverter.ToUInt32(bytes, 8) ^
                BitConverter.ToUInt32(bytes, 12);
        }
    }
}