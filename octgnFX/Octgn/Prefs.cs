using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Octgn.Data;
using Octgn.Extentions;

namespace Octgn
{
    using System.Windows.Controls;

    using Octgn.Library;

    public static class Prefs
    {
        private static string _hideLoginNotifications;

        static Prefs()
        {
            _hideLoginNotifications = SimpleConfig.Get().ReadValue("Options_HideLoginNotifications", "false");
        }

        public static bool InstallOnBoot
        {
            get
            {
				return SimpleConfig.Get().ReadValue("InstallOnBoot", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("InstallOnBoot", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static bool CleanDatabase
        {
            get
            {
				return SimpleConfig.Get().ReadValue("CleanDatabase", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("CleanDatabase", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static string Password
        {
            get { return SimpleConfig.Get().ReadValue("Password", ""); }
            set
            {
                SimpleConfig.Get().WriteValue("Password", value);
            }
        }

        public static string Username
        {
            get { return SimpleConfig.Get().ReadValue("Username", ""); }
            set
            {
                SimpleConfig.Get().WriteValue("Username", value);
            }
        }

        public static bool EnableWhisperSound
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableWhisperSound", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableWhisperSound", value.ToString());
            }
        }

        public static bool EnableNameSound
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableNameSound", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableNameSound", value.ToString());
            }
        }

        public static int MaxChatHistory
        {
            get
            {
	            return SimpleConfig.Get().ReadValue("MaxChatHistory", "100").ToInt32(100);
            }
            set
            {
                SimpleConfig.Get().WriteValue("MaxChatHistory", value.ToString());
            }
        }

        public static bool EnableChatImages
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableChatImages", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableChatImages", value.ToString());
            }
        }

        public static bool EnableChatGifs
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableChatGifs", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableChatGifs", value.ToString());
            }
        }

		//public static bool getFilterGame(string name)
		//{
		//	bool ret = true;
		//	if (!Boolean.TryParse(SimpleConfig.Get().ReadValue("FilterGames_" + name, "true"), out ret))
		//	{
		//		ret = true;
		//		SimpleConfig.Get().WriteValue("FilterGames_" + name, true.ToString(CultureInfo.InvariantCulture));
		//	}
		//	return ret;
		//}
        
		//public static void setFilterGame(string name, bool value)
		//{
		//	SimpleConfig.Get().WriteValue("FilterGames_" + name, value.ToString(CultureInfo.InvariantCulture));
		//}

        public static string Nickname
        {
            get { return SimpleConfig.Get().ReadValue("Nickname", "null"); }
            set
            {
                SimpleConfig.Get().WriteValue("Nickname", value);
            }
        }

        public static string LastFolder
        {
            get
            {
                return SimpleConfig.Get().ReadValue("lastFolder", "");
            }
            set
            {
                SimpleConfig.Get().WriteValue("lastFolder", value);
            }
        }

        public static string HideLoginNotifications
        {
            get { return _hideLoginNotifications; }
            set
            {
                _hideLoginNotifications = value;
                SimpleConfig.Get().WriteValue("Options_HideLoginNotifications", value);
            }
        }
        
        public static string DataDirectory
        {
            get { return SimpleConfig.Get().DataDirectory; }
            set { SimpleConfig.Get().DataDirectory = value; }
        }

        public static string LastRoomName
        {
	        get { return SimpleConfig.Get().ReadValue("lastroomname", Skylabs.Lobby.Randomness.RandomRoomName()); }
	        set
            {
                SimpleConfig.Get().WriteValue("lastroomname", value);
            }
        }

        public static Guid LastHostedGameType
        {
            get
            {
                var ret = Guid.Empty;
                if (Guid.TryParse(SimpleConfig.Get().ReadValue("lasthostedgametype", Guid.Empty.ToString()), out ret)) return ret;
                return Guid.Empty;
            }
            set
            {
                SimpleConfig.Get().WriteValue("lasthostedgametype", value.ToString());
            }
        }
        
        public static bool TwoSidedTable
        {
            get
            {
				return SimpleConfig.Get().ReadValue("twosidedtable", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("twosidedtable", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static Point LoginLocation
        {
            get
            {
                var y = SimpleConfig.Get().ReadValue("LoginTopLoc", "100").ToDouble(100);
                var x = SimpleConfig.Get().ReadValue("LoginLeftLoc", "100").ToDouble(100);
                return new Point(x, y);
            }
            set
            {
                SimpleConfig.Get().WriteValue("LoginTopLoc", value.Y.ToString(CultureInfo.InvariantCulture));
                SimpleConfig.Get().WriteValue("LoginLeftLoc", value.X.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static Point MainLocation
        {
            get
            {
	            var y = SimpleConfig.Get().ReadValue("MainTopLoc", "100").ToDouble(100);
	            var x = SimpleConfig.Get().ReadValue("MainLeftLoc", "100").ToDouble(100);
	            return new Point(x, y);
            }
            set
            {
                SimpleConfig.Get().WriteValue("MainTopLoc", value.Y.ToString(CultureInfo.InvariantCulture));
                SimpleConfig.Get().WriteValue("MainLeftLoc", value.X.ToString(CultureInfo.InvariantCulture));
            }
        } 

        public static int LoginTimeout
        {
            get
            {
	            return SimpleConfig.Get().ReadValue("LoginTimeout", "30000").ToInt32(30000);
            }
            set
            {
                SimpleConfig.Get().WriteValue("LoginTimeout", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static bool UseLightChat
        {
            get
            {
                return SimpleConfig.Get().ReadValue("LightChat", "false").ToBool(false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("LightChat", value.ToString());
            }
        }

        public static bool UseHardwareRendering
        {
            get
            {
                return SimpleConfig.Get().ReadValue("UseHardwareRendering", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("UseHardwareRendering", value.ToString());
            }
        }

        public static bool UseWindowTransparency
        {
            get
            {
                return SimpleConfig.Get().ReadValue("UseWindowTransparency", "false").ToBool(false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("UseWindowTransparency", value.ToString());
            }
        }

        public static bool EnableGameSound
        {
            get
            {
                return SimpleConfig.Get().ReadValue("EnableGameSound", "true").ToBool(true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableGameSound",value.ToString());
            }
        }

        public static string WindowSkin
        {
            get
            {
                var fpath =  SimpleConfig.Get().ReadValue("WindowSkin", "");
                if (string.IsNullOrWhiteSpace(fpath)) return fpath;
                if (!File.Exists(fpath))
                {
                    WindowSkin = "";
                    return "";
                }
                return fpath;

            }
            set
            {
                SimpleConfig.Get().WriteValue("WindowSkin",value);
            }
        }

        public static bool TileWindowSkin
        {
            get
            {
                return SimpleConfig.Get().ReadValue("TileWindowSkin", "false").ToBool(false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("TileWindowSkin", value.ToString());
            }
        }

        public static string DefaultGameBack
        {
            get
            {
                var fpath = SimpleConfig.Get().ReadValue("DefaultGameBack", "");
                if (string.IsNullOrWhiteSpace(fpath)) return fpath;
                if (!File.Exists(fpath))
                {
                    DefaultGameBack = "";
                    return "";
                }
                return fpath;
            }
            set
            {
                SimpleConfig.Get().WriteValue("DefaultGameBack", value);
            }
        }

	    public static bool HideUninstalledGamesInList
	    {
			get
			{
				return SimpleConfig.Get().ReadValue("HideUninstalledGamesInList", "false").ToBool(false);
			}
			set
			{
				SimpleConfig.Get().WriteValue("HideUninstalledGamesInList", value.ToString());
			}
	    }

    }
}