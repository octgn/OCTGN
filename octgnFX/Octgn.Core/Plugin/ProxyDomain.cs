using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using log4net;

namespace Octgn.Core.Plugin
{
    public class ProxyDomain : MarshalByRefObject
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static ProxyDomain CreateDomain(string name, string path)
        {
            var domaininfo = new AppDomainSetup { ApplicationBase = path, PrivateBinPath = path };
            var adevidence = AppDomain.CurrentDomain.Evidence;
            var domain = AppDomain.CreateDomain(name, adevidence, domaininfo, new PermissionSet(PermissionState.Unrestricted));
            //var domain = AppDomain.CreateDomain(name, adevidence, path, path, true);

            Type type = typeof(ProxyDomain);
            var value = (ProxyDomain)domain.CreateInstanceFromAndUnwrap(
                type.Assembly.Location,
                type.FullName);
            value.Domain = domain;
            return value;
        }

        public AppDomain Domain { get; set; }

        public void LoadAssembly(string assemblyPath)
        {
            try
            {
                Log.Debug(AppDomain.CurrentDomain.FriendlyName);
                var ass = Assembly.LoadFrom(assemblyPath);
                Log.Debug(ass.FullName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error loading assembly " + assemblyPath, ex);
            }
        }

        public IEnumerable<Type> GetImplementingTypes<T>()
        {
            var to = typeof(T);
            var ass = Domain.GetAssemblies();
            return from a in ass
                   from t in a.GetTypes()
                   where !t.IsInterface
                   where !t.IsAbstract
                   where to.IsInterface ? t.GetInterface(to.Name) != null : t.IsSubclassOf(to)
                   select t;
        }

        /// <summary>
        /// Create an instance of <typeparamref name="T"/> in <see cref="Module"/>'s <see cref="AppDomain"/>
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <returns>Instance of T</returns>
        public T Create<T>() where T : class
        {
            var assList = GetImplementingTypes<T>();
            var ass = assList.Single();
            //var vwrap = Activator.CreateInstanceFrom(Domain, ass.Assembly.Location, ass.FullName);
            //T value = AsType<T>(vwrap.Unwrap());
            var value = Domain.CreateInstanceFrom(
                ass.Assembly.Location,
                ass.FullName);
            return (T)value.Unwrap();
        }
    }
}