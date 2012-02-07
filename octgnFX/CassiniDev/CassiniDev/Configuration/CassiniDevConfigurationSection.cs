using System.Configuration;

namespace CassiniDev.Configuration
{
    public class CassiniDevConfigurationSection : ConfigurationSection
    {
        public static CassiniDevConfigurationSection Instance
        {
            get { return (CassiniDevConfigurationSection) ConfigurationManager.GetSection("cassinidev"); }
        }

        [ConfigurationProperty("profiles")]
        public CassiniDevProfileElementCollection Profiles
        {
            get { return (CassiniDevProfileElementCollection) this["profiles"]; }
        }
    }
}