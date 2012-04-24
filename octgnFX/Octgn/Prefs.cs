using System;
using System.IO;
using Octgn.Data;
namespace Octgn
{
    public static class Prefs
    {
        private static string _hideLoginNotifications;

        static Prefs()
        {
            string hln = SimpleConfig.ReadValue("Options_HideLoginNotifications");
            _hideLoginNotifications = hln == null || hln == "false" ? "false" : "true";
        }

        public static string HideLoginNotifications
        {
            get { return _hideLoginNotifications; }
            set
            {
                _hideLoginNotifications = value;
                SimpleConfig.WriteValue("Options_HideLoginNotifications", value);
            }
        }
        
        public static string DataDirectory
        {
            get
            {
                return SimpleConfig.ReadValue("datadirectory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn"));
            }
            set
            {
                SimpleConfig.WriteValue("datadirectory",value);
            }
        }

        public static string LastRoomName
        {
            get { return SimpleConfig.ReadValue("lastroomname" , Skylabs.Lobby.Randomness.RandomRoomName()); }
            set
            {
                SimpleConfig.WriteValue("lastroomname",value);
            }
        }
    }
}