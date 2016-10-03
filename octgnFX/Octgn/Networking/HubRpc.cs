/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Octgn.Play;
using log4net;
using Microsoft.AspNet.SignalR.Client;

namespace Octgn.Networking
{
	public class HubRpc : IRpc
	{
		internal static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IHubProxy _hub;

        internal void InitializeHub(IHubProxy hub) {
            var oldHub = System.Threading.Interlocked.Exchange(ref _hub, hub);
            if (oldHub != null) {
                // TODO unregister all event handlers
            }
        }

        public async Task Error(string msg)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                msg
            };
            await _hub.Invoke(nameof(Error), invokeArgs);
		}

        public async Task Boot(Player player, string reason)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                reason
            };
            await _hub.Invoke(nameof(Boot), invokeArgs);
		}

        public async Task Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                nick,
                pkey,
                client,
                clientVer,
                octgnVer,
                gameId,
                gameVersion,
                password,
                spectator
            };
            await _hub.Invoke(nameof(Hello), invokeArgs);
		}

        public async Task HelloAgain(Guid pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                pid,
                nick,
                pkey,
                client,
                clientVer,
                octgnVer,
                gameId,
                gameVersion,
                password
            };
            await _hub.Invoke(nameof(HelloAgain), invokeArgs);
		}

        public async Task Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                twoSidedTable,
                allowSpectators,
                muteSpectators
            };
            await _hub.Invoke(nameof(Settings), invokeArgs);
		}

        public async Task PlayerSettings(Player playerId, bool invertedTable, bool spectator)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                playerId.Id,
                invertedTable,
                spectator
            };
            await _hub.Invoke(nameof(PlayerSettings), invokeArgs);
		}

        public async Task Leave(Player player)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id
            };
            await _hub.Invoke(nameof(Leave), invokeArgs);
		}

        public async Task NickReq(string nick)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                nick
            };
            await _hub.Invoke(nameof(NickReq), invokeArgs);
		}

        public async Task Start()
        {
            if(Program.Client == null)return;

            await _hub.Invoke(nameof(Start));
		}

        public async Task ResetReq()
        {
            if(Program.Client == null)return;

            await _hub.Invoke(nameof(ResetReq));
		}

        public async Task NextTurn(Player nextPlayer)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                nextPlayer.Id
            };
            await _hub.Invoke(nameof(NextTurn), invokeArgs);
		}

        public async Task StopTurnReq(int turnNumber, bool stop)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                turnNumber,
                stop
            };
            await _hub.Invoke(nameof(StopTurnReq), invokeArgs);
		}

        public async Task ChatReq(string text)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                text
            };
            await _hub.Invoke(nameof(ChatReq), invokeArgs);
		}

        public async Task PrintReq(string text)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                text
            };
            await _hub.Invoke(nameof(PrintReq), invokeArgs);
		}

        public async Task RandomReq(int min, int max)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                min,
                max
            };
            await _hub.Invoke(nameof(RandomReq), invokeArgs);
		}

        public async Task CounterReq(Counter counter, int value, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                counter.Id,
                value,
                isScriptChange
            };
            await _hub.Invoke(nameof(CounterReq), invokeArgs);
		}

        public async Task LoadDeck(Guid[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id,
                type,
                group.Select(grp=>grp.Id).ToArray(),
                size,
                sleeve,
                limited
            };
            await _hub.Invoke(nameof(LoadDeck), invokeArgs);
		}

        public async Task CreateCard(Guid[] id, Guid[] type, string[] size, Group group)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id,
                type,
                size,
                group.Id
            };
            await _hub.Invoke(nameof(CreateCard), invokeArgs);
		}

        public async Task CreateCardAt(Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id,
                modelId,
                x,
                y,
                faceUp,
                persist
            };
            await _hub.Invoke(nameof(CreateCardAt), invokeArgs);
		}

        public async Task MoveCardReq(Guid[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id,
                group.Id,
                idx,
                faceUp,
                isScriptMove
            };
            await _hub.Invoke(nameof(MoveCardReq), invokeArgs);
		}

        public async Task MoveCardAtReq(Guid[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id,
                x,
                y,
                idx,
                isScriptMove,
                faceUp
            };
            await _hub.Invoke(nameof(MoveCardAtReq), invokeArgs);
		}

        public async Task PeekReq(Card card)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id
            };
            await _hub.Invoke(nameof(PeekReq), invokeArgs);
		}

        public async Task UntargetReq(Card card, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                isScriptChange
            };
            await _hub.Invoke(nameof(UntargetReq), invokeArgs);
		}

        public async Task TargetReq(Card card, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                isScriptChange
            };
            await _hub.Invoke(nameof(TargetReq), invokeArgs);
		}

        public async Task TargetArrowReq(Card card, Card otherCard, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                otherCard.Id,
                isScriptChange
            };
            await _hub.Invoke(nameof(TargetArrowReq), invokeArgs);
		}

        public async Task Highlight(Card card, Color? color)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                color
            };
            await _hub.Invoke(nameof(Highlight), invokeArgs);
		}

        public async Task TurnReq(Card card, bool up)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                up
            };
            await _hub.Invoke(nameof(TurnReq), invokeArgs);
		}

        public async Task RotateReq(Card card, CardOrientation rot)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                rot
            };
            await _hub.Invoke(nameof(RotateReq), invokeArgs);
		}

        public async Task Shuffled(Player player, Group group, Guid[] card, short[] pos)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                group.Id,
                card,
                pos
            };
            await _hub.Invoke(nameof(Shuffled), invokeArgs);
		}

        public async Task AddMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                id,
                name,
                count,
                origCount,
                isScriptChange
            };
            await _hub.Invoke(nameof(AddMarkerReq), invokeArgs);
		}

        public async Task RemoveMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                id,
                name,
                count,
                origCount,
                isScriptChange
            };
            await _hub.Invoke(nameof(RemoveMarkerReq), invokeArgs);
		}

        public async Task TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                from.Id,
                to.Id,
                id,
                name,
                count,
                origCount,
                isScriptChange
            };
            await _hub.Invoke(nameof(TransferMarkerReq), invokeArgs);
		}

        public async Task PassToReq(ControllableObject id, Player to, bool requested)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Id,
                to.Id,
                requested
            };
            await _hub.Invoke(nameof(PassToReq), invokeArgs);
		}

        public async Task TakeFromReq(ControllableObject id, Player from)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Id,
                from.Id
            };
            await _hub.Invoke(nameof(TakeFromReq), invokeArgs);
		}

        public async Task DontTakeReq(ControllableObject id, Player to)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Id,
                to.Id
            };
            await _hub.Invoke(nameof(DontTakeReq), invokeArgs);
		}

        public async Task FreezeCardsVisibility(Group group)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                group.Id
            };
            await _hub.Invoke(nameof(FreezeCardsVisibility), invokeArgs);
		}

        public async Task GroupVisReq(Group group, bool defined, bool visible)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                group.Id,
                defined,
                visible
            };
            await _hub.Invoke(nameof(GroupVisReq), invokeArgs);
		}

        public async Task GroupVisAddReq(Group group, Player who)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                group.Id,
                who.Id
            };
            await _hub.Invoke(nameof(GroupVisAddReq), invokeArgs);
		}

        public async Task GroupVisRemoveReq(Group group, Player who)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                group.Id,
                who.Id
            };
            await _hub.Invoke(nameof(GroupVisRemoveReq), invokeArgs);
		}

        public async Task LookAtReq(Guid uniqueid, Group group, bool look)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                uniqueid,
                group.Id,
                look
            };
            await _hub.Invoke(nameof(LookAtReq), invokeArgs);
		}

        public async Task LookAtTopReq(Guid uniqueid, Group group, int count, bool look)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                uniqueid,
                group.Id,
                count,
                look
            };
            await _hub.Invoke(nameof(LookAtTopReq), invokeArgs);
		}

        public async Task LookAtBottomReq(Guid uniqueid, Group group, int count, bool look)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                uniqueid,
                group.Id,
                count,
                look
            };
            await _hub.Invoke(nameof(LookAtBottomReq), invokeArgs);
		}

        public async Task StartLimitedReq(Guid[] packs)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                packs
            };
            await _hub.Invoke(nameof(StartLimitedReq), invokeArgs);
		}

        public async Task CancelLimitedReq()
        {
            if(Program.Client == null)return;

            await _hub.Invoke(nameof(CancelLimitedReq));
		}

        public async Task CardSwitchTo(Player player, Card card, string alternate)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                card.Id,
                alternate
            };
            await _hub.Invoke(nameof(CardSwitchTo), invokeArgs);
		}

        public async Task PlayerSetGlobalVariable(Player player, string name, string oldval, string val)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                name,
                oldval,
                val
            };
            await _hub.Invoke(nameof(PlayerSetGlobalVariable), invokeArgs);
		}

        public async Task SetGlobalVariable(string name, string oldval, string val)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                name,
                oldval,
                val
            };
            await _hub.Invoke(nameof(SetGlobalVariable), invokeArgs);
		}

        public async Task IsTableBackgroundFlipped(bool isFlipped)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                isFlipped
            };
            await _hub.Invoke(nameof(IsTableBackgroundFlipped), invokeArgs);
		}

        public async Task PlaySound(Player player, string name)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                name
            };
            await _hub.Invoke(nameof(PlaySound), invokeArgs);
		}

        public async Task Ready(Player player)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id
            };
            await _hub.Invoke(nameof(Ready), invokeArgs);
		}

        public async Task RemoteCall(Player player, string function, string args)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                function,
                args
            };
            await _hub.Invoke(nameof(RemoteCall), invokeArgs);
		}

        public async Task GameStateReq(Player player)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id
            };
            await _hub.Invoke(nameof(GameStateReq), invokeArgs);
		}

        public async Task GameState(Player toPlayer, string state)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                toPlayer.Id,
                state
            };
            await _hub.Invoke(nameof(GameState), invokeArgs);
		}

        public async Task DeleteCard(Card card, Player player)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                player.Id
            };
            await _hub.Invoke(nameof(DeleteCard), invokeArgs);
		}

        public async Task AddPacksReq(Guid[] packs, bool selfOnly)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                packs,
                selfOnly
            };
            await _hub.Invoke(nameof(AddPacksReq), invokeArgs);
		}

        public async Task AnchorCard(Card id, Player player, bool anchor)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Id,
                player.Id,
                anchor
            };
            await _hub.Invoke(nameof(AnchorCard), invokeArgs);
		}

        public async Task SetCardProperty(Card id, Player player, string name, string val, string valtype)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Id,
                player.Id,
                name,
                val,
                valtype
            };
            await _hub.Invoke(nameof(SetCardProperty), invokeArgs);
		}

        public async Task ResetCardProperties(Card id, Player player)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Id,
                player.Id
            };
            await _hub.Invoke(nameof(ResetCardProperties), invokeArgs);
		}

        public async Task Filter(Card card, Color? color)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                card.Id,
                color
            };
            await _hub.Invoke(nameof(Filter), invokeArgs);
		}

        public async Task SetBoard(string name)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                name
            };
            await _hub.Invoke(nameof(SetBoard), invokeArgs);
		}

        public async Task SetPlayerColor(Player player, string color)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                color
            };
            await _hub.Invoke(nameof(SetPlayerColor), invokeArgs);
		}

        public async Task SetPhase(byte phase, byte nextPhase)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                phase,
                nextPhase
            };
            await _hub.Invoke(nameof(SetPhase), invokeArgs);
		}

        public async Task StopPhaseReq(int turnNumber, byte phase, bool stop)
        {
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                turnNumber,
                phase,
                stop
            };
            await _hub.Invoke(nameof(StopPhaseReq), invokeArgs);
		}
	}
}
