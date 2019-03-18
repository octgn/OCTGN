using System;
using System.IO;

namespace Octgn.Play.Save
{
    public struct ReplayEvent
    {
        public int Id { get; set; }

        public ReplayEventType Type { get; set; }

        public byte PlayerId { get; set; }

        public byte[] Action { get; set; }

        public TimeSpan Time { get; set; }

        public static void Write(ReplayEvent eve, BinaryWriter writer) {
            writer.Write((byte)eve.Type);
            writer.Write(eve.Time.Ticks);
            writer.Write(eve.PlayerId);
            writer.Write(eve.Action.Length);
            writer.Write(eve.Action);
        }

        public static ReplayEvent Read(BinaryReader reader) {
            var eventType = (ReplayEventType)reader.ReadByte();
            var eventTimeTicks = reader.ReadInt64();
            var playerId = reader.ReadByte();

            var actionLength = reader.ReadInt32();
            var action = reader.ReadBytes(actionLength);

            var eventTime = TimeSpan.FromTicks(eventTimeTicks);

            return new ReplayEvent {
                Type = eventType,
                Time = eventTime,
                PlayerId = playerId,
                Action = action
            };
        }

        public static bool operator ==(ReplayEvent e1, ReplayEvent e2) {
            if (e1.Id > 0 || e2.Id > 0) {
                return e1.Id == e2.Id;
            } else {
                if (e1.Action != e2.Action) return false;
                if (e1.PlayerId != e2.PlayerId) return false;
                if (e1.Time != e2.Time) return false;
                if (e1.Type != e2.Type) return false;

                return true;
            }
        }

        public static bool operator !=(ReplayEvent e1, ReplayEvent e2) {
            return !(e1 == e2);
        }
    }
}
