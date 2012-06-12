using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Data.Models
{
    public class Game : IDisposable
    {

        public Guid Guid { get; set; }
        public string Name { get; set; }

        //todo the database stuff. something with only one connection in the docs?
        public Database Db { get; set; }

        public IEnumerable<GameObject> GetallObjects()
        {
            IEnumerable<GameObject> ret = Db.DbConnection.Query<GameObject>(gameObject => gameObject.GameGuid == Guid);
            return (ret);
        }

        public IEnumerable<GameObject> GetAllObjectsByType(string type)
        {
            IEnumerable<GameObject> ret = Db.DbConnection.Query<GameObject>(
                                                                            gameObject =>
                                                                            gameObject.GameGuid == Guid
                                                                            && gameObject.Type == type);
            return (ret);
        }

        public GameObject GetObjectByGuid(Guid guid)
        {
            GameObject ret = Db.DbConnection.Query<GameObject>(
                                                               gameObject =>
                                                               gameObject.GameGuid == Guid && gameObject.Guid == guid).FirstOrDefault<GameObject>();
            return (ret);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
