using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public interface ILauncher
    {
        Task<bool> Launch(ILoadingView view);
    }
}