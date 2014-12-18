// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Logging;
using Octgn.Core;
using Octgn.Extentions;
using Octgn.Library;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;
using Octgn.Windows;

namespace Octgn
{
    public class SleeveManager
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static SleeveManager SingletonContext { get; set; }

        private static readonly object SleeveManagerSingletonLocker = new object();

        public static SleeveManager Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (SleeveManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new SleeveManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private ApiSleeve[] _sleeveCache;
        private DateTime _lastGetTime = DateTime.MinValue;

        public Task<IEnumerable<ApiSleeve>> GetAllSleevesAsync()
        {
            return Task.Factory.StartNew<IEnumerable<ApiSleeve>>(GetAllSleeves);
        }

        public Task<IEnumerable<ApiSleeve>> GetUserSleevesAsync()
        {
            return Task.Factory.StartNew<IEnumerable<ApiSleeve>>(GetUserSleeves);
        }

        public Task<bool> AddSleeveToAccountAsync(int id)
        {
            return Task.Factory.StartNew(() => AddSleeveToAccount(id));
        }

        public Task<ApiSleeve> SleeveFromIdAsync(int id)
        {
            return Task.Factory.StartNew(() => SleeveFromId(id));
        }

        public IEnumerable<ApiSleeve> GetAllSleeves()
        {
            if (_sleeveCache != null)
                return _sleeveCache;
            if (_lastGetTime >= DateTime.Now.AddMinutes(-1))
                return _sleeveCache ?? new ApiSleeve[0];
            try
            {
                _lastGetTime = DateTime.Now;
                var c = new Octgn.Site.Api.ApiClient();
                var sleeves = c.GetAllSleeves(0, 100);
                if (sleeves.Sleeves == null || sleeves.Sleeves.Length == 0)
                {
                    WindowManager.GrowlWindow.AddNotification(new ErrorNotification("There was a problem downloading the sleeve list from the web. Please check your internet settings."));
                    return new ApiSleeve[0];
                }
                _sleeveCache = sleeves.Sleeves.ToArray();
                return _sleeveCache;
            }
            catch (Exception e)
            {
                Log.Error("GetAllSleeves", e);
            }
            WindowManager.GrowlWindow.AddNotification(new ErrorNotification("There was a problem downloading the sleeve list from the web. Please check your internet settings."));
            return new ApiSleeve[0];
        }

        public IEnumerable<ApiSleeve> GetUserSleeves()
        {
            try
            {
                var c = new Octgn.Site.Api.ApiClient();

                AuthorizedResponse<IEnumerable<ApiSleeve>> ownedSleeves;
                ownedSleeves = Program.LobbyClient.IsConnected == false ? c.GetUserSleeves(Prefs.Username, Prefs.Password.Decrypt()) : c.GetUserSleeves(Program.LobbyClient.Username, Program.LobbyClient.Password);

                if (ownedSleeves.Authorized == false)
                {
                    Log.Warn("Could not download user sleeves, the account was not authorized.");
                    return new ApiSleeve[0];
                }
                if (ownedSleeves == null || ownedSleeves.Response == null)
                {
                    WindowManager.GrowlWindow.AddNotification(new ErrorNotification("There was a problem downloading the user sleeve list from the web. Please check your internet settings."));
                    return new ApiSleeve[0];
                }

                return ownedSleeves.Response;
            }
            catch (Exception e)
            {
                Log.Error("GetUserSleeves", e);
            }
            WindowManager.GrowlWindow.AddNotification(new ErrorNotification("There was a problem downloading the user sleeve list from the web. Please check your internet settings."));
            return new ApiSleeve[0];
        }

        public bool AddSleeveToAccount(int id)
        {
            try
            {
                var req = new UpdateUserSleevesRequest();
                req.Add(new ApiSleeve()
                {
                    Id = id
                });

                var c = new ApiClient();
                AuthorizedResponse<int[]> result;
                result = Program.LobbyClient.IsConnected == false ? c.AddUserSleeves(Prefs.Username, Prefs.Password.Decrypt(), req) : c.AddUserSleeves(Program.LobbyClient.Username, Program.LobbyClient.Password, req);
                if (result == null || result.Authorized == false || result.Response == null)
                {
                    WindowManager.GrowlWindow.AddNotification(new ErrorNotification("There was a problem adding the sleeve to your account. Please check your internet settings."));
                    return false;
                }
                if (result.Response.Contains(id))
                    return true;
            }
            catch (Exception e)
            {
                Log.Error("AddSleeveToAccount", e);
            }
            WindowManager.GrowlWindow.AddNotification(new ErrorNotification("There was a problem adding the sleeve to your account. Please check your internet settings."));
            return false;
        }

        public ApiSleeve SleeveFromId(int id)
        {
            var sleeves = GetAllSleeves();
            var sleeve = sleeves.FirstOrDefault(x => x.Id == id);
            return sleeve;
        }

        public string GetSleeveString(int id)
        {
            if (id <= 0)
                return "";
            var sleeve = GetAllSleeves().FirstOrDefault(x => x.Id == id);
            if (sleeve == null)
                return "";
            return id + "\t" + sleeve.Url;
        }
    }
}