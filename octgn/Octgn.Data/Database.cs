using System;
using System.Diagnostics;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;

namespace Octgn.Data
{
	public static class Database
	{
		public static IObjectServer DbServer { get; set; }
		public static bool TestMode { get; set; }
		public static IObjectContainer TestClient { get; set; }
		static Database()
		{
			TestClient = new ObjectContainerStub();
			Process.GetCurrentProcess().Exited += DatabaseExited;
			DbServer = Db4oFactory.OpenServer(Db4oFactory.Configure() , "master.db" , 0);
		}

		static void DatabaseExited(object sender, EventArgs e)
		{
			try
			{
				DbServer.Close();
			}
			catch(Exception) {}
		}

		public static void Close()
		{
			DbServer.Close();
		}

		public static IObjectContainer GetClient()
		{
			if (!TestMode)
				return DbServer.OpenClient();
			return TestClient;
		}
	}
}
