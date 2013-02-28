using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Octgn.Utils
{
    class CryptoRNG
    {
        RNGCryptoServiceProvider rnd;
        Byte[] randomBytes = new Byte[4];
        public CryptoRNG()
        {
            rnd = new RNGCryptoServiceProvider();
        }
        public UInt32 Next()
        {
            rnd.GetBytes(randomBytes);
            return BitConverter.ToUInt32(randomBytes, 0);
        }
        public int Next(int maxValue)
        {
            return this.Next(0, maxValue);
        }
        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException();
            if (minValue == maxValue) return minValue;
            int diff = maxValue - minValue;
            double percent = this.NextDouble();
            return (int) (percent * diff + minValue);
        }
        public double NextDouble()
        {
            return this.Next() / ((double)(UInt32.MaxValue) + 1);
        }
    }
}
