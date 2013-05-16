using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Skylabs.Net
{
    [Serializable]
    public class NameValuePair
    {
        public NameValuePair()
        {
            Key = "";
            Value = "";
        }

        public NameValuePair(string key)
        {
            Key = key;
            Value = "";
        }

        public NameValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public NameValuePair(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public object Value { get; set; }
    }

    [Serializable]
    public class SocketMessage : ICloneable
    {
        private NameValuePair[] _data;

        public SocketMessage(string header)
        {
            Header = header;
            Data = new NameValuePair[0];
        }

        public NameValuePair[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string Header { get; set; }

        public object this[string key]
        {
            get { return (from t in _data where t.Key == key select t.Value).FirstOrDefault(); }
            set
            {
                foreach (NameValuePair t in _data.Where(t => t.Key == key))
                {
                    t.Value = value;
                }
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            var sm = new SocketMessage(Header.Clone() as String) {Data = Data.Clone() as NameValuePair[]};
            return sm;
        }

        #endregion

        public void AddData(NameValuePair data)
        {
            Array.Resize(ref _data, Data.Length + 1);
            _data[_data.Length - 1] = data;
        }

        public void AddData(string key, object value)
        {
            Array.Resize(ref _data, Data.Length + 1);
            _data[_data.Length - 1] = new NameValuePair(key, value);
        }

        public static byte[] Serialize(SocketMessage message)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, message);
                    ms.Flush();
                    return ms.ToArray();
                }
                catch (Exception e)
                {
                    Trace.TraceError("sm1:" + e.Message, e);
                }
            }
            return null;
        }

        public static SocketMessage Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                try
                {
                    var sm = bf.Deserialize(ms) as SocketMessage;
                    return sm;
                }
                catch (Exception e)
                {
                    Trace.TraceError("sm0:" + e.Message, e);
                    return null;
                }
            }
        }
    }
}