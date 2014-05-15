// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Threading;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using Octgn.DataNew.Entities;
using Skylabs.Lobby.Messages;
using Skylabs.Lobby.Messages.Matchmaking;

namespace Skylabs.Lobby
{
    public class MatchmakingCog
    {
        private readonly Client _client;
        private readonly XmppClientConnection _xmpp;
        private readonly Messanger _messanger;
        private Action<MatchmakingMessage> _onMatchmakingUpdate;

        public MatchmakingCog(Client c, XmppClientConnection xmpp)
        {
            _client = c;
            _xmpp = xmpp;
			_messanger = new Messanger();
			_messanger.OnResetXmpp(xmpp);

			_messanger.Map<MatchmakingInLineUpdateMessage>(OnMatchmakingInLineUpdateMessage);
			_messanger.Map<MatchmakingReadyRequest>(OnMatchmakingReadyRequest);
			_messanger.Map<MatchmakingReadyFail>(OnMatchmakingFail);

			xmpp.OnMessage += XmppOnOnMessage;
        }

        public Task<StartMatchmakingResponse> JoinMatchmakingQueueAsync(Game game, GameMode mode, int msTimeout = -1)
        {
            return Task.Factory.StartNew(() => JoinMatchmakingQueue(game, mode, msTimeout));
        }

        public StartMatchmakingResponse JoinMatchmakingQueue(Game game, GameMode mode, int msTimeout = 10000)
        {
            var req = new StartMatchmakingRequest(game.Id, mode.Name, game.Name, game.Version, mode.PlayerCount, typeof(MatchmakingCog).Assembly.GetName().Version, _client.Config.MatchamkingBotUser.JidUser);
            var mut = new AutoResetEvent(false);
            using (var mess = new Messanger())
            {
                mess.OnResetXmpp(_xmpp);
                StartMatchmakingResponse resp = null;
                mess.Map<StartMatchmakingResponse>(x =>
                {
                    if (x.RequestId == req.RequestId)
                        resp = x;
                    mut.Set();
                });

                mess.Send(req);
                if (!mut.WaitOne(msTimeout))
                {
                    return null;
                }
                return resp;
            }
        }

        public void OnMatchmakingUpdate(Action<MatchmakingMessage> action)
        {
            _onMatchmakingUpdate = action;
        }

        public void Ready(MatchmakingReadyRequest req)
        {
			var resp = new MatchmakingReadyResponse(_client.Config.MatchamkingBotUser.JidUser, req.QueueId);

			_messanger.Send(resp);
        }

        public void LeaveQueue(Guid id)
        {
            var msg = new MatchmakingLeaveQueueMessage(_client.Config.MatchamkingBotUser.JidUser, id);

			_messanger.Send(msg);
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
            // Catch message gameready and join game.
        }

        private void OnMatchmakingReadyRequest(MatchmakingReadyRequest obj)
        {
            if (_onMatchmakingUpdate != null)
                _onMatchmakingUpdate(obj);
        }

        private void OnMatchmakingInLineUpdateMessage(MatchmakingInLineUpdateMessage obj)
        {
            if (_onMatchmakingUpdate != null)
                _onMatchmakingUpdate(obj);
        }

        private void OnMatchmakingFail(MatchmakingReadyFail obj)
        {
            if (_onMatchmakingUpdate != null)
                _onMatchmakingUpdate(obj);
        }
    }
}