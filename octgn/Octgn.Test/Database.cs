using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using FakeItEasy;
using System.Web.Security;
using NUnit.Framework;

namespace Octgn.Test
{
	public class Database
	{
		[Test]
		public void CreateUserTest()
		{
			var f = A.Fake<IObjectContainer>();
			var fList = new List<Data.Models.User>
			{
				new Data.Models.User
				{
					Email = "fake@email.com" ,
					Username = "fakeuser" ,
					PasswordHash = "123456"
				}
			};
			A.CallTo(() => f.Query<Data.Models.User>()).Returns(fList);
			A.CallTo(() => f.Store(new object())).WithAnyArguments().DoesNothing();
			using(Data.Database d = new Data.Database(f))
			{
				MembershipCreateStatus ret = MembershipCreateStatus.ProviderError;

				//Test with non conflicting user
				ret = d.CreateUser("nuser" , "npass" , "nemail");
				Assert.AreEqual(ret,MembershipCreateStatus.Success);

				//Test with Conflicting username
				ret = d.CreateUser("fakeuser" , "pass" , "email");
				Assert.AreEqual(ret , MembershipCreateStatus.DuplicateUserName);

				//Test with conflicting email
				ret = d.CreateUser("nuser" , "pass" , "fake@email.com");
				Assert.AreEqual(ret , MembershipCreateStatus.DuplicateEmail);

			}
			
		}
	}
}
