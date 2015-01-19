using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            return new Mute(Program.GameEngine.ScriptEngine.CurrentJob.Muted);
        }

        public void RegisterEvent(string name, IronPython.Runtime.PythonFunction derp)
        {
            ScriptEngine.RegisterFunction(name, derp);
        }

        private RandomAsync _randRequest;
        public int Random(int min, int max)
        {
            _randRequest = new RandomAsync(ScriptEngine, min, max);
            return _randRequest.Get();
        }

        public void RandomResult(int result)
        {
            _randRequest.Continuation(result);
        }

        private class RandomAsync
        {
            private readonly Engine _engine;
            private int _result;
            private readonly int _min;
            private readonly int _max;
            private bool _gotResult;

            public RandomAsync(Engine engine, int min, int max)
            {
                _engine = engine;
                _min = min;
                _max = max;
            }

            public int Get()
            {
                Task.Factory.StartNew(RunThread);
                _engine.Suspend();
                return _result;
            }

            private void RunThread()
            {
                try
                {
                    while (Program.IsGameRunning)
                    {
                        try
                        {
                            lock (this)
                            {
                                if (_gotResult)
                                    return;
                            }
                            if (Program.Client == null)
                                return;
                            if (Program.GameEngine == null)
                                return;
                            Program.Client.Rpc.RandomReq(_min, _max);
                            Thread.Sleep(3000);
                            lock (this)
                            {
                                if (_gotResult)
                                    return;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    if (_gotResult == false)
                        Continuation(0);
                }
            }

            public void Continuation(int result)
            {
                lock (this)
                {
                    if (_gotResult)
                        return;
                    _gotResult = true;
                }
                _result = result;
                _engine.Resume();
            }
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