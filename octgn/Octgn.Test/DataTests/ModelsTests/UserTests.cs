using System.Collections.Generic;
using Db4objects.Db4o;
using FakeItEasy;
using System.Web.Security;
using Octgn.Data;
using Xunit;
using User = Octgn.Data.Models.User;

namespace Octgn.Test.DataTests.ModelsTests
{
	public class UserTests
	{
		[Fact]
		public void CreateUserTest()
		{
			var fList = new List<User>();
			Database.TestMode = true;
			var user = new User()
			{
				Email = "fake@email.com" ,
				Username = "fakeuser" ,
				PasswordHash = "123456"
			};
			Database.TestClient = A.Fake<IObjectContainer>();

			A.CallTo(() => Database.TestClient.Query<User>()).Returns(fList);

			fList.Add(user);

			MembershipCreateStatus ret;

			//Test with non conflicting user
			//ret = user.CreateUser("nuser", "npass", "nemail");
			//Assert.Equal(ret, MembershipCreateStatus.Success);

			//Test with Conflicting username
			//ret = user.CreateUser("fakeuser", "pass", "email");
			//Assert.Equal(ret, MembershipCreateStatus.DuplicateUserName);

			//Test with conflicting email
			//ret = user.CreateUser("nuser", "pass", "fake@email.com");
			//Assert.Equal(ret, MembershipCreateStatus.DuplicateEmail);
		}
		[Fact]
		public void ValidateUserTest()
		{
			var fList = new List<User>();
			Database.TestMode = true;
			var user = new User()
			{
				Email = "fake@email.com" ,
				Username = "fakeuser" ,
				PasswordHash = "123456"
			};

			Database.TestClient = A.Fake<IObjectContainer>();

			A.CallTo(() => Database.TestClient.Query<User>()).Returns(fList);

			fList.Add(user);

			//Validate good username & password
			//Assert.True(user.ValidateUser("fakeuser" , "123456"));

			//Validate good username & bad password
			//Assert.False(user.ValidateUser("fakeuser", "BAD PASS"));

			//Validate bad username & good password
			//Assert.False(user.ValidateUser("Bad Username", "123456"));

		}
	}
}
