using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using log4net;
using Octgn.Networking;

namespace Octgn.Scripting.Versions
{
    public abstract class ScriptBase : DynamicObject
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public abstract Version Version { get; }
        public abstract DateTime ReleaseDate { get; }
        public abstract DateTime DeprecatedDate { get; }
        public abstract ReleaseMode ReleaseMode { get; }
        public abstract ScriptBase Inherits { get; }

        public bool Deprecated
        {
            get
            {
                return IsNewest == false;
            }
        }

        public bool PastLifetime
        {
            get
            {
                return IsNewest == false && DateTime.Now > DeprecatedDate.AddMonths(6);
            }
        }

        public bool IsNewest
        {
            get
            {
                return LiveScripts.Where(x => x.ReleaseMode == ReleaseMode.Live)
                        .OrderByDescending(x => x.Version)
                        .First()
                        .Version == Version;
            }
        }

        public static ScriptBase[] LiveScripts
        {
            get
            {
                return Scripts.Where(x=>x.ReleaseMode == ReleaseMode.Live).ToArray();
            }
        }

        public static ScriptBase[] Scripts
        {
            get
            {
                return
                    typeof(ScriptBase).Assembly.GetModules()
						.SelectMany(x=>x.GetTypes())
                        .Where(x => x.IsSubclassOf(typeof(ScriptBase)))
                        .Select(x => Activator.CreateInstance(x) as ScriptBase)
                        .ToArray();
            }
        }

        protected Engine ScriptEngine { get { return Program.GameEngine.ScriptEngine; } }

        protected ScriptBase()
        {

        }

		/// <summary>
		/// Queues action to run on the script thread
		/// synchronusly. This action will be placed at the bottom of the 
		/// current queue
		/// </summary>
		/// <param name="a">Action</param>
        protected void QueueAction(Action a)
        {
            Program.GameEngine.ScriptEngine.Invoke(a);
        }

        /// <summary>
        /// Queues action to run on the script thread
        /// synchronusly. This action will be placed at the bottom of the 
        /// current queue
        /// </summary>
        protected T QueueAction<T>(Func<T> a)
        {
            return Program.GameEngine.ScriptEngine.Invoke<T>(a);
        }

        protected void Suspend()
        {
            Program.GameEngine.ScriptEngine.Suspend();
        }

        protected void Resume()
        {
            Program.GameEngine.ScriptEngine.Resume();
        }

        protected Mute CreateMute()
        {
            return new Mute(Program.GameEngine.ScriptEngine.CurrentJob.muted);
        }

        protected IEnumerable<MethodInfo> GetMethods()
        {
            var curScript = this;
            var deletedList = new List<string>();
            while (curScript != null)
            {
                foreach (var m in curScript.GetType().GetMethods())
                {
                    if (m.GetCustomAttributes(typeof (IgnoreScriptMethod), true).Any(x => x is IgnoreScriptMethod))
                    {
                        deletedList.Add(m.Name);
                        continue;
                    }
                    if (deletedList.Contains(m.Name, StringComparer.InvariantCultureIgnoreCase))
                        continue;
                    yield return m;
                }
                curScript = curScript.Inherits;
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var meth = GetMethods()
                .FirstOrDefault(x => x.Name.Equals(binder.Name, StringComparison.InvariantCultureIgnoreCase));

            if (meth == null)
            {
                result = null;
                return false;
            }

            result = meth.Invoke(this, args);

            return true;
        }
    }

    public enum ReleaseMode { Live, Test }

    public class IgnoreScriptMethod : Attribute
    {
        
    }
}