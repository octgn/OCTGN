using System;
using System.Linq;

namespace Octgn.Library
{
    public class RandomXDigitNumber
    {
        public string Number { get; private set; }

        private static Random _rnd;

        static RandomXDigitNumber() {
            _rnd = new Random();
        }

        public RandomXDigitNumber(int digits) {
            var maxNum = int.Parse(new string(Enumerable.Repeat('9', digits).ToArray()));
            Number = _rnd.Next(0, maxNum).ToString(new string(Enumerable.Repeat('#', digits).ToArray()));
        }

        public static implicit operator string(RandomXDigitNumber input) {
            return input.Number;
        }
    }
}
