using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
    public class Game : IDisposable
    {

        public Guid Guid { get; set; }

        //todo the database stuff. something with only one connection in the docs?
        public Database DB { get; set; }

        public Set GetSet(Guid setGuid)
        {
            Set example = new Set();
            example.GameGuid = Guid;
            example.Guid = setGuid;
            Set ret = (Set)DB.DBConnection.QueryByExample(example)[0];
            return (ret);
        }

        public List<Set> GetAllSets()
        {
            List<Set> ret = (List<Set>)DB.DBConnection.Query<Set>(delegate(Set set)
            {
                return set.GameGuid == Guid;
            });
            return (ret);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
