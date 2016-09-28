/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Windows.Media;
using Octgn.Play;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Octgn.Networking
{
	internal abstract class HandlerBase
	{
        private IHubProxy _hub;


        internal void InitializeHub(IHubProxy hub) {
            var oldHub = System.Threading.Interlocked.Exchange(ref _hub, hub);
            if (oldHub != null) {
                // TODO unregister all event handlers
            }

            Subscription sub = null;

            sub = _hub.Subscribe(nameof(Error));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Error(args[0].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Kick));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Kick(args[0].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Welcome));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Welcome(args[0].ToObject<uint>(), args[1].ToObject<Guid>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Settings));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Settings(args[0].ToObject<bool>(), args[1].ToObject<bool>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(PlayerSettings));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    PlayerSettings(args[0].ToObject<Player>(), args[1].ToObject<bool>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(NewPlayer));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    NewPlayer(args[0].ToObject<uint>(), args[1].ToObject<string>(), args[2].ToObject<ulong>(), args[3].ToObject<bool>(), args[4].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Leave));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Leave(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Nick));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Nick(args[0].ToObject<Player>(), args[1].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Start));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Start();
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Reset));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Reset(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(NextTurn));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    NextTurn(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(StopTurn));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    StopTurn(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Chat));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Chat(args[0].ToObject<Player>(), args[1].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Print));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Print(args[0].ToObject<Player>(), args[1].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Random));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Random(args[0].ToObject<int>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Counter));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Counter(args[0].ToObject<Player>(), args[1].ToObject<Counter>(), args[2].ToObject<int>(), args[3].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(LoadDeck));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    LoadDeck(args[0].ToObject<Player>(), args[1].ToObject<ulong[]>(), args[2].ToObject<Guid[]>(), args[3].ToObject<Group[]>(), args[4].ToObject<string[]>(), args[5].ToObject<string>(), args[6].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(CreateCard));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    CreateCard(args[0].ToObject<Player>(), args[1].ToObject<ulong[]>(), args[2].ToObject<Guid[]>(), args[3].ToObject<string[]>(), args[4].ToObject<Group>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(CreateCardAt));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    CreateCardAt(args[0].ToObject<Player>(), args[1].ToObject<ulong[]>(), args[2].ToObject<Guid[]>(), args[3].ToObject<int[]>(), args[4].ToObject<int[]>(), args[5].ToObject<bool>(), args[6].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(MoveCard));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    MoveCard(args[0].ToObject<Player>(), args[1].ToObject<ulong[]>(), args[2].ToObject<Group>(), args[3].ToObject<int[]>(), args[4].ToObject<bool[]>(), args[5].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(MoveCardAt));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    MoveCardAt(args[0].ToObject<Player>(), args[1].ToObject<ulong[]>(), args[2].ToObject<int[]>(), args[3].ToObject<int[]>(), args[4].ToObject<int[]>(), args[5].ToObject<bool[]>(), args[6].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Peek));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Peek(args[0].ToObject<Player>(), args[1].ToObject<Card>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Untarget));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Untarget(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Target));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Target(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(TargetArrow));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    TargetArrow(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<Card>(), args[3].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Highlight));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Highlight(args[0].ToObject<Card>(), args[1].ToObject<Color?>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Turn));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Turn(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Rotate));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Rotate(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<CardOrientation>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Shuffled));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Shuffled(args[0].ToObject<Player>(), args[1].ToObject<Group>(), args[2].ToObject<ulong[]>(), args[3].ToObject<short[]>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(AddMarker));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    AddMarker(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<Guid>(), args[3].ToObject<string>(), args[4].ToObject<ushort>(), args[5].ToObject<ushort>(), args[6].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(RemoveMarker));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    RemoveMarker(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<Guid>(), args[3].ToObject<string>(), args[4].ToObject<ushort>(), args[5].ToObject<ushort>(), args[6].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(TransferMarker));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    TransferMarker(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<Card>(), args[3].ToObject<Guid>(), args[4].ToObject<string>(), args[5].ToObject<ushort>(), args[6].ToObject<ushort>(), args[7].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(PassTo));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    PassTo(args[0].ToObject<Player>(), args[1].ToObject<ControllableObject>(), args[2].ToObject<Player>(), args[3].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(TakeFrom));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    TakeFrom(args[0].ToObject<ControllableObject>(), args[1].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(DontTake));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    DontTake(args[0].ToObject<ControllableObject>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(FreezeCardsVisibility));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    FreezeCardsVisibility(args[0].ToObject<Group>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(GroupVis));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    GroupVis(args[0].ToObject<Player>(), args[1].ToObject<Group>(), args[2].ToObject<bool>(), args[3].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(GroupVisAdd));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    GroupVisAdd(args[0].ToObject<Player>(), args[1].ToObject<Group>(), args[2].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(GroupVisRemove));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    GroupVisRemove(args[0].ToObject<Player>(), args[1].ToObject<Group>(), args[2].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(LookAt));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    LookAt(args[0].ToObject<Player>(), args[1].ToObject<uint>(), args[2].ToObject<Group>(), args[3].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(LookAtTop));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    LookAtTop(args[0].ToObject<Player>(), args[1].ToObject<uint>(), args[2].ToObject<Group>(), args[3].ToObject<int>(), args[4].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(LookAtBottom));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    LookAtBottom(args[0].ToObject<Player>(), args[1].ToObject<uint>(), args[2].ToObject<Group>(), args[3].ToObject<int>(), args[4].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(StartLimited));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    StartLimited(args[0].ToObject<Player>(), args[1].ToObject<Guid[]>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(CancelLimited));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    CancelLimited(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(CardSwitchTo));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    CardSwitchTo(args[0].ToObject<Player>(), args[1].ToObject<Card>(), args[2].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(PlayerSetGlobalVariable));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    PlayerSetGlobalVariable(args[0].ToObject<Player>(), args[1].ToObject<string>(), args[2].ToObject<string>(), args[3].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(SetGlobalVariable));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    SetGlobalVariable(args[0].ToObject<string>(), args[1].ToObject<string>(), args[2].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Ping));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Ping();
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(IsTableBackgroundFlipped));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    IsTableBackgroundFlipped(args[0].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(PlaySound));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    PlaySound(args[0].ToObject<Player>(), args[1].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Ready));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Ready(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(PlayerState));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    PlayerState(args[0].ToObject<Player>(), args[1].ToObject<byte>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(RemoteCall));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    RemoteCall(args[0].ToObject<Player>(), args[1].ToObject<string>(), args[2].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(GameStateReq));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    GameStateReq(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(GameState));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    GameState(args[0].ToObject<Player>(), args[1].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(DeleteCard));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    DeleteCard(args[0].ToObject<Card>(), args[1].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(PlayerDisconnect));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    PlayerDisconnect(args[0].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(AddPacks));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    AddPacks(args[0].ToObject<Player>(), args[1].ToObject<Guid[]>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(AnchorCard));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    AnchorCard(args[0].ToObject<Card>(), args[1].ToObject<Player>(), args[2].ToObject<bool>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(SetCardProperty));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    SetCardProperty(args[0].ToObject<Card>(), args[1].ToObject<Player>(), args[2].ToObject<string>(), args[3].ToObject<string>(), args[4].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(ResetCardProperties));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    ResetCardProperties(args[0].ToObject<Card>(), args[1].ToObject<Player>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(Filter));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    Filter(args[0].ToObject<Card>(), args[1].ToObject<Color?>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(SetBoard));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    SetBoard(args[0].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(SetPlayerColor));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    SetPlayerColor(args[0].ToObject<Player>(), args[1].ToObject<string>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(SetPhase));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    SetPhase(args[0].ToObject<byte>(), args[1].ToObject<byte>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };

            sub = _hub.Subscribe(nameof(StopPhase));
            sub.Received += (args) => {
                if (Program.Client == null) return;

                try {
                    StopPhase(args[0].ToObject<Player>(), args[1].ToObject<byte>());
                } finally {
                    if (Program.Client != null) Program.Client.Muted = 0;
                }
            };


        }
		protected abstract void Error(string msg);
		protected abstract void Kick(string reason);
		protected abstract void Welcome(uint id, Guid gameSessionId, bool waitForGameState);
		protected abstract void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		protected abstract void PlayerSettings(Player playerId, bool invertedTable, bool spectator);
		protected abstract void NewPlayer(uint id, string nick, ulong pkey, bool tableSide, bool spectator);
		protected abstract void Leave(Player player);
		protected abstract void Nick(Player player, string nick);
		protected abstract void Start();
		protected abstract void Reset(Player player);
		protected abstract void NextTurn(Player nextPlayer);
		protected abstract void StopTurn(Player player);
		protected abstract void Chat(Player player, string text);
		protected abstract void Print(Player player, string text);
		protected abstract void Random(int result);
		protected abstract void Counter(Player player, Counter counter, int value, bool isScriptChange);
		protected abstract void LoadDeck(Player player, ulong[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited);
		protected abstract void CreateCard(Player player, ulong[] id, Guid[] type, string[] size, Group group);
		protected abstract void CreateCardAt(Player player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		protected abstract void MoveCard(Player player, ulong[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove);
		protected abstract void MoveCardAt(Player player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove);
		protected abstract void Peek(Player player, Card card);
		protected abstract void Untarget(Player player, Card card, bool isScriptChange);
		protected abstract void Target(Player player, Card card, bool isScriptChange);
		protected abstract void TargetArrow(Player player, Card card, Card otherCard, bool isScriptChange);
		protected abstract void Highlight(Card card, Color? color);
		protected abstract void Turn(Player player, Card card, bool up);
		protected abstract void Rotate(Player player, Card card, CardOrientation rot);
		protected abstract void Shuffled(Player player, Group group, ulong[] card, short[] pos);
		protected abstract void AddMarker(Player player, Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		protected abstract void RemoveMarker(Player player, Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		protected abstract void TransferMarker(Player player, Card from, Card to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		protected abstract void PassTo(Player player, ControllableObject id, Player to, bool requested);
		protected abstract void TakeFrom(ControllableObject id, Player to);
		protected abstract void DontTake(ControllableObject id);
		protected abstract void FreezeCardsVisibility(Group group);
		protected abstract void GroupVis(Player player, Group group, bool defined, bool visible);
		protected abstract void GroupVisAdd(Player player, Group group, Player who);
		protected abstract void GroupVisRemove(Player player, Group group, Player who);
		protected abstract void LookAt(Player player, uint uid, Group group, bool look);
		protected abstract void LookAtTop(Player player, uint uid, Group group, int count, bool look);
		protected abstract void LookAtBottom(Player player, uint uid, Group group, int count, bool look);
		protected abstract void StartLimited(Player player, Guid[] packs);
		protected abstract void CancelLimited(Player player);
		protected abstract void CardSwitchTo(Player player, Card card, string alternate);
		protected abstract void PlayerSetGlobalVariable(Player player, string name, string oldval, string val);
		protected abstract void SetGlobalVariable(string name, string oldval, string val);
		protected abstract void Ping();
		protected abstract void IsTableBackgroundFlipped(bool isFlipped);
		protected abstract void PlaySound(Player player, string name);
		protected abstract void Ready(Player player);
		protected abstract void PlayerState(Player player, byte state);
		protected abstract void RemoteCall(Player player, string function, string args);
		protected abstract void GameStateReq(Player player);
		protected abstract void GameState(Player toPlayer, string state);
		protected abstract void DeleteCard(Card card, Player player);
		protected abstract void PlayerDisconnect(Player player);
		protected abstract void AddPacks(Player player, Guid[] packs, bool selfOnly);
		protected abstract void AnchorCard(Card id, Player player, bool anchor);
		protected abstract void SetCardProperty(Card id, Player player, string name, string val, string valtype);
		protected abstract void ResetCardProperties(Card id, Player player);
		protected abstract void Filter(Card card, Color? color);
		protected abstract void SetBoard(string name);
		protected abstract void SetPlayerColor(Player player, string color);
		protected abstract void SetPhase(byte phase, byte nextPhase);
		protected abstract void StopPhase(Player player, byte phase);
	}
}
