using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using System.Threading;

namespace Octgn.Scripting
{
  class ScriptJob
  {
    // The unique id of this job
    private int uniqueId = 0;
    public int id
    {
      get
      {
        if (uniqueId == 0) uniqueId = ((int)Play.Player.LocalPlayer.Id) << 16 | Program.Game.GetUniqueId();
        return uniqueId;
      }
    }
    // The script to execute
    public ScriptSource source;
    // The scope to execute it in
    public ScriptScope scope;
    // The continuation to call when execution completes (on Dispatcher thread)
    public Action<ExecutionResult> continuation;
    // Indicates whether OCTGN logs actions or is muted
    public int muted = 0;
    // Indicates whether the script is suspend (waiting on an async event, such as random value, reveal or shuffle)
    public bool suspended = false;
    // The execution result
    public ExecutionResult result;
    // The signals used to synchronise the Dispatcher thread and the Scripting thread    
    public AutoResetEvent signal;
    // It's tempting to use only one but doesn't work reliably
    public AutoResetEvent signal2;
    // Helper fields used to invoke an operation on the Dispatcher thread from the Scripting thread
    public Func<object> invokedOperation;
    public object invokeResult;
  }
}
