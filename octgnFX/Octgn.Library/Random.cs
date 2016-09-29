using System;

namespace Octgn.Library
{
    public static class Random
    {
        public static Randomizer XDigit(int digits) {
            return new Randomizer(digits);
        }

        public static Randomizer Between(int min, int max) {
            return new Randomizer(min: min + 1, max: max - 1);
        }
        public static Randomizer Inclusive(int min, int max) {
            return new Randomizer(min: min, max: max);
        }
    }

    public struct Randomizer
    {
        private static System.Random _random = new System.Random();

        private int? _digitCount;
        private int? _min;
        private int? _max;

        public Randomizer(int? digitCount = null, int? min = null, int? max = null) {
            _digitCount = digitCount;
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Do not use with <see cref="Random.Inclusive(int, int)"/> or <see cref="Random.Between(int, int)"/>
        /// </summary>
        public double Double {
            get {
                if (_digitCount != null) {
                    var rand = _random.NextDouble();
                    var updigit = Math.Pow(10, _digitCount.Value);
                    return (rand * updigit);
                } else if(_min != null || _max != null) {
                    throw new NotImplementedException();
                    //if(_min != null && _max != null) {
                    //    return _random.Next(_min.Value, _max.Value);
                    //} else if(_min != null) {
                    //    return _random.Next(_min.Value, int.MaxValue);
                    //} else if(_max != null) {
                    //    return _random.Next(_max.Value);
                    //}
                }

                return _random.NextDouble();
            }
        }

        public int Int {
            get {
                if (_digitCount != null) {
                    var maxdigi = Math.Pow(10, _digitCount.Value) - 1;
                    return _random.Next(0, (int)maxdigi);
                } else if (_min != null || _max != null) {
                    if (_min != null && _max != null) {
                        return _random.Next(_min.Value, _max.Value);
                    } else if (_min != null) {
                        return _random.Next(_min.Value, int.MaxValue);
                    } else if (_max != null) {
                        return _random.Next(_max.Value);
                    }
                }
                return _random.Next();
            }
        }
    }
}
