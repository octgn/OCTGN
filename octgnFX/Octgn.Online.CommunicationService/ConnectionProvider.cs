using System;
using Octgn.Communication;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.Linq;

namespace Octgn
{
    public class ConnectionProvider : IConnectionProvider
    {
        public const string OnlineStatus = "Online";
        public const string OfflineStatus = "Offline";

        private UserConnectionMap OnlineUsers { get; }

        public ConnectionProvider() {
            OnlineUsers = new UserConnectionMap();
            OnlineUsers.UserConnectionChanged += OnlineUsers_UserConnectionChanged;
        }

        private async void OnlineUsers_UserConnectionChanged(object sender, UserConnectionChangedEventArgs e) {
            try {
                await _server.UpdateUserStatus(e.User, e.IsConnected ? OnlineStatus : OfflineStatus);
            } catch (Exception ex) {
                Signal.Exception(ex);
            }
        }

        public IEnumerable<IConnection> GetConnections(string userId) {
            return OnlineUsers.GetConnections(userId);
        }

        public Task AddConnection(IConnection connection, User user) {
            return OnlineUsers.AddConnection(connection, user);
        }

        private Server _server;
        public void Initialize(Server server) {
            _server = server;
        }

        public User GetUser(IConnection connection) {
            return OnlineUsers.GetUser(connection);
        }

        public string GetUserStatus(string userId) {
            bool userOnline = OnlineUsers
                .GetOnlineUsers()
                .Any(x => x.Id.Equals(userId, StringComparison.InvariantCulture));

            return userOnline ? OnlineStatus : OfflineStatus;
        }

        public IEnumerable<IConnection> GetConnections() {
            return OnlineUsers.GetConnections();
        }
    }
}
