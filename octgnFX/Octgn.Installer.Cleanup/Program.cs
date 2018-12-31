using Microsoft.Win32;
using System;

namespace Octgn.Installer.Cleanup
{
    class Program
    {
        static void Main(string[] args) {
            try {
                asdF();
            } catch (Exception ex) {
                Console.Error.WriteLine("ERROR: " + ex.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("DONE");
            Console.ReadKey();
        }

        static void asdF() {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Components", true)) {
                foreach (var v in key.GetSubKeyNames()) {
                    using (var productKey = key.OpenSubKey(v)) {
                        if (productKey == null) continue;

                        foreach (var valueName in productKey.GetValueNames()) {
                            var value = Convert.ToString(productKey.GetValue(valueName));

                            if (value.Contains(@"C:\Program Files (x86)\OCTGN\Octgn")) {
                                key.DeleteSubKeyTree(v);
                                Console.WriteLine(value);
                            }
                        }
                    }
                }
            }
        }
    }
}
