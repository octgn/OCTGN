using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Octgn.Data;
using Octgn.Extentions;

namespace Octgn
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Controls;

    using Octgn.Library;

    public static class Prefs
    {
        public enum ZoomType : byte { OriginalOrProxy=0,OriginalAndProxy=1,ProxyOnKeypress=2 };

        private static bool _hideLoginNotifications;

        static Prefs()
        {
            _hideLoginNotifications = SimpleConfig.Get().ReadValue("Options_HideLoginNotifications", false);
        }

        public static bool InstallOnBoot
        {
            get
            {
				return SimpleConfig.Get().ReadValue("InstallOnBoot", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("InstallOnBoot", value);
            }
        }

        public static bool CleanDatabase
        {
            get
            {
				return SimpleConfig.Get().ReadValue("CleanDatabase", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("CleanDatabase", value);
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
				return SimpleConfig.Get().ReadValue("EnableWhisperSound", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableWhisperSound", value);
            }
        }

        public static bool EnableNameSound
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableNameSound", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableNameSound", value);
            }
        }

        public static ZoomType ZoomOption
        {
            get
            {
                return SimpleConfig.Get().ReadValue("ZoomOption", ZoomType.OriginalOrProxy);
            }
            set
            {
                SimpleConfig.Get().WriteValue("ZoomOption", value);
            }
        }

        public static int MaxChatHistory
        {
            get
            {
	            return SimpleConfig.Get().ReadValue("MaxChatHistory", 100);
            }
            set
            {
                SimpleConfig.Get().WriteValue("MaxChatHistory", value);
            }
        }

        public static bool EnableChatImages
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableChatImages", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableChatImages", value);
            }
        }

        public static bool EnableChatGifs
        {
            get
            {
				return SimpleConfig.Get().ReadValue("EnableChatGifs", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableChatGifs", value);
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

        public static bool HideLoginNotifications
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
				return SimpleConfig.Get().ReadValue("twosidedtable", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("twosidedtable", value);
            }
        }

        public static Point LoginLocation
        {
            get
            {
                return SimpleConfig.Get().ReadValue("LoginLoc", new Point(100, 100));
            }
            set
            {
                SimpleConfig.Get().WriteValue("LoginLoc",value);
            }
        }

        public static Point MainLocation
        {
            get
            {
                return SimpleConfig.Get().ReadValue("MainLoc", new Point(100, 100));
            }
            set
            {
                SimpleConfig.Get().WriteValue("MainLoc",value);
            }
        } 

        public static int LoginTimeout
        {
            get
            {
	            return SimpleConfig.Get().ReadValue("LoginTimeout", 30000);
            }
            set
            {
                SimpleConfig.Get().WriteValue("LoginTimeout", value);
            }
        }

        public static bool UseLightChat
        {
            get
            {
                return SimpleConfig.Get().ReadValue("LightChat", false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("LightChat", value);
            }
        }

        public static bool UseHardwareRendering
        {
            get
            {
                return SimpleConfig.Get().ReadValue("UseHardwareRendering", true);
            }
            set
            {
                SimpleConfig.Get().WriteValue("UseHardwareRendering", value);
            }
        }

        public static bool UseWindowTransparency
        {
            get
            {
                return SimpleConfig.Get().ReadValue("UseWindowTransparency", false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("UseWindowTransparency", value);
            }
        }

        public static bool EnableGameSound
        {
            get
            {
                return SimpleConfig.Get().ReadValue("EnableGameSound", false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableGameSound",value);
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
                return SimpleConfig.Get().ReadValue("TileWindowSkin", false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("TileWindowSkin", value);
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
				return SimpleConfig.Get().ReadValue("HideUninstalledGamesInList", false);
			}
			set
			{
				SimpleConfig.Get().WriteValue("HideUninstalledGamesInList", value);
			}
	    }

        public static bool IgnoreSSLCertificates
        {
            get
            {
                return SimpleConfig.Get().ReadValue("IgnoreSSLCertificates", false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("IgnoreSSLCertificates", value);
            }
        }

        public static T GetGameSetting<T>(Octgn.DataNew.Entities.Game game, string propName, T def)
        {
            var defSettings = new Hashtable();
            defSettings["name"] = game.Name;
            var settings = SimpleConfig.Get().ReadValue("GameSettings_" + game.Id.ToString(), defSettings);

            if (settings.ContainsKey(propName))
            {
                if (settings[propName] is T)
                    return (T)settings[propName];
            }
            SetGameSetting(game, propName, def);
            return def;
        }

        public static void SetGameSetting<T>(DataNew.Entities.Game game, string propName, T val)
        {
            var defSettings = new Hashtable();
            defSettings["name"] = game.Name;
            var settings = SimpleConfig.Get().ReadValue("GameSettings_" + game.Id.ToString(), defSettings);

            if(!settings.ContainsKey(propName))
                settings.Add(propName,val);
            else
                settings[propName] = val;

            SimpleConfig.Get().WriteValue("GameSettings_" + game.Id.ToString(),settings);
        }

        public static bool UseWindowsForChat
        {
            get
            {
                return SimpleConfig.Get().ReadValue("UseWindowsForChat", false);
            }
            set
            {
                SimpleConfig.Get().WriteValue("UseWindowsForChat",value);
            }
        }

        public static int ChatFontSize
        {
            get { return SimpleConfig.Get().ReadValue("ChatFontSize", 12); }
            set { SimpleConfig.Get().WriteValue("ChatFontSize", value); }
        }

        public static bool InstantSearch
        {
            get { return SimpleConfig.Get().ReadValue("InstantSearch", true); }
            set { SimpleConfig.Get().WriteValue("InstantSearch", value); }
        }

        public static bool AcceptedCustomDataAgreement
        {
            get{return SimpleConfig.Get().ReadValue("AcceptedCustomDataAgreement",false);}
            set{SimpleConfig.Get().WriteValue("AcceptedCustomDataAgreement",value);}
        }

        public static string CustomDataAgreementHash
        {
            get { return SimpleConfig.Get().ReadValue("CustomDataAgreementHash", ""); }
            set { SimpleConfig.Get().WriteValue("CustomDataAgreementHash", value); }
        }
    }
}