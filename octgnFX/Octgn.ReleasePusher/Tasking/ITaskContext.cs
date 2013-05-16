namespace Octgn.ReleasePusher.Tasking
{
    using System.Collections.Generic;
    using System.IO.Abstractions;

    public interface ITaskContext
    {
        log4net.ILog Log { get; }
        IFileSystem FileSystem { get; }
        IDictionary<string, object> Data { get; }
    }
}
