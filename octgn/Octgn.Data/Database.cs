using System;
using Db4objects.Db4o;

namespace Octgn.Data
{
	public class Database : IDisposable
	{
		public IObjectContainer DbConnection { get; set; }
		public Database()
		{
			DbConnection = Db4oEmbedded.OpenFile(Db4oEmbedded.NewConfiguration(), "master.db");
		}

		public Database(IObjectContainer db)
		{
			DbConnection = db;
		}

		public void Dispose()
		{
			DbConnection.Close();
		}
	}
}
