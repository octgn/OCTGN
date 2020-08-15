using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public interface ILauncher
    {
        string Name { get; }
        Task<bool> Launch(ILoadingView view);
    }
}