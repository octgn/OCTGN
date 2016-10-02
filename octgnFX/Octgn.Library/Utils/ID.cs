using System;

namespace Octgn.Library.Utils
{
    public struct ID
    {
        //  |---------------------------------64----------------------------------|
        //  00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000
        //  |---------------32----------------| |---------------32----------------|
        //  |--8---| |--8---| |------16-------| |------16-------| |------16-------|
        //  | Type | | ???? | |    Game ID    | |   Player ID   | |    Item ID    |
        //  -----------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public IDType Type;
        public uint GameId;
        public uint PlayerId;
        public uint Id;
        public bool IsGameTable => Id == 0x01000000;

        private static uint _currentId;

        public ID(IDType type, uint gameId, uint playerId) {
            Type = type;
            GameId = gameId;
            PlayerId = playerId;
            Id = _currentId++;
        }

        public static implicit operator ID(ulong id) {
            return new ID {
                Type = ID.GetIDType(id),
                PlayerId = ID.GetPlayerId(id),
                GameId = ID.GetGameId(id),
                Id = (uint)id,
            };
        }

        public static implicit operator ulong(ID id) {
            return (ulong)((byte)id.Type | 0x0 << 8 | id.GameId << 16 | id.PlayerId << 16 | id.Id);
        }

        public static ID CreateCardID(uint gameId, uint playerId) {
            return new Utils.ID(IDType.Card, gameId, playerId);
        }

        public static ID CreateGroupID(uint gameId, uint playerId) {
            return new Utils.ID(IDType.Group, gameId, playerId);
        }

        public static ID CreateCounterID(uint gameId, uint playerId) {
            return new Utils.ID(IDType.Counter, gameId, playerId);
        }

        public static ID CreateScriptJobID(uint gameId, uint playerId) {
            return new Utils.ID(IDType.ScriptJob, gameId, playerId);
        }

        internal static bool IsCard(ulong id) => GetIDType(id) == IDType.Card;

        internal static bool IsGroup(ulong id) => GetIDType(id) == IDType.Group;

        internal static IDType GetIDType(ulong id) => (IDType)(byte)(id >> 56);

        internal static uint GetPlayerId(ulong id) => (uint)(id >> 16);
        internal static uint GetGameId(ulong id) => (uint)(id >> 32);

        public override bool Equals(object obj) {
            return ulong.Equals(obj, this);
        }
    }

    public enum IDType : byte
    {
        Card = 0, Group = 1, Counter = 2, ScriptJob = 3
    }
}
