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
using System.IO;
using System.Threading;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using db4oProviders;
using NUnit.Framework;

namespace WCSoft.db4oProviders
{
    [TestFixture]
    public class SafeDBTests
    {
        private Exception exception;
        private readonly List<string> log = new List<string>();

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            SafeDB.DropAll();
            File.Delete(Path.GetTempPath() + @"/tempdb1.db");
        }

        private void UseTempFile1ManyTimes()
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    using (SafeDB dbc = new SafeDB(Path.GetTempPath() + @"/tempdb1.db"))
                    {
                        dbc.Store(new SafeDBTests());
                    }

                    log.Add("TempFile1 used and released");

                    Thread.Sleep(new Random().Next(0, 200));
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        /// <summary>
        /// This test just verifies that we can't call OpenFile on the same file twice
        /// before Dispose-ing the first IObjectContainer.  If this test doesn't pass,
        /// then the passing of ShouldSerializeAccessForTheSameConnectionString doesn't mean much.
        /// </summary>
        [Test, ExpectedException(typeof (DatabaseFileLockedException))]
        public void ShouldNotBeAbleToOpenTheSameDBFileMoreThanOnce()
        {
            using (IObjectContainer db1 = Db4oFactory.OpenFile(Path.GetTempPath() + @"/tempdb1.db"))
            {
                using (IObjectContainer db2 = Db4oFactory.OpenFile(Path.GetTempPath() + @"/tempdb1.db"))
                {
                    db1.Store(new SafeDBTests());
                    db2.Store(new SafeDBTests());
                }
            }
        }

        [Test]
        public void ShouldSerializeAccessForTheSameConnectionString()
        {
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < 5; i++)
                threads.Add(new Thread(UseTempFile1ManyTimes));

            foreach (Thread t in threads)
                t.Start();

            foreach (Thread t in threads)
                t.Join();

            string strException = "";
            if (exception != null)
                strException = exception.ToString();

            Assert.IsNull(exception, "There was an exception: " + strException);
        }
    }
}
