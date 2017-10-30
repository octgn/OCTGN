using System;
using Octgn.Communication;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.Linq;

namespace Octgn
{
    public class OctgnDataUserProvider : IConnectionProvider
    {
        public const string OnlineStatus = "Online";
        public const string OfflineStatus = "Offline";

        private UserConnectionMap OnlineUsers { get; }

        public OctgnDataUserProvider() {
            OnlineUsers = new UserConnectionMap();
            OnlineUsers.UserConnectionChanged += OnlineUsers_UserConnectionChanged;
        }

        private async void OnlineUsers_UserConnectionChanged(object sender, UserConnectionChangedEventArgs e) {
            try {
                await _server.UpdateUserStatus(e.UserId, e.IsConnected ? OnlineStatus : OfflineStatus);
            } catch (Exception ex) {
                Signal.Exception(ex);
            }
        }

        public IEnumerable<IConnection> GetConnections(string userId) {
            return OnlineUsers.GetConnections(userId);
        }

        public Task AddConnection(IConnection connection, string userId) {
            return OnlineUsers.AddConnection(connection, userId);
        }

        private Server _server;
        public void Initialize(Server server) {
            _server = server;
        }

        public string GetUserId(IConnection connection) {
            return OnlineUsers.GetUserId(connection);
        }

        public string GetUserStatus(string userId) {
            bool userOnline = OnlineUsers
                .GetOnlineUsers()
                .Any(x => x.Equals(userId, StringComparison.InvariantCulture));

            return userOnline ? OnlineStatus : OfflineStatus;
        }
    }
}
