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
        public static string ReadValue(string valName)
        {
            Stream f = null;
            try
            {
                if (File.Exists(GetPath()))
                {
                    var serializer = new SharpSerializer();
                    TryOpen(GetPath(), FileMode.OpenOrCreate, TimeSpan.FromSeconds(2), out f);
                    var config = (Hashtable)serializer.Deserialize(f);
                    if (config.ContainsKey(valName))
                    {
                        return config[valName].ToString();
                    }
                }
            }
            catch(Exception e)
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
                    UnlockStream(f);
                    f.Close();
                    f = null;
                }
            }
            return null;
        }

        public static string ReadValue(string valName, string d)
        {
            string r = ReadValue(valName);
            if(r == null)
                WriteValue(valName,d);
            return r ?? d;
        }

        /// <summary>
        ///   Writes a string value to the Octgn registry
        /// </summary>
        /// <param name="valName"> Name of the value </param>
        /// <param name="value"> String to write for value </param>
        public static void WriteValue(string valName, string value)
        {
            Stream f = null;
            try
            {
                var serializer = new SharpSerializer();
                var config = new Hashtable();
                if (File.Exists(GetPath()))
                {
                    TryOpen(GetPath(), FileMode.OpenOrCreate, TimeSpan.FromSeconds(2), out f);
                    LockStream(f);
                    config = (Hashtable)serializer.Deserialize(f);
                }
                else
                {
                    TryOpen(GetPath(), FileMode.OpenOrCreate, TimeSpan.FromSeconds(2), out f);
                    LockStream(f);
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
                    UnlockStream(f);
                    f.Close();
                    f = null;
                }
            }

        }

        private static void LockStream(Stream f)
        {
            FileStream file = (FileStream)f;
            file.Lock(0, f.Length);
        }

        private static void UnlockStream(Stream f)
        {
            FileStream file = (FileStream)f;
            file.Unlock(0, f.Length);
        }

        public static bool TryOpen(string path, FileMode fileMode, TimeSpan timeout, out Stream stream)
        {
            var endTime = DateTime.Now + timeout;
            while (DateTime.Now < endTime)
            {
                if (TryOpen(path, fileMode, out stream))
                    return true;
            }
            stream = null;
            return false;
        }

        public static bool TryOpen(string path, FileMode fileMode, out Stream stream)
        {
            try
            {
                stream = File.Open(path, fileMode);
                return true;
            }
            catch (IOException e)
            {
                if (!FileIsLocked(e))
                    throw;

                stream = null;
                return false;
            }
        }

        public static bool FileIsLocked(IOException ioException)
        {
            var errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }
    }
}