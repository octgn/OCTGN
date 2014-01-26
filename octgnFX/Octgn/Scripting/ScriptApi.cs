using System;
using System.Reflection;
using Octgn.Networking;
using log4net;

namespace Octgn.Scripting
{
    public abstract class ScriptApi
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected Engine ScriptEngine { get { return Program.GameEngine.ScriptEngine; } }

        protected ScriptApi()
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

        //protected IEnumerable<MethodInfo> GetMethods()
        //{
        //    var curScript = this;
        //    var deletedList = new List<string>();
        //    while (curScript != null)
        //    {
        //        foreach (var m in curScript.GetType().GetMethods())
        //        {
        //            if (m.GetCustomAttributes(typeof (IgnoreScriptMethod), true).Any(x => x is IgnoreScriptMethod))
        //            {
        //                deletedList.Add(m.Name);
        //                continue;
        //            }
        //            if (deletedList.Contains(m.Name, StringComparer.InvariantCultureIgnoreCase))
        //                continue;
        //            yield return m;
        //        }
        //        curScript = curScript.Inherits;
        //    }
        //}

        //public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        //{
        //    var meth = GetMethods()
        //        .FirstOrDefault(x => x.Name.Equals(binder.Name, StringComparison.InvariantCultureIgnoreCase));

        //    if (meth == null)
        //    {
        //        result = null;
        //        return false;
        //    }

        //    result = meth.Invoke(this, args);

        //    return true;
        //}
    }
}