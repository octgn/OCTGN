using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Octgn.Play
{
    public class RandomRequest
    {
        public readonly int Id;
        private readonly int max;
        private readonly int min;
        private readonly Player player;
        private readonly List<RandomValue> values = new List<RandomValue>();
        public int Result;
        private RandomValue myValue;
        private int phase2Count;

        public RandomRequest(Player player, int id, int min, int max)
        {
            this.player = player;
            Id = id;
            this.min = min;
            this.max = max;
        }

        public static event EventHandler Completed;

        public static int GenerateId()
        {
            return (Player.LocalPlayer.Id << 16) | Program.Game.GetUniqueId();
        }

        public void Answer1()
        {
            Debug.Assert(myValue == null);

            myValue = new RandomValue();
            Program.Client.Rpc.RandomAnswer1Req(Id, myValue.encrypted);
        }

        public void Answer2()
        {
            Program.Client.Rpc.RandomAnswer2Req(Id, myValue.decrypted);
        }

        public void AddAnswer1(Player lPlayer, ulong encrypted)
        {
            values.Add(new RandomValue(lPlayer, encrypted));
        }

        public void AddAnswer2(Player lPlayer, ulong decrypted)
        {
            foreach (RandomValue v in values)
                if (v.player == lPlayer)
                {
                    v.decrypted = decrypted;
                    v.CheckConsistency();
                    phase2Count++;
                    return;
                }
            Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                     "[AddAnswer] Protocol inconsistency. One client is buggy or tries to cheat.");
        }

        public void Complete()
        {
            Program.Game.RandomRequests.Remove(this);

            if (max < min)
            {
                Result = -1;
            }
            else
            {
                int xorResult = 0;
                foreach (RandomValue value in values)
                    xorResult ^= (int) (value.decrypted & 0xffffff);
                double relativeValue = xorResult/(double) 0xffffff;
                Result = (int) Math.Truncate((max - min + 1)*relativeValue) + min;
                if (Result > max) Result = max; // this handles the extremely rare case where relativeValue == 1.0
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event + EventIds.PlayerFlag(player),
                                     "{0} randomly picks {1} in [{2}, {3}]", player, Result, min, max);

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public bool IsPhase1Completed()
        {
            return values.Count == Player.Count;
        }

        public bool IsPhase2Completed()
        {
            return phase2Count == Player.Count;
        }

        #region Nested type: RandomValue

        private class RandomValue
        {
            public readonly ulong encrypted;
            public readonly Player player;
            public ulong decrypted;

            public RandomValue()
            {
                player = Player.LocalPlayer;
                decrypted = (ulong) Crypto.PositiveRandom() << 32 | Crypto.Random();
                encrypted = Crypto.ModExp(decrypted);
            }

            public RandomValue(Player player, ulong encrypted)
            {
                this.player = player;
                this.encrypted = encrypted;
            }

            public void CheckConsistency()
            {
                ulong correct = Crypto.ModExp(decrypted);
                if (correct != encrypted)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                             "[CheckConsistency] Random number doesn't match. One client is buggy or tries to cheat.");
            }
        }

        #endregion
    }
}