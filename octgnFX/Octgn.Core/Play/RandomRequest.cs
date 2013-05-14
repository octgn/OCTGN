using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Octgn.Utils;

namespace Octgn.Play
{
    using Octgn.Core;
    using Octgn.Core.Play;
    using Octgn.Networking;

    public class RandomRequest
    {
        public readonly int Id;
        private readonly int _max;
        private readonly int _min;
        private readonly IPlayPlayer _player;
        private readonly List<RandomValue> _values = new List<RandomValue>();
        public int Result;
        private RandomValue _myValue;
        private int _phase2Count;

        public RandomRequest(IPlayPlayer player, int id, int min, int max)
        {
            _player = player;
            Id = id;
            _min = min;
            _max = max;
        }

        public static event EventHandler Completed;

        public static int GenerateId()
        {
            return (K.C.Get<PlayerStateMachine>().LocalPlayer.Id << 16) | K.C.Get<IGameEngine>().GetUniqueId();
        }

        public void Answer1()
        {
            Debug.Assert(_myValue == null);

            _myValue = new RandomValue();
            K.C.Get<Client>().Rpc.RandomAnswer1Req(Id, _myValue.Encrypted);
        }

        public void Answer2()
        {
            K.C.Get<Client>().Rpc.RandomAnswer2Req(Id, _myValue.Decrypted);
        }

        public void AddAnswer1(IPlayPlayer lPlayer, ulong encrypted)
        {
            _values.Add(new RandomValue(lPlayer, encrypted));
        }

        public void AddAnswer2(IPlayPlayer lPlayer, ulong decrypted)
        {
            foreach (RandomValue v in _values.Where(v => v.Player == lPlayer))
            {
                v.Decrypted = decrypted;
                v.CheckConsistency();
                _phase2Count++;
                return;
            }
            K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Warning, EventIds.Event,
                                     "[AddAnswer] Protocol inconsistency. One client is buggy or tries to cheat.");
        }

        public void Complete()
        {
            K.C.Get<IGameEngine>().RandomRequests.Remove(this);

            if (_max < _min)
            {
                Result = -1;
            }
            else
            {
                int xorResult = _values.Aggregate(0, (current, value) => current ^ (int) (value.Decrypted & 0xffffff));
                double relativeValue = xorResult/(double) 0xffffff;
                Result = (int) Math.Truncate((_max - _min + 1)*relativeValue) + _min;
                if (Result > _max) Result = _max; // this handles the extremely rare case where relativeValue == 1.0
            }
            K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event + EventIds.PlayerFlag(_player),
                                     "{0} randomly picks {1} in [{2}, {3}]", _player, Result, _min, _max);

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public bool IsPhase1Completed()
        {
            return _values.Count == K.C.Get<PlayerStateMachine>().Count;
        }

        public bool IsPhase2Completed()
        {
            return _phase2Count == K.C.Get<PlayerStateMachine>().Count;
        }

        #region Nested type: RandomValue

        private class RandomValue
        {
            public readonly ulong Encrypted;
            public readonly IPlayPlayer Player;
            public ulong Decrypted;

            public RandomValue()
            {
                Player = K.C.Get<PlayerStateMachine>().LocalPlayer;
                Decrypted = (ulong) Crypto.PositiveRandom() << 32 | Crypto.Random();
                Encrypted = Crypto.ModExp(Decrypted);
            }

            public RandomValue(IPlayPlayer player, ulong encrypted)
            {
                Player = player;
                Encrypted = encrypted;
            }

            public void CheckConsistency()
            {
                ulong correct = Crypto.ModExp(Decrypted);
                if (correct != Encrypted)
                    K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Warning, EventIds.Event,
                                             "[CheckConsistency] Random number doesn't match. One client is buggy or tries to cheat.");
            }
        }

        #endregion
    }
}