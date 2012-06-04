using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Utils
{
    class XmlSimpleValidate
    {
        public XmlSetParser xml_set;

        public XmlSimpleValidate(XmlSetParser _xml_set)
        {
            xml_set = _xml_set;
        }

        public void CheckXml(Octgn.Data.Game game)
        {
            XmlSetParser xmls = xml_set;
            if (game.Id.ToString() != xmls.game())
            {
                throw new Exception("Error! Wrong game specified in xml");
            }
            xmls.check();
        }

        public void CheckXml(Windows.ChangeSetsProgressDialog wnd, int max, Octgn.Data.Game game)
        {
            XmlSetParser xmls = xml_set;
            if (game.Id.ToString() != xmls.game())
            {
                wnd.UpdateProgress(max, max, string.Format("Error! Wrong game specified in xml"), false);
                return;
            }
            xmls.check();
        }

        public void CheckVerboseXml(Windows.ChangeSetsProgressDialog wnd, int max, Octgn.Data.Game game)
        {
            XmlSetParser xmls = xml_set;
            wnd.UpdateProgress(1, max, "Parsing retrieved xml...", false);
            xmls.check();
            if (game.Id.ToString() != xmls.game())
            {
                wnd.UpdateProgress(10, 10, string.Format("Error! Wrong game specified in xml"), false);
                return;
            }
            wnd.UpdateProgress(2, max, "Name: " + xmls.name(), false);
            wnd.UpdateProgress(3, max, "Game: " + xmls.game(), false);
            wnd.UpdateProgress(4, max, "UUID: " + xmls.uuid(), false);
            wnd.UpdateProgress(5, max, "Version: " + xmls.version(), false);
            wnd.UpdateProgress(6, max, "Date: " + xmls.date(), false);
            wnd.UpdateProgress(7, max, "Link: " + xmls.link(), false);
            wnd.UpdateProgress(8, max, "Login: " + xmls.user(), false);
            wnd.UpdateProgress(9, max, "Password: " + xmls.password(), false);
            wnd.UpdateProgress(10, 10, string.Format("Xml seems ok"), false);
        }
    }
}
