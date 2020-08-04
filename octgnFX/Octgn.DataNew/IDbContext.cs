using Octgn.DataNew.Entities;
using Octgn.DataNew.FileDB;
using Octgn.ProxyGenerator;
using System;
using System.Collections.Generic;

namespace Octgn.DataNew
{
    public interface IDbContext
    {
        IEnumerable<Card> Cards { get; }
        IEnumerable<Game> Games { get; }
        IEnumerable<ProxyDefinition> ProxyDefinitions { get; }
        IEnumerable<GameScript> Scripts { get; }
        CollectionQuery<Set> SetQuery { get; }
        IEnumerable<Set> Sets { get; }

        void Dispose();
        Game GameById(Guid gameId);
        void Save(Game game);
        void Save(Set set);
        IEnumerable<Set> SetsById(Guid setId);
    }
}