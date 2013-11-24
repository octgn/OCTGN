namespace Octgn.Library
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;

    using log4net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Octgn.Library.ExtensionMethods;

    public class ConfigFile : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Stream FileStream { get; internal set; }

        public bool OpenedFile { get; internal set; }

        internal Hashtable ConfigData { get; set; }

        public ConfigFile()
        {
            Stream fs;
            OpenedFile = X.Instance.File.OpenFile(
                Config.Instance.ConfigPath,
                FileMode.OpenOrCreate,
                FileShare.None,
                TimeSpan.FromSeconds(10000),
                out fs);
            FileStream = fs;

            ConfigData = new Hashtable();

            if (!OpenedFile) return;

            try
            {
                var sr = new StreamReader(FileStream);
                var configString = sr.ReadToEnd();

                ConfigData = (JsonConvert.DeserializeObject<Hashtable>(configString) ?? new Hashtable());
            }
            catch (Exception e)
            {
				Log.Warn("Error reading config file",e);
                ConfigData = new Hashtable();
            }
        }

        public bool Contains(string key)
        {
			if(!OpenedFile)
				return false;

            return ConfigData.ContainsKey(key);
        }

        public void Add<T>(string key, T val)
        {
            if (!OpenedFile) return;

            if (typeof(T).IsEnum)
            {
                ConfigData.Add(key,val.ToString());
            }
            else if (typeof(T).IsIntegerType(true)) 
                ConfigData.Add(key, val.ToString());
			else
                ConfigData.Add(key,val);
        }

        public void AddOrSet<T>(string key, T val)
        {
            if (!OpenedFile) return;

            if (typeof(T).IsEnum)
            {
                ConfigData[key] = val.ToString();
            }
            else if (typeof(T).IsIntegerType(true))
                ConfigData[key] = val.ToString();
            else
                ConfigData[key] = val;
        }

        public bool Get<T>(string key, out T val)
        {
            val = default(T);
            if (!OpenedFile) return false;

            if (typeof(T).IsEnum)
            {
                val = (T)Enum.Parse(typeof(T),ConfigData[key].ToString());
            }
            else if (typeof(T).IsIntegerType(true))
                val = (T)((object)int.Parse(ConfigData[key].ToString()));
			else if (typeof(T) == typeof(Hashtable))
			{
			    var jo = (JObject)ConfigData[key];
			    var ret = jo.ToObject<Hashtable>();
			    val = (T)((object)ret);
			}
			else if (typeof(T) == typeof(ulong))
			{
			    dynamic jo = (ulong)(long)ConfigData[key];
			    val = (T)jo;
			}
            else
                val = (T)ConfigData[key];

            return true;
        }

        public void Save()
        {
            if (!OpenedFile) return;

            try
            {
				FileStream.SetLength(0);
                var configString = JsonConvert.SerializeObject(ConfigData,Formatting.Indented);

                var sw = new StreamWriter(FileStream);
				sw.Write(configString);
				sw.Flush();
            }
            catch (Exception e)
            {
                Log.Warn("Error writing config file", e);
            }
        }

        public void Dispose()
        {
            if (FileStream != null)
            {
                X.Instance.Try(FileStream.Dispose);
                FileStream = null;
            }

            ConfigData.Clear();
            ConfigData = null;
        }
    }
}