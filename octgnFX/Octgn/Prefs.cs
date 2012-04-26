using System;
using System.Globalization;
using System.IO;
using System.Windows;
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

        public static bool InstallOnBoot
        {
            get
            {
                bool ret = true;
                bool.TryParse(SimpleConfig.ReadValue("InstallOnBoot", true.ToString(CultureInfo.InvariantCulture)), out ret);
                return ret;
            }
            set
            {
                SimpleConfig.WriteValue("InstallOnBoot", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static string Password
        {
            get { return SimpleConfig.ReadValue("Password" , ""); }
            set
            {
                SimpleConfig.WriteValue("Password",value);
            }
        }

        public static string Username
        {
            get { return SimpleConfig.ReadValue("Username", ""); }
            set
            {
                SimpleConfig.WriteValue("Username", value);
            }
        }        

        public static bool getFilterGame(string name)
        {
            bool ret = true;
            if (!Boolean.TryParse(SimpleConfig.ReadValue("FilterGames_" + name, "true"), out ret))
            {
                ret = true;
                SimpleConfig.WriteValue("FilterGames_" + name , true.ToString(CultureInfo.InvariantCulture));
            }
            return ret;
        }
        
        public static void setFilterGame(string name, bool value)
        {
            SimpleConfig.WriteValue("FilterGames_" + name, value.ToString(CultureInfo.InvariantCulture));
        }

        public static string Nickname
        {
            get { return SimpleConfig.ReadValue("Nickname" , "null"); }
            set
            {
                SimpleConfig.WriteValue("Nickname",value);
            }
        }

        public static string LastFolder
        {
            get
            {
                return SimpleConfig.ReadValue("lastFolder" ,"");

            }
            set
            {
                SimpleConfig.WriteValue("lastFolder",value);
            }
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
            get { return SimpleConfig.DataDirectory; }
            set { SimpleConfig.DataDirectory = value; }
        }

        public static string LastRoomName
        {
            get { return SimpleConfig.ReadValue("lastroomname" , Skylabs.Lobby.Randomness.RandomRoomName()); }
            set
            {
                SimpleConfig.WriteValue("lastroomname",value);
            }
        }
        
        public static bool TwoSidedTable
        {
            get
            {
                bool val = true;
                if(!Boolean.TryParse(SimpleConfig.ReadValue("twosidedtable" , true.ToString(CultureInfo.InvariantCulture)),out val))
                {
                    SimpleConfig.WriteValue("twosidedtable",true.ToString(CultureInfo.InvariantCulture));
                }
                return val;
            }
            set
            {
                SimpleConfig.WriteValue("twosidedtable",value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static Point LoginLocation
        {
            get
            {
                var y = SimpleConfig.ReadValue("LoginTopLoc", "100");
                var x = SimpleConfig.ReadValue("LoginLeftLoc", "100");
                double dx, dy = 100;
                Double.TryParse(x, out dx);
                Double.TryParse(y, out dy);
                return new Point(dx, dy);
            }
            set
            {
                SimpleConfig.WriteValue("LoginTopLoc", value.Y.ToString(CultureInfo.InvariantCulture));
                SimpleConfig.WriteValue("LoginLeftLoc", value.X.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static Point MainLocation
        {
            get
            {
                var y = SimpleConfig.ReadValue("MainTopLoc", "100");
                var x = SimpleConfig.ReadValue("MainLeftLoc", "100");
                double dx, dy = 100;
                Double.TryParse(x, out dx);
                Double.TryParse(y, out dy);
                return new Point(dx, dy);
            }
            set
            {
                SimpleConfig.WriteValue("MainTopLoc", value.Y.ToString(CultureInfo.InvariantCulture));
                SimpleConfig.WriteValue("MainLeftLoc", value.X.ToString(CultureInfo.InvariantCulture));
            }
        } 
    }
}