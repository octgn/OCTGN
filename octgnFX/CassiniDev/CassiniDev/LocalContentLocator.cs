using System;
using System.IO;

namespace CassiniDev
{
    /// <summary>
    ///   Locates content local to the project, e.g. copy always or deployment item/directory
    /// 
    ///   Depending on the runner in question, the content directory may be in any one over several relative locations. 
    ///   Let's just start backing out of the current directory  looking for it...
    /// 
    ///   For MSTest we added a deployment directory so can find somewhere in the hierarchy /web 
    ///   For NUnit the content will be in /deploy/web right here in /bin/xxx
    /// </summary>
    public class LocalContentLocator : IContentLocator
    {
        #region IContentLocator Members

        public string LocateContent()
        {
            var path = Environment.CurrentDirectory;

            while (!Directory.Exists(Path.Combine(path + "", "web")) &&
                   !Directory.Exists(Path.Combine(path + "", @"deploy\web")))
            {
                path = Path.GetDirectoryName(path);
            }

            if (Directory.Exists(Path.Combine(path + "", "web")))
            {
                path = Path.Combine(path + "", "web");
            }
            else if (Directory.Exists(Path.Combine(path + "", @"deploy\web")))
            {
                path = Path.Combine(path + "", @"deploy\web");
            }
            else
            {
                throw new Exception("could not find content");
            }

            return path;
        }

        #endregion
    }
}
