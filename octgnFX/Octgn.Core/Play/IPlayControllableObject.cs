namespace Octgn.Play
{
    public interface IPlayControllableObject
    {
        int Id { get; }

        string Name { get; }

        string FullName { get; }

        IPlayPlayer Owner { get; }

        IPlayPlayer Controller { get; set; }

        void PassControlTo(IPlayPlayer p);

        void TakeControl();

        void TakingControl(IPlayPlayer p);

        void PassControlTo(IPlayPlayer p, IPlayPlayer who, bool notifyServer, bool requested);

        void KeepControl();

        void ReleaseControl();

        bool CanManipulate();

        bool TryToManipulate();

        void NotControlledError();

        void DontTakeError();

        void OnControllerChanged();

        void CopyControllersTo(IPlayControllableObject other);
    }
}