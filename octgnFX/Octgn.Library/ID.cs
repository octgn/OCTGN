namespace Octgn
{
    public struct ID
    {
        //  |---------------------------------64----------------------------------|
        //  00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000
        //  |---------------32----------------| |---------------32----------------|
        //  |--8---| |--8---| |------16-------| |------16-------| |------16-------|
        //  | Type | | ???? | |    Game ID    | |   Player ID   | |    Item ID    |
        //  -----------------------------------------------------------------------

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

        public ID(uint id, IDType type, uint gameId, uint playerId) {
            Type = type;
            GameId = gameId;
            PlayerId = playerId;
            Id = id;
        }

        public static implicit operator ID(ulong id) {
            return new ID (
                // TODO FML, uint is 32 bit, not 16.......
                id:         (uint)   id,        //  00000000 00000000 00000000 00000000 00000000 00000000[00000000 00000000]
                playerId:   (uint)  (id >> 16), //  00000000 00000000 00000000 00000000[00000000 00000000]00000000 00000000
                gameId:     (uint)  (id >> 32), //  00000000 00000000[00000000 00000000]00000000 00000000 00000000 00000000
                type:       (IDType)(id >> 56)  // [00000000]00000000 00000000 00000000 00000000 00000000 00000000 00000000
            );
        }

        public static implicit operator ulong(ID id) {
            return (
                  ((ulong)id.Type << 56)        // [00000000]00000000 00000000 00000000 00000000 00000000 00000000 00000000
                | ((ulong)id.GameId << 32)      //  00000000 00000000[00000000 00000000]00000000 00000000 00000000 00000000
                | ((ulong)id.PlayerId << 16)    //  00000000 00000000 00000000 00000000[00000000 00000000]00000000 00000000
                | ((ulong)id.Id)                //  00000000 00000000 00000000 00000000 00000000 00000000[00000000 00000000]
            );
        }

        public static ID CreateCardID(uint gameId, uint playerId) {
            return new ID(IDType.Card, gameId, playerId);
        }

        public static ID CreateGroupID(uint gameId, uint playerId) {
            return new ID(IDType.Group, gameId, playerId);
        }

        public static ID CreateCounterID(uint gameId, uint playerId) {
            return new ID(IDType.Counter, gameId, playerId);
        }

        public static ID CreateScriptJobID(uint gameId, uint playerId) {
            return new ID(IDType.ScriptJob, gameId, playerId);
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is ulong) || !(obj is ID)) return false;
            var id = (ID)obj;
            if (this.Type != id.Type) return false;
            if (this.GameId != id.GameId) return false;
            if (this.PlayerId != id.PlayerId) return false;
            if (this.Id != id.Id) return false;
            return true;
        }

        public override int GetHashCode() {
            return ((ulong)this).GetHashCode();
        }
    }

    public enum IDType : byte
    {
        Card = 0, Group = 1, Counter = 2, ScriptJob = 3
    }
}
