//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Net;
//using System.IO;

//namespace Octgn.Utils
//{
//    class XmlInstaller
//    {

//        public XmlSetParser xml_set;

//        public XmlInstaller(XmlSetParser _xml_set)
//        {
//            xml_set = _xml_set;
//        }

//        public void installSet(Windows.ChangeSetsProgressDialog wnd, int max, DataNew.Entities.Game game)
//        {
//            wnd.UpdateProgress(0, max, "Retrieving xml...", false);
//            XmlSetParser xmls = xml_set;
//            wnd.UpdateProgress(1, max, "Parsing retrieved xml...", false);
//            xmls.check();
//            bool is_spoiler_installed = true;
//            DataNew.Entities.Set set = null;
//            string path = Path.Combine(Prefs.DataDirectory, "Games", game.Id.ToString(), "Sets");
//            string downloadto = Path.Combine(path) + xmls.name() + ".o8s";
//            var cli = new WebClient();
//            wnd.UpdateProgress(2, max, "Downloading new definition...", false);
//            cli.Credentials = new System.Net.NetworkCredential(xmls.user(), xmls.password());
//            cli.DownloadFile(xmls.link(), downloadto);
//            wnd.UpdateProgress(3, max, "Checking for existence of old definition...", false);
//            try
//            {
//                set = game.Sets.First<DataNew.Entities.Set>(_set => _set.Id.ToString() == xmls.uuid());
//            }
//            catch
//            {
//                is_spoiler_installed = false;
//            }
//            if (is_spoiler_installed)
//            {
//                wnd.UpdateProgress(4, max, "Removing old definition...", false);
//                game.DeleteSet(set);
//            }
//            wnd.UpdateProgress(5, max, "Installing new definition...", false);
//            game.InstallSet(downloadto);
//            wnd.UpdateProgress(6, max, "Set installed correctly", false);
//        }

//        public void installSet(Windows.UpdateChecker wnd, DataNew.Entities.Game game)
//        {
//            wnd.UpdateStatus("Retrieving xml...");
//            XmlSetParser xmls = xml_set;
//            wnd.UpdateStatus("Parsing retrieved xml...");
//            xmls.check();
//            bool is_spoiler_installed = true;
//            DataNew.Entities.Set set = null;
//            string path = Path.Combine(Prefs.DataDirectory, "Games", game.Id.ToString(), "Sets");
//            string downloadto = Path.Combine(path) + xmls.name() + ".o8s";
//            var cli = new WebClient();
//            wnd.UpdateStatus("Downloading new definition...");
//            if (xmls.user() != null && xmls.user() != "")
//                cli.Credentials = new System.Net.NetworkCredential(xmls.user(), xmls.password());
//            cli.DownloadFile(xmls.link(), downloadto);
//            wnd.UpdateStatus( "Checking for existence of old definition...");
//            try
//            {
//                set = game.Sets.First<DataNew.Entities.Set>(_set => _set.Id.ToString() == xmls.uuid());
//            }
//            catch
//            {
//                is_spoiler_installed = false;
//            }
//            if (is_spoiler_installed)
//            {
//                wnd.UpdateStatus("Removing old definition...");
//                game.DeleteSet(set);
//            }
//            wnd.UpdateStatus("Installing new definition...");
//            game.InstallSet(downloadto);
//            wnd.UpdateStatus("Set installed correctly");
//        }
//    }
//}
