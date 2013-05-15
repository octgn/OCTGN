namespace Octgn.Core.Play
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Octgn.Data;
    using Octgn.Networking;
    using Octgn.Play;

    public class GameStateMachine
    {
        public static GameStateMachine C { get { return K.C.Get<GameStateMachine>(); } }

        public IGameEngine Engine { get; internal set; }

        public event Action OnLocalPlayerWelcomed;
        public event EventHandler<PlayerEventArgs> PlayerAdded;
        public event EventHandler<PlayerEventArgs> PlayerRemoved;

        public Dictionary<int, IPlayCard> AllCards { get; internal set; }
        public ObservableCollection<IPlayPlayer> AllPlayers { get; internal set; }

        public IPlayPlayer LocalPlayer { get; internal set; }
        public IPlayPlayer GlobalPlayer { get; internal set; }
        public IEnumerable<IPlayPlayer> AllExceptGlobal{get{return AllPlayers.Where(x => x != GlobalPlayer);}}
        public int PlayerCount { get { return GlobalPlayer == null ? AllPlayers.Count : AllPlayers.Count - 1; } }

        public string DefaultFront{get { return K.C.Get<IGameEngine>().Definition.CardFront; }}
        public string DefaultBack{get { return K.C.Get<IGameEngine>().Definition.CardBack; }}

        public GameSettings GameSettings { get; internal set; }

        public bool IsHost { get; internal set; }

        public string OnlineGameName { get; set; }

        public GameStateMachine()
        {
            AllCards = new Dictionary<int, IPlayCard>();
            AllPlayers = new ObservableCollection<IPlayPlayer>();
            IsHost = false;
        }

        public T Find<T>(int id) where T : class
        {
            var type = typeof(T);
            if (type == typeof(IPlayControllableObject))
            {
                switch ((byte)(id >> 24))
                {
                    case 0:
                        return Find<IPlayCard>(id) as T;
                    case 1:
                        return Find<IPlayGroup>(id) as T;
                    case 2:
                        //TODO: make counters controllable objects
                        //return Counter.Find(id);
                        return null;
                    default:
                        return null;
                }
            }
            if (type == typeof(IPlayCounter))
            {
                var p = Find<IPlayPlayer>((byte)(id >> 16));
                if (p == null || (byte)id > p.Counters.Length || (byte)id == 0)
                    return null;
                return p.Counters[(byte)id - 1] as T;
            }
            if (type == typeof(IPlayGroup))
            {
                if (id == 0x01000000) return K.C.Get<IGameEngine>().Table as T;
                var player = Find<IPlayPlayer>((byte)(id >> 16));
                return player.IndexedGroups[(byte)id] as T;
            }
            if (type == typeof(IPlayCard))
            {
                IPlayCard res;
                bool success = AllCards.TryGetValue(id, out res);
                return success ? res as T : default(T);
            }
            if (type == typeof(IPlayPlayer))
            {
                return AllPlayers.FirstOrDefault(x => x.Id == id) as T;
            }
            throw new NotImplementedException("Can not find type of " + type.Name + " a find for it has not been implemented.");
        }

        public void Reset()
        {
            AllCards.Clear();
            AllPlayers.Clear();
            LocalPlayer = GlobalPlayer = null;
        }

        public void StartGame()
        {
            // Reset the InvertedTable flags if they were set and they are not used
            if (!GameSettings.UseTwoSidedTable)
                foreach (var player in AllExceptGlobal)
                    player.InvertedTable = false;

            // At start the global items belong to the player with the lowest id
            if (GlobalPlayer != null)
            {
                var host = AllExceptGlobal.OrderBy(p => p.Id).First();
                foreach (var group in GlobalPlayer.Groups)
                    group.Controller = host;
            }
            K.C.Get<Client>().Rpc.Start();
            throw new NotImplementedException("Shouldn't be here, and should launch a window or something?");
        }
        public void StopGame()
        {
            if (K.C.Get<Client>() != null)
            {
                K.C.Get<Client>().Disconnect();
            }
            if (Engine != null)
                Engine.End();
            Engine = null;
            throw new NotImplementedException("Shouldn't be here, and should set a boolean saying game is done?");
        }

        #region EventInvocators
        public void FireLocalPlayerWelcomed()
        {
            if (OnLocalPlayerWelcomed != null)
                OnLocalPlayerWelcomed.Invoke();
        }
        public void FirePlayerRemoved(PlayerEventArgs args)
        {
            if (PlayerRemoved != null) PlayerRemoved(this, args);
        }
        public void FirePlayerAdded(PlayerEventArgs args)
        {
            if (PlayerAdded != null) PlayerAdded(this, args);
        }
        #endregion EventInvocators
    }
}