namespace Octgn.Online.Test.Library.SignalR
{
    using System;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Octgn.Online.Library.SignalR.TypeSafe;

    public class DynamicProxyTests
    {
        [Test]
        public void Constructor()
        {
            var proxy = DynamicProxy<IDynamicProxyTestInterface>.Get();
            bool test = false;
            string testString = "";
            int i = 0;

            proxy.On(x => () => x.Hello(default(string))).Calls(mi =>
                {
                    Assert.AreEqual("asdf", mi.Args[0]);
                    test = true;
                });
            proxy.Instance.Hello("asdf");
            Assert.True(test);
            test = false;

            proxy.On<string, Task>(x => x.Hello3).Calls(mi => new Task(() => { test = true;
                                                                                 testString = mi.Args[0] as string;
            }));
            var ret = proxy.Instance.Hello3("chicken");
            Assert.NotNull(ret);
            ret.Start();
            ret.Wait();
            Assert.True(test);
            Assert.AreEqual("chicken",testString);
        }
    }

    public interface IDynamicProxyTestInterface
    {
        void Hello(string a);

        void Hello2();

        Task Hello3(string a);
    }
}