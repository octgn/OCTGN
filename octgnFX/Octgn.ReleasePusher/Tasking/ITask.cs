namespace Octgn.ReleasePusher.Tasking
{
    public interface ITask
    {
        void Run(object sender, ITaskContext context);
    }
}
