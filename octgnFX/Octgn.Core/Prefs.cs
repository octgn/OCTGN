namespace Octgn.Core
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Windows;

    using Octgn.Core.Util;
    using Octgn.Library;

    public static class Prefs
    {
        public enum ZoomType : byte { OriginalOrProxy,OriginalAndProxy,ProxyOnKeypress };
        public enum CardAnimType : byte { None, NormalAnimation, MinimalAnimation };

        static Prefs()
        {
        }

        public static bool InstallOnBoot
        {
            get
            {
				return Config.Instance.ReadValue("InstallOnBoot", true);
            }
            set
            {
                Config.Instance.WriteValue("InstallOnBoot", value);
            }
        }

        public static bool CleanDatabase
        {
            get
            {
				return Config.Instance.ReadValue("CleanDatabase", true);
            }
            set
            {
                Config.Instance.WriteValue("CleanDatabase", value);
            }
        }

        public static string Password
        {
            get { return Config.Instance.ReadValue("Password", ""); }
            set
            {
                Config.Instance.WriteValue("Password", value);
            }
        }

        public static string Username
        {
            get { return Config.Instance.ReadValue("Username", ""); }
            set
            {
                Config.Instance.WriteValue("Username", value);
            }
        }

        public static bool EnableWhisperSound
        {
            get
            {
				return Config.Instance.ReadValue("EnableWhisperSound", true);
            }
            set
            {
                Config.Instance.WriteValue("EnableWhisperSound", value);
            }
        }

        public static bool EnableNameSound
        {
            get
            {
				return Config.Instance.ReadValue("EnableNameSound", true);
            }
            set
            {
                Config.Instance.WriteValue("EnableNameSound", value);
            }
        }

        public static ZoomType ZoomOption
        {
            get
            {
                return Config.Instance.ReadValue("ZoomOption", ZoomType.OriginalOrProxy);
            }
            set
            {
                Config.Instance.WriteValue("ZoomOption", value);
            }
        }

        public static CardAnimType CardMoveNotification
        {
            get
            {
                return Config.Instance.ReadValue("CardMoveNotification", CardAnimType.NormalAnimation);
            }
            set
            {
                Config.Instance.WriteValue("CardMoveNotification", value);
            }
        }

        public static int MaxChatHistory
        {
            get
            {
	            return Config.Instance.ReadValue("MaxChatHistory", 100);
            }
            set
            {
                Config.Instance.WriteValue("MaxChatHistory", value);
            }
        }

        public static bool EnableChatImages
        {
            get
            {
				return Config.Instance.ReadValue("EnableChatImages", true);
            }
            set
            {
                Config.Instance.WriteValue("EnableChatImages", value);
            }
        }

        public static bool EnableChatGifs
        {
            get
            {
				return Config.Instance.ReadValue("EnableChatGifs", true);
            }
            set
            {
                Config.Instance.WriteValue("EnableChatGifs", value);
            }
        }

        public static string Nickname
        {
            get { return Config.Instance.ReadValue("Nickname", "null"); }
            set
            {
                Config.Instance.WriteValue("Nickname", value);
            }
        }

        public static string LastFolder
        {
            get
            {
                return Config.Instance.ReadValue("lastFolder", "");
            }
            set
            {
                Config.Instance.WriteValue("lastFolder", value);
            }
        }
        
        public static string DataDirectory
        {
            get { return Config.Instance.DataDirectory; }
            set { Config.Instance.DataDirectory = value; }
        }

        public static string LastRoomName
        {
	        get { return Config.Instance.ReadValue<string>("lastroomname", null); }
	        set
            {
                Config.Instance.WriteValue("lastroomname", value);
            }
        }

        public static Guid LastHostedGameType
        {
            get
            {
                var ret = Guid.Empty;
                if (Guid.TryParse(Config.Instance.ReadValue("lasthostedgametype", Guid.Empty.ToString()), out ret)) return ret;
                return Guid.Empty;
            }
            set
            {
                Config.Instance.WriteValue("lasthostedgametype", value.ToString());
            }
        }
        
        public static bool TwoSidedTable
        {
            get
            {
				return Config.Instance.ReadValue("twosidedtable", true);
            }
            set
            {
                Config.Instance.WriteValue("twosidedtable", value);
            }
        }

        public static Point LoginLocation
        {
            get
            {
                return Config.Instance.ReadValue("LoginLoc", new Point(100, 100));
            }
            set
            {
                Config.Instance.WriteValue("LoginLoc",value);
            }
        }

        public static Point MainLocation
        {
            get
            {
                return Config.Instance.ReadValue("MainLoc", new Point(100, 100));
            }
            set
            {
                Config.Instance.WriteValue("MainLoc",value);
            }
        } 

        public static int LoginTimeout
        {
            get
            {
	            return Config.Instance.ReadValue("LoginTimeout", 30000);
            }
            set
            {
                Config.Instance.WriteValue("LoginTimeout", value);
            }
        }

        public static bool UseLightChat
        {
            get
            {
                return Config.Instance.ReadValue("LightChat", false);
            }
            set
            {
                Config.Instance.WriteValue("LightChat", value);
            }
        }

        public static bool UseHardwareRendering
        {
            get
            {
                return Config.Instance.ReadValue("UseHardwareRendering", true);
            }
            set
            {
                Config.Instance.WriteValue("UseHardwareRendering", value);
            }
        }

        public static bool UseWindowTransparency
        {
            get
            {
                return Config.Instance.ReadValue("UseWindowTransparency", false);
            }
            set
            {
                Config.Instance.WriteValue("UseWindowTransparency", value);
            }
        }

        public static bool EnableGameSound
        {
            get
            {
                return Config.Instance.ReadValue("ReallyEnableGameSound", true);
            }
            set
            {
                Config.Instance.WriteValue("ReallyEnableGameSound", value);
            }
        }

        public static string WindowSkin
        {
            get
            {
                var fpath =  Config.Instance.ReadValue("WindowSkin", "");
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
                Config.Instance.WriteValue("WindowSkin",value);
            }
        }

        public static bool TileWindowSkin
        {
            get
            {
                return Config.Instance.ReadValue("TileWindowSkin", false);
            }
            set
            {
                Config.Instance.WriteValue("TileWindowSkin", value);
            }
        }

        public static string DefaultGameBack
        {
            get
            {
                var fpath = Config.Instance.ReadValue("DefaultGameBack", "");
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
                Config.Instance.WriteValue("DefaultGameBack", value);
            }
        }

	    public static bool HideUninstalledGamesInList
	    {
			get
			{
				return Config.Instance.ReadValue("HideUninstalledGamesInList", false);
			}
			set
			{
				Config.Instance.WriteValue("HideUninstalledGamesInList", value);
			}
	    }

        public static bool IgnoreSSLCertificates
        {
            get
            {
                return Config.Instance.ReadValue("IgnoreSSLCertificates", false);
            }
            set
            {
                Config.Instance.WriteValue("IgnoreSSLCertificates", value);
            }
        }

        public static T GetGameSetting<T>(Octgn.DataNew.Entities.Game game, string propName, T def)
        {
            var defSettings = new Hashtable();
            defSettings["name"] = game.Name;
            var settings = Config.Instance.ReadValue("GameSettings_" + game.Id.ToString(), defSettings);

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
            var settings = Config.Instance.ReadValue("GameSettings_" + game.Id.ToString(), defSettings);

            if(!settings.ContainsKey(propName))
                settings.Add(propName,val);
            else
                settings[propName] = val;

            Config.Instance.WriteValue("GameSettings_" + game.Id.ToString(),settings);
        }

        public static bool UseWindowsForChat
        {
            get
            {
                return Config.Instance.ReadValue("UseWindowsForChat", false);
            }
            set
            {
                Config.Instance.WriteValue("UseWindowsForChat",value);
            }
        }

        public static int ChatFontSize
        {
            get { return Config.Instance.ReadValue("ChatFontSize", 12); }
            set { Config.Instance.WriteValue("ChatFontSize", value); }
        }

        public static bool InstantSearch
        {
            get { return Config.Instance.ReadValue("InstantSearch", true); }
            set { Config.Instance.WriteValue("InstantSearch", value); }
        }

        public static bool HideResultCount
        {
            get { return Config.Instance.ReadValue("HideResultCount", false); }
            set { Config.Instance.WriteValue("HideResultCount", value); }
        }

        public static bool AcceptedCustomDataAgreement
        {
            get{return Config.Instance.ReadValue("AcceptedCustomDataAgreement",false);}
            set{Config.Instance.WriteValue("AcceptedCustomDataAgreement",value);}
        }

        public static string CustomDataAgreementHash
        {
            get { return Config.Instance.ReadValue("CustomDataAgreementHash", ""); }
            set { Config.Instance.WriteValue("CustomDataAgreementHash", value); }
        }
        public static int LastLocalHostedGamePort
        {
            get { return Config.Instance.ReadValue("LastLocalHostedGamePort", 5000); }
            set { Config.Instance.WriteValue("LastLocalHostedGamePort", value); }
        }

        public static ulong PrivateKey
        {
            get { return Config.Instance.ReadValue("PrivateKey", ((ulong)Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom()); }
            set { Config.Instance.WriteValue("PrivateKey", value); }
        }
        public static bool EnableAdvancedOptions
        {
            get { return Config.Instance.ReadValue("EnableAdvancedOptions", false); }
            set { Config.Instance.WriteValue("EnableAdvancedOptions", value); }
        }
        public static bool UseGameFonts
        {
            get { return Config.Instance.ReadValue("UseGameFonts", false); }
            set { Config.Instance.WriteValue("UseGameFonts", value); }
        }

        public static bool SpectateGames
        {
            get { return Config.Instance.ReadValue("SpectateGames", false); }
            set { Config.Instance.WriteValue("SpectateGames", value); }    
        }

        public static bool UnderstandsChat
        {
            get { return Config.Instance.ReadValue("UnderstandsChat", false); }
            set { Config.Instance.WriteValue("UnderstandsChat", value); }    
        }

        public static bool EnableGameScripts
        {
            get { return Config.Instance.ReadValue("EnableGameScripts", true); }
            set { Config.Instance.WriteValue("EnableGameScripts", value); }
        }
        public static double HandDensity
        {
            get { return Config.Instance.ReadValue("HandDensity", 20d); }
            set { Config.Instance.WriteValue("HandDensity", value); }
        }
        public static bool HasSeenSpectateMessage
        {
            get { return Config.Instance.ReadValue("HasSeenSpectateMessage", false); }
            set { Config.Instance.WriteValue("HasSeenSpectateMessage", value); }
        }

        public static bool UsingWine
        {
            get { return Config.Instance.ReadValue("UsingWine", false); }
            set { Config.Instance.WriteValue("UsingWine", value); }
        }

        public static bool AskedIfUsingWine
        {
            get { return Config.Instance.ReadValue("AskedIfUsingWine", false); }
            set { Config.Instance.WriteValue("AskedIfUsingWine", value); }
        }
    }
}