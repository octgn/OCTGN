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
        public static Randomizer Between(long min, long max) {
            return new Randomizer(min: min + 1, max: max - 1);
        }
        public static Randomizer Inclusive(long min, long max) {
            return new Randomizer(min: min, max: max);
        }
    }

    public struct Randomizer
    {
        private static System.Random _random = new System.Random();

        private int? _digitCount;
        private long? _min;
        private long? _max;

        public Randomizer(int? digitCount = null, long? min = null, long? max = null) {
            _digitCount = digitCount;
            _min = min;
            _max = max;
        }

        public double Double {
            get {
                if (_digitCount != null) {
                    var rand = _random.NextDouble();
                    var updigit = Math.Pow(10, _digitCount.Value);
                    return (rand * updigit);
                } else if(_min != null || _max != null) {
                    if(_min != null && _max != null) {
                        return (_random.NextDouble() * (_max.Value - _min.Value) + _min.Value);
                    } else if(_min != null) {
                        return (_random.NextDouble() * (int.MaxValue - _min.Value) + _min.Value);
                    } else if(_max != null) {
                        return (_random.NextDouble() * (_max.Value - double.MinValue) + double.MinValue);
                    }
                }

                return (_random.NextDouble() * (double.MaxValue - double.MinValue) + double.MinValue);
            }
        }

        public int Int {
            get {
                if (_digitCount != null) {
                    var maxdigi = Math.Pow(10, _digitCount.Value) - 1;
                    return _random.Next(0, (int)maxdigi);
                } else if (_min != null || _max != null) {
                    if (_min != null && _max != null) {
                        return _random.Next((int)_min.Value, (int)_max.Value);
                    } else if (_min != null) {
                        return _random.Next((int)_min.Value, int.MaxValue);
                    } else if (_max != null) {
                        return _random.Next((int)_max.Value);
                    }
                }
                return _random.Next();
            }
        }

        public uint UInt {
            get {
                if (_digitCount != null) {
                    var maxdigi = Math.Pow(10, _digitCount.Value) - 1;
                    return (uint)_random.Next(0, (int)maxdigi);
                } else if (_min != null || _max != null) {
                    if (_min != null && _max != null) {
                        return RandomNextUInt((uint)_min.Value, (uint)_max.Value);
                    } else if (_min != null) {
                        return RandomNextUInt(min: (uint)_min.Value);
                    } else if (_max != null) {
                        return RandomNextUInt((uint)_max.Value);
                    }
                }
                return RandomNextUInt();
            }
        }

        private static uint RandomNextUInt(uint max = uint.MaxValue, uint min = uint.MinValue) {
            return (uint)(_random.NextDouble() * (max - min) + min);
        }
    }
}
