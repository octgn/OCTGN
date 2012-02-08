using System;
using System.Collections;
using System.IO;
using Polenter.Serialization;

namespace Octgn
{
    internal class SimpleConfig
    {
        private static string GetPath()
        {
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config");
            const string f = "settings.xml";
            var fullPath = Path.Combine(p, f);

            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            return fullPath;
        }

        /// <summary>
        ///   Reads a string value from the OCTGN registry
        /// </summary>
        /// <param name="valName"> The name of the value </param>
        /// <returns> A string value </returns>
        public static string ReadValue(string valName)
        {
            if (File.Exists(GetPath()))
            {
                using (var reader = new StreamReader(GetPath()))
                {
                    var serializer = new SharpSerializer();
                    var config = (Hashtable) serializer.Deserialize(GetPath());
                    if (config.ContainsKey(valName))
                    {
                        return config[valName].ToString();
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///   Writes a string value to the OCTGN registry
        /// </summary>
        /// <param name="valName"> Name of the value </param>
        /// <param name="value"> String to write for value </param>
        public static void WriteValue(string valName, string value)
        {
            var serializer = new SharpSerializer();
            Hashtable config = new Hashtable();
            if (File.Exists(GetPath()))
            {
                using (var reader = new StreamReader(GetPath()))
                {
                    config = (Hashtable) serializer.Deserialize(GetPath());
                }
            }
            config[valName] = value;
            serializer.Serialize(config, GetPath());
        }
    }
}