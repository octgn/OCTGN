using Newtonsoft.Json;
using System;
using System.Text;

namespace Octgn.Core.Play.Save
{
    public class History : IHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid GameId { get; set; }

        [JsonIgnore]
        IGameSaveState IHistory.State => State;

        public GameSaveState State { get; set; }

        public DateTimeOffset DateSaved { get; set; }

        public DateTimeOffset DateStarted { get; set; }

        public History() {
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
