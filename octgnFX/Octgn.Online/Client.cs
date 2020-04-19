/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Octgn.Communication;
using Octgn.Communication.Modules;
using Octgn.Communication.Modules.SubscriptionModule;
using Octgn.Online;
using Octgn.Online.Hosting;

namespace Octgn.Library.Communication
{
    public class Client : Octgn.Communication.Client
    {
        private static readonly ILogger Log = LoggerFactory.Create(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IClientConfig _config;
        private readonly DefaultHandshaker _handshaker;

        public Client(IClientConfig config, Version octgnVersion) : base(config.ConnectionCreator, new Octgn.Communication.Serializers.XmlSerializer()) {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (!(config.ConnectionCreator.Handshaker is DefaultHandshaker defaultHandshaker))
                throw new InvalidOperationException($"{nameof(IConnectionCreator)} must use the {nameof(DefaultHandshaker)}. No other {nameof(IHandshaker)} is currently supported.");

            _handshaker = defaultHandshaker;

            this.InitializeSubscriptionModule();
            this.InitializeHosting(octgnVersion);
            this.InitializeStatsModule();
        }

        public void ConfigureSession(string sessionKey, User user, string deviceId) {
            if (sessionKey == null) throw new ArgumentNullException(nameof(sessionKey));
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));

            _config.ConnectionCreator.Initialize(this);

            if (!DefaultHandshaker.Validate(sessionKey, user.Id, deviceId, out var errors)) {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }

            _handshaker.SessionKey = sessionKey;
            _handshaker.UserId = user.Id;
            _handshaker.DeviceId = deviceId;
        }

        public Task Connect(CancellationToken cancellationToken) {
            return base.Connect(_config.ChatHost, cancellationToken);
        }

        public void Stop() {
            Log.Info(nameof(Stop));

            Connection?.Close();
        }
    }
}
