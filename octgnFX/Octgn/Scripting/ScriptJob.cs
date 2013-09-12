using System;
using System.Threading;
using Microsoft.Scripting.Hosting;
using Octgn.Play;

namespace Octgn.Scripting
{
    internal class ScriptJob
    {
        // The unique id of this job
        // The continuation to call when execution completes (on Dispatcher thread)
        private int _uniqueId;
        public Action<ExecutionResult> continuation;
        public object invokeResult;
        public Func<object> invokedOperation;
        // Indicates whether Octgn logs actions or is muted
        public int muted;
        // Indicates whether the script is suspend (waiting on an async event, such as random value, reveal or shuffle)
        // The execution result
        public ExecutionResult result;
        public ScriptScope scope;
        // The signals used to synchronise the Dispatcher thread and the Scripting thread    
        public AutoResetEvent dispatcherSignal;
        // It's tempting to use only one but doesn't work reliably
        public AutoResetEvent workerSignal;
        public ScriptSource source;
        public bool suspended;

        public int id
        {
            get
            {
                if (_uniqueId == 0) _uniqueId = (Player.LocalPlayer.Id) << 16 | Program.GameEngine.GetUniqueId();
                return _uniqueId;
            }
        }

        // Helper fields used to invoke an operation on the Dispatcher thread from the Scripting thread
    }
}