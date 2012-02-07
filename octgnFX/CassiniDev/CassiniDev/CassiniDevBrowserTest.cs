using System;
using System.Diagnostics;
using System.Threading;
using CassiniDev.ServerLog;

namespace CassiniDev
{
    /// <summary>
    ///   A web test executor base on an idea from Nikhil Kothari's Script# http://projects.nikhilk.net/ScriptSharp TODO: finer grained control over browser instances. TODO: create parser/abstraction for RequestEventArgs
    /// </summary>
    public class CassiniDevBrowserTest : CassiniDevServer
    {
        private readonly string _postKey = "testresults.axd";

        public CassiniDevBrowserTest(string postKey)
        {
            _postKey = postKey;
        }

        public CassiniDevBrowserTest()
        {
        }

        public string PostKey
        {
            get { return _postKey; }
        }

        public RequestEventArgs RunTest(string url)
        {
            return RunTest(url, WebBrowser.InternetExplorer, TimeSpan.FromMinutes(1.0));
        }

        public RequestEventArgs RunTest(string url, WebBrowser browser)
        {
            return RunTest(url, browser, TimeSpan.FromMinutes(1.0));
        }

        public RequestEventArgs RunTest(string url, WebBrowser browser, TimeSpan timeout)
        {
            if (browser == null)
            {
                throw new ArgumentNullException("browser");
            }
            if (string.IsNullOrEmpty(browser.ExecutablePath))
            {
                throw new InvalidOperationException("The specified browser could not be located.");
            }
            if (Math.Abs(timeout.TotalMilliseconds - 0.0) < double.Epsilon)
            {
                timeout = TimeSpan.FromMinutes(1.0);
            }
            var waitHandle = new AutoResetEvent(false);
            RequestEventArgs result = null;
            Process process;
            EventHandler<RequestEventArgs> logEventHandler = null;
            EventHandler<RequestEventArgs> handler = logEventHandler;
            logEventHandler = delegate(object sender, RequestEventArgs e)
                                  {
                                      if (e.RequestLog.Url.ToLower().Contains(_postKey))
                                      {
                                          Server.RequestComplete -= handler;
                                          result = e;
                                          waitHandle.Set();
                                      }
                                  };
            try
            {
                var startInfo = new ProcessStartInfo(browser.ExecutablePath, url)
                                    {
                                        UseShellExecute = true,
                                        WindowStyle = ProcessWindowStyle.Minimized
                                    };
                Server.RequestComplete += logEventHandler;
                process = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Server.RequestComplete -= logEventHandler;
                return new RequestEventArgs(Guid.Empty, new LogInfo {StatusCode = -1, Exception = ex.ToString()},
                                            new LogInfo());
            }
            bool flag = waitHandle.WaitOne(timeout);
            try
            {
                if (!process.CloseMainWindow())
                {
                    process.Kill();
                }
            }
            catch
            {
            }
            return flag ? result : new RequestEventArgs(Guid.Empty, new LogInfo {StatusCode = -2}, new LogInfo());
        }
    }
}