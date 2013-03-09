using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Octgn.Definitions;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Properties;

namespace Octgn.Scripting
{
    [Export]
    public class Engine : IDisposable
    {
        public readonly ScriptScope ActionsScope;
        private readonly ScriptApi _api;
        private readonly ScriptEngine _engine;
        private readonly Queue<ScriptJob> _executionQueue = new Queue<ScriptJob>(4);
        private readonly MemoryStream _outputStream = new MemoryStream();
        private readonly StreamWriter _outputWriter;
        // This is a hack. The sponsor object is used to keep the remote side of the Dialog API alive.
        // I would like to make this cleaner but it really seems to be an impass at the moment.
        // Combining Scripting + Remoting + Lifetime management + Garbage Collection + Partial trust
        // is an aweful and ugly mess.
        private Sponsor _sponsor;

        public Engine()
            : this(false)
        {
        }

        public Engine(bool forTesting)
        {
            AppDomain sandbox = CreateSandbox(forTesting);
            _engine = Python.CreateEngine(sandbox);
            _outputWriter = new StreamWriter(_outputStream);
            _engine.Runtime.IO.SetOutput(_outputStream, _outputWriter);
            _engine.SetSearchPaths(new[] {Path.Combine(sandbox.BaseDirectory, @"Scripting\Lib")});

            _api = new ScriptApi(this);

            var workingDirectory = Directory.GetCurrentDirectory();
            if (Program.Game != null)
            {
                workingDirectory = Path.Combine(Prefs.DataDirectory, "Games", Program.Game.Definition.Id.ToString());
                var search = _engine.GetSearchPaths();
                search.Add(workingDirectory);
                _engine.SetSearchPaths(search);
            }
            ActionsScope = CreateScope(workingDirectory);
            if (Program.Game == null || forTesting) return;
            foreach (
                ScriptSource src in
                    Program.Game.Definition.Scripts.Select(
                        s => _engine.CreateScriptSourceFromString(s.Python, SourceCodeKind.Statements)))
            {
                src.Execute(ActionsScope);
            }
        }

        internal ScriptJob CurrentJob
        {
            get { return _executionQueue.Peek(); }
        }

        public String[] TestScripts(Game game)
        {
            var errors = new List<string>();
            foreach (ScriptDef s in game.Definition.Scripts)
            {
                try
                {
                    ScriptSource src = _engine.CreateScriptSourceFromString(s.Python, SourceCodeKind.Statements);
                    src.Execute(ActionsScope);
                }
                catch (Exception e)
                {
                    var eo = _engine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(e);
                    errors.Add(String.Format("[{2}:{0}]: Python Error:\n{1}", game.Definition.Name, error, s.FileName));
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

        public void ExecuteOnGroup(string function, Group group)
        {
            string pythonGroup = ScriptApi.GroupCtor(group);
            ScriptSource src = _engine.CreateScriptSourceFromString(string.Format("{0}({1})", function, pythonGroup),
                                                                    SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, null);
        }

        public void ExecuteOnGroup(string function, Group group, Point position)
        {
            string pythonGroup = ScriptApi.GroupCtor(group);
            ScriptSource src = _engine.CreateScriptSourceFromString(
                string.Format(CultureInfo.InvariantCulture,
                              "{0}({1}, {2:F3}, {3:F3})",
                              function, pythonGroup, position.X, position.Y),
                SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, null);
        }

        public void ExecuteOnCards(string function, IEnumerable<Card> cards, Point? position = null)
        {
            string posArguments = position == null
                                      ? ""
                                      : string.Format(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}",
                                                      position.Value.X, position.Value.Y);
            var sb = new StringBuilder();
            foreach (Card card in cards)
                sb.AppendFormat(CultureInfo.InvariantCulture,
                                "{0}(Card({1}){2})\n",
                                function, card.Id, posArguments);
            ScriptSource src = _engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, null);
        }

        public void ExecuteOnBatch(string function, IEnumerable<Card> cards, Point? position = null)
        {
            var sb = new StringBuilder();
            sb.Append(function).Append("([");
            foreach (Card c in cards)
                sb.Append("Card(").Append(c.Id.ToString(CultureInfo.InvariantCulture)).Append("),");
            sb.Append("]");
            if (position != null)
                sb.AppendFormat(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}", position.Value.X, position.Value.Y);
            sb.Append(")\n");
            ScriptSource src = _engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
            StartExecution(src, ActionsScope, null);
        }

        private void StartExecution(ScriptSource src, ScriptScope scope, Action<ExecutionResult> continuation)
        {
            var job = new ScriptJob {source = src, scope = scope, continuation = continuation};
            _executionQueue.Enqueue(job);
            if (_executionQueue.Count == 1) // Other scripts may be hung. Scripts are executed in order.
                ProcessExecutionQueue();
        }

        private void ProcessExecutionQueue()
        {
            do
            {
                ScriptJob job = _executionQueue.Peek();
                // Because some scripts have to be suspended during asynchronous operations (e.g. shuffle, reveal or random),
                // their evaluation is done on another thread.
                // The process still looks synchronous (no concurrency is allowed when manipulating the game model),
                // which is why a ManualResetEvent is used to synchronise the work of both threads
                if (job.suspended)
                {
                    job.suspended = false;
                    job.signal2.Set();
                }
                else
                {
                    job.signal = new AutoResetEvent(false);
                    job.signal2 = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(Execute, job);
                }

                job.signal.WaitOne();
                while (job.invokedOperation != null)
                {
                    using (new Mute(job.muted))
                        job.invokeResult = job.invokedOperation();
                    job.invokedOperation = null;
                    job.signal2.Set();
                    job.signal.WaitOne();
                }
                if (job.result != null && !String.IsNullOrWhiteSpace(job.result.Error))
                {
                    Program.TraceWarning("----Python Error----\n{0}\n----End Error----\n", job.result.Error);
                }
                if (job.suspended) return;
                job.signal.Dispose();
                job.signal2.Dispose();
                _executionQueue.Dequeue();

                if (job.continuation != null)
                    job.continuation(job.result);
            } while (_executionQueue.Count > 0);
        }

        private void Execute(Object state)
        {
            var job = (ScriptJob) state;
            var result = new ExecutionResult();
            try
            {
                job.source.Execute(job.scope);
                result.Output = Encoding.UTF8.GetString(_outputStream.ToArray(), 0, (int) _outputStream.Length);
                // It looks like Python adds some \r in front of \n, which sometimes 
                // (depending on the string source) results in doubled \r\r
                result.Output = result.Output.Replace("\r\r", "\r");
                _outputStream.SetLength(0);
            }
            catch (Exception ex)
            {
                var eo = _engine.GetService<ExceptionOperations>();
                string error = eo.FormatException(ex);
                result.Error = error + Environment.NewLine + job.source.GetCode();
                //result.Error = String.Format("{0}\n{1}",ex.Message,ex.StackTrace);
                //Program.TraceWarning("----Python Error----\n{0}\n----End Error----\n", result.Error);
            }
            job.result = result;
            job.signal.Set();
        }

        internal void Suspend()
        {
            ScriptJob job = CurrentJob;
            job.suspended = true;
            job.signal.Set();
            job.signal2.WaitOne();
        }

        internal void Resume()
        {
            ProcessExecutionQueue();
        }

        internal void Invoke(Action action)
        {
            ScriptJob job = CurrentJob;
            job.invokedOperation = () =>
                                       {
                                           action();
                                           return null;
                                       };
            job.signal.Set();
            job.signal2.WaitOne();
        }

        internal T Invoke<T>(Func<object> func)
        {
            ScriptJob job = _executionQueue.Peek();
            job.invokedOperation = func;
            job.signal.Set();
            job.signal2.WaitOne();
            return (T) job.invokeResult;
        }

        private void InjectOctgnIntoScope(ScriptScope scope, string workingDirectory)
        {
            scope.SetVariable("_api", _api);
            scope.SetVariable("_wd", workingDirectory);
            
            // For convenience reason, the definition of Python API objects is in a seperate file: PythonAPI.py
            _engine.Execute(Resources.CaseInsensitiveDict, scope);
            _engine.Execute(Resources.PythonAPI, scope);

            // See comment on sponsor declaration
            // Note: this has to be done after api has been activated at least once remotely,
            // that's why the code is here rather than in the c'tor
            if (_sponsor != null) return;
            _sponsor = new Sponsor();
            var life = (ILease) RemotingServices.GetLifetimeService(_api);
            life.Register(_sponsor);
            life = (ILease) RemotingServices.GetLifetimeService(_outputWriter);
            life.Register(_sponsor);
        }

        private static AppDomain CreateSandbox(bool forTesting)
        {
            var permissions = new PermissionSet(PermissionState.None);
            //if (forTesting)
                permissions = new PermissionSet(PermissionState.Unrestricted);
            
            //permissions.AddPermission(new Permission)

            permissions.AddPermission(
                new SecurityPermission(SecurityPermissionFlag.AllFlags));
            permissions.AddPermission(new UIPermission(UIPermissionWindow.AllWindows));
            permissions.AddPermission(
                new TypeDescriptorPermission(TypeDescriptorPermissionFlags.RestrictedRegistrationAccess));
            permissions.AddPermission(
                new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery,
                                     AppDomain.CurrentDomain.BaseDirectory));
            permissions.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
            var appinfo = new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.BaseDirectory};
            return AppDomain.CreateDomain("Scripting sandbox", null, appinfo, permissions);
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            if (_sponsor == null) return;
            // See comment on sponsor declaration
            var life = (ILease) RemotingServices.GetLifetimeService(_api);
            life.Unregister(_sponsor);
            life = (ILease) RemotingServices.GetLifetimeService(_outputWriter);
            life.Unregister(_sponsor);
        }

        #endregion

        #region Nested type: Sponsor

        private class Sponsor : ISponsor
        {
            #region ISponsor Members

            public TimeSpan Renewal(ILease lease)
            {
                return TimeSpan.FromMinutes(10);
            }

            #endregion
        }

        #endregion
    }
}