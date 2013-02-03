namespace Octgn.Hosting.Services
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Linq;

    using Mono.Options;

    internal static class Program
    {
        private static bool runInConsole;
        private static Type serviceType;

        internal static void Main(string[] args)
        {
            runInConsole = false;
#if(DEBUG)
            runInConsole = true;
#endif
            var options = CreateOptions();
            options.WriteOptionDescriptions(Console.Out);
            Console.ReadLine();
        }

        private static OptionSet CreateOptions()
        {
            var ret = new OptionSet();
            ret.Add(
                "c|console",
                "Run the service in console mode instead of as a service",
                new Action<bool>(b => runInConsole = b));
            ret.Add(
                "s|service=",
                "Which service to run. Available options are: " + string.Join(", ", Services.Select(x => x.Name)),
                SelectService);
            return ret;
        }

        static Type[] Services
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ServiceBase))).ToArray();
            }
        }

        static void SelectService(string name)
        {
            serviceType = Services.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
        }

        static void RunService(Type t)
        {
            var service = Activator.CreateInstance(t) as ServiceBase;
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                service 
            };
            ServiceBase.Run(ServicesToRun);
        }

        static void RunService<T>() where T : ServiceBase
        {
            var service = Activator.CreateInstance<T>();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                service 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
