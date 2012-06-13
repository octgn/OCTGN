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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web.Profile;
using Db4objects.Db4o;
using db4oProviders;
using NUnit.Framework;

namespace WCSoft.db4oProviders
{
    [TestFixture]
    public class db4oProfileProviderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void TestSetUp()
        {
            this.config = new NameValueCollection();

            this.config.Add("connectionStringName", "connectionstringname");
            this.config.Add("ApplicationName", "db4otest");

            this.provider = new db4oProfileProvider();
            this.provider.ConnectionStringStore = new MockConnectionStringStore();
            this.provider.Initialize("db4otest", config);
        }

        [TearDown]
        public void TestTearDown()
        {
            SafeDB.DropAll();
            File.Delete(MockConnectionStringStore.DBFileName);
            this.provider = null;
        }

        #endregion

        private NameValueCollection config;
        private db4oProfileProvider provider;

        [Test]
        public void DeleteInactiveProfiles()
        {
            DateTime time1 = DateTime.Now;
            Thread.Sleep(1000);

            // username1
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time2 = DateTime.Now;
            Thread.Sleep(1000);

            // username2
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", false);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time3 = DateTime.Now;
            Thread.Sleep(1000);

            // username3
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time4 = DateTime.Now;

            int count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.All, time1);
            Assert.AreEqual(0, count, "should have deleted 0 for time1");

            count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.Anonymous, time2);
            Assert.AreEqual(0, count, "should have deleted 0 for time2");

            count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.Authenticated, time2);
            Assert.AreEqual(1, count, "should have deleted 1 for time2");

            count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.All, time2);
            Assert.AreEqual(0, count, "should have deleted 0 for time2 again");

            count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.Anonymous, time4);
            Assert.AreEqual(1, count, "should have deleted 1 for time4");

            count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.Authenticated, time4);
            Assert.AreEqual(1, count, "should have deleted 1 for time4");

            count = provider.DeleteInactiveProfiles(ProfileAuthenticationOption.All, time4);
            Assert.AreEqual(0, count, "should have deleted 0 for time4 again");
        }

        [Test]
        public void DeleteProfiles()
        {
            // username1
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            // username2
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            // username3
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            int count = provider.DeleteProfiles(new string[] {"foo", "bar"});
            Assert.AreEqual(0, count, "should have deleted 0");

            SafeDB.DropAll();

            using (
                IObjectContainer db = Db4oFactory.OpenFile(this.provider.ConnectionStringStore.GetConnectionString("")))
            {
                IList<Profile> profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username1"; });

                Assert.AreEqual(1, profiles.Count, "did not find 1 profile for username1");
                Assert.AreEqual("username1", profiles[0].Username, "wrong Username for username1");

                profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username2"; });

                Assert.AreEqual(1, profiles.Count, "did not find 1 profile for username2");
                Assert.AreEqual("username2", profiles[0].Username, "wrong Username for username2");

                profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username3"; });

                Assert.AreEqual(1, profiles.Count, "did not find 1 profile for username3");
                Assert.AreEqual("username3", profiles[0].Username, "wrong Username for username3");
            }

            ProfileInfoCollection pic = new ProfileInfoCollection();
            pic.Add(new ProfileInfo("username1", false, DateTime.Now, DateTime.Now, 1));
            pic.Add(new ProfileInfo("username3", false, DateTime.Now, DateTime.Now, 1));

            count = provider.DeleteProfiles(pic);
            Assert.AreEqual(2, count, "should have deleted 2");

            SafeDB.DropAll();

            using (
                IObjectContainer db = Db4oFactory.OpenFile(this.provider.ConnectionStringStore.GetConnectionString("")))
            {
                IList<Profile> profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username1"; });

                Assert.AreEqual(0, profiles.Count, "should not find username1");

                profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username2"; });

                Assert.AreEqual(1, profiles.Count, "did not find 1 profile for username2");
                Assert.AreEqual("username2", profiles[0].Username, "wrong Username for username2");

                profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username3"; });

                Assert.AreEqual(0, profiles.Count, "should not find username3");
            }
        }

        [Test]
        public void FindInactiveProfilesByUserName()
        {
            DateTime time1 = DateTime.Now;
            Thread.Sleep(1000);

            // username1 - authenticated
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time2 = DateTime.Now;
            Thread.Sleep(1000);

            // username2 - anonymous
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", false);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time3 = DateTime.Now;
            Thread.Sleep(1000);

            // username3 - authenticated
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time4 = DateTime.Now;

            int totalRecords;

            ProfileInfoCollection pic =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "name", time1, 0, 99,
                                                        out totalRecords);
            Assert.AreEqual(0, pic.Count, "wrong count 1");
            Assert.AreEqual(0, totalRecords, "wrong totalrecords 1");

            pic =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "name", time4, 1, 1,
                                                        out totalRecords);
            Assert.AreEqual(1, pic.Count, "wrong count 2");
            Assert.AreEqual(3, totalRecords, "wrong totalrecords 2");
            Assert.AreEqual("username2", pic["username2"].UserName, "wrong username 2");

            pic =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "3", time4, 1, 1,
                                                        out totalRecords);
            Assert.AreEqual(0, pic.Count, "wrong count 3");
            Assert.AreEqual(1, totalRecords, "wrong totalrecords 3");

            pic =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "3", time4, 0, 1,
                                                        out totalRecords);
            Assert.AreEqual(1, pic.Count, "wrong count 4");
            Assert.AreEqual(1, totalRecords, "wrong totalrecords 4");
            Assert.AreEqual("username3", pic["username3"].UserName, "wrong username 4");

            pic =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.Anonymous, null, time4, 0, 99,
                                                        out totalRecords);
            Assert.AreEqual(1, pic.Count, "wrong count 5");
            Assert.AreEqual(1, totalRecords, "wrong totalrecords 5");
            Assert.AreEqual("username2", pic["username2"].UserName, "wrong username 5");

            pic =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.Authenticated, null, time4, 0, 99,
                                                        out totalRecords);
            Assert.AreEqual(2, pic.Count, "wrong count 6");
            Assert.AreEqual(2, totalRecords, "wrong totalrecords 6");
            Assert.AreEqual("username1", pic["username1"].UserName, "wrong username 6");
            Assert.AreEqual("username3", pic["username3"].UserName, "wrong username 6");
        }

        [Test]
        public void FindProfilesByUserName()
        {
            // username1 - authenticated
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            // username2 - anonymous
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", false);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            // username3 - authenticated
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            int totalRecords;

            ProfileInfoCollection pic =
                provider.FindProfilesByUserName(ProfileAuthenticationOption.All, "name", 0, 99, out totalRecords);
            Assert.AreEqual(3, pic.Count, "wrong count 1");
            Assert.AreEqual(3, totalRecords, "wrong totalrecords 1");

            pic =
                provider.FindProfilesByUserName(ProfileAuthenticationOption.Anonymous, "name", 0, 99, out totalRecords);
            Assert.AreEqual(1, pic.Count, "wrong count 2");
            Assert.AreEqual(1, totalRecords, "wrong totalrecords 2");

            pic =
                provider.FindProfilesByUserName(ProfileAuthenticationOption.Authenticated, "name", 0, 99,
                                                out totalRecords);
            Assert.AreEqual(2, pic.Count, "wrong count 3");
            Assert.AreEqual(2, totalRecords, "wrong totalrecords 3");

            pic =
                provider.FindProfilesByUserName(ProfileAuthenticationOption.Authenticated, "username2", 0, 99,
                                                out totalRecords);
            Assert.AreEqual(0, pic.Count, "wrong count 4");
            Assert.AreEqual(0, totalRecords, "wrong totalrecords 4");

            pic = provider.FindProfilesByUserName(ProfileAuthenticationOption.All, "", 1, 2, out totalRecords);
            Assert.AreEqual(1, pic.Count, "wrong count 5");
            Assert.AreEqual(3, totalRecords, "wrong totalrecords 5");
            Assert.AreEqual("username3", pic["username3"].UserName, "wrong profile found 5");
        }

        [Test]
        public void GetAllInactiveProfiles()
        {
            DateTime time1 = DateTime.Now;
            Thread.Sleep(1000);

            // username1 - authenticated
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time2 = DateTime.Now;
            Thread.Sleep(1000);

            // username2 - anonymous
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", false);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time3 = DateTime.Now;
            Thread.Sleep(1000);

            // username3 - authenticated
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time4 = DateTime.Now;

            int totalRecords1;
            int totalRecords2;

            ProfileInfoCollection pic1 =
                provider.GetAllInactiveProfiles(ProfileAuthenticationOption.All, time2, 0, 99, out totalRecords1);
            ProfileInfoCollection pic2 =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "", time2, 0, 99,
                                                        out totalRecords2);
            Assert.AreEqual(pic2.Count, pic1.Count, "wrong data 1");
            Assert.AreEqual(totalRecords2, totalRecords1, "wrong totalrecords 1");

            pic1 = provider.GetAllInactiveProfiles(ProfileAuthenticationOption.All, time2, 0, 2, out totalRecords1);
            pic2 =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "", time2, 0, 2,
                                                        out totalRecords2);
            Assert.AreEqual(pic2.Count, pic1.Count, "wrong data 2");
            Assert.AreEqual(totalRecords2, totalRecords1, "wrong totalrecords 2");

            pic1 = provider.GetAllInactiveProfiles(ProfileAuthenticationOption.All, time2, 1, 2, out totalRecords1);
            pic2 =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "", time2, 1, 2,
                                                        out totalRecords2);
            Assert.AreEqual(pic2.Count, pic1.Count, "wrong data 3");
            Assert.AreEqual(totalRecords2, totalRecords1, "wrong totalrecords 3");
        }

        [Test]
        public void GetAllProfiles()
        {
            DateTime time1 = DateTime.Now;
            Thread.Sleep(1000);

            // username1 - authenticated
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time2 = DateTime.Now;
            Thread.Sleep(1000);

            // username2 - anonymous
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", false);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time3 = DateTime.Now;
            Thread.Sleep(1000);

            // username3 - authenticated
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time4 = DateTime.Now;

            int totalRecords1;
            int totalRecords2;

            ProfileInfoCollection pic1 =
                provider.GetAllProfiles(ProfileAuthenticationOption.All, 0, 99, out totalRecords1);
            ProfileInfoCollection pic2 =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "", time4, 0, 99,
                                                        out totalRecords2);
            Assert.AreEqual(pic2.Count, pic1.Count, "wrong data 1");
            Assert.AreEqual(totalRecords2, totalRecords1, "wrong totalrecords 1");

            pic1 = provider.GetAllProfiles(ProfileAuthenticationOption.All, 0, 2, out totalRecords1);
            pic2 =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "", time4, 0, 2,
                                                        out totalRecords2);
            Assert.AreEqual(pic2.Count, pic1.Count, "wrong data 2");
            Assert.AreEqual(totalRecords2, totalRecords1, "wrong totalrecords 2");

            pic1 = provider.GetAllProfiles(ProfileAuthenticationOption.All, 1, 2, out totalRecords1);
            pic2 =
                provider.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "", time4, 1, 2,
                                                        out totalRecords2);
            Assert.AreEqual(pic2.Count, pic1.Count, "wrong data 3");
            Assert.AreEqual(totalRecords2, totalRecords1, "wrong totalrecords 3");
        }

        [Test]
        public void GetNumberOfInactiveProfiles()
        {
            DateTime time1 = DateTime.Now;
            Thread.Sleep(1000);

            // username1 - authenticated
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            SettingsPropertyValue spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time2 = DateTime.Now;
            Thread.Sleep(1000);

            // username2 - anonymous
            context = new SettingsContext();
            context.Add("UserName", "username2");
            context.Add("IsAuthenticated", false);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName1");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value2";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time3 = DateTime.Now;
            Thread.Sleep(1000);

            // username3 - authenticated
            context = new SettingsContext();
            context.Add("UserName", "username3");
            context.Add("IsAuthenticated", true);

            spvc = new SettingsPropertyValueCollection();
            sp1 = new SettingsProperty("propertyName2");
            sp1.Attributes.Add("AllowAnonymous", true);
            spv1 = new SettingsPropertyValue(sp1);
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value3";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            DateTime time4 = DateTime.Now;

            int count = provider.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.All, time1);
            Assert.AreEqual(0, count, "wrong data 1");

            count = provider.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.All, time2);
            Assert.AreEqual(1, count, "wrong data 2");

            count = provider.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.All, time3);
            Assert.AreEqual(2, count, "wrong data 3");

            count = provider.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.All, time4);
            Assert.AreEqual(3, count, "wrong data 4");

            count = provider.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.Anonymous, time4);
            Assert.AreEqual(1, count, "wrong data 5");

            count = provider.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.Authenticated, time4);
            Assert.AreEqual(2, count, "wrong data 6");
        }

        [Test]
        public void GetPropertyValues_NotFound()
        {
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyCollection spc = new SettingsPropertyCollection();
            SettingsProperty sp1 = new SettingsProperty("propertyName1");
            spc.Add(sp1);
            SettingsProperty sp2 = new SettingsProperty("propertyName2");
            sp2.DefaultValue = 3.14M;
            spc.Add(sp2);

            SettingsPropertyValueCollection spvc = provider.GetPropertyValues(context, spc);
            Assert.AreEqual(2, spvc.Count, "should be 2 elements");
            Assert.AreEqual("propertyName1", spvc["propertyName1"].Name, "wrong name 1");
            Assert.IsTrue(spvc["propertyName1"].UsingDefaultValue, "wrong usingdefaultvalue 1");
            Assert.AreEqual("propertyName2", spvc["propertyName2"].Name, "wrong name 2");
            Assert.IsTrue(spvc["propertyName2"].UsingDefaultValue, "wrong usingdefaultvalue 2");
        }

        [Test]
        public void Initialize()
        {
            Assert.AreEqual("db4otest", provider.ApplicationName, "wrong ApplicationName");
        }

        [Test]
        public void SetAndGetPropertyValues()
        {
            SettingsContext context = new SettingsContext();
            context.Add("UserName", "username1");
            context.Add("IsAuthenticated", true);

            SettingsPropertyValueCollection spvc = new SettingsPropertyValueCollection();
            SettingsPropertyValue spv1 = new SettingsPropertyValue(new SettingsProperty("propertyName1"));
            spv1.Property.PropertyType = typeof (string);
            spv1.PropertyValue = "value1";
            spv1.IsDirty = false;
            spv1.Deserialized = true;
            spvc.Add(spv1);

            provider.SetPropertyValues(context, spvc);

            #region test that dates are set and old enough

            Thread.Sleep(1000);
            DateTime now = DateTime.Now;

            SafeDB.DropAll();

            using (
                IObjectContainer db = Db4oFactory.OpenFile(this.provider.ConnectionStringStore.GetConnectionString("")))
            {
                IList<Profile> profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username1"; });

                Profile profile = profiles[0];

                Assert.AreEqual(-1, profile.LastActivityDate.CompareTo(now), "LastActivityDate not old enough");
                Assert.AreEqual(profile.LastActivityDate, profile.LastUpdatedDate, "LastUpdatedDate not old enough");
            }

            #endregion

            SettingsPropertyCollection spc = new SettingsPropertyCollection();
            spc.Add(new SettingsProperty("propertyName1"));

            SettingsProperty sp2 = new SettingsProperty("propertyName2");
            sp2.DefaultValue = 3.14M;
            spc.Add(sp2);

            SettingsPropertyValueCollection result = provider.GetPropertyValues(context, spc);
            Assert.AreEqual(2, result.Count, "should be 2 elements");
            Assert.IsFalse(result["propertyName1"].UsingDefaultValue, "wrong usingdefaultvalue 1");
            Assert.AreEqual("value1", result["propertyName1"].PropertyValue, "wrong value");
            Assert.AreEqual("propertyName1", result["propertyName1"].Name, "wrong spv name");
            Assert.AreEqual("propertyName1", result["propertyName1"].Property.Name, "wrong property name");
            Assert.AreEqual("propertyName2", result["propertyName2"].Name, "wrong name 2");
            Assert.IsTrue(result["propertyName2"].UsingDefaultValue, "wrong usingdefaultvalue 2");

            #region test that dates are set and old enough

            Thread.Sleep(1000);

            SafeDB.DropAll();

            using (
                IObjectContainer db = Db4oFactory.OpenFile(this.provider.ConnectionStringStore.GetConnectionString("")))
            {
                IList<Profile> profiles = db.Query<Profile>(
                    delegate(Profile p) { return p.Username == "username1"; });

                Profile profile = profiles[0];

                Assert.AreEqual(1, profile.LastActivityDate.CompareTo(now), "LastActivityDate was not updated by Get");
                Assert.AreNotEqual(profile.LastActivityDate, profile.LastUpdatedDate,
                                   "LastUpdatedDate was updated by Get");
            }

            #endregion
        }
    }
}