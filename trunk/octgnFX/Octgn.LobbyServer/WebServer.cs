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
            int port = 8901;
            try
            {
                port = Int32.Parse(Program.Settings["webserverport"]);
            }
            catch (Exception)
            {
                port = 8901;
            }
            _server.Prefixes.Add(String.Format("http://+:{0}/",port));
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
                switch (page)
                {
                    case "":
                        {
                            var spage = File.ReadAllText("webserver/index.htm");
                            spage = ReplaceVariables(spage);
                            SendItem(con.Response, spage);
                            break;
                        }
                    default:
                        {
                            var spage = "";
                            try
                            {
                                spage = File.ReadAllText("webserver/" + page);
                            }
                            catch (Exception)
                            {
                                spage = "";
                                con.Response.StatusCode = 404;
                            }
                            spage = ReplaceVariables(spage);
                            SendItem(con.Response, spage);
                            break;
                        }
                }
            }
        }
        private string ReplaceVariables(string rawpage)
        {
            string ret = rawpage;
            Version v = Assembly.GetCallingAssembly().GetName().Version;
            ret = rawpage.Replace("$version", v.ToString());
            ret = ret.Replace("$runtime", Server.ServerRunTime.ToString());
            return ret;
        }
        private void SendItem(HttpListenerResponse res,string page)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(page);
            res.ContentLength64 = buffer.Length;
            using (Stream o = res.OutputStream)
            {
                o.Write(buffer, 0, buffer.Length);
                o.Close();
            }
        }
    }
}
