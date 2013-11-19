using System.IO;

namespace Octgn.Library.ExtensionMethods
{
    using System;
    using System.ComponentModel;

    public static class StringExtensionMethods
    {
        public static bool Is<T>(this string input)
        {
            try
            {
                TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
            }
            catch
            {
                return false;
            }

            return true;
        }
        public static bool Is(this string input, Type targetType)
        {
            try
            {
                TypeDescriptor.GetConverter(targetType).ConvertFromString(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static T To<T>(this string input)
        {
            try
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
            }
            catch
            {
                return default(T);
            }
        }
        public static object To(this string input, Type targetType)
        {
            try
            {
                return TypeDescriptor.GetConverter(targetType).ConvertFromString(input);
            }
            catch
            {
                if (targetType.IsValueType)
                {
                    return Activator.CreateInstance(targetType);
                }
                return null;
            }
        }
        public static Stream ToStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}