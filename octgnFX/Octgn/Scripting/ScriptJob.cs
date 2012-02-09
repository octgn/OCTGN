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
        public Action<ExecutionResult> continuation;
        public object invokeResult;
        public Func<object> invokedOperation;
        // Indicates whether OCTGN logs actions or is muted
        public int muted;
        // Indicates whether the script is suspend (waiting on an async event, such as random value, reveal or shuffle)
        // The execution result
        public ExecutionResult result;
        public ScriptScope scope;
        // The signals used to synchronise the Dispatcher thread and the Scripting thread    
        public AutoResetEvent signal;
        // It's tempting to use only one but doesn't work reliably
        public AutoResetEvent signal2;
        public ScriptSource source;
        public bool suspended;
        private int uniqueId;

        public int id
        {
            get
            {
                if (uniqueId == 0) uniqueId = (Player.LocalPlayer.Id) << 16 | Program.Game.GetUniqueId();
                return uniqueId;
            }
        }

        // Helper fields used to invoke an operation on the Dispatcher thread from the Scripting thread
    }
}