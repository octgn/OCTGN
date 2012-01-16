using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn
{
    class Registry
    {
        /// <summary>
        /// Reads a string value from the OCTGN registry
        /// </summary>
        /// <param name="valName">The name of the value</param>
        /// <returns>A string value</returns>
        public static string ReadValue(string valName)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\OCTGN");
            if (key == null)
            {
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\OCTGN");
                return null;
            }
            if (key != null)
                return (string)key.GetValue(valName);
            else return null;
        }

        /// <summary>
        /// Writes a string value to the OCTGN registry
        /// </summary>
        /// <param name="valName">Name of the value</param>
        /// <param name="value">String to write for value</param>
        public static void WriteValue(string valName, string value)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\OCTGN", true);
            if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\OCTGN");
            key.SetValue(valName, value, Microsoft.Win32.RegistryValueKind.String);
            key.Close();
        }
    }
}
