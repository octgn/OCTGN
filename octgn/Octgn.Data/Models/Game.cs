using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Data.Models
{
    public class Game : IDisposable
    {

        public Guid Guid { get; set; }
        public string Name { get; set; }

        public IEnumerable<GameObject> GetallObjects()
        {
			using (var client = Database.GetClient())
			{
				IEnumerable<GameObject> ret = client.Query<GameObject>(gameObject => gameObject.GameGuid == Guid);
				return (ret);
			}
        }

        public IEnumerable<GameObject> GetAllObjectsByType(string type)
        {
			using (var client = Database.GetClient())
			{
				IEnumerable<GameObject> ret = client.Query<GameObject>(
				                                                                gameObject =>
				                                                                gameObject.GameGuid == Guid
				                                                                && gameObject.Type == type);
				return (ret);
			}
        }

        public GameObject GetObjectByGuid(Guid guid)
        {
			using (var client = Database.GetClient())
			{
				GameObject ret = client.Query<GameObject>(
				                                                   gameObject =>
				                                                   gameObject.GameGuid == Guid && gameObject.Guid == guid).
					FirstOrDefault<GameObject>();
				return (ret);
			}
        }

        public void Dispose()
        {
            
        }
    }
}
