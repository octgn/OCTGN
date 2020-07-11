/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Online.Hosting;
using Octgn.Tabs.GameHistory;
using Octgn.Tabs.Play;
using System;

namespace Octgn
{
    public class JodsEngineIntegration
    {
        public void HostGame(HostedGame hostedGame) {
            throw new NotImplementedException();
        }

        public void HostGame(int? hostPort, Guid? gameId) {
            throw new NotImplementedException();
        }

        public void LaunchDeckEditor(string deckPath = null) {
            throw new NotImplementedException();
        }

        public void JoinGame(DataNew.Entities.Game game, HostedGame hostedGame, string password) {
            var username = Program.LobbyClient.User.DisplayName;

            var host = hostedGame.HostAddress;

            //foreach(var address in Dns.GetHostAddresses(AppConfig.GameServerPath)) {
            //    try {
            //        if (address == IPAddress.IPv6Loopback) continue;

            //        // Should use gameData.IpAddress sometime.
            //        Log.Info($"{nameof(StartOnlineGame)}: Trying to connect to {address}:{result.Port}");

            //        Program.Client = new ClientSocket(address, result.Port);
            //        await Program.Client.Connect();
            //        SuccessfulHost = true;
            //        return;
            //    } catch (Exception ex) {
            //        Log.Error($"{nameof(StartOnlineGame)}: Couldn't connect to address {address}:{result.Port}", ex);
            //    }
            //}
            //throw new InvalidOperationException($"Unable to connect to {AppConfig.GameServerPath}.{result.Port}");

            throw new NotImplementedException();
        }

        public void JoinGame(HostedGameViewModel hostedGame) {
            var spectate = hostedGame.Status == HostedGameStatus.GameInProgress && hostedGame.Spectator;
            Program.IsHost = false;
            var password = "";
            if (hostedGame.HasPassword) {
                //TODO: Password Dialog
                //var dlg = new InputDlg("Password", "Please enter this games password", "");
                //password = dlg.GetString();
                throw new NotImplementedException();
            }
            var username = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.User == null
                || Program.LobbyClient.User.DisplayName == null) ? Prefs.Nickname : Program.LobbyClient.User.DisplayName;

            var game = GameManager.Get().GetById(hostedGame.GameId);

            throw new NotImplementedException();
        }

        internal void LaunchReplay(GameHistoryViewModel history) => throw new NotImplementedException();
        internal void JoinOfflineGame() => throw new NotImplementedException();
        internal void HostGame() => throw new NotImplementedException();
    }
}
