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

        public ActionResult Render(string resourcePath, string o8xPath)
        {
            MemoryStream ms = new MemoryStream();
            
            string contentType = string.Empty;
            if (o8xPath != null || o8xPath.Length == 0)
            {
                contentType = LoadResourceFromO8x(ms, resourcePath, o8xPath);
            }
            else
            {
                contentType = LoadFileResource(ms, resourcePath);
            }

            return new FileStreamResult(ms, contentType);
        }

        private string LoadResourceFromO8x(MemoryStream ms, string resourcePath, string o8xPath)
        {
            string ret = string.Empty;
            o8xPath = o8xPath.Replace("--", Path.DirectorySeparatorChar.ToString());
            resourcePath = resourcePath.Replace("--", "/");
            if (o8xPath.EndsWith("o8g") || o8xPath.EndsWith("o8s"))
            {
                string o8xTarget = Path.Combine(gamesPath, o8xPath);
                using (ZipFile zip = ZipFile.Read(o8xTarget))
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
                    ret = ContentType(resourcePath);
                }
            }
            return (ret);
        }

        private string LoadFileResource(MemoryStream ms, string resourcePath)
        {
            string ret = string.Empty;
            resourcePath = resourcePath.Replace("--", Path.DirectorySeparatorChar.ToString());
            string target = Path.Combine(gamesPath, resourcePath);
            if (System.IO.File.Exists(target))
            {
                using (FileStream stream = System.IO.File.OpenRead(target))
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;
                }
                ret = ContentType(target);
            }
            return (ret);
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
