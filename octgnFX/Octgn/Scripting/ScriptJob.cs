using System;
using System.Threading;
using Microsoft.Scripting.Hosting;

namespace Octgn.Scripting
{
    internal class ScriptJob : ScriptJobBase
    {
        public ScriptScope Scope;
        public ScriptSource Source;
        // Helper fields used to invoke an operation on the Dispatcher thread from the Scripting thread

        public ScriptJob(ScriptSource source, ScriptScope scope, Action<ExecutionResult> continuation)
        {
            Source = source;
            Scope = scope;
            Continuation = continuation;
        }
    }

    internal class InvokedScriptJob : ScriptJobBase
    {
        public Action ExecuteAction { get; set; }

        public InvokedScriptJob(Action a)
        {
            ExecuteAction = a;
        }
    }

    internal abstract class ScriptJobBase
    {
        public Guid id
        {
            get
            {
                if (_uniqueId == Guid.Empty) _uniqueId = Guid.NewGuid();
                return _uniqueId;
            }
        }
        // Indicates whether Octgn logs actions or is muted
        public Guid? Muted;
        // The continuation to call when execution completes (on Dispatcher thread)
        public Action<ExecutionResult> Continuation;
        // The execution result
        public ExecutionResult Result;
        // The signals used to synchronise the Dispatcher thread and the Scripting thread
        public AutoResetEvent DispatcherSignal;
        // It's tempting to use only one but doesn't work reliably
        public AutoResetEvent WorkerSignal;
        // Indicates whether the script is suspend (waiting on an async event, such as random value, reveal or shuffle)
        public bool Suspended;
        public object InvokeResult;
        public Delegate InvokedOperation;

        // The unique id of this job
        private Guid _uniqueId;
    }
}