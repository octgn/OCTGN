using Octgn.Scripting;
using Octgn.Windows;
using System;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public class VersionedLoader : ILoader
    {
        private readonly log4net.ILog Log
            = log4net.LogManager.GetLogger(typeof(VersionedLoader));

        public string Name { get; } = "Versioning";

        public Task Load(ILoadingView view) {
            return Task.Run(LoadSync);
        }

        private void LoadSync() {
            Versioned.Setup(Program.DeveloperMode);
            /* This section is automatically generated from the file Scripting/ApiVersions.xml. So, if you enjoy not getting pissed off, don't modify it.*/
            //START_REPLACE_API_VERSION
			Versioned.RegisterVersion(Version.Parse("3.1.0.0"),DateTime.Parse("2014-1-12"),ReleaseMode.Live );
			Versioned.RegisterVersion(Version.Parse("3.1.0.1"),DateTime.Parse("2014-1-22"),ReleaseMode.Live );
			Versioned.RegisterVersion(Version.Parse("3.1.0.2"),DateTime.Parse("2015-8-26"),ReleaseMode.Live );
			Versioned.RegisterFile("PythonApi", "pack://application:,,,/Scripting/Versions/3.1.0.0.py", Version.Parse("3.1.0.0"));
			Versioned.RegisterFile("PythonApi", "pack://application:,,,/Scripting/Versions/3.1.0.1.py", Version.Parse("3.1.0.1"));
			Versioned.RegisterFile("PythonApi", "pack://application:,,,/Scripting/Versions/3.1.0.2.py", Version.Parse("3.1.0.2"));
			//END_REPLACE_API_VERSION
            Versioned.Register<ScriptApi>();
        }
    }
}
