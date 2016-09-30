/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Linq;
using System.Windows.Media;
using Octgn.Play;
using log4net;
using Microsoft.AspNet.SignalR.Client;

namespace Octgn.Networking
{
	public class HubRpc : IServerCalls
	{
		internal static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IHubProxy _hub;

        internal void InitializeHub(IHubProxy hub) {
            var oldHub = System.Threading.Interlocked.Exchange(ref _hub, hub);
            if (oldHub != null) {
                // TODO unregister all event handlers
            }
        }

        public void Error(string msg)
        {
            //Log.Debug("[ProtOut] Error");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                msg
            };
            _hub.Invoke(nameof(Error), invokeArgs);
		}

        public void Boot(Player player, string reason)
        {
            //Log.Debug("[ProtOut] Boot");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                reason
            };
            _hub.Invoke(nameof(Boot), invokeArgs);
		}

        public void Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator)
        {
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
            _hub.Invoke(nameof(Hello), invokeArgs);
		}

        public void HelloAgain(uint pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password)
        {
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
            _hub.Invoke(nameof(HelloAgain), invokeArgs);
		}

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            //Log.Debug("[ProtOut] Settings");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                twoSidedTable,
                allowSpectators,
                muteSpectators
            };
            _hub.Invoke(nameof(Settings), invokeArgs);
		}

        public void PlayerSettings(Player playerId, bool invertedTable, bool spectator)
        {
            //Log.Debug("[ProtOut] PlayerSettings");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                playerId.Id,
                invertedTable,
                spectator
            };
            _hub.Invoke(nameof(PlayerSettings), invokeArgs);
		}

        public void Leave(Player player)
        {
            //Log.Debug("[ProtOut] Leave");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id
            };
            _hub.Invoke(nameof(Leave), invokeArgs);
		}

        public void NickReq(string nick)
        {
            //Log.Debug("[ProtOut] NickReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                nick
            };
            _hub.Invoke(nameof(NickReq), invokeArgs);
		}

        public void Start()
        {
            //Log.Debug("[ProtOut] Start");
            if(Program.Client == null)return;

            _hub.Invoke(nameof(Start));
		}

        public void ResetReq()
        {
            //Log.Debug("[ProtOut] ResetReq");
            if(Program.Client == null)return;

            _hub.Invoke(nameof(ResetReq));
		}

        public void NextTurn(Player nextPlayer)
        {
            //Log.Debug("[ProtOut] NextTurn");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                nextPlayer.Id
            };
            _hub.Invoke(nameof(NextTurn), invokeArgs);
		}

        public void StopTurnReq(int turnNumber, bool stop)
        {
            //Log.Debug("[ProtOut] StopTurnReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                turnNumber,
                stop
            };
            _hub.Invoke(nameof(StopTurnReq), invokeArgs);
		}

        public void ChatReq(string text)
        {
            //Log.Debug("[ProtOut] ChatReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                text
            };
            _hub.Invoke(nameof(ChatReq), invokeArgs);
		}

        public void PrintReq(string text)
        {
            //Log.Debug("[ProtOut] PrintReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                text
            };
            _hub.Invoke(nameof(PrintReq), invokeArgs);
		}

        public void RandomReq(int min, int max)
        {
            //Log.Debug("[ProtOut] RandomReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                min,
                max
            };
            _hub.Invoke(nameof(RandomReq), invokeArgs);
		}

        public void CounterReq(Counter counter, int value, bool isScriptChange)
        {
            //Log.Debug("[ProtOut] CounterReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)counter.Id,
                value,
                isScriptChange
            };
            _hub.Invoke(nameof(CounterReq), invokeArgs);
		}

        public void LoadDeck(ulong[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited)
        {
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
            _hub.Invoke(nameof(LoadDeck), invokeArgs);
		}

        public void CreateCard(ulong[] id, Guid[] type, string[] size, Group group)
        {
            //Log.Debug("[ProtOut] CreateCard");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Cast<long>().ToArray(),
                type,
                size,
                (long)group.Id
            };
            _hub.Invoke(nameof(CreateCard), invokeArgs);
		}

        public void CreateCardAt(ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
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
            _hub.Invoke(nameof(CreateCardAt), invokeArgs);
		}

        public void MoveCardReq(ulong[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            //Log.Debug("[ProtOut] MoveCardReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                id.Cast<long>().ToArray(),
                (long)group.Id,
                idx,
                faceUp,
                isScriptMove
            };
            _hub.Invoke(nameof(MoveCardReq), invokeArgs);
		}

        public void MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
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
            _hub.Invoke(nameof(MoveCardAtReq), invokeArgs);
		}

        public void PeekReq(Card card)
        {
            //Log.Debug("[ProtOut] PeekReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id
            };
            _hub.Invoke(nameof(PeekReq), invokeArgs);
		}

        public void UntargetReq(Card card, bool isScriptChange)
        {
            //Log.Debug("[ProtOut] UntargetReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                isScriptChange
            };
            _hub.Invoke(nameof(UntargetReq), invokeArgs);
		}

        public void TargetReq(Card card, bool isScriptChange)
        {
            //Log.Debug("[ProtOut] TargetReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                isScriptChange
            };
            _hub.Invoke(nameof(TargetReq), invokeArgs);
		}

        public void TargetArrowReq(Card card, Card otherCard, bool isScriptChange)
        {
            //Log.Debug("[ProtOut] TargetArrowReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                (long)otherCard.Id,
                isScriptChange
            };
            _hub.Invoke(nameof(TargetArrowReq), invokeArgs);
		}

        public void Highlight(Card card, Color? color)
        {
            //Log.Debug("[ProtOut] Highlight");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                color
            };
            _hub.Invoke(nameof(Highlight), invokeArgs);
		}

        public void TurnReq(Card card, bool up)
        {
            //Log.Debug("[ProtOut] TurnReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                up
            };
            _hub.Invoke(nameof(TurnReq), invokeArgs);
		}

        public void RotateReq(Card card, CardOrientation rot)
        {
            //Log.Debug("[ProtOut] RotateReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                rot
            };
            _hub.Invoke(nameof(RotateReq), invokeArgs);
		}

        public void Shuffled(Player player, Group group, ulong[] card, short[] pos)
        {
            //Log.Debug("[ProtOut] Shuffled");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                (long)group.Id,
                card.Cast<long>().ToArray(),
                pos
            };
            _hub.Invoke(nameof(Shuffled), invokeArgs);
		}

        public void AddMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
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
            _hub.Invoke(nameof(AddMarkerReq), invokeArgs);
		}

        public void RemoveMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
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
            _hub.Invoke(nameof(RemoveMarkerReq), invokeArgs);
		}

        public void TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
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
            _hub.Invoke(nameof(TransferMarkerReq), invokeArgs);
		}

        public void PassToReq(ControllableObject id, Player to, bool requested)
        {
            //Log.Debug("[ProtOut] PassToReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)id.Id,
                to.Id,
                requested
            };
            _hub.Invoke(nameof(PassToReq), invokeArgs);
		}

        public void TakeFromReq(ControllableObject id, Player from)
        {
            //Log.Debug("[ProtOut] TakeFromReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)id.Id,
                from.Id
            };
            _hub.Invoke(nameof(TakeFromReq), invokeArgs);
		}

        public void DontTakeReq(ControllableObject id, Player to)
        {
            //Log.Debug("[ProtOut] DontTakeReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)id.Id,
                to.Id
            };
            _hub.Invoke(nameof(DontTakeReq), invokeArgs);
		}

        public void FreezeCardsVisibility(Group group)
        {
            //Log.Debug("[ProtOut] FreezeCardsVisibility");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)group.Id
            };
            _hub.Invoke(nameof(FreezeCardsVisibility), invokeArgs);
		}

        public void GroupVisReq(Group group, bool defined, bool visible)
        {
            //Log.Debug("[ProtOut] GroupVisReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)group.Id,
                defined,
                visible
            };
            _hub.Invoke(nameof(GroupVisReq), invokeArgs);
		}

        public void GroupVisAddReq(Group group, Player who)
        {
            //Log.Debug("[ProtOut] GroupVisAddReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)group.Id,
                who.Id
            };
            _hub.Invoke(nameof(GroupVisAddReq), invokeArgs);
		}

        public void GroupVisRemoveReq(Group group, Player who)
        {
            //Log.Debug("[ProtOut] GroupVisRemoveReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)group.Id,
                who.Id
            };
            _hub.Invoke(nameof(GroupVisRemoveReq), invokeArgs);
		}

        public void LookAtReq(uint uid, Group group, bool look)
        {
            //Log.Debug("[ProtOut] LookAtReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                uid,
                (long)group.Id,
                look
            };
            _hub.Invoke(nameof(LookAtReq), invokeArgs);
		}

        public void LookAtTopReq(uint uid, Group group, int count, bool look)
        {
            //Log.Debug("[ProtOut] LookAtTopReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                uid,
                (long)group.Id,
                count,
                look
            };
            _hub.Invoke(nameof(LookAtTopReq), invokeArgs);
		}

        public void LookAtBottomReq(uint uid, Group group, int count, bool look)
        {
            //Log.Debug("[ProtOut] LookAtBottomReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                uid,
                (long)group.Id,
                count,
                look
            };
            _hub.Invoke(nameof(LookAtBottomReq), invokeArgs);
		}

        public void StartLimitedReq(Guid[] packs)
        {
            //Log.Debug("[ProtOut] StartLimitedReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                packs
            };
            _hub.Invoke(nameof(StartLimitedReq), invokeArgs);
		}

        public void CancelLimitedReq()
        {
            //Log.Debug("[ProtOut] CancelLimitedReq");
            if(Program.Client == null)return;

            _hub.Invoke(nameof(CancelLimitedReq));
		}

        public void CardSwitchTo(Player player, Card card, string alternate)
        {
            //Log.Debug("[ProtOut] CardSwitchTo");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                (long)card.Id,
                alternate
            };
            _hub.Invoke(nameof(CardSwitchTo), invokeArgs);
		}

        public void PlayerSetGlobalVariable(Player player, string name, string oldval, string val)
        {
            //Log.Debug("[ProtOut] PlayerSetGlobalVariable");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                name,
                oldval,
                val
            };
            _hub.Invoke(nameof(PlayerSetGlobalVariable), invokeArgs);
		}

        public void SetGlobalVariable(string name, string oldval, string val)
        {
            //Log.Debug("[ProtOut] SetGlobalVariable");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                name,
                oldval,
                val
            };
            _hub.Invoke(nameof(SetGlobalVariable), invokeArgs);
		}

        public void Ping()
        {
            if(Program.Client == null)return;

            _hub.Invoke(nameof(Ping));
		}

        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            //Log.Debug("[ProtOut] IsTableBackgroundFlipped");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                isFlipped
            };
            _hub.Invoke(nameof(IsTableBackgroundFlipped), invokeArgs);
		}

        public void PlaySound(Player player, string name)
        {
            //Log.Debug("[ProtOut] PlaySound");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                name
            };
            _hub.Invoke(nameof(PlaySound), invokeArgs);
		}

        public void Ready(Player player)
        {
            //Log.Debug("[ProtOut] Ready");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id
            };
            _hub.Invoke(nameof(Ready), invokeArgs);
		}

        public void RemoteCall(Player player, string function, string args)
        {
            //Log.Debug("[ProtOut] RemoteCall");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                function,
                args
            };
            _hub.Invoke(nameof(RemoteCall), invokeArgs);
		}

        public void GameStateReq(Player player)
        {
            //Log.Debug("[ProtOut] GameStateReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id
            };
            _hub.Invoke(nameof(GameStateReq), invokeArgs);
		}

        public void GameState(Player toPlayer, string state)
        {
            //Log.Debug("[ProtOut] GameState");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                toPlayer.Id,
                state
            };
            _hub.Invoke(nameof(GameState), invokeArgs);
		}

        public void DeleteCard(Card card, Player player)
        {
            //Log.Debug("[ProtOut] DeleteCard");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                player.Id
            };
            _hub.Invoke(nameof(DeleteCard), invokeArgs);
		}

        public void AddPacksReq(Guid[] packs, bool selfOnly)
        {
            //Log.Debug("[ProtOut] AddPacksReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                packs,
                selfOnly
            };
            _hub.Invoke(nameof(AddPacksReq), invokeArgs);
		}

        public void AnchorCard(Card id, Player player, bool anchor)
        {
            //Log.Debug("[ProtOut] AnchorCard");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)id.Id,
                player.Id,
                anchor
            };
            _hub.Invoke(nameof(AnchorCard), invokeArgs);
		}

        public void SetCardProperty(Card id, Player player, string name, string val, string valtype)
        {
            //Log.Debug("[ProtOut] SetCardProperty");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)id.Id,
                player.Id,
                name,
                val,
                valtype
            };
            _hub.Invoke(nameof(SetCardProperty), invokeArgs);
		}

        public void ResetCardProperties(Card id, Player player)
        {
            //Log.Debug("[ProtOut] ResetCardProperties");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)id.Id,
                player.Id
            };
            _hub.Invoke(nameof(ResetCardProperties), invokeArgs);
		}

        public void Filter(Card card, Color? color)
        {
            //Log.Debug("[ProtOut] Filter");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                (long)card.Id,
                color
            };
            _hub.Invoke(nameof(Filter), invokeArgs);
		}

        public void SetBoard(string name)
        {
            //Log.Debug("[ProtOut] SetBoard");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                name
            };
            _hub.Invoke(nameof(SetBoard), invokeArgs);
		}

        public void SetPlayerColor(Player player, string color)
        {
            //Log.Debug("[ProtOut] SetPlayerColor");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                player.Id,
                color
            };
            _hub.Invoke(nameof(SetPlayerColor), invokeArgs);
		}

        public void SetPhase(byte phase, byte nextPhase)
        {
            //Log.Debug("[ProtOut] SetPhase");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                phase,
                nextPhase
            };
            _hub.Invoke(nameof(SetPhase), invokeArgs);
		}

        public void StopPhaseReq(int turnNumber, byte phase, bool stop)
        {
            //Log.Debug("[ProtOut] StopPhaseReq");
            if(Program.Client == null)return;

            var invokeArgs = new object[]{
                turnNumber,
                phase,
                stop
            };
            _hub.Invoke(nameof(StopPhaseReq), invokeArgs);
		}
	}
}
