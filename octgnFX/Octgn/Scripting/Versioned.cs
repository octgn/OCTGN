using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Octgn.Library;
using log4net;

namespace Octgn.Scripting
{
	public class Versioned
	{
	    private static bool isDeveloperMode;
        internal static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	    private static readonly object Locker = new object();
		private static Dictionary<Version,VersionMetaData> _versionData = new Dictionary<Version, VersionMetaData>(); 
	    private static readonly Dictionary<Type, Dictionary<VersionedAttribute, Type>> Versions = new Dictionary<Type, Dictionary<VersionedAttribute, Type>>(); 
		private static readonly Dictionary<string,Dictionary<Version,VersionedFileMetaData>> FileVersions = new Dictionary<string, Dictionary<Version, VersionedFileMetaData>>(StringComparer.InvariantCultureIgnoreCase);

		public static void Setup(bool developerMode)
		{
		    isDeveloperMode = developerMode;
            //_versionData.Clear();
            //Versions.Clear();
            //FileVersions.Clear();
		}

		public static bool ValidVersion(Version v)
		{
            if (X.Instance.Debug || isDeveloperMode)
            {
                return _versionData.ContainsKey(v);
            }
            else
            {
                return _versionData.Where(x => x.Value.Mode == ReleaseMode.Live).Any(x=>x.Key == v);
            }
		}

		public static Version LatestVersion
		{
		    get
		    {
		        if (X.Instance.Debug || isDeveloperMode)
		        {
		            return _versionData.Select(x => x.Key).FirstOrDefault();
		        }
		        else
		        {
		            return _versionData.Where(x => x.Value.Mode == ReleaseMode.Live).Select(x => x.Key).FirstOrDefault();
		        }
		    }
		}

		public static void RegisterVersion(Version version, DateTime releaseDate, ReleaseMode mode)
		{
		    var newv = new VersionMetaData()
		        {
		            Version = version,
		            ReleaseDate = releaseDate,
		            Mode = mode
		        };
			if(_versionData.ContainsKey(version))
				throw new ArgumentException("Can't register version " + version + " it is already registered.","version");

			if(mode == ReleaseMode.Test && _versionData.Any(x=>x.Value.Mode == ReleaseMode.Test))
				throw new ArgumentException("Can't have multiple versions in Test at once","mode");

			_versionData.Add(version,newv);

			// Process versions
			// Reorder
		    _versionData = _versionData.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

			// Set all proper delete times
		    var foundLive = false;
		    var i = 0;
			foreach (var v in _versionData)
			{
				if (foundLive == false && i != 2 && v.Value.Mode == ReleaseMode.Live)
				{
                    v.Value.DeleteDate = DateTime.MaxValue;
			        foundLive = true;
                    i++;
				    continue;
				}
				if (v.Value.Mode == ReleaseMode.Test)
				{
				    v.Value.DeleteDate = DateTime.MaxValue;
                    i++;
				    continue;
				}
			    v.Value.DeleteDate = v.Value.ReleaseDate.AddMonths(6);
                i++;
			}
		}

		public static T Get<T>(Version version)
		{
			if (_versionData.ContainsKey(version) == false)
			{
                throw new InvalidOperationException("Can't get versioned type for " + typeof(T) + " because version " + version + " wasn't registered.");
			}
			if (Versions.ContainsKey(typeof(T)) == false)
			{
				throw new InvalidOperationException("Can't get versioned type for " + typeof(T) + " because it wasn't registered.");
			}
            if (Versions[typeof(T)].Any(x => x.Key.Version == version) == false)
            {
                throw new InvalidOperationException("Can't get versioned type for " + typeof(T) + " because the version number " + version + " is incorrect.");
            }
            if (X.Instance.Debug || isDeveloperMode)
            {
                var ret = Versions[typeof (T)].FirstOrDefault(x => x.Key.Version == version).Value;

                return (T)Activator.CreateInstance(ret);
            }
            else
            {
                var okVersions = _versionData.Where(x => x.Value.Mode == ReleaseMode.Live).Select(x => x.Key).ToArray();

                if (Versions[typeof(T)].Where(x=>okVersions.Contains(x.Key.Version))
                    .Any(x => x.Key.Version == version) == false)
                {
                    throw new InvalidOperationException("Can't get versioned type for " + typeof(T) + " because the version requested isn't live yet.");
                }

                var ret = Versions[typeof(T)].FirstOrDefault(x => x.Key.Version == version).Value;

                return (T)Activator.CreateInstance(ret);
            }
		}

		public static VersionedFileMetaData GetFile(string name, Version version)
		{
            if (_versionData.ContainsKey(version) == false)
            {
                throw new InvalidOperationException("Can't get versioned file for " + name + " because version " + version + " wasn't registered.");
            }
            if (FileVersions.ContainsKey(name) == false)
            {
                throw new InvalidOperationException("Can't get versioned file for " + name + " because it wasn't registered.");
            }
            if (FileVersions[name].ContainsKey(version) == false)
            {
                throw new InvalidOperationException("Can't get versioned file for " + name + " v" + version + " because the version number " + version + " is incorrect or wasn't registered.");
            }
            if (X.Instance.Debug || Program.DeveloperMode)
            {
                return FileVersions[name][version];
            }

			// It's live mode, so we don't want any test versions being used.
		    var okVersions = _versionData.Where(x => x.Value.Mode == ReleaseMode.Live).Select(x => x.Key).ToArray();

		    if (okVersions.Contains(version) == false)
		    {
		        throw new InvalidOperationException("Can't get versioned file for " + name + " because the version requested isn't live yet.");
		    }

            return FileVersions[name][version];
		}

		public static void Register<T>()
		{
		    var allSupers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x=>x.GetModules())
		        .SelectMany(x => x.GetTypes())
		        .Where(x => x.IsSubclassOf(typeof (T)))
                .ToArray();

			var types = new Dictionary<VersionedAttribute, Type>();
			foreach (var s in allSupers)
			{
			    var attr = s.GetCustomAttributes(typeof (VersionedAttribute), false).FirstOrDefault() as VersionedAttribute;
			    if (attr == null)
			    {
					Log.WarnFormat("Can't load {0} for type {1}, it does not have a Versioned attribute",s.Name,typeof(T).Name);
			        continue;
			    }

				types.Add(attr,s);
			}

		    var typesOrdered = types.OrderByDescending(x => x.Key.Version).ToDictionary(x => x.Key, x => x.Value);

            Versions.Add(typeof(T), typesOrdered);
		}

		public static void RegisterFile(string name, string path, Version version)
		{
			var types = new Dictionary<Version, VersionedFileMetaData>();
			if (FileVersions.ContainsKey(name))
			{
			    types = FileVersions[name];
			}
			else
			{
			    FileVersions[name] = types;
			}

			if(types.ContainsKey(version))
				throw new ArgumentException(String.Format("Can't add file {0} version {1}, version {1} is already registered.",name,version),"version");

			if(types.Any(x=>x.Value.Path.Equals(path,StringComparison.InvariantCultureIgnoreCase)))
                throw new ArgumentException(String.Format("Can't add file {0} path {1}, path {1} is already registered.", name, path),"path");

			types.Add(version,new VersionedFileMetaData()
			    {
			        Name = name,
					Path=path
			    });

            var typesOrdered = types.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            FileVersions[name] = typesOrdered;
		}

		public static void FinishLoading()
		{
		    // Assert here that we have a version of everything
		}
	}

	public class VersionedAttribute : Attribute
	{
        public Version Version { get; set; }
        public VersionedAttribute(string version)
        {
            Version = Version.Parse(version);
            //if(!Versioned.ValidVersion(Version))
            //    throw new ArgumentException(String.Format("Can't set version to {0}, version {0} was never registered.",Version),"version");
        }
	}

    public enum ReleaseMode { Live, Test }

	public class VersionMetaData
	{
        public Version Version { get; set; }
        public ReleaseMode Mode { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime DeleteDate { get; set; }
	}

	public class VersionedFileMetaData
	{
	    public string Name { get; set; }
		public string Path { get; set; }
	}
}