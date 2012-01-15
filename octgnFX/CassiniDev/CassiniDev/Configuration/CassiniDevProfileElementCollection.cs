using System;
using System.Configuration;

namespace CassiniDev.Configuration
{
    [ConfigurationCollection(typeof(CassiniDevProfileElement))]
    public class CassiniDevProfileElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CassiniDevProfileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CassiniDevProfileElement)element).Port;
        }
    }
}