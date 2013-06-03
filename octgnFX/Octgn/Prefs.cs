﻿using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Octgn.Data;
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
                bool ret = true;
                bool.TryParse(SimpleConfig.Get().ReadValue("InstallOnBoot", true.ToString(CultureInfo.InvariantCulture)), out ret);
                return ret;
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
                bool ret = false;
                bool present = bool.TryParse(SimpleConfig.Get().ReadValue("CleanDatabase", true.ToString(CultureInfo.InvariantCulture)), out ret);
                if (!present)
                {
                    ret = true;
                }
                return (ret);
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
                return bool.Parse(SimpleConfig.Get().ReadValue("EnableWhisperSound", "true"));
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
                return bool.Parse(SimpleConfig.Get().ReadValue("EnableNameSound", "true"));
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
                return int.Parse(SimpleConfig.Get().ReadValue("MaxChatHistory", "100"));
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
                return bool.Parse(SimpleConfig.Get().ReadValue("EnableChatImages", "true"));
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
                return bool.Parse(SimpleConfig.Get().ReadValue("EnableChatGifs", "true"));
            }
            set
            {
                SimpleConfig.Get().WriteValue("EnableChatGifs", value.ToString());
            }
        }

        public static bool getFilterGame(string name)
        {
            bool ret = true;
            if (!Boolean.TryParse(SimpleConfig.Get().ReadValue("FilterGames_" + name, "true"), out ret))
            {
                ret = true;
                SimpleConfig.Get().WriteValue("FilterGames_" + name, true.ToString(CultureInfo.InvariantCulture));
            }
            return ret;
        }
        
        public static void setFilterGame(string name, bool value)
        {
            SimpleConfig.Get().WriteValue("FilterGames_" + name, value.ToString(CultureInfo.InvariantCulture));
        }

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
                bool val = true;
                if (!Boolean.TryParse(SimpleConfig.Get().ReadValue("twosidedtable", true.ToString(CultureInfo.InvariantCulture)), out val))
                {
                    SimpleConfig.Get().WriteValue("twosidedtable", true.ToString(CultureInfo.InvariantCulture));
                }
                return val;
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
                var y = SimpleConfig.Get().ReadValue("LoginTopLoc", "100");
                var x = SimpleConfig.Get().ReadValue("LoginLeftLoc", "100");
                double dx, dy = 100;
                Double.TryParse(x, out dx);
                Double.TryParse(y, out dy);
                return new Point(dx, dy);
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
                var y = SimpleConfig.Get().ReadValue("MainTopLoc", "100");
                var x = SimpleConfig.Get().ReadValue("MainLeftLoc", "100");
                double dx, dy = 100;
                Double.TryParse(x, out dx);
                Double.TryParse(y, out dy);
                return new Point(dx, dy);
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
                var t = SimpleConfig.Get().ReadValue("LoginTimeout", "30000");
                var to = 30000;
                int.TryParse(t, out to);
                return to;
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
                var val = SimpleConfig.Get().ReadValue("LightChat", "false");
                var valout = false;
                bool.TryParse(val, out valout);
                return valout;
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
                var val = SimpleConfig.Get().ReadValue("UseHardwareRendering", "true");
                var valout = false;
                bool.TryParse(val, out valout);
                return valout;
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
                var val = SimpleConfig.Get().ReadValue("UseWindowTransparency", "false");
                var valout = false;
                bool.TryParse(val, out valout);
                return valout;
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
                var val = SimpleConfig.Get().ReadValue("EnableGameSound", "true");
                var vout = true;
                bool.TryParse(val, out vout);
                return vout;
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
                var val = SimpleConfig.Get().ReadValue("TileWindowSkin", "false");
                var vout = false;
                bool.TryParse(val, out vout);
                return vout;
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
    }
}