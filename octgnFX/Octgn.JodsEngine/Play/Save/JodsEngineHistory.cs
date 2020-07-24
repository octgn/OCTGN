using Newtonsoft.Json;
using Octgn.Core.Play.Save;
using Octgn.Play.State;
using System;
using System.Text;

namespace Octgn.Play.Save
{
    [Serializable]
    public class JodsEngineHistory : IHistory
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid GameId { get; set; }

        public DateTimeOffset DateSaved { get; set; }

        public DateTimeOffset DateStarted { get; set; }

        [JsonIgnore]
        IGameSaveState IHistory.State => State;

        public JodsEngineGameSaveState State { get; set; }

        public JodsEngineHistory() {
            Id = Guid.NewGuid();

            State = new JodsEngineGameSaveState();
        }

        public JodsEngineHistory(Guid gameId) {
            Id = Guid.NewGuid();

            State = new JodsEngineGameSaveState();

            GameId = gameId;

            DateStarted = DateTimeOffset.Now;
        }

        public byte[] GetSnapshot(GameEngine engine, Player localPlayer) {
            var state = (JodsEngineGameSaveState)State;

            state.Create(engine, localPlayer, true);
            DateSaved = DateTimeOffset.Now;

            var serialized = Serialize(this);

            return serialized;
        }

        public static byte[] Serialize(JodsEngineHistory history) {
            var str = JsonConvert.SerializeObject(history, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(str);

            return bytes;
        }

        public static JodsEngineHistory Deserialize(byte[] data) {
            var str = Encoding.UTF8.GetString(data);

            var history = JsonConvert.DeserializeObject<JodsEngineHistory>(str);

            return history;
        }
    }
}
