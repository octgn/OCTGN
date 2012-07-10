using System;
using System.Diagnostics;
using System.IO;
using System.Web.Security;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Octgn.Common;

namespace Octgn.Data
{
	public static class Database
	{
		public static IObjectServer DbServer { get; set; }
		public static bool TestMode { get; set; }
		public static IObjectContainer TestClient { get; set; }
		static Database()
		{
			try
			{
				Common.Log.L("Initializing Database");
				TestMode = false;
				TestClient = new ObjectContainerStub();
				Process.GetCurrentProcess().Exited += DatabaseExited;
				Common.Log.L ("Creating/Opening Database...");
				DbServer = Db4oFactory.OpenServer(Db4oFactory.Configure() , "master.db" , 0);
				if(Membership.FindUsersByName("admin").Count != 0) return;
				if(!Roles.RoleExists("administrator"))
					Roles.CreateRole("administrator");
				var u = Membership.CreateUser("admin" , ValueConverters.CreateShaHash("password"));
				Roles.AddUserToRole("admin" , "administrator");
				Common.Log.L ("Database Ready.");
			}catch(Exception e )
			{
				//if(Debugger.IsAttached)
				//	Debugger.Break();
				Common.Log.L("Database Init Error: {0}",e);
			}
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
