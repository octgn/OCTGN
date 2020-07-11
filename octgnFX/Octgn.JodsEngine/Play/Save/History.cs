using Newtonsoft.Json;
using Octgn.Core.Play.Save;
using Octgn.Play.State;
using System;
using System.Text;

namespace Octgn.Play.Save
{
    public class History : IHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid GameId { get; set; }

        public IGameSaveState State { get; set; }

        public DateTimeOffset DateSaved { get; set; }

        public DateTimeOffset DateStarted { get; set; }

        public History() {
            State = new GameSaveState();
            Id = Guid.NewGuid();
        }

        public History(Guid gameId) {
            Id = Guid.NewGuid();

            GameId = gameId;

            State = new GameSaveState();

            DateStarted = DateTimeOffset.Now;
        }

        public byte[] GetSnapshot(GameEngine engine, Player localPlayer) {
            var state = (GameSaveState)State;

            state.Create(engine, localPlayer, true);
            DateSaved = DateTimeOffset.Now;

            var serialized = Serialize(this);

            return serialized;
        }

        public static byte[] Serialize(History history) {
            var str = JsonConvert.SerializeObject(history, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(str);

            return bytes;
        }

        public static History Deserialize(byte[] data) {
            var str = Encoding.UTF8.GetString(data);

            var history = JsonConvert.DeserializeObject<History>(str);

            return history;
        }
    }
}
