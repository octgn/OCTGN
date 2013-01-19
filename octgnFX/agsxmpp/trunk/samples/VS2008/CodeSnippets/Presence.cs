using System;

using agsXMPP;
using agsXMPP.protocol.x;
using agsXMPP.protocol.extensions.caps;

namespace CodeSnippets
{
    /*
        <presence>
            <show>xa</show>
            <status>Away from the computer.</status>
            <priority>5</priority>
            <c xmlns="http://jabber.org/protocol/caps" node="http://www.ag-software.de/caps" ver="1.1.0" ext="rtf html-im crypt" />
            <x xmlns="jabber:x:avatar">
            <hash>1bb12134f2528c4617fcc393180ddcfcc8462311</hash>
            </x>
        </presence>
     */
    class Presence
    {
        internal Presence()
        {
            agsXMPP.protocol.client.Presence pres = new agsXMPP.protocol.client.Presence();
            pres.Show = agsXMPP.protocol.client.ShowType.xa;
            pres.Status = "Away from the computer.";
            pres.Priority = 5;

            Capabilities caps = new Capabilities();
            caps.Node = "http://www.ag-software.de/caps";
            caps.Version = "1.1.0";
            // don't use the follwing anymore, its deprecated
            caps.Extensions = new string[] { "rtf", "html-im", "crypt"};

            Avatar avatar = new Avatar();
            avatar.Hash = "1bb12134f2528c4617fcc393180ddcfcc8462311";

            pres.AddChild(caps);
            pres.AddChild(avatar);
                        
            Console.WriteLine("Custom Presence Packet:");
            Program.Print(pres);           
        }
    }
}
