/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.Communication;
using Octgn.Communication.Packets;
using Octgn.Site.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Online
{
    public class DefaultHandshaker : IHandshaker
    {
        private readonly ILogger Log = LoggerFactory.Create(nameof(DefaultHandshaker));

        public const string DefaultHandshakeType = "DEFAULT";

        public string SessionKey { get; set; }

        public string UserId { get; set; }

        public string DeviceId { get; set; }

        private readonly ApiClient _apiClient = new ApiClient();

        public DefaultHandshaker() {

        }

        public async Task<HandshakeResult> Handshake(IConnection connection, CancellationToken cancellationToken) {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var sessionKey = SessionKey;
            var userId = UserId;
            var deviceId = DeviceId;

            if (!Validate(sessionKey, userId, deviceId, out var invalidFields)) {
                return new HandshakeResult() {
                    Successful = false,
                    ErrorCode = $"{nameof(DefaultHandshaker)}: These fields are invalid: {string.Join(",", invalidFields)}"
                };
            }

            var req = new HandshakeRequestPacket(DefaultHandshakeType) {
                [nameof(SessionKey)] = SessionKey,
                [nameof(UserId)] = UserId,
                [nameof(DeviceId)] = DeviceId
            };

            var result = await connection.Request(req, cancellationToken);

            if (result.Data == null) {
                return new HandshakeResult() {
                    Successful = false,
                    ErrorCode = $"{nameof(DefaultHandshaker)}: Invalid response data. Expecting {nameof(HandshakeResult)}, but no data received"
                };
            }

            if (result.Data is HandshakeResult handshakeResult) {
                return handshakeResult;
            }

            return new HandshakeResult() {
                Successful = false,
                ErrorCode = $"{nameof(DefaultHandshaker)}: Invalid response data. Expecting {nameof(HandshakeResult)}, received {result.Data.GetType().Name}"
            };
        }

        public async Task<HandshakeResult> OnHandshakeRequest(HandshakeRequestPacket request, IConnection connection, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (request.HandshakeType != DefaultHandshakeType) {
                return new HandshakeResult() {
                    Successful = false,
                    ErrorCode = $"{nameof(DefaultHandshaker)}: Invalid {nameof(HandshakeRequestPacket.HandshakeType)}: {request.HandshakeType}, expected {DefaultHandshakeType}"
                };
            }

            var sessionKey = (string)request[nameof(SessionKey)];
            var userId = (string)request[nameof(UserId)];
            var deviceId = (string)request[nameof(DeviceId)];

            if (!Validate(sessionKey, userId, deviceId, out var errors)) {
                return new HandshakeResult() {
                    Successful = false,
                    ErrorCode = $"{nameof(DefaultHandshaker)}: These fields are invalid: {string.Join(Environment.NewLine, errors)}"
                };
            }

            try {
                if (!await _apiClient.ValidateSession(userId, deviceId, sessionKey, cancellationToken)) {
                    return new HandshakeResult {
                        ErrorCode = "SessionInvalid",
                        Successful = false
                    };
                }

                cancellationToken.ThrowIfCancellationRequested();

                var apiUser = await _apiClient.UserFromUserId(userId, cancellationToken);

                if (apiUser == null) {
                    return new HandshakeResult {
                        ErrorCode = "ApiUserNotFound",
                        Successful = false
                    };
                }

                var user = new User(userId, apiUser.UserName);

                return new HandshakeResult {
                    Successful = true,
                    User = user
                };
            } catch (TaskCanceledException ex) {
                Log.Warn($"{nameof(OnHandshakeRequest)}", ex);

                return new HandshakeResult {
                    ErrorCode = "Canceled",
                    Successful = false
                };
            } catch (ApiClientException ex) {
                Log.Warn($"{nameof(OnHandshakeRequest)}", ex);

                return new HandshakeResult {
                    ErrorCode = "ApiClientError",
                    Successful = false
                };
            }
        }

        public static bool Validate(string sessionKey, string userId, string deviceId, out IEnumerable<string> errors) {
            var errorList = new List<string>();

            errors = errorList;

            if (string.IsNullOrWhiteSpace(sessionKey)) {
                errorList.Add("SessionKey is empty");
            } else if (sessionKey.Length > 128) {
                errorList.Add("SessionKey is too long");
            }

            if (string.IsNullOrWhiteSpace(userId)) {
                errorList.Add("UserId is empty");
            } else if (userId.Length > 128) {
                errorList.Add("UserId is too long");
            }

            if (string.IsNullOrWhiteSpace(deviceId)) {
                errorList.Add("DeviceId is empty");
            } else if (deviceId.Length > 128) {
                errorList.Add("DeviceId is too long");
            }

            return errorList.Count == 0;
        }
    }
}
