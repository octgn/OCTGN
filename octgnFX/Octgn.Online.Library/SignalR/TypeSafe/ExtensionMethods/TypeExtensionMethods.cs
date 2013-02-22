namespace Octgn.Online.Library.SignalR.TypeSafe.ExtensionMethods
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensionMethods
    {
        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new[] { 
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Any(x => x == type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static dynamic Cast(this object o, Type t)
        {
            var castMethod = typeof(TypeExtensionMethods).GetMethods().First(x => x.Name == "Cast" && x.IsGenericMethod);
            var castMethodGeneric = castMethod.MakeGenericMethod(t);
            return castMethodGeneric.Invoke(o, new []{o});
        }

        public static T Cast<T>(this object o)
        {
            return (T)o;
        }
    }
}