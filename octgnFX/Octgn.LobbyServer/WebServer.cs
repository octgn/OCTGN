using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Skylabs.LobbyServer
{
    public class WebServer
    {
        private Thread _thread;
        private HttpListener _server;
        private bool _running;
        public WebServer()
        {
            _running = false;
            _thread =new Thread(Run);
            _server = new HttpListener();
            _server.Prefixes.Add("http://+:8901/");
        }
        public bool Start()
        {
            if(!_running)
            {
                try
                {
                    _server.Start();
                    _thread.Start();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
        public void Stop()
        {
            _running = false;
        }
        private void Run()
        {
            _running = true;
            while(_running)
            {
                HttpListenerContext con = _server.GetContext();
                HttpListenerRequest req = con.Request;

                var page = req.Url.AbsolutePath.Trim('/');
                page = page.ToLower();
                switch(page)
                {
                    case "":
                        {
                            Version v = Assembly.GetCallingAssembly().GetName().Version;
                            var spage = File.ReadAllText("webserver/index.htm");
                            spage = spage.Replace("$version", v.ToString());
                            byte[] buffer = Encoding.UTF8.GetBytes(spage);
                            con.Response.ContentLength64 = buffer.Length;
                            using (Stream o = con.Response.OutputStream)
                            {
                                o.Write(buffer, 0, buffer.Length);
                                o.Close();
                            }
                            break;
                        }
                }
            }
        }
    }
}
