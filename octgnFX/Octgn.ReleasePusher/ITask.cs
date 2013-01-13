namespace Octgn.ReleasePusher
{
    public interface ITask
    {
        void Run(object sender, ITaskContext context);
    }
}
