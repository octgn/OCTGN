/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using log4net;
using Octgn.Communication;

namespace Octgn.Utils
{
    public class Log4NetLogger : ILogger
    {
        public string Name { get; set; }

        private ILog Log;

        public Log4NetLogger(string name)
        {
            Name = name;
            Log = LogManager.GetLogger(name);
        }

        public void Error(string message) => Log.Error(message);

        public void Error(string message, Exception ex) => Log.Error(message, ex);

        public void Error(Exception ex) => Log.Error(ex);

        public void Info(string message) => Log.Info(message);

        public void Warn(string message) => Log.Warn(message);

        public void Warn(string message, Exception ex) => Log.Warn(message, ex);

        public void Warn(Exception ex) => Log.Warn(ex);
    }
}
