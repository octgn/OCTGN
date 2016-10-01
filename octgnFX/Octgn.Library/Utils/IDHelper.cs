using System;

namespace Octgn.Library.Utils
{
    public class IDHelper
    {
        public static ulong CreateCounterId(ulong cid, uint? playerId) {
            return (ulong)0x02000000 | ((playerId ?? 0) << 32) | cid;
        }

        public static ulong CreateGroupId(ulong gid, uint? playerId) {
            return 0x01000000 | ((playerId ?? 0) << 32) | gid;
        }

        public static bool IsCard(ulong id) => GetIDType(id) == IDType.Card;

        public static bool IsGroup(ulong id) => GetIDType(id) == IDType.Group;

        public static bool IsGameTable(ulong id) => id == 0x01000000;

        public static IDType GetIDType(ulong id) => (IDType)(byte)(id >> 24);

        public static uint GetPlayerId(ulong id) => (uint)(id >> 32);
    }

    public struct ID
    {
        public IDType Type;
        public uint PlayerId;
        public bool IsGameTable;
        public ulong Id;


        public static implicit operator ID(ulong id) {
            return new ID {
                Type = IDHelper.GetIDType(id),
                PlayerId = IDHelper.GetPlayerId(id),
                Id = (uint)id,
                IsGameTable = id == 0x01000000
            };
        }

        public static implicit operator ulong(ID id) {
            return (ulong)id.Type | id.PlayerId << 32 | id.Id;
        }
    }

    public enum IDType : byte
    {
        Card, Group, Counter
    }
}
