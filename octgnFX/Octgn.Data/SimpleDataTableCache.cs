using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.IO;

namespace Octgn.Data
{
    class SimpleDataTableCache
    {

        private Dictionary<string, DataTable> _cache = null;
        private static SimpleDataTableCache _instance = null;
        private Dictionary<string, string> serializeTable = null;

        public SimpleDataTableCache()
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, DataTable>();
                serializeTable = new Dictionary<string, string>();
                LoadCache();
            }
        }

        public static SimpleDataTableCache GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SimpleDataTableCache();
            }
            return (_instance);
        }

        private string GetPath()
        {
            string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
            string cacheDir = Path.Combine(rootPath, "Cache");
            string sealedDir = Path.Combine(cacheDir, "Sealed");

            return (sealedDir);
        }

        private void EnsurePathExists()
        {
            string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
            string cacheDir = Path.Combine(rootPath, "Cache");
            string sealedDir = Path.Combine(cacheDir, "Sealed");
            if(!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
            if(!Directory.Exists(sealedDir))
            {
                Directory.CreateDirectory(sealedDir);
            }
        }

        private void LoadCache()
        {
            EnsurePathExists();
            string[] files = Directory.GetFiles(GetPath());
            foreach (string file in files)
            {
                using (Stream stream = File.OpenRead(file))
                {
                    DataTable temp = new DataTable();
                    temp.ReadXml(stream);
                    temp.TableName = string.Empty;
                    _cache.Add(DecodeFrom64(file.Substring(file.LastIndexOf("\\")+1)), temp);
                }
            }
            
        }


        private void SaveCacheFile()
        {
            if (_cache.Count > 0)
            {
                EnsurePathExists();
                foreach (KeyValuePair<string, DataTable> kvi in _cache)
                {
                    string fileName = EncodeTo64UTF8(kvi.Key);
                    using (Stream stream = File.Open(Path.Combine(GetPath(), fileName), FileMode.Create))
                    {
                        kvi.Value.TableName = kvi.Key;
                        kvi.Value.WriteXml(stream, XmlWriteMode.WriteSchema, true);
                    }
                }
            }
        }

        public string EncodeTo64UTF8(string m_enc)
        {
            byte[] toEncodeAsBytes =
            System.Text.Encoding.UTF8.GetBytes(m_enc);
            string returnValue =
            System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public string DecodeFrom64(string m_enc)
        {
            byte[] encodedDataAsBytes =
            System.Convert.FromBase64String(m_enc);
            string returnValue =
            System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }

        public static void ClearCache()
        {
            string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
            string cacheDir = Path.Combine(rootPath, "Cache");
            string sealedDir = Path.Combine(cacheDir, "Sealed");
            if (Directory.GetFiles(sealedDir).Length > 0)
            {
                Directory.Delete(sealedDir);
                Directory.CreateDirectory(sealedDir);
            }
        }

        public static bool CacheExists()
        {
            bool ret = false;

            string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
            string cacheDir = Path.Combine(rootPath, "Cache");
            string sealedDir = Path.Combine(cacheDir, "Sealed");
            ret = (Directory.GetFiles(sealedDir).Length > 0);

            return (ret);
        }

        public DataTable GetCache(string[] conditions)
        {
            DataTable ret = _cache[ConcentateConditions(conditions)];
            return (ret);
        }

        public void AddToCache(string[] conditions, DataTable table)
        {
            _cache.Add(ConcentateConditions(conditions), table.Copy());
            SaveCacheFile();
        }

        public bool IsCached(string[] conditions)
        {
            bool ret = _cache.ContainsKey(ConcentateConditions(conditions));
            return (ret);
        }

        private string ConcentateConditions(string[] conditions)
        {
            StringBuilder s = new StringBuilder();
            foreach (string condition in conditions)
            {
                s.Append(condition);
            }
            return (s.ToString());
        }
    }

 
}
