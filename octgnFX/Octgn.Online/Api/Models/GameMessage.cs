using System;

namespace Octgn.Site.Api.Models
{
    public class GameMessage
    {
        public GameMessageType Type { get; set; }
        public Guid SessionId { get; set; }
        public string Message { get; set; }
        public DateTime Sent { get; set; }
    }

    public enum GameMessageType
    {
        Chat,Event
    }
}