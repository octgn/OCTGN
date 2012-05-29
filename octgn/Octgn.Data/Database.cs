using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Db4objects.Db4o;
using User = Octgn.Data.Models.User;

namespace Octgn.Data
{
	public class Database : IDisposable
	{
		public IObjectContainer DBConnection { get; set; }
		public Database()
		{
			DBConnection = Db4oEmbedded.OpenFile(Db4oEmbedded.NewConfiguration(), "master.db");
		}

		public Database(IObjectContainer db)
		{
			DBConnection = db;
		}

		public void Dispose()
		{
			DBConnection.Close();
		}
	}
}
