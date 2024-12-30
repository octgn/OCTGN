﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Octgn.Networking;
using Octgn.Play;
using System.Reflection;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;

using Microsoft.Scripting.Utils;

using Octgn.Core;
using Octgn.Core.DataExtensionMethods;

using log4net;
using Octgn.Library;

namespace Octgn.Scripting
{

    [Export]
    public class Engine : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ScriptScope ActionsScope;
        private ScriptEngine _engine;
        private readonly Queue<ScriptJobBase> _executionQueue = new Queue<ScriptJobBase>(4);
        private readonly MemoryStream _outputStream = new MemoryStream();
        private StreamWriter _outputWriter;
        // This is a hack. The sponsor object is used to keep the remote side of the Dialog API alive.
        // I would like to make this cleaner but it really seems to be an impass at the moment.
        // Combining Scripting + Remoting + Lifetime management + Garbage Collection + Partial trust
        // is an aweful and ugly mess.
        //private Sponsor _sponsor;

        public Engine()
            : this(false)
        {
        }

        public Engine(bool forTesting)
        {
            Program.GameEngine.ScriptEngine = this;
            Program.GameEngine.EventProxy = new GameEventProxy(this, Program.GameEngine);
        }

        public void SetupEngine(bool testing)
        {
            Log.DebugFormat("Creating scripting engine: forTesting={0}", testing);
            //AppDomain sandbox = CreateSandbox(testing);
            _engine = Python.CreateEngine();
            //_engine.SetTrace(OnTraceback);
            _outputWriter = new StreamWriter(_outputStream);
            _engine.Runtime.IO.SetOutput(_outputStream, _outputWriter);
            _engine.SetSearchPaths(new[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Scripting\Lib") });

            var workingDirectory = Directory.GetCurrentDirectory();
            Log.DebugFormat("Setting working directory: {0}", workingDirectory);
            if (Program.GameEngine != null)
            {
                workingDirectory = Path.Combine(Config.Instance.Paths.DatabasePath, Program.GameEngine.Definition.Id.ToString());
                var search = _engine.GetSearchPaths();
                search.Add(workingDirectory);
                _engine.SetSearchPaths(search);
            }
            //var workingDirectory = Directory.GetCurrentDirectory();
            if (Program.GameEngine != null)
            {
                workingDirectory = Path.Combine(Config.Instance.Paths.DatabasePath, Program.GameEngine.Definition.Id.ToString());
            }

            ActionsScope = CreateScope(workingDirectory);
            if (Program.GameEngine == null || testing) return;
            Log.Debug("Loading Scripts...");
            foreach (var script in Program.GameEngine.Definition.GetScripts().ToArray())
            {
                try
                {
                    Log.DebugFormat("Loading Script {0}", script.Path);
                    var str = File.ReadAllText(script.Path);
                    str = ConvertWinforms(str);
                    var src = _engine.CreateScriptSourceFromString(str, script.Path);
                    //var src = _engine.CreateScriptSourceFromFile(script.Path);
                    src.Execute(ActionsScope);
                    Log.DebugFormat("Script Loaded");
                }
                catch (Exception e)
                {
                    var gs = script ?? new Octgn.DataNew.Entities.GameScript()
                    {
                        Path = "Unknown"
                    };
                    var eo = _engine.GetService<ExceptionOperations>();
                    var error = eo.FormatException(e);
                    Program.GameMess.Warning("Could not load script " + gs.Path + Environment.NewLine + error);
                }
            }
            Log.Debug("Scripts Loaded.");
        }

        public TracebackDelegate OnTraceback(TraceBackFrame frame, string result, object payload)
        {

            var code = (FunctionCode)frame.f_code;
            if (result == "call")
            {
                Program.GameMess.GameDebug("[{0}:{1}]{2}", code.co_filename, (int)frame.f_lineno, code.co_name);
            }
            return this.OnTraceback;
        }

        public void ReloadScripts()
        {
            this.SetupEngine(false);
        }

        internal ScriptJobBase CurrentJob
        {
            get { return _executionQueue.Peek(); }
        }

        public String[] TestScripts(GameEngine game)
        {
            var errors = new List<string>();
            foreach (var s in game.Definition.GetScripts())
            {
                try
                {
                    ScriptSource src = _engine.CreateScriptSourceFromString(s.Script, SourceCodeKind.Statements);
                    src.Execute(ActionsScope);
                }
                catch (Exception e)
                {
                    var eo = _engine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(e);
                    errors.Add(String.Format("[{2}:{0}]: Python Error:\n{1}", game.Definition.Name, error, s.Path));
                }
            }
            return errors.ToArray();
        }

        public ScriptScope CreateScope(string workingDirectory)
        {
            ScriptScope scope = _engine.CreateScope();
            InjectOctgnIntoScope(scope, workingDirectory);
            return scope;
        }

        public bool TryExecuteInteractiveCode(string code, ScriptScope scope, Action<ExecutionResult> continuation)
        {
            ScriptSource src = _engine.CreateScriptSourceFromString(code, SourceCodeKind.InteractiveCode);
            switch (src.GetCodeProperties())
            {
                case ScriptCodeParseResult.IncompleteToken:
                    return false;
                case ScriptCodeParseResult.IncompleteStatement:
                    // An empty line ends the statement
                    if (!code.TrimEnd(' ', '\t').EndsWith("\n"))
                        return false;
                    break;
            }
            StartExecution(src, scope, continuation);
            return true;
        }

        public void RegisterFunction(string function, PythonFunction derp)
        {
            //efcache[function] = derp;
        }

        //private readonly Dictionary<string, PythonFunction> efcache = new Dictionary<string, PythonFunction>();
        //private readonly Version ev = new Version("3.1.0.2");
        //private bool didMsg = false;
        public void ExecuteFunction(string function, params object[] args)
        {
            //if (Program.GameEngine.Definition.ScriptVersion < ev)
            //{
            //    if (!didMsg)
            //    {
            //        didMsg = true;
            //        Program.Print(Player.LocalPlayer, "Using old event system");
            //    }
            var sb = new StringBuilder();

            if (args.Length == 1 && args[0] is System.Dynamic.ExpandoObject)
            {
                sb.Append("EventArgument({");
                IDictionary<string, object> propertyValues = (IDictionary<string, object>)args[0];
                var i = 0;
                foreach (var prop in propertyValues)
                {
                    var isLast = i == propertyValues.Count - 1;
                    sb.Append("\"" + prop.Key + "\":");
                    var a = prop.Value;
                    if (a is Array)
                    {
                        var arr = a as Array;
                        sb.Append("[");
                        var argStrings = new List<string>();
                        foreach (var o in arr)
                        {
                            argStrings.Add(FormatObject(o));
                        }
                        sb.Append(string.Join(",", argStrings));
                        sb.Append("]");
                    }
                    else sb.Append(FormatObject(a));
                    if (!isLast) sb.Append(", ");
                    i++;
                }
                sb.Append("})");
            }
            else
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var isLast = i == args.Length - 1;
                    var a = args[i];
                    if (a is Array)
                    {
                        var arr = a as Array;
                        sb.Append("[");
                        var argStrings = new List<string>();
                        foreach (var o in arr)
                        {
                            argStrings.Add(FormatObject(o));
                        }
                        sb.Append(string.Join(",", argStrings));
                        sb.Append("]");
                    }
                    else sb.Append(FormatObject(a));

                    if (!isLast) sb.Append(", ");

                }
            }
            ExecuteFunctionNoFormat(function, sb.ToString());
            //}
            //else
            //{
            //    if (!didMsg)
            //    {
            //        didMsg = true;
            //        Program.Print(Player.LocalPlayer, "Using new event system");
            //    }
            //    if (efcache.ContainsKey(function) == false)
            //    {
            //        const string format = @"_api.RegisterEvent(""{0}"", {0})";
            //        var str = string.Format(format, function);

            //        var src = _engine.CreateScriptSourceFromString(str, SourceCodeKind.Statements);
            //        StartExecution(src, ActionsScope, (x) =>
            //        {
            //            if (efcache.ContainsKey(function))
            //            {
            //                ExecuteFunction(function, args);
            //                return;
            //            }
            //            Log.Error("The function should have been registered... " + function);
            //        });
            //        return;
            //    }

            //    if (_executionQueue.Count == 0)
            //    {
            //        var jerb = new InvokedScriptJob(() => ExecuteFunction(function, args));

            //        //ExecuteFunction(function, args);
            //        StartExecution(jerb);
            //        return;
            //    }

            //    var fun = efcache[function];
            //    //var con = HostingHelpers.GetLanguageContext(_engine);
            //    try
            //    {
            //        // Get the args
            //        var newArgList = new List<object>();
            //        foreach (var arg in args)
            //        {
            //            var na = ConvertArgs(arg);
            //            newArgList.Add(na);
            //        }
            //        _engine.Operations.Invoke(fun, newArgList.ToArray());

            //    }
            //    catch (Exception e)
            //    {
            //        Log.Error(e);
            //    }
            //}
        }

        public void ExecuteFunctionNoFormat(string function, string args)
        {
            //            const string Template = @"if '{0}' in dir():
            //  {0}({1})";

            const string Template = @"{0}({1})";

            var stringSource = string.Format(Template, function, args);

            var src = _engine.CreateScriptSourceFromString(stringSource, SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, null);
        }

        public object ConvertArgs(object arg)
        {
            if (arg is Player)
            {
                var ptype = ActionsScope.GetVariable("Player");
                var na = _engine.Operations.CreateInstance(ptype, (arg as Player).Id);
                return na;
            }
            else if (arg is Table)
            {
                var ptype = ActionsScope.GetVariable("table");
                return ptype;
            }
            else if (arg is Group)
            {
                var cur = arg as Group;
                var type = ActionsScope.GetVariable("Pile");
                var na = _engine.Operations.CreateInstance(type, cur.Id, cur.Name, ConvertArgs(cur.Owner));
                return na;
            }
            else if (arg is Card)
            {
                var cur = arg as Card;
                var type = ActionsScope.GetVariable("Card");
                var na = _engine.Operations.CreateInstance(type, cur.Id);
                return na;
            }
            else if (arg is Counter)
            {
                var cur = arg as Counter;
                var type = ActionsScope.GetVariable("Counter");
                var player = Player.All.FirstOrDefault(x => x.Counters.Any(y => y.Id == cur.Id));
                var na = _engine.Operations.CreateInstance(type, cur.Id, cur.Name, ConvertArgs(player));
                return na;
            }
            return arg;
        }

        public string FormatObject(object o)
        {
            if (o == null)
            {
                return string.Format("None");
            }
            if (o is Array)
            {
                var o2 = o as Array;
                return string.Format("[{0}]", string.Join(",", o2.Select(this.FormatObject)));
            }
            if (o is Player)
            {
                return string.Format("Player({0})", (o as Player).Id);
            }
            if (o is Group)
            {
                var h = o as Group;
                return PythonConverter.GroupCtor(h);
                //return string.Format("Group({0},\"{1}\",{2})", h.Id, h.Name,h.Owner == null ? "None" : FormatObject(h.Owner));
            }
            if (o is Card)
            {
                var h = o as Card;
                return string.Format("Card({0})", h.Id);
            }
            if (o is Counter)
            {
                var h = o as Counter;
                var player = Player.All.FirstOrDefault(x => x.Counters.Any(y => y.Id == h.Id));
                return string.Format("Counter({0},{1},{2})", h.Id, FormatObject(h.Name), FormatObject(player));
            }
            if (o is string)
            {
                return string.Format("\"{0}\"", o);
            }
            return o.ToString();
        }

        public void ExecuteOnGroup(string function, Group group, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            string pythonGroup = PythonConverter.GroupCtor(group);
            ScriptSource src = _engine.CreateScriptSourceFromString(string.Format("{0}({1})", function, pythonGroup),
                                                                    SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        public void ExecuteOnGroup(string function, Group group, Point position, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            string pythonGroup = PythonConverter.GroupCtor(group);
            ScriptSource src = _engine.CreateScriptSourceFromString(
                string.Format(CultureInfo.InvariantCulture,
                              "result = {0}({1}, {2:F3}, {3:F3})",
                              function, pythonGroup, position.X, position.Y),
                SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        public void ExecuteOnCards(string function, IEnumerable<Card> cards, Point? position = null, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            string posArguments = position == null
                                      ? ""
                                      : string.Format(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}",
                                                      position.Value.X, position.Value.Y);
            var sb = new StringBuilder();
            foreach (Card card in cards)
                sb.AppendFormat(CultureInfo.InvariantCulture,
                                "result = {0}(Card({1}){2})\n",
                                function, card.Id, posArguments);
            ScriptSource src = _engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        public void ExecuteOnBatch(string function, IEnumerable<Card> cards, Point? position = null, Action<ExecutionResult> continuation = null)
        {
            if (_engine == null) return; //TODO: This blocks spectators from using hotkeys. Need to implement a better solution.
            var sb = new StringBuilder();
            sb.Append("result = ").Append(function).Append("([");
            foreach (Card c in cards)
                sb.Append("Card(").Append(c.Id.ToString(CultureInfo.InvariantCulture)).Append("),");
            sb.Append("]");
            if (position != null)
                sb.AppendFormat(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}", position.Value.X, position.Value.Y);
            sb.Append(")\n");
            ScriptSource src = _engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, continuation);
        }

        private void StartExecution(ScriptSource src, ScriptScope scope, Action<ExecutionResult> continuation)
        {
            var job = new ScriptJob(src, scope, continuation);
            StartExecution(job);
        }

        private void StartExecution(ScriptJobBase job)
        {
            if (Prefs.EnableGameScripts == false) return;
            _executionQueue.Enqueue(job);
            if (_executionQueue.Count == 1) // Other scripts may be hung. Scripts are executed in order.
                ProcessExecutionQueue();
        }

        private void ProcessExecutionQueue()
        {
            do
            {
                ScriptJobBase job = _executionQueue.Peek();
                var scriptjob = job as ScriptJob;
                if (scriptjob != null)
                    Program.GameMess.GameDebug(scriptjob.Source.GetCode());
                // Because some scripts have to be suspended during asynchronous operations (e.g. shuffle, reveal or random),
                // their evaluation is done on another thread.
                // The process still looks synchronous (no concurrency is allowed when manipulating the game model),
                // which is why a ManualResetEvent is used to synchronise the work of both threads
                if (job.Suspended)
                {
                    job.Suspended = false;
                    job.WorkerSignal.Set();
                }
                else
                {
                    job.DispatcherSignal = new AutoResetEvent(false);
                    job.WorkerSignal = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(Execute, job);
                }

                job.DispatcherSignal.WaitOne();
                while (job.InvokedOperation != null)
                {
                    using (new Mute(job.Muted))
                        job.InvokeResult = job.InvokedOperation.DynamicInvoke();
                    job.InvokedOperation = null;
                    job.WorkerSignal.Set();
                    job.DispatcherSignal.WaitOne();
                }
                if (job.Result != null && !String.IsNullOrWhiteSpace(job.Result.Error))
                {
                    Program.GameMess.Warning("{0}", job.Result.Error.Trim());
                }
                if (job.Suspended) return;
                job.DispatcherSignal.Dispose();
                job.WorkerSignal.Dispose();
                _executionQueue.Dequeue();

                if (job.Continuation != null)
                    job.Continuation(job.Result);
            } while (_executionQueue.Count > 0);
        }

        private void Execute(Object state)
        {
            var job = (ScriptJobBase)state;
            var result = new ExecutionResult();
            try
            {
                //if (job is ScriptJob)
                //{
                var sj = job as ScriptJob;
                var scriptResult = sj.Source.Execute(sj.Scope);
                var hasResult = sj.Scope.TryGetVariable("result", out result.ReturnValue);
                result.Output = Encoding.UTF8.GetString(_outputStream.ToArray(), 0, (int)_outputStream.Length);
                // It looks like Python adds some \r in front of \n, which sometimes
                // (depending on the string source) results in doubled \r\r
                result.Output = result.Output.Replace("\r\r", "\r");
                _outputStream.SetLength(0);
                //}
                //else if (job is InvokedScriptJob)
                //{
                //    var ij = job as InvokedScriptJob;
                //    ij.ExecuteAction();
                //}
            }
            catch (Exception ex)
            {
                var eo = _engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(ex);
                var source = (job is ScriptJob) ? (job as ScriptJob).Source.GetCode() : "";
                result.Error = error + Environment.NewLine + source;
                //result.Error = String.Format("{0}\n{1}",ex.Message,ex.StackTrace);
                //Program.TraceWarning("----Python Error----\n{0}\n----End Error----\n", result.Error);
            }
            job.Result = result;
            job.DispatcherSignal.Set();
        }

        internal void Suspend()
        {
            var job = CurrentJob;
            job.Suspended = true;
            job.DispatcherSignal.Set();
            job.WorkerSignal.WaitOne();
        }

        internal void Resume()
        {
            ProcessExecutionQueue();
        }

        internal void Invoke(Action action)
        {
            ScriptJobBase job = CurrentJob;
            job.InvokedOperation = action;
            //job.invokedOperation = () =>
            //                           {
            //                               action();
            //                               return null;
            //                           };
            job.DispatcherSignal.Set();
            job.WorkerSignal.WaitOne();
        }

        internal T Invoke<T>(Func<T> func)
        {
            ScriptJobBase job = _executionQueue.Peek();
            job.InvokedOperation = func;
            job.DispatcherSignal.Set();
            job.WorkerSignal.WaitOne();
            return (T)job.InvokeResult;
        }

        private void InjectOctgnIntoScope(ScriptScope scope, string workingDirectory)
        {
            scope.SetVariable("_api", Program.GameEngine.ScriptApi);
            scope.SetVariable("_wd", workingDirectory);

            // For convenience reason, the definition of Python API objects is in a seperate file: PythonAPI.py
            _engine.Execute(Properties.Resources.CaseInsensitiveDict, scope);

//            _engine.Execute(
//                @"
//import clr
//clr.AddReference(""mscorlib"")
//",
//                scope
            //);

            //_engine.Runtime.LoadAssembly(typeof(Directory).Assembly);

            var file = Versioned.GetFile("PythonApi", Program.GameEngine.Definition.ScriptVersion);
            using (var str = Application.GetResourceStream(new Uri(file.Path)).Stream)
            using (var sr = new StreamReader(str))
            {
                var script = sr.ReadToEnd();
                _engine.Execute(script, scope);
            }

            // See comment on sponsor declaration
            // Note: this has to be done after api has been activated at least once remotely,
            // that's why the code is here rather than in the c'tor
            //if (_sponsor != null) return;
            //_sponsor = new Sponsor();
            //var life = (ILease)RemotingServices.GetLifetimeService(_api);
            //life.Register(_sponsor);
            //life = (ILease)RemotingServices.GetLifetimeService(_outputWriter);
            //life.Register(_sponsor);
        }

        private string ConvertWinforms(string str)
        {
            var ret = new StringBuilder();
            using (var sr = new StringReader(str))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Contains("ShowDialog()") == false || line.Contains("#") == true)
                    {
                        ret.AppendLine(line);
                        line = sr.ReadLine();
                        continue;
                    }
                    var isb = new StringBuilder();
                    string name = null, padding = null;
                    foreach (char t in line)
                    {
                        if (padding == null)
                        {
                            if (char.IsWhiteSpace(t))
                            {
                                isb.Append(t);
                            }
                            else
                            {
                                padding = isb.ToString();
                                isb.Clear();
                                isb.Append(t);
                            }
                            continue;
                        }
                        if (t != '.')
                        {
                            isb.Append(t);
                        }
                        else
                        {
                            name = isb.ToString();
                            isb.Clear();
                            break;
                        }
                    }
                    var newLine = string.Format("{0}showWinForm({1})", padding, name);
                    ret.AppendLine(newLine);
                    line = sr.ReadLine();
                }
            }
            return ret.ToString();
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            //if (_sponsor == null) return;
            // See comment on sponsor declaration
            //var life = (ILease)RemotingServices.GetLifetimeService(_api);
            //life.Unregister(_sponsor);
            //life = (ILease)RemotingServices.GetLifetimeService(_outputWriter);
            //life.Unregister(_sponsor);
        }

        #endregion
    }
}