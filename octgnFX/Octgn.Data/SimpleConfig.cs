using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Polenter.Serialization;

namespace Octgn.Data
{
    public static class SimpleConfig
    {
        private static object lockObject = new Object();

        /// <summary>
        /// Special case since it's required in Octgn.Data, and Prefs can't go there
        /// </summary>
        public static string DataDirectory
        {
            get
            {
                return ReadValue("datadirectory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn"));
            }
            set
            {
                WriteValue("datadirectory", value);
            }
        }

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
        ///   Reads a string value from the Octgn registry
        /// </summary>
        /// <param name="valName"> The name of the value </param>
        /// <returns> A string value </returns>
        public static string ReadValue(string valName, string def)
        {
            lock (lockObject)
            {
                var ret = def;
                Stream f = null;
                try
                {
                    if (File.Exists(GetPath()))
                    {
                        var serializer = new SharpSerializer();
                        
                        Hashtable config = new Hashtable();
                        if (OpenFile(GetPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f))
                        {
                            config = (Hashtable)serializer.Deserialize(f);
                        }
                        if (config.ContainsKey(valName))
                        {
                            return config[valName].ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("[SimpleConfig]ReadValue Error: " + e.Message);
                    try
                    {
                        File.Delete(GetPath());
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine("[SimpleConfig]ReadValue Error: Couldn't delete the corrupt config file.");
                    }
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                        f = null;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        ///   Writes a string value to the Octgn registry
        /// </summary>
        /// <param name="valName"> Name of the value </param>
        /// <param name="value"> String to write for value </param>
        public static void WriteValue(string valName, string value)
        {
            lock (lockObject)
            {
                Stream f = null;
                try
                {
                    var serializer = new SharpSerializer();
                    var config = new Hashtable();
                    if (File.Exists(GetPath()))
                    {
                        if (OpenFile(GetPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f))
                        {
                            config = (Hashtable)serializer.Deserialize(f);
                        }
                    }
                    else
                    {
                        OpenFile(GetPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f);
                    }
                    config[valName] = value;
                    serializer.Serialize(config, f);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("[SimpleConfig]WriteValue Error: " + e.Message);
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                        f = null;
                    }
                }
            }
        }

        private static bool OpenFile(string path, FileMode fileMode, FileShare share, TimeSpan timeout, out Stream stream)
        {
            var endTime = DateTime.Now + timeout;
            while (DateTime.Now < endTime)
            {
                try
                {
                    stream = File.Open(path, fileMode, FileAccess.ReadWrite, share);
                    return true;
                }
                catch (IOException e)
                {
                    //ignore this
                }
            }
            stream = null;
            return false;
        }
    }
}