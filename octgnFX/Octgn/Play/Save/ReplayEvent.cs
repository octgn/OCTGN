using System;
using System.IO;

namespace Octgn.Play.Save
{
    public struct ReplayEvent
    {
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
    }
}
