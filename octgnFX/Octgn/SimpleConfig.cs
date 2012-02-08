using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace Octgn
{
    internal class SimpleConfig
    {
        static string GetPath()
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
        public string ReadValue(string valName)
        {
            var serializer = new XmlSerializer(typeof (Hashtable));
            if (File.Exists(SimpleConfig.GetPath()))
            {
                using (var reader = new StreamReader(SimpleConfig.GetPath()))
                {
                    var config = (Hashtable) serializer.Deserialize(reader);
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
        public void WriteValue(string valName, string value)
        {
            var serializer = new XmlSerializer(typeof (Hashtable));
            Hashtable config;
            if (File.Exists(SimpleConfig.GetPath()))
            {
                using (var reader = new StreamReader(SimpleConfig.GetPath()))
                {
                    config = (Hashtable)serializer.Deserialize(reader);
                    config[valName] = value;
                    using (var writer = new StreamWriter(SimpleConfig.GetPath()))
                    {
                        serializer.Serialize(writer, config);
                    }
                }
            }
        }
    }
}