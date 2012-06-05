using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
    public class Game : IDisposable
    {

        public Guid Guid { get; set; }
        public string Name { get; set; }

        //todo the database stuff. something with only one connection in the docs?
        public Database DB { get; set; }

        public IEnumerable<GameObject> GetallObjects()
        {
            IEnumerable<GameObject> ret = DB.DBConnection.Query<GameObject>(delegate(GameObject gameObject)
            {
                return gameObject.GameGuid == Guid;
            });
            return (ret);
        }

        public IEnumerable<GameObject> GetAllObjectsByType(string type)
        {
            IEnumerable<GameObject> ret = DB.DBConnection.Query<GameObject>(delegate(GameObject gameObject)
            {
                return gameObject.GameGuid == Guid && gameObject.Type == type;
            });
            return (ret);
        }

        public GameObject GetObjectByGuid(Guid guid)
        {
            GameObject ret = DB.DBConnection.Query<GameObject>(delegate(GameObject gameObject)
            {
                return gameObject.GameGuid == Guid && gameObject.Guid == guid;
            }).FirstOrDefault<GameObject>();
            return (ret);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
