using Octgn.DataNew.Entities;
using Octgn.ProxyGenerator;

namespace Octgn.DataNew
{
    public interface IDbContextCaching
    {
        void Invalidate(Game game);
        void Invalidate(GameScript script);
        void Invalidate(ProxyDefinition proxy);
        void Invalidate(Set set);
    }
}