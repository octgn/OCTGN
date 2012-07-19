using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;

namespace Octgn.App.Controllers
{
    public class DefinitionResourceController : Controller
    {
        //TODO : change this to a general location once one is decided.
        private static string gamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Games");
        //
        // GET: /DefinitionResource/

        public ActionResult Render(string gameID, string file, string resourcePath)
        {
            MemoryStream ms = new MemoryStream();
            string contentType = string.Empty;
            string path = Path.Combine(gamesPath, gameID);
            if (file.EndsWith("o8g") || file.EndsWith("o8s"))
            {
                resourcePath = resourcePath.Replace("--", "/");
                string[] files = Directory.GetFiles(path, file, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    using (ZipFile zip = ZipFile.Read(files[0]))
                    {
                        ICollection<ZipEntry> found = null;
                        if (resourcePath.Contains('/'))
                        {
                            found = zip.SelectEntries(resourcePath.Substring(resourcePath.LastIndexOf('/') + 1), resourcePath.Substring(0, resourcePath.LastIndexOf('/')));
                        }
                        else
                        {
                            found = zip.SelectEntries(resourcePath);
                        }
                        foreach (ZipEntry entry in found)
                        {
                            if (entry.FileName == resourcePath)
                            {
                                entry.OpenReader().CopyTo(ms);
                                ms.Position = 0;
                                break;
                            }
                        }
                        contentType = ContentType(resourcePath);
                    }
                }
            }
            else
            {
                resourcePath = resourcePath.Replace("--", Path.DirectorySeparatorChar.ToString());
                string target = Path.Combine(path, resourcePath,file);
                if (System.IO.File.Exists(target))
                {
                    using (FileStream stream = System.IO.File.OpenRead(target))
                    {
                        stream.CopyTo(ms);
                        ms.Position = 0;
                    }
                    contentType = ContentType(target);
                }
            }
            return new FileStreamResult(ms, contentType);
        }

        private string ContentType(string resourcePath)
        {
            string ret = string.Empty;

            string ext = resourcePath.Substring(resourcePath.LastIndexOf('.') + 1);
            switch (ext)
            {
                case "png":
                    ret = "image/png";
                    break;
                case "txt":
                    ret = "text/plain";
                    break;
                case "xml":
                    ret = "text/xml";
                    break;
                case "rels":
                    ret = "text/xml";
                    break;
                default:
                    ret = "image/jpeg";
                    break;
            }

            return (ret);
        }

    }
}
