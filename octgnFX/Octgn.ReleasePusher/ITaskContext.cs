namespace Octgn.ReleasePusher
{
    using System;
    using System.Collections.Generic;

    public interface ITaskContext
    {
        log4net.ILog Log { get; }
        IDictionary<string, object> Data { get; }
    }
}
