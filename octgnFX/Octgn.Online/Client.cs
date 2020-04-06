/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using Octgn.Communication;
using Octgn.Communication.Modules;
using Octgn.Communication.Modules.SubscriptionModule;
using Octgn.Online.Hosting;

namespace Octgn.Library.Communication
{
    public class Client : Octgn.Communication.Client
    {
        private static readonly ILogger Log = LoggerFactory.Create(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IClientConfig _config;

        public Client(IClientConfig config, Version octgnVersion) : base(new ConnectionCreator(config), new Octgn.Communication.Serializers.XmlSerializer()) {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            this.InitializeSubscriptionModule();
            this.InitializeHosting(octgnVersion);
            this.InitializeStatsModule();
        }

        public void ConfigureSession(string sessionKey, User user, string deviceId) {
            this.Attach(new ClientAuthenticator());

            var authenticator = Authenticator as ClientAuthenticator;
            authenticator.SessionKey = sessionKey;
            authenticator.UserId = user.Id;
            authenticator.DeviceId = deviceId;
        }

        public void Stop() {
            Log.Info(nameof(Stop));

            Connection?.Close();
        }

        private class ConnectionCreator : IConnectionCreator
        {
            private readonly IClientConfig _config;

            public ConnectionCreator(IClientConfig config) {
                _config = config ?? throw new ArgumentNullException(nameof(config));
            }

            public IConnection Create(string host)
                => _config.CreateConnection(_config.ChatHost);

            public void Initialize(Octgn.Communication.Client client) {

            }
        }
    }
}
