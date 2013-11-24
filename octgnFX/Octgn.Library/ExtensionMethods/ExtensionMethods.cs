namespace Octgn.Library.ExtensionMethods
{
    using System;

    public static class ExtensionMethods
    {
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
    }
}