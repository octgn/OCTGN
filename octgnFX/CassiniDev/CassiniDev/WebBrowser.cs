// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.IO;
using Microsoft.Win32;

namespace CassiniDev
{
    /// <summary>
    /// A simple user agent locator - derived from Nikhil Kothari's Script#
    /// http://projects.nikhilk.net/ScriptSharp
    /// I would have written it, but how many ways are there to get paths from
    /// the registry?
    /// 
    /// TODO:  Add Opera
    /// </summary>
    public sealed class WebBrowser
    {
        private readonly string _executablePath;
        private readonly string _name;
        public static readonly WebBrowser Chrome = new WebBrowser("Chrome", GetChromeExecutablePath());
        public static readonly WebBrowser Firefox = new WebBrowser("Firefox", GetFirefoxExecutablePath());
        public static readonly WebBrowser InternetExplorer = new WebBrowser("Internet Explorer", "iexplore.exe");
        public static readonly WebBrowser Safari = new WebBrowser("Safari", GetSafariExecutablePath());
        public static readonly WebBrowser Opera = new WebBrowser("Opera", GetOperaExecutablePath());


        //HKEY_CURRENT_USER\Software\Opera Software
        private WebBrowser(string name, string executablePath)
        {
            _name = name;
            _executablePath = executablePath;
        }
        private static string GetOperaExecutablePath()
        {
            string path = null;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Opera Software\");
            if (key != null)
            {
                path = (string)key.GetValue("Last CommandLine v2");
                if (!File.Exists(path))
                {
                    path = null;
                }
            }
            return path;
        }
        private static string GetChromeExecutablePath()
        {
            string path = null;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome\");
            if (key != null)
            {
                path = Path.Combine((string)key.GetValue("InstallLocation"), "chrome.exe");
                if (!File.Exists(path))
                {
                    path = null;
                }
            }
            return path;
        }

        private static string GetFirefoxExecutablePath()
        {
            string path = null;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Mozilla\Mozilla Firefox");
            if (key != null)
            {
                var str2 = (string)key.GetValue("CurrentVersion");
                if (!string.IsNullOrEmpty(str2))
                {
                    RegistryKey key2 = key.OpenSubKey(string.Format(@"{0}\Main", str2));
                    if (key2 == null)
                    {
                        return path;
                    }
                    path = (string)key2.GetValue("PathToExe");
                    if (!File.Exists(path))
                    {
                        path = null;
                    }
                }
                return path;
            }
            string str3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla FireFox\FireFox.exe");
            if (File.Exists(str3))
            {
                return str3;
            }
            str3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + " (x86)", @"Mozilla FireFox\FireFox.exe");
            if (!File.Exists(str3))
            {
                path = str3;
            }
            return path;
        }

        private static string GetSafariExecutablePath()
        {
            string path = null;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Apple Computer, Inc.\Safari");
            if (key != null)
            {
                path = (string)key.GetValue("BrowserExe");
                if (!File.Exists(path))
                {
                    path = null;
                }
            }
            return path;
        }

        internal string ExecutablePath
        {
            get
            {
                return _executablePath;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}