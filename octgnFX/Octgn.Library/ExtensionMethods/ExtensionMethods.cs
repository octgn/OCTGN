using System.Collections.Generic;
using System.Reflection;
using System;
using log4net;

namespace Octgn.Library.ExtensionMethods
{
    public static class ExtensionMethods
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static bool IsIntegerType(this Type type, bool exclude64bit = false)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return true;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return exclude64bit == false;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsIntegerType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }

        public static Type[] GetTypesSafe(this Module ass)
        {
            try
            {
                return ass.GetTypes();

            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var e in ex.LoaderExceptions)
                {
                    Log.Warn("GetTypesSafe",e);
                }
                return ex.Types ?? new Type[0];
            }
        }

        public static Type[] GetTypesSafe(this Assembly ass)
        {
            var ret = new List<Type>();
            foreach (var m in ass.GetModules())
            {
                ret.AddRange(m.GetTypesSafe());
            }

            return ret.ToArray();
        }
    }
}