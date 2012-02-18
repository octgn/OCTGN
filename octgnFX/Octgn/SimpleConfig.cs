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
            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config");
            const string f = "settings.xml";
            string fullPath = Path.Combine(p, f);

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
                var serializer = new SharpSerializer();
                var config = (Hashtable) serializer.Deserialize(GetPath());
                if (config.ContainsKey(valName))
                {
                    return config[valName].ToString();
                }
            }
            return null;
        }

        public static string ReadValue(string valName, string d)
        {
            string r = ReadValue(valName);
            return r ?? d;
        }

        /// <summary>
        ///   Writes a string value to the OCTGN registry
        /// </summary>
        /// <param name="valName"> Name of the value </param>
        /// <param name="value"> String to write for value </param>
        public static void WriteValue(string valName, string value)
        {
            var serializer = new SharpSerializer();
            var config = new Hashtable();
            if (File.Exists(GetPath()))
            {
                config = (Hashtable) serializer.Deserialize(GetPath());
            }
            config[valName] = value;
            serializer.Serialize(config, GetPath());
        }
    }
}