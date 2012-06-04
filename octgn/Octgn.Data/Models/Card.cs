using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
    public class Card : IDisposable
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public Guid SetGuid { get; set; }
        public Guid GameGuid { get; set; }

        //todo the database stuff. something with only one connection in the docs?
        public Database DB { get; set; }

        public Set Set
        {
            get
            {
                IList<Set> setList = DB.DBConnection.Query<Set>(delegate(Set set)
                {
                    return set.Guid == SetGuid && set.GameGuid == GameGuid;
                });
                return setList[0];
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
