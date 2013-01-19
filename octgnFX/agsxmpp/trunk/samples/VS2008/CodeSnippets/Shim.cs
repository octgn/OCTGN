using System;
using System.Text;
using System.Xml;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.shim;

namespace CodeSnippets
{
    class Shim
    {
        internal Shim()
        {
            /*
                Example of an Urgent Message

                <message 
                    from='romeo@shakespeare.lit/orchard'
                    to='juliet@capulet.com'
                    type='chat'>
                  <body>Wherefore are thou?!?</body>
                  <headers xmlns='http://jabber.org/protocol/shim'>
                    <header name='Urgency'>high</header>
                  </headers>
                </message>
            */

            // Create the message aboute which includes the shim header urgency
            agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
            msg.From = new Jid("romeo@shakespeare.lit/orchard");
            msg.To = new Jid("juliet@capulet.com");
            msg.Type = MessageType.chat;
            msg.Body = "Wherefore are thou?!?";
            
            Headers headers = new Headers();
            headers.AddHeader("Urgency", "high");

            msg.Headers = headers;
            // or
            // msg.AddChild(headers);
            Program.Print(msg);

            // after we created this header we want to change the urgency of the existing message to low
            Headers heads = msg.Headers;
            if (heads != null)
                heads.SetHeader("Urgency", "low");

            Program.Print(msg);

            // Add another header
            // <header name='In-Reply-To'>123456789@capulet.com</header>
            msg.Headers.AddHeader("In-Reply-To", "123456789@capulet.com");
            Program.Print(msg);

            // Remove all Headers
            msg.Headers = null;
            Program.Print(msg);            
        }
    }
}