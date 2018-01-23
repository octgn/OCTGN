using System.Collections.Generic;

namespace Octgn.Site.Api.Models
{
    public class PutGameHistoryReq
    {
        public string Key { get; set; }
        public string SessionId { get; set; }
        public List<string> Usernames { get; set; }

        public PutGameHistoryReq(string key, string sessionId)
        {
            Key = key;
            SessionId = sessionId;
			Usernames = new List<string>();
        }
    }

    public class PutDisconnectReq
    {
        public string Key { get; set; }
        public string SessionId { get; set; }
        public string Username { get; set; }

        public PutDisconnectReq(string key, string sessionId, string username)
        {
            Key = key;
            SessionId = sessionId;
            Username = username;
        }
    }

    public class PutGameCompleteReq
    {
        public string Key { get; set; }
        public string SessionId { get; set; }
        public string[] DisconnectedUsers { get; set; }

        public PutGameCompleteReq(string key, string sessionId, string[] disconnectedUsers)
        {
            Key = key;
            SessionId = sessionId;
            DisconnectedUsers = disconnectedUsers;
        }
    }
}