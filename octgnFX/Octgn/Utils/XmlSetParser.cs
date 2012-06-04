using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Globalization;


namespace Octgn.Utils
{
    class XmlSetParser
    {
        private string xmlUrl;
        XPathNavigator nav;
        XPathDocument docNav;
        XPathNodeIterator NodeIter;
        String strExpression;

        public XmlSetParser(string _xmlUrl)
        {
            xmlUrl = _xmlUrl;
            docNav = new XPathDocument(xmlUrl);
            nav = docNav.CreateNavigator();
        }

        public XmlSetParser(XmlReader _xmlReader)
        {
            docNav = new XPathDocument(_xmlReader);
            nav = docNav.CreateNavigator();
        }


        public string name()
        {
            NodeIter = nav.Select("/set/name");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public string game()
        {
            NodeIter = nav.Select("/set/game");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public string uuid()
        {
            NodeIter = nav.Select("/set/uuid");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public float version()
        {
            NodeIter = nav.Select("/set/version");
            NodeIter.MoveNext();
            //because in Poland default separator is ',' which causes problems
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            float n = float.Parse(NodeIter.Current.Value, NumberStyles.Any,ci);
            return n;
        }

        public string date()
        {
            NodeIter = nav.Select("/set/date");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public string link()
        {
            NodeIter = nav.Select("/set/link");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public string user()
        {
            NodeIter = nav.Select("/set/user");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public string password()
        {
            NodeIter = nav.Select("/set/password");
            NodeIter.MoveNext();
            return NodeIter.Current.Value;
        }

        public bool check()
        {
            name();
            game();
            uuid();
            version();
            date();
            link();
            user();
            password();
            return true;
        }



    }
}
