namespace Octgn.ReleasePusher.Tasking
{
    using System.Collections.Generic;
    using System.IO.Abstractions;

    using log4net;

    public class TaskContext: ITaskContext
    {
        public ILog Log { get; private set; }
        public IFileSystem FileSystem { get; private set; }

        public IDictionary<string, object> Data { get; private set; }

        public TaskContext(ILog log)
            :this(log,new FileSystem(),new Dictionary<string, object>())
        {
        }

        public TaskContext(ILog log, IFileSystem fileSystem)
            :this(log,fileSystem,new Dictionary<string, object>())
        {
        }

        public TaskContext(ILog log, IDictionary<string, object> data)
            :this(log,new FileSystem(),data)
        {
        }

        public TaskContext(ILog log, IFileSystem fileSystem, IDictionary<string, object> data)
        {
            Log = log;
            FileSystem = fileSystem;
            Data = data;
        }
    }
}
