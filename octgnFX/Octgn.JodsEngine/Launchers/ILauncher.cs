using log4net;

namespace Octgn.Launchers
{
    public interface ILauncher
    {
		ILog Log { get; }
		bool Shutdown { get; }
        void Launch();
    }
}