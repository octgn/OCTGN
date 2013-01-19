using System;
using System.Text;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;

using System.Xml;

using agsXMPP.Xml.Dom;

namespace CodeSnippets
{
    class Program
    {
        static void Main(string[] args)
        {
            Disco disco = new Disco();
            Shim shim = new Shim();
            Presence pres = new Presence();
            XmppPing ping = new XmppPing();
            Privacy p = new Privacy();
            Message message = new Message();

            Console.WriteLine("Press enter key to close this program!");
            Console.ReadLine();            
        }

        internal static void Print(Element el)
        {
            Console.WriteLine(el.ToString(Formatting.Indented));
            Console.WriteLine("\r\n");
        }
    }
}





