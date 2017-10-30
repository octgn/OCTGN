using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Site.Api.Models
{
    public class WebhookQueueMessage
    {
        public WebhookEndpoint Endpoint { get; set; }
        public string Body { get; set; }
        public Dictionary<string, List<string>> Headers { get; set; }

        public WebhookQueueMessage()
        {
            
        }

        public WebhookQueueMessage(WebhookEndpoint end, string body, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers )
        {
            Endpoint = end;
            if(String.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException("body");
            Body = body;
            Headers = headers.ToDictionary(x=>x.Key, x=>x.Value.ToList());
        }
    }
    
    public enum WebhookEndpoint
    {
        Octgn,OctgnDev,OctgnLobby
    }
}