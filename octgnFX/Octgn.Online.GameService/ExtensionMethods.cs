/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Octgn.Library;

namespace Octgn.Online.GameService
{
    public static class ExtensionMethods
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static bool TryKillGame(this IHostedGameData game)
        {
            try
            {
                using (var p = Process.GetProcessById(game.ProcessId))
                {
                    p.Kill();
                    return p.WaitForExit(10000);
                }
            }
            catch (Exception e)
            {
                Log.Error("TryKillGame Error",e);
            }
            return false;
        }
    }
}