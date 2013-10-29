namespace Octgn.Launchers
{
    using log4net;

    public interface ILauncher
    {
		ILog Log { get; }
		bool Shutdown { get; }
        void Launch();
    }
}