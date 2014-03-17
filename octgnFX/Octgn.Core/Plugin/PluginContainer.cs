using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace Octgn.Core.Plugin
{
    public class PluginContainer : MarshalByRefObject, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IPlugin _plugin;

        public PluginContainer(string fullPath)
        {
            Assemblies = new List<string>();
            Path = fullPath;
            Load();
        }

        #region Loading

        public bool IsInvalid
        {
            get { return Directory.Exists(Path) == false || _plugin == null; }
        }

        internal void Load()
        {
            (Assemblies as List<string>).Clear();

            foreach (FileInfo f in new DirectoryInfo(Path).GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                if (isDotNetAssembly(f.FullName))
                {
                    (Assemblies as List<string>).Add(f.FullName);
                }
            }
            if (Domain != null)
            {
                FireHandleUnloadAppDomain();
                Domain = null;
            }

            Domain = ProxyDomain.CreateDomain(PluginId.ToString(), Path);
            foreach (string p in Assemblies)
            {
                Domain.LoadAssembly(p);
            }
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            var m = Domain.Create<IPlugin>();
            _plugin = m;
        }

        private bool isDotNetAssembly(string filename)
        {
            try
            {
                return AssemblyName.GetAssemblyName(filename) != null;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return args.RequestingAssembly;
        }

        #endregion Loading

        public Guid PluginId { get; set; }
        public string Path { get; set; }
        public IEnumerable<string> Assemblies { get; private set; }
        public ProxyDomain Domain { get; private set; }

        public IPlugin Plugin
        {
            get
            {
                return _plugin;
            }
        }

        public event EventHandler HandleUnloadAppDomain;

        protected virtual void FireHandleUnloadAppDomain()
        {
            EventHandler handler = HandleUnloadAppDomain;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            (Assemblies as List<string>).Clear();
            if (_plugin != null)
            {
                string sname = _plugin.Name;
                try
                {
                    //_plugin.Dispose();
                }
                catch (Exception e)
                {
                    Log.Warn("Error disposing plugin=" + sname + " iid=" +PluginId, e);
                }
            }
            _plugin = null;
            FireHandleUnloadAppDomain();
        }
    }
}