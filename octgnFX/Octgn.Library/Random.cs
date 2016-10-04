using System;
using System.Reflection;

namespace Octgn.Library
{
    public static class Random
    {
        public static IntRandomGenerator Int { get; private set; } = new IntRandomGenerator();
        public static UIntRandomGenerator UInt { get; private set; } = new UIntRandomGenerator();
        public static ByteRandomGenerator Byte { get; private set; } = new ByteRandomGenerator();

        public static Randomizer XDigit(int digits) {
            return new Randomizer(digits);
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

    public abstract class RandomGeneratorBase<T> where T : struct, IConvertible
    {
        protected static readonly System.Random Random = new System.Random();
        T _minValue;
        T _maxValue;
        protected RandomGeneratorBase() {
            var numberType = typeof(T);
            var minValueField = numberType.GetField(nameof(int.MinValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var maxValueField = numberType.GetField(nameof(int.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            _minValue = (T)minValueField.GetValue(null);
            _maxValue = (T)maxValueField.GetValue(null);
        }

        public abstract T Between(T min, T max, bool inclusive = true);

        public T Between(T min, bool inclusive = true) => Between(min, _maxValue, inclusive);
        public T Between(bool inclusive = true) => Between(_minValue, _maxValue, inclusive);
    }

    public class IntRandomGenerator : RandomGeneratorBase<int>
    {
        public override int Between(int min, int max, bool inclusive = true) {
            var inc = inclusive ? 1 : 0;
            return Random.Next(min + inc, max - inc);
        }
    }

    public class UIntRandomGenerator : RandomGeneratorBase<uint>
    {
        public override uint Between(uint min, uint max, bool inclusive = true) {
            var inc = inclusive ? 1 : 0;
            var ran = (uint)(Random.NextDouble() * ((max + inc) - (min + inc)) + (min + inc));
            return ran;
        }
    }

    public class ByteRandomGenerator : RandomGeneratorBase<byte>
    {
        public override byte Between(byte min, byte max, bool inclusive = true) {
            var inc = inclusive ? 1 : 0;
            var ran = (byte)(Random.NextDouble() * ((max + inc) - (min + inc)) + (min + inc));
            return ran;
        }
    }
}
