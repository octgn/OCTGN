/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using log4net;

namespace Octgn.Online.GameService
{
    public class NetworkHelper
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string PortFilePath { get; }

        private readonly string _uniqueName;
        private readonly int _minPortNumber;
        private readonly int _maxPortNumber;

        public NetworkHelper(int minPortNumber, int maxPortNumber, string uniqueName) {
            if (minPortNumber <= 0 || minPortNumber >= ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(minPortNumber), minPortNumber, "Invalid port number");
            if (maxPortNumber <= 0 || maxPortNumber >= ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(minPortNumber), minPortNumber, "Invalid port number");

            if (minPortNumber >= maxPortNumber) throw new ArgumentException($"{nameof(maxPortNumber)} must be greater than {nameof(minPortNumber)}");

            if (string.IsNullOrWhiteSpace(uniqueName))
                throw new ArgumentNullException(nameof(uniqueName));

            if (uniqueName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException($"Unique name '{uniqueName}' is invalid.");
            if (uniqueName.IndexOf('\\') >= 0) throw new ArgumentException($"Unique name '{uniqueName}' is invalid.");
            if (uniqueName.Length > 25) throw new ArgumentException($"Unique name '{uniqueName}' is too long.");

            _uniqueName = uniqueName;
            _minPortNumber = minPortNumber;
            _maxPortNumber = maxPortNumber;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "OCTGN", "GameService");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, $"NEXTPORT-{_uniqueName}");

            if (!File.Exists(path)) {
                var nextPort = FindNextAvailblePort(_minPortNumber);
                File.WriteAllText(path, nextPort.ToString());
            }

            PortFilePath = path;
        }

        public int NextPort {
            get {
                Mutex mutex = null;
                bool aquiredMutex = false;
                try {
                    mutex = new Mutex(true, $"OCTGNGameServicePortLock-{_uniqueName}", out aquiredMutex);

                    if (!aquiredMutex) {
                        aquiredMutex = mutex.WaitOne(TimeSpan.FromSeconds(10));
                    }

                    if (!aquiredMutex) throw new InvalidOperationException("Could not aquire the mutex to get the next available port.");

                    // Now we are thread and interprocess safe
                    var nextPortString = File.ReadAllText(PortFilePath);

                    var nextPort = int.Parse(nextPortString);

                    nextPort = FindNextAvailblePort(nextPort);

                    var savePort = nextPort + 1;

                    if (savePort >= _maxPortNumber)
                        savePort = _minPortNumber;

                    File.WriteAllText(PortFilePath, savePort.ToString());

                    return nextPort;
                } finally {
                    if(aquiredMutex)
                        mutex?.ReleaseMutex();
                }
            }
        }

        private int FindNextAvailblePort(int port) {
            for(var i = 0; i < (_maxPortNumber - _minPortNumber); i++) {
                if (Octgn.Library.Utils.NetworkHelper.IsPortAvailable(port)) return port;

                port++;

                if (port >= _maxPortNumber)
                    port = _minPortNumber;
            }

            throw new InvalidOperationException("Could not find an available port to use.");
        }

        private DateTime _expireExternalIp = DateTime.Now;
        private IPAddress _externalIp = null;

        public IPAddress ExternalIp
        {
            get
            {
                try
                {
                    if (_externalIp == null || DateTime.Now > _expireExternalIp)
                    {
                        const string Dyndns = "http://checkip.dyndns.org";
                        using (var wc = new WebClient())
                        {
                            var utf8 = new System.Text.UTF8Encoding();
                            var requestHtml = "";
                            var ipAddress = "";
                            requestHtml = utf8.GetString(wc.DownloadData(Dyndns));
                            var fullStr = requestHtml.Split(':');
                            ipAddress = fullStr[1].Remove(fullStr[1].IndexOf('<')).Trim();
                            var externalIp = IPAddress.Parse(ipAddress);
                            _externalIp = externalIp;
                            _expireExternalIp = DateTime.Now.AddMinutes(5);
                            return _externalIp;
                        }
                    }
                    return _externalIp;
                }
                catch (Exception e)
                {
                    Log.Error("ExternalIp Error",e);
                }
                return _externalIp ?? IPAddress.Parse("96.31.76.45");
            }
        }
    }
}