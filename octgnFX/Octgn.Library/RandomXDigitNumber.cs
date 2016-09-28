using System;
using System.Linq;

namespace Octgn.Library
{
    public struct RandomXDigitNumber
    {
        public static string Next4Digit => new RandomXDigitNumber(4);
        public readonly string NumberString;
        public readonly int Number;

        private static readonly Random _rnd = new Random();

        public RandomXDigitNumber(int digits) {
            var maxNum = int.Parse(new string(Enumerable.Repeat('9', digits).ToArray()));
            NumberString = (Number = _rnd.Next(0, maxNum)).ToString($"D{digits}");
        }

        public static implicit operator string(RandomXDigitNumber input) {
            return input.NumberString;
        }

        public static implicit operator int(RandomXDigitNumber input) {
            return input.Number;
        }

        public static explicit operator RandomXDigitNumber(int input) {
            return new RandomXDigitNumber(input);
        }
    }
}
