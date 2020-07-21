using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public interface ILoader
    {
        string Name { get; }

        Task Load(ILoadingView view);
    }
}
