namespace Octgn.Launchers
{
    using log4net;
    using System.Threading.Tasks;

    public interface ILauncher
    {
		ILog Log { get; }
		bool Shutdown { get; }
        Task Launch();
    }
}