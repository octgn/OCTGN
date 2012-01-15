using System.Configuration;

namespace CassiniDev.Configuration
{
    [ConfigurationCollection(typeof(PluginElement))]
    public class PluginElementCollection: ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginElement)element).Name;
        }
    }
}