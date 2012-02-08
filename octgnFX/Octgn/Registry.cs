using Microsoft.Win32;

namespace Octgn
{
    internal class Registry
    {
        /// <summary>
        ///   Reads a string value from the OCTGN registry
        /// </summary>
        /// <param name="valName"> The name of the value </param>
        /// <returns> A string value </returns>
        public static string ReadValue(string valName)
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\OCTGN");
            if (key == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\OCTGN");
                return null;
            }
            return (string) key.GetValue(valName);
        }

        /// <summary>
        ///   Writes a string value to the OCTGN registry
        /// </summary>
        /// <param name="valName"> Name of the value </param>
        /// <param name="value"> String to write for value </param>
        public static void WriteValue(string valName, string value)
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\OCTGN", true);
            if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\OCTGN");
            if (key != null)
            {
                key.SetValue(valName, value, RegistryValueKind.String);
                key.Close();
            }
        }
    }
}