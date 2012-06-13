/// Copyright (c) 2008-2011 Brad Williams
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
/// associated documentation files (the "Software"), to deal in the Software without restriction,
/// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
/// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
/// subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial
/// portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
/// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
/// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
/// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Specialized;
using System.Configuration.Provider;
using System.IO;
using System.Reflection;
using System.Web.Security;
using db4oProviders;
using NUnit.Framework;

namespace WCSoft.db4oProviders
{
    [TestFixture]
    public class db4oRoleProviderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void TestSetUp()
        {
            this.config = new NameValueCollection();

            this.config.Add("connectionStringName", "connectionstringname");
            this.config.Add("ApplicationName", "db4otest");

            this.provider = new db4oRoleProvider();
            this.provider.ConnectionStringStore = new MockConnectionStringStore();
            this.provider.Initialize("db4otest", config);

            // unit test project is a library project, so web.config is ignored, so no way to specify a provider
            // instead, custom provider is forced into system using this hack found at http://gigablast.com/get?d=66151861944
            SetReadWrite();
            Roles.Providers.Add(this.provider);
            Roles.Providers.SetReadOnly();
        }

        [TearDown]
        public void TestTearDown()
        {
            SetReadWrite();
            Roles.Providers.Remove(this.provider.Name);
            Roles.Providers.SetReadOnly();
            SafeDB.DropAll();
            File.Delete(MockConnectionStringStore.DBFileName);
            this.provider = null;
        }

        #endregion

        private NameValueCollection config;
        private db4oRoleProvider provider;

        private void SetReadWrite()
        {
            typeof (ProviderCollection)
                .GetField("_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(Roles.Providers, false);
        }

        [Test]
        public void AllRoleMethods()
        {
            string[] roles = provider.GetAllRoles();
            Assert.AreEqual(0, roles.Length, "should be no roles yet");

            provider.CreateRole("role1");
            roles = provider.GetAllRoles();
            Assert.AreEqual(1, roles.Length, "should be 1 role");
            Assert.AreEqual("role1", roles[0], "wrong role name");

            provider.CreateRole("role2");
            roles = provider.GetAllRoles();
            Assert.AreEqual(2, roles.Length, "should be 2 role");

            bool test = provider.RoleExists("role1");
            Assert.AreEqual(true, test, "role1 should exist");

            test = provider.RoleExists("role2");
            Assert.AreEqual(true, test, "role2 should exist");

            test = provider.RoleExists("role3");
            Assert.AreEqual(false, test, "role3 should not exist");

            provider.DeleteRole("role1", false);
            roles = provider.GetAllRoles();
            Assert.AreEqual(1, roles.Length, "should be 1 role after delete");
            Assert.AreEqual("role2", roles[0], "wrong role name after delete");

            provider.DeleteRole("role2", false);
            roles = provider.GetAllRoles();
            Assert.AreEqual(0, roles.Length, "should be no roles after delete");

            test = provider.RoleExists("role1");
            Assert.AreEqual(false, test, "role1 should not exist");

            test = provider.RoleExists("role2");
            Assert.AreEqual(false, test, "role2 should not exist");
        }

        [Test]
        public void AllUserMethods()
        {
            provider.CreateRole("role1");
            provider.CreateRole("role2");
            provider.CreateRole("role3");

            provider.AddUsersToRoles(new string[] {"bert"}, new string[] {"role1", "role2"});
            provider.AddUsersToRoles(new string[] {"ernie"}, new string[] {"role2", "role3"});
            provider.AddUsersToRoles(new string[] {"harold", "maude"}, new string[] {"role1", "role3"});

            provider.RemoveUsersFromRoles(new string[] {"ernie", "maude"}, new string[] {"role1", "role3"});

            string[] results = provider.GetRolesForUser("bert");
            Assert.AreEqual(2, results.Length, "wrong length for bert");
            Assert.AreEqual("role1", results[0], "wrong result 0 for bert");
            Assert.AreEqual("role2", results[1], "wrong result 1 for bert");

            results = provider.GetRolesForUser("ernie");
            Assert.AreEqual(1, results.Length, "wrong length for ernie");
            Assert.AreEqual("role2", results[0], "wrong result 0 for ernie");

            results = provider.GetRolesForUser("harold");
            Assert.AreEqual(2, results.Length, "wrong length for harold");
            Assert.AreEqual("role1", results[0], "wrong result 0 for harold");
            Assert.AreEqual("role3", results[1], "wrong result 1 for harold");

            results = provider.GetRolesForUser("maude");
            Assert.AreEqual(0, results.Length, "wrong length for maude");

            results = provider.GetUsersInRole("role1");
            Assert.AreEqual(2, results.Length, "wrong length for role1");
            Assert.AreEqual("bert", results[0], "wrong result 0 for role1");
            Assert.AreEqual("harold", results[1], "wrong result 1 for role1");

            results = provider.GetUsersInRole("role2");
            Assert.AreEqual(2, results.Length, "wrong length for role2");
            Assert.AreEqual("bert", results[0], "wrong result 0 for role2");
            Assert.AreEqual("ernie", results[1], "wrong result 1 for role2");

            results = provider.GetUsersInRole("role3");
            Assert.AreEqual(1, results.Length, "wrong length for role3");
            Assert.AreEqual("harold", results[0], "wrong result 0 for role3");

            results = provider.FindUsersInRole("role1", "r");
            Assert.AreEqual(2, results.Length, "wrong length for FindUserInRole role1");
            Assert.AreEqual("bert", results[0], "wrong result 0 for FindUserInRole role1");
            Assert.AreEqual("harold", results[1], "wrong result 1 for FindUserInRole role1");

            results = provider.FindUsersInRole("role2", "er");
            Assert.AreEqual(2, results.Length, "wrong length for FindUserInRole role2");
            Assert.AreEqual("bert", results[0], "wrong result 0 for FindUserInRole role2");
            Assert.AreEqual("ernie", results[1], "wrong result 1 for FindUserInRole role2");

            results = provider.FindUsersInRole("role2", "rt");
            Assert.AreEqual(1, results.Length, "wrong length for FindUserInRole role2 for rt");
            Assert.AreEqual("bert", results[0], "wrong result 0 for FindUserInRole role2 for rt");
        }

        [Test]
        public void Initialize()
        {
            Assert.AreEqual("db4otest", provider.ApplicationName, "wrong ApplicationName");
        }
    }
}