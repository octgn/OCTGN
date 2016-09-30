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
            try {
                //Log.Debug("[ProtOut] Error");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    msg
                };
                await _hub.Invoke(nameof(Error), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Error), ex);
            }
		}

        public async Task Boot(Player player, string reason)
        {
            try {
                //Log.Debug("[ProtOut] Boot");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                reason
                };
                await _hub.Invoke(nameof(Boot), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Boot), ex);
            }
		}

        public async Task Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator)
        {
            try {
                //Log.Debug("[ProtOut] Hello");
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
            } catch (Exception ex) {
                Log.Error(nameof(Hello), ex);
            }
		}

        public async Task HelloAgain(uint pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password)
        {
            try {
                //Log.Debug("[ProtOut] HelloAgain");
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
            } catch (Exception ex) {
                Log.Error(nameof(HelloAgain), ex);
            }
		}

        public async Task Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            try {
                //Log.Debug("[ProtOut] Settings");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    twoSidedTable,
                allowSpectators,
                muteSpectators
                };
                await _hub.Invoke(nameof(Settings), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Settings), ex);
            }
		}

        public async Task PlayerSettings(Player playerId, bool invertedTable, bool spectator)
        {
            try {
                //Log.Debug("[ProtOut] PlayerSettings");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    playerId.Id,
                invertedTable,
                spectator
                };
                await _hub.Invoke(nameof(PlayerSettings), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(PlayerSettings), ex);
            }
		}

        public async Task Leave(Player player)
        {
            try {
                //Log.Debug("[ProtOut] Leave");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id
                };
                await _hub.Invoke(nameof(Leave), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Leave), ex);
            }
		}

        public async Task NickReq(string nick)
        {
            try {
                //Log.Debug("[ProtOut] NickReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    nick
                };
                await _hub.Invoke(nameof(NickReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(NickReq), ex);
            }
		}

        public async Task Start()
        {
            try {
                //Log.Debug("[ProtOut] Start");
                if(Program.Client == null)return;

                await _hub.Invoke(nameof(Start));
            } catch (Exception ex) {
                Log.Error(nameof(Start), ex);
            }
		}

        public async Task ResetReq()
        {
            try {
                //Log.Debug("[ProtOut] ResetReq");
                if(Program.Client == null)return;

                await _hub.Invoke(nameof(ResetReq));
            } catch (Exception ex) {
                Log.Error(nameof(ResetReq), ex);
            }
		}

        public async Task NextTurn(Player nextPlayer)
        {
            try {
                //Log.Debug("[ProtOut] NextTurn");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    nextPlayer.Id
                };
                await _hub.Invoke(nameof(NextTurn), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(NextTurn), ex);
            }
		}

        public async Task StopTurnReq(int turnNumber, bool stop)
        {
            try {
                //Log.Debug("[ProtOut] StopTurnReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    turnNumber,
                stop
                };
                await _hub.Invoke(nameof(StopTurnReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(StopTurnReq), ex);
            }
		}

        public async Task ChatReq(string text)
        {
            try {
                //Log.Debug("[ProtOut] ChatReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    text
                };
                await _hub.Invoke(nameof(ChatReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(ChatReq), ex);
            }
		}

        public async Task PrintReq(string text)
        {
            try {
                //Log.Debug("[ProtOut] PrintReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    text
                };
                await _hub.Invoke(nameof(PrintReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(PrintReq), ex);
            }
		}

        public async Task RandomReq(int min, int max)
        {
            try {
                //Log.Debug("[ProtOut] RandomReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    min,
                max
                };
                await _hub.Invoke(nameof(RandomReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(RandomReq), ex);
            }
		}

        public async Task CounterReq(Counter counter, int value, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] CounterReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)counter.Id,
                value,
                isScriptChange
                };
                await _hub.Invoke(nameof(CounterReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(CounterReq), ex);
            }
		}

        public async Task LoadDeck(ulong[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited)
        {
            try {
                //Log.Debug("[ProtOut] LoadDeck");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    id.Cast<long>().ToArray(),
                type,
                group.Select(grp=>(long)grp.Id).ToArray(),
                size,
                sleeve,
                limited
                };
                await _hub.Invoke(nameof(LoadDeck), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(LoadDeck), ex);
            }
		}

        public async Task CreateCard(ulong[] id, Guid[] type, string[] size, Group group)
        {
            try {
                //Log.Debug("[ProtOut] CreateCard");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    id.Cast<long>().ToArray(),
                type,
                size,
                (long)group.Id
                };
                await _hub.Invoke(nameof(CreateCard), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(CreateCard), ex);
            }
		}

        public async Task CreateCardAt(ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            try {
                //Log.Debug("[ProtOut] CreateCardAt");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    id.Cast<long>().ToArray(),
                modelId,
                x,
                y,
                faceUp,
                persist
                };
                await _hub.Invoke(nameof(CreateCardAt), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(CreateCardAt), ex);
            }
		}

        public async Task MoveCardReq(ulong[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            try {
                //Log.Debug("[ProtOut] MoveCardReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    id.Cast<long>().ToArray(),
                (long)group.Id,
                idx,
                faceUp,
                isScriptMove
                };
                await _hub.Invoke(nameof(MoveCardReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(MoveCardReq), ex);
            }
		}

        public async Task MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            try {
                //Log.Debug("[ProtOut] MoveCardAtReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    id.Cast<long>().ToArray(),
                x,
                y,
                idx,
                isScriptMove,
                faceUp
                };
                await _hub.Invoke(nameof(MoveCardAtReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(MoveCardAtReq), ex);
            }
		}

        public async Task PeekReq(Card card)
        {
            try {
                //Log.Debug("[ProtOut] PeekReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id
                };
                await _hub.Invoke(nameof(PeekReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(PeekReq), ex);
            }
		}

        public async Task UntargetReq(Card card, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] UntargetReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                isScriptChange
                };
                await _hub.Invoke(nameof(UntargetReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(UntargetReq), ex);
            }
		}

        public async Task TargetReq(Card card, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] TargetReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                isScriptChange
                };
                await _hub.Invoke(nameof(TargetReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(TargetReq), ex);
            }
		}

        public async Task TargetArrowReq(Card card, Card otherCard, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] TargetArrowReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                (long)otherCard.Id,
                isScriptChange
                };
                await _hub.Invoke(nameof(TargetArrowReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(TargetArrowReq), ex);
            }
		}

        public async Task Highlight(Card card, Color? color)
        {
            try {
                //Log.Debug("[ProtOut] Highlight");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                color
                };
                await _hub.Invoke(nameof(Highlight), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Highlight), ex);
            }
		}

        public async Task TurnReq(Card card, bool up)
        {
            try {
                //Log.Debug("[ProtOut] TurnReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                up
                };
                await _hub.Invoke(nameof(TurnReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(TurnReq), ex);
            }
		}

        public async Task RotateReq(Card card, CardOrientation rot)
        {
            try {
                //Log.Debug("[ProtOut] RotateReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                rot
                };
                await _hub.Invoke(nameof(RotateReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(RotateReq), ex);
            }
		}

        public async Task Shuffled(Player player, Group group, ulong[] card, short[] pos)
        {
            try {
                //Log.Debug("[ProtOut] Shuffled");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                (long)group.Id,
                card.Cast<long>().ToArray(),
                pos
                };
                await _hub.Invoke(nameof(Shuffled), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Shuffled), ex);
            }
		}

        public async Task AddMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] AddMarkerReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                id,
                name,
                count,
                origCount,
                isScriptChange
                };
                await _hub.Invoke(nameof(AddMarkerReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(AddMarkerReq), ex);
            }
		}

        public async Task RemoveMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] RemoveMarkerReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                id,
                name,
                count,
                origCount,
                isScriptChange
                };
                await _hub.Invoke(nameof(RemoveMarkerReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(RemoveMarkerReq), ex);
            }
		}

        public async Task TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            try {
                //Log.Debug("[ProtOut] TransferMarkerReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)from.Id,
                (long)to.Id,
                id,
                name,
                count,
                origCount,
                isScriptChange
                };
                await _hub.Invoke(nameof(TransferMarkerReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(TransferMarkerReq), ex);
            }
		}

        public async Task PassToReq(ControllableObject id, Player to, bool requested)
        {
            try {
                //Log.Debug("[ProtOut] PassToReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)id.Id,
                to.Id,
                requested
                };
                await _hub.Invoke(nameof(PassToReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(PassToReq), ex);
            }
		}

        public async Task TakeFromReq(ControllableObject id, Player from)
        {
            try {
                //Log.Debug("[ProtOut] TakeFromReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)id.Id,
                from.Id
                };
                await _hub.Invoke(nameof(TakeFromReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(TakeFromReq), ex);
            }
		}

        public async Task DontTakeReq(ControllableObject id, Player to)
        {
            try {
                //Log.Debug("[ProtOut] DontTakeReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)id.Id,
                to.Id
                };
                await _hub.Invoke(nameof(DontTakeReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(DontTakeReq), ex);
            }
		}

        public async Task FreezeCardsVisibility(Group group)
        {
            try {
                //Log.Debug("[ProtOut] FreezeCardsVisibility");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)group.Id
                };
                await _hub.Invoke(nameof(FreezeCardsVisibility), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(FreezeCardsVisibility), ex);
            }
		}

        public async Task GroupVisReq(Group group, bool defined, bool visible)
        {
            try {
                //Log.Debug("[ProtOut] GroupVisReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)group.Id,
                defined,
                visible
                };
                await _hub.Invoke(nameof(GroupVisReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(GroupVisReq), ex);
            }
		}

        public async Task GroupVisAddReq(Group group, Player who)
        {
            try {
                //Log.Debug("[ProtOut] GroupVisAddReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)group.Id,
                who.Id
                };
                await _hub.Invoke(nameof(GroupVisAddReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(GroupVisAddReq), ex);
            }
		}

        public async Task GroupVisRemoveReq(Group group, Player who)
        {
            try {
                //Log.Debug("[ProtOut] GroupVisRemoveReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)group.Id,
                who.Id
                };
                await _hub.Invoke(nameof(GroupVisRemoveReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(GroupVisRemoveReq), ex);
            }
		}

        public async Task LookAtReq(uint uid, Group group, bool look)
        {
            try {
                //Log.Debug("[ProtOut] LookAtReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    uid,
                (long)group.Id,
                look
                };
                await _hub.Invoke(nameof(LookAtReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(LookAtReq), ex);
            }
		}

        public async Task LookAtTopReq(uint uid, Group group, int count, bool look)
        {
            try {
                //Log.Debug("[ProtOut] LookAtTopReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    uid,
                (long)group.Id,
                count,
                look
                };
                await _hub.Invoke(nameof(LookAtTopReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(LookAtTopReq), ex);
            }
		}

        public async Task LookAtBottomReq(uint uid, Group group, int count, bool look)
        {
            try {
                //Log.Debug("[ProtOut] LookAtBottomReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    uid,
                (long)group.Id,
                count,
                look
                };
                await _hub.Invoke(nameof(LookAtBottomReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(LookAtBottomReq), ex);
            }
		}

        public async Task StartLimitedReq(Guid[] packs)
        {
            try {
                //Log.Debug("[ProtOut] StartLimitedReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    packs
                };
                await _hub.Invoke(nameof(StartLimitedReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(StartLimitedReq), ex);
            }
		}

        public async Task CancelLimitedReq()
        {
            try {
                //Log.Debug("[ProtOut] CancelLimitedReq");
                if(Program.Client == null)return;

                await _hub.Invoke(nameof(CancelLimitedReq));
            } catch (Exception ex) {
                Log.Error(nameof(CancelLimitedReq), ex);
            }
		}

        public async Task CardSwitchTo(Player player, Card card, string alternate)
        {
            try {
                //Log.Debug("[ProtOut] CardSwitchTo");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                (long)card.Id,
                alternate
                };
                await _hub.Invoke(nameof(CardSwitchTo), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(CardSwitchTo), ex);
            }
		}

        public async Task PlayerSetGlobalVariable(Player player, string name, string oldval, string val)
        {
            try {
                //Log.Debug("[ProtOut] PlayerSetGlobalVariable");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                name,
                oldval,
                val
                };
                await _hub.Invoke(nameof(PlayerSetGlobalVariable), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(PlayerSetGlobalVariable), ex);
            }
		}

        public async Task SetGlobalVariable(string name, string oldval, string val)
        {
            try {
                //Log.Debug("[ProtOut] SetGlobalVariable");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    name,
                oldval,
                val
                };
                await _hub.Invoke(nameof(SetGlobalVariable), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(SetGlobalVariable), ex);
            }
		}

        public async Task IsTableBackgroundFlipped(bool isFlipped)
        {
            try {
                //Log.Debug("[ProtOut] IsTableBackgroundFlipped");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    isFlipped
                };
                await _hub.Invoke(nameof(IsTableBackgroundFlipped), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(IsTableBackgroundFlipped), ex);
            }
		}

        public async Task PlaySound(Player player, string name)
        {
            try {
                //Log.Debug("[ProtOut] PlaySound");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                name
                };
                await _hub.Invoke(nameof(PlaySound), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(PlaySound), ex);
            }
		}

        public async Task Ready(Player player)
        {
            try {
                //Log.Debug("[ProtOut] Ready");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id
                };
                await _hub.Invoke(nameof(Ready), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Ready), ex);
            }
		}

        public async Task RemoteCall(Player player, string function, string args)
        {
            try {
                //Log.Debug("[ProtOut] RemoteCall");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                function,
                args
                };
                await _hub.Invoke(nameof(RemoteCall), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(RemoteCall), ex);
            }
		}

        public async Task GameStateReq(Player player)
        {
            try {
                //Log.Debug("[ProtOut] GameStateReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id
                };
                await _hub.Invoke(nameof(GameStateReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(GameStateReq), ex);
            }
		}

        public async Task GameState(Player toPlayer, string state)
        {
            try {
                //Log.Debug("[ProtOut] GameState");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    toPlayer.Id,
                state
                };
                await _hub.Invoke(nameof(GameState), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(GameState), ex);
            }
		}

        public async Task DeleteCard(Card card, Player player)
        {
            try {
                //Log.Debug("[ProtOut] DeleteCard");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                player.Id
                };
                await _hub.Invoke(nameof(DeleteCard), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(DeleteCard), ex);
            }
		}

        public async Task AddPacksReq(Guid[] packs, bool selfOnly)
        {
            try {
                //Log.Debug("[ProtOut] AddPacksReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    packs,
                selfOnly
                };
                await _hub.Invoke(nameof(AddPacksReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(AddPacksReq), ex);
            }
		}

        public async Task AnchorCard(Card id, Player player, bool anchor)
        {
            try {
                //Log.Debug("[ProtOut] AnchorCard");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)id.Id,
                player.Id,
                anchor
                };
                await _hub.Invoke(nameof(AnchorCard), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(AnchorCard), ex);
            }
		}

        public async Task SetCardProperty(Card id, Player player, string name, string val, string valtype)
        {
            try {
                //Log.Debug("[ProtOut] SetCardProperty");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)id.Id,
                player.Id,
                name,
                val,
                valtype
                };
                await _hub.Invoke(nameof(SetCardProperty), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(SetCardProperty), ex);
            }
		}

        public async Task ResetCardProperties(Card id, Player player)
        {
            try {
                //Log.Debug("[ProtOut] ResetCardProperties");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)id.Id,
                player.Id
                };
                await _hub.Invoke(nameof(ResetCardProperties), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(ResetCardProperties), ex);
            }
		}

        public async Task Filter(Card card, Color? color)
        {
            try {
                //Log.Debug("[ProtOut] Filter");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    (long)card.Id,
                color
                };
                await _hub.Invoke(nameof(Filter), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(Filter), ex);
            }
		}

        public async Task SetBoard(string name)
        {
            try {
                //Log.Debug("[ProtOut] SetBoard");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    name
                };
                await _hub.Invoke(nameof(SetBoard), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(SetBoard), ex);
            }
		}

        public async Task SetPlayerColor(Player player, string color)
        {
            try {
                //Log.Debug("[ProtOut] SetPlayerColor");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    player.Id,
                color
                };
                await _hub.Invoke(nameof(SetPlayerColor), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(SetPlayerColor), ex);
            }
		}

        public async Task SetPhase(byte phase, byte nextPhase)
        {
            try {
                //Log.Debug("[ProtOut] SetPhase");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    phase,
                nextPhase
                };
                await _hub.Invoke(nameof(SetPhase), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(SetPhase), ex);
            }
		}

        public async Task StopPhaseReq(int turnNumber, byte phase, bool stop)
        {
            try {
                //Log.Debug("[ProtOut] StopPhaseReq");
                if(Program.Client == null)return;

                var invokeArgs = new object[]{
                    turnNumber,
                phase,
                stop
                };
                await _hub.Invoke(nameof(StopPhaseReq), invokeArgs);
            } catch (Exception ex) {
                Log.Error(nameof(StopPhaseReq), ex);
            }
		}
	}
}
