using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;

namespace Octgn.App.Areas.api.Controllers
{
    public class ResourceListController : Controller
    {
        //TODO : change this to a general location once one is decided.
        private static string gamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Games");
        
        //
        // GET: /api/ResourceList/

        [AcceptVerbs("get","post")]
        public ActionResult Get(string resourcePath, string o8xPath)
        {
            Tuple<List<string>, List<string>> list;
            if (o8xPath != null && o8xPath.Length > 0)
            {
                list = ListFromO8x(resourcePath, o8xPath);
            }
            else
            {
                list = ListFileResources(resourcePath);
            }

            var jsonResult = new { directories = list.Item1.ToArray(), files = list.Item2.ToArray() };
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        private Tuple<List<string>, List<string>> ListFileResources(string resourcePath)
        {
            List<string> directoryList = new List<string>();
            List<string> fileList = new List<string>();

            string target = string.Empty;

            if (resourcePath == null || resourcePath.Length == 0)
            {
                target = gamesPath;
                string[] fileListArray = Directory.GetFiles(gamesPath);
                for (int i = 0; i < fileListArray.Length; i++)
                {
                    fileListArray[i] = fileListArray[i].Substring(fileListArray[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
                }
                string[] directoryListArray = Directory.GetDirectories(gamesPath);
                for (int i = 0; i < directoryListArray.Length; i++)
                {
                    directoryListArray[i] = directoryListArray[i].Substring(directoryListArray[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
                }
                directoryList.AddRange(directoryListArray);
                fileList.AddRange(fileListArray);
            }
            else
            {
                target = Path.Combine(gamesPath, resourcePath);
                string[] fileListArray = Directory.GetFiles(target);
                for (int i = 0; i < fileListArray.Length; i++)
                {
                    fileListArray[i] = fileListArray[i].Substring(gamesPath.Length + 1).Replace(Path.DirectorySeparatorChar, '/');// Replace(gamesPath, "").Replace('\\', '/');
                }
                string[] directoryListArray = Directory.GetDirectories(target);
                for (int i = 0; i < directoryListArray.Length; i++)
                {
                    directoryListArray[i] = directoryListArray[i].Substring(gamesPath.Length + 1).Replace(Path.DirectorySeparatorChar, '/');//Replace(gamesPath, "").Replace('\\', '/');
                }
                directoryList.AddRange(directoryListArray);
                fileList.AddRange(fileListArray);
            }
            return new Tuple<List<string>, List<string>>(directoryList, fileList);
        }

        private Tuple<List<string>, List<string>> ListFromO8x(string resourcePath, string o8xPath)
        {
            List<string> directoryList = new List<string>();
            List<string> fileList = new List<string>();

            string target = Path.Combine(gamesPath, o8xPath);
            using (ZipFile zip = ZipFile.Read(target))
            {
                ICollection<ZipEntry> found = null;
                if(resourcePath == null || resourcePath.Length == 0)
                {
                    found = zip.SelectEntries("*.*");
                }
                else
                {
                    found = zip.SelectEntries("*.*", resourcePath);
                }
                foreach (ZipEntry entry in found)
                {
                    if (entry.IsDirectory)
                    {
                        directoryList.Add(entry.FileName);
                    }
                    else
                    {
                        fileList.Add(entry.FileName);
                    }
                }
            }

            return new Tuple<List<string>, List<string>>(directoryList, fileList);
        }

    }
}
