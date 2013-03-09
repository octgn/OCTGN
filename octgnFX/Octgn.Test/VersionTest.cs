using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace Octgn.Test
{
    public class VersionTest
    {
        [Test]
        public void CheckAssemblyVersions()
        {
            //Get the correct version number from currentversion.xml
            var vstream = Assembly.GetAssembly(typeof (Program)).GetManifestResourceStream("Octgn.CurrentVersion.txt");
            var versionString = "";
            using(var sr = new StreamReader(vstream))
            {
                versionString = sr.ReadToEnd().Trim();
            }
            Debug.WriteLine("Specified Version: {0}",(object)versionString);
            Version realVersion = null;
            Assert.True(Version.TryParse(versionString,out realVersion));

            Debug.WriteLine("Check Version Of Octgn");
            Assert.AreEqual(realVersion , typeof (Program).Assembly.GetName().Version);
            Debug.WriteLine("Check Version Of Octgn.Data");
            Assert.AreEqual(realVersion , typeof(Octgn.Data.Game).Assembly.GetName().Version);
            Debug.WriteLine("Check Version Of Octgn.Server");
            Assert.AreEqual(realVersion  ,typeof(Octgn.Server.Server).Assembly.GetName().Version);
            Debug.WriteLine("Check Version Of Skylabs.Lobby");
            Assert.AreEqual(realVersion , typeof(Skylabs.Lobby.Client).Assembly.GetName().Version);
            Debug.WriteLine("Check Version Of Octgn.LobbyServer");
            Assert.AreEqual(realVersion , typeof(Skylabs.LobbyServer.Program).Assembly.GetName().Version);
        }
    }
}
