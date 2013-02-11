namespace Octgn.Online.Library.UpdateManager
{
    using System.Configuration;

    public class UpdateManagerConfig : ConfigurationSection
    {
        public UpdateManagerConfig()
        {
            
        }
        [ConfigurationProperty("UpdateFrequency")]
        public int UpdateFrequency
        {
            get
            {
                return (int)this["UpdateFrequency"];
            }
            set
            {
                this["UpdateFrequency"] = value;
            }
        }
        [ConfigurationProperty("UpdateFeed")]
        public string UpdateFeed
        {
            get
            {
                return (string)this["UpdateFeed"];
            }
            set
            {
                this["UpdateFeed"] = value;
            }
        }
        [ConfigurationProperty("PackageName")]
        public string PackageName
        {
            get
            {
                return (string)this["PackageName"];
            }
            set
            {
                this["PackageName"] = value;
            }
        }
        [ConfigurationProperty("Enabled",DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["Enabled"];
            }
            set
            {
                this["Enabled"] = value;
            }
        }
    }
}
