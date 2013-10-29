namespace Octgn.Core.ExtensionMethods
{
    using System.Net.Sockets;
    using System.Reflection;

    public static class ExtensionMethods
    {
        public static object ReadPrivateField<T>(this T obj, string name)
        {
            var field = typeof(T).GetField(name, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            var value = field.GetValue(obj);
            return value;
        }

        public static bool IsDisposed(this TcpClient client)
        {
            return (bool)client.ReadPrivateField("m_CleanedUp");
        }
    }
}