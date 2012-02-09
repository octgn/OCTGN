using System;
using System.Collections.Generic;

namespace CassiniDev
{
    [Serializable]
    public class BrowserTestResultItem
    {
        public BrowserTestResultItem()
        {
            Items = new Dictionary<string, BrowserTestResultItem>();
            Log = new List<string>();
        }

        public bool Success { get; set; }
        public string Name { get; set; }
        public int Failures { get; set; }
        public int Total { get; set; }
        public List<string> Log { get; set; }

        public Dictionary<string, BrowserTestResultItem> Items { get; set; }

        public virtual void Parse(string log)
        {
            throw new NotImplementedException();
        }
    }
}