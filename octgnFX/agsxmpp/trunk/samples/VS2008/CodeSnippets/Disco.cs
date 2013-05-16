using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using agsXMPP;
using agsXMPP.protocol.iq.disco;

using agsXMPP.protocol.extensions.caps;

namespace CodeSnippets
{
    class Disco
    {
        internal Disco()
        {
            /*
                <iq from='romeo@montague.lit/orchard' 
                    id='disco1'
                    to='juliet@capulet.lit/balcony' 
                    type='result'>
                  <query xmlns='http://jabber.org/protocol/disco#info'/>
                    <identity category='client' type='pc'/>
                    <feature var='http://jabber.org/protocol/disco#info'/>
                    <feature var='http://jabber.org/protocol/disco#items'/>
                    <feature var='http://jabber.org/protocol/muc'/>
                  </query>
                </iq>
             */

            DiscoInfoIq diiq = new DiscoInfoIq();
            diiq.Id = "disco1";
            diiq.To = new agsXMPP.Jid("juliet@capulet.lit/balcony");
            diiq.Type = agsXMPP.protocol.client.IqType.result;

            diiq.Query.AddIdentity(new DiscoIdentity("pc", "client"));
            
            diiq.Query.AddFeature(new DiscoFeature(agsXMPP.Uri.DISCO_INFO));
            diiq.Query.AddFeature(new DiscoFeature(agsXMPP.Uri.DISCO_ITEMS));
            diiq.Query.AddFeature(new DiscoFeature(agsXMPP.Uri.MUC));

            Program.Print(diiq);

            // Build caps from this disco info
            Capabilities caps = new Capabilities();
            caps.Node = "http://www.ag-software.de/miniclient/caps";
            caps.SetVersion(diiq.Query);

            Program.Print(caps);
        }

       
    }   
}