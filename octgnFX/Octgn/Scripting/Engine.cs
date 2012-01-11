using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Windows;
using System.Globalization;

namespace Octgn.Scripting
{
  [Export]
  public class Engine : IDisposable
  {
    private ScriptEngine engine;
    private MemoryStream outputStream = new MemoryStream();
    private StreamWriter outputWriter;
    private Queue<ScriptJob> executionQueue = new Queue<ScriptJob>(4);
    private ScriptApi api;
    public readonly ScriptScope ActionsScope;
    // This is a hack. The sponsor object is used to keep the remote side of the OCTGN API alive.
    // I would like to make this cleaner but it really seems to be an impass at the moment.
    // Combining Scripting + Remoting + Lifetime management + Garbage Collection + Partial trust
    // is an aweful and ugly mess.
    private Sponsor sponsor;

    public Engine()
    {      
      AppDomain sandbox = CreateSandbox();
      engine = Python.CreateEngine(sandbox);
      outputWriter = new StreamWriter(outputStream);
      engine.Runtime.IO.SetOutput(outputStream, outputWriter);
      engine.SetSearchPaths(new string[] { Path.Combine(sandbox.BaseDirectory, @"Scripting\Lib") });

      api = new ScriptApi(this);

      ActionsScope = CreateScope();
      // TODO: what if a new game is played (other definition, or maybe even simply a reset?)
      foreach (var s in Program.Game.Definition.Scripts)
      {
        var src = engine.CreateScriptSourceFromString(s.Python, SourceCodeKind.Statements);
        src.Execute(ActionsScope);
      }
    }

    public ScriptScope CreateScope(bool injectApi = true)
    {
      var scope = engine.CreateScope();
      if (injectApi)
        InjectOctgnIntoScope(scope);
      return scope;
    }

    public bool TryExecuteInteractiveCode(string code, ScriptScope scope, Action<ExecutionResult> continuation)
    {
      var src = engine.CreateScriptSourceFromString(code, SourceCodeKind.InteractiveCode);
      switch (src.GetCodeProperties())
      {
        case ScriptCodeParseResult.IncompleteToken: return false;
        case ScriptCodeParseResult.IncompleteStatement:
          // An empty line ends the statement
          if (!code.TrimEnd(' ', '\t').EndsWith("\n"))
            return false;
          break;
      }
      StartExecution(src, scope, continuation);
      return true;
    }

    public void ExecuteOnGroup(string function, Play.Group group)
    {
      string pythonGroup = ScriptApi.GroupCtor(group);
      var src = engine.CreateScriptSourceFromString(string.Format("{0}({1})", function, pythonGroup), SourceCodeKind.Statements);
      StartExecution(src, ActionsScope, null);
    }

    public void ExecuteOnGroup(string function, Play.Group group, Point position)
    {
      string pythonGroup = ScriptApi.GroupCtor(group);
      var src = engine.CreateScriptSourceFromString(
        string.Format(System.Globalization.CultureInfo.InvariantCulture,
          "{0}({1}, {2:F3}, {3:F3})", 
          function, pythonGroup, position.X, position.Y), 
        SourceCodeKind.Statements);
      StartExecution(src, ActionsScope, null);
    }

    public void ExecuteOnCards(string function, IEnumerable<Play.Card> cards, Point? position = null)
    {
      string posArguments = position == null ? "" :
        string.Format(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}", position.Value.X, position.Value.Y);      
      var sb = new StringBuilder();
      foreach (var card in cards)
        sb.AppendFormat(CultureInfo.InvariantCulture,
                        "{0}(Card({1}){2})\n", 
                        function, card.Id, posArguments);
      var src = engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
      StartExecution(src, ActionsScope, null);
    }

    public void ExecuteOnBatch(string function, IEnumerable<Play.Card> cards, Point? position = null)
    {
      var sb = new StringBuilder();
      sb.Append(function).Append("([");
      foreach (var c in cards) 
        sb.Append("Card(").Append(c.Id.ToString(CultureInfo.InvariantCulture)).Append("),");
      sb.Append("]");
      if (position != null)
        sb.AppendFormat(CultureInfo.InvariantCulture, ", {0:F3}, {1:F3}", position.Value.X, position.Value.Y);
      sb.Append(")\n");

      var src = engine.CreateScriptSourceFromString(sb.ToString(), SourceCodeKind.Statements);
      StartExecution(src, ActionsScope, null);
    }

    private void StartExecution(ScriptSource src, ScriptScope scope, Action<ExecutionResult> continuation)
    {
      var job = new ScriptJob { source = src, scope = scope, continuation = continuation };
      executionQueue.Enqueue(job);
      if (executionQueue.Count == 1) // Other scripts may be hung. Scripts are executed in order.
        ProcessExecutionQueue();            
    }

    private void ProcessExecutionQueue()
    {
      do
      {
        var job = executionQueue.Peek();
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
          using (new Networking.Mute(job.muted))
            job.invokeResult = job.invokedOperation();
          job.invokedOperation = null;
          job.signal2.Set();
          job.signal.WaitOne();
        }

        if (job.suspended) return;
        job.signal.Dispose();
        job.signal2.Dispose();
        executionQueue.Dequeue();

        if (job.continuation != null)
          job.continuation(job.result);

      } while (executionQueue.Count > 0);
    }

    private void Execute(Object state)
    {
      var job = (ScriptJob)state;
      var result = new ExecutionResult();
      try
      {
        job.source.Execute(job.scope);        
        result.Output = Encoding.UTF8.GetString(outputStream.ToArray(), 0, (int)outputStream.Length);
        // HACK: It looks like Python adds some \r in front of \n, which sometimes 
        // (depending on the string source) results in doubled \r\r
        result.Output = result.Output.Replace("\r\r", "\r");
        outputStream.SetLength(0);
      }
      catch (Exception ex)
      {
        result.Error = ex.Message;
      }
      job.result = result;
      job.signal.Set();
    }

    internal void Suspend()
    {
      var job = CurrentJob;
      job.suspended = true;
      job.signal.Set();
      job.signal2.WaitOne();
    }

    internal void Resume()
    { ProcessExecutionQueue(); }

    internal void Invoke(Action action)
    {
      var job = CurrentJob;
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
      var job = executionQueue.Peek();
      job.invokedOperation = func;
      job.signal.Set();
      job.signal2.WaitOne();
      return (T)job.invokeResult;
    }

    internal ScriptJob CurrentJob
    {
      get { return executionQueue.Peek(); }
    }
    
    private void InjectOctgnIntoScope(ScriptScope scope)
    {
      scope.SetVariable("_api", api);
      // For convenience reason, the definition of Python API objects is in a seperate file: PythonAPI.py
      engine.Execute(Properties.Resources.CaseInsensitiveDict, scope);
      engine.Execute(Properties.Resources.PythonAPI, scope);

      // See comment on sponsor declaration
      // Note: this has to be done after api has been activated at least once remotely,
      // that's why the code is here rather than in the c'tor
      if (sponsor == null)
      {
        sponsor = new Sponsor();        
        var life = (ILease)RemotingServices.GetLifetimeService(api);
        life.Register(sponsor);
        life = (ILease)RemotingServices.GetLifetimeService(outputWriter);
        life.Register(sponsor);
      }
    }

    private static AppDomain CreateSandbox()
    {
      var permissions = new PermissionSet(PermissionState.None);
      permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
      permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, AppDomain.CurrentDomain.BaseDirectory));
      
      var appinfo = new AppDomainSetup();
      appinfo.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

      return AppDomain.CreateDomain("Scripting sandbox", null, appinfo, permissions);
    }

    #region IDisposable

    void IDisposable.Dispose()
    {
      if (sponsor != null)
      {
        // See comment on sponsor declaration
        var life = (ILease)RemotingServices.GetLifetimeService(api);
        life.Unregister(sponsor);
        life = (ILease)RemotingServices.GetLifetimeService(outputWriter);
        life.Unregister(sponsor);
      }
    }

    #endregion

    private class Sponsor : ISponsor
    {
      public TimeSpan Renewal(ILease lease)
      {
        return TimeSpan.FromMinutes(10);
      }
    }
  }
}
