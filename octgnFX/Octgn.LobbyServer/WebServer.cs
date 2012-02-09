using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public class WebServer
    {
        private readonly HttpListener _server;
        private bool _running;

        //private CassiniDevServer _webServer = null;
        public WebServer()
        {
            //_webServer = new CassiniDevServer();
            //_webServer.StartServer(Path.Combine(Environment.CurrentDirectory, "webserver"), int.Parse(Program.Settings["webserverport"]), "/", Environment.MachineName);

            //CassiniDev.Server s = _webServer.Server;
            //List<Assembly> assemblyList = new List<Assembly>();
            //assemblyList.Add(Assembly.GetExecutingAssembly());
            //_webServer.Server.Assemblies = assemblyList;


            _running = false;
            _server = new HttpListener();
            int port;
            try
            {
                port = Int32.Parse(Program.Settings["webserverport"]);
            }
            catch (Exception)
            {
                port = 8901;
            }
            _server.Prefixes.Add(String.Format("http://+:{0}/", port));
        }

        public bool Start()
        {
            if (!_running)
            {
                try
                {
                    _server.Start();
                    AcceptConnections();
                    //_webServer.Server.Start();
                    //_webServer.Server.StartWithAssemblies();
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
            try
            {
                _server.Abort();
                _server.Close();
                _server.Stop();
                //_webServer.StopServer();
            }
            catch (Exception e) { Debug.WriteLine(e); if (Debugger.IsAttached) Debugger.Break(); }
        }

        private void AcceptConnections()
        {
            try
            {
                _server.BeginGetContext(HandleConnection, _server);
            }
            catch (Exception e) { Debug.WriteLine(e); if (Debugger.IsAttached) Debugger.Break(); }
        }

        private void HandleConnection(IAsyncResult res)
        {
            try
            {
                HttpListenerContext con = _server.EndGetContext(res);
                HttpListenerRequest req = con.Request;

                string page = req.Url.AbsolutePath.Trim('/');
                page = page.ToLower();
                switch (page)
                {
                    case "":
                        {
                            string spage = File.ReadAllText("webserver/index.htm");
                            spage = ReplaceVariables(spage);
                            SendItem(con.Response, spage);
                            break;
                        }
                    case "games.htm":
                        {
                            string spage = File.ReadAllText("webserver/games.htm");
                            spage = InsertRunningGames(spage);
                            SendItem(con.Response, spage);
                            break;
                        }
                    case "index.htm":
                        {
                            string time = req.QueryString["time"];
                            if (time != null)
                            {
                                int t;
                                if (Int32.TryParse(time, out t))
                                {
                                    Program.KillServerInTime(t);
                                    SendItem(con.Response, "1");
                                    break;
                                }
                            }
                            string spage = File.ReadAllText("webserver/index.htm");
                            spage = ReplaceVariables(spage);
                            SendItem(con.Response, spage);
                            break;
                        }
                    default:
                        {
                            string spage = "";
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
            catch (Exception e) { Debug.WriteLine(e); if (Debugger.IsAttached) Debugger.Break(); }
            AcceptConnections();
        }

        private static string ReplaceVariables(string rawpage)
        {
            Version v = Assembly.GetCallingAssembly().GetName().Version;
            //Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
            string ret = rawpage.Replace("$version", v.ToString());
            ret = ret.Replace("$runtime", Server.ServerRunTime.ToString());
            ret = ret.Replace("$onlineusers", Server.OnlineCount().ToString(CultureInfo.InvariantCulture));
            ret = ret.Replace("$hostedgames", Gaming.GameCount().ToString(CultureInfo.InvariantCulture));
            ret = ret.Replace("$totalhostedgames", Gaming.TotalHostedGames().ToString(CultureInfo.InvariantCulture));
            ret = ret.Replace("$proctime", Process.GetCurrentProcess().TotalProcessorTime.ToString());
            ret = ret.Replace("$memusage", ToFileSize(Process.GetCurrentProcess().WorkingSet64));
            ret = ret.Replace("$totmem", "256 MB");
            return ret;
        }

        private static string InsertRunningGames(string rawpage)
        {
            string insert = string.Empty;
            List<Lobby.HostedGame> games = Gaming.GetLobbyList();

            Version v = Assembly.GetCallingAssembly().GetName().Version;
            string ret = rawpage.Replace("$version", v.ToString());
            ret = ret.Replace("$runtime", Server.ServerRunTime.ToString());
            ret = ret.Replace("$proctime", Process.GetCurrentProcess().TotalProcessorTime.ToString());
            ret = ret.Replace("$memusage", ToFileSize(Process.GetCurrentProcess().WorkingSet64));
            ret = ret.Replace("$totmem", "256 MB");

            //construct game table
            foreach (Lobby.HostedGame game in games)
            {
                var ts = new TimeSpan(DateTime.Now.Ticks - game.TimeStarted.Ticks);
                insert = insert + "<tr>";
                insert = insert + "<td>" + game.Name + "</td>";
                insert = insert + "<td>" + game.Port + "</td>";
                insert = insert + "<td>" + game.GameStatus + "</td>";
                insert = insert + "<td>" + game.GameVersion + "</td>";
                insert = insert + "<td>" + ts + "</td>";
                Client c = Server.GetOnlineClientByUid(game.UserHosting.Uid);
                User user;
                if (c == null)
                {
                    user = game.UserHosting;
                    user.Status = UserStatus.Offline;
                }
                else
                    user = c.Me;

                insert = insert + "<td>Name: " + user.DisplayName + "<br />";
                insert = insert + "Status: " + Enum.GetName(typeof (UserStatus), user.Status) + "<br />";
                insert = insert + "Email: " + user.Email + "<br />";
                insert = insert + "Uid: " + user.Uid + "</td>";

                insert = insert + "</tr>";
            }

            ret = ret.Replace("$hostedgames", insert);
            return (ret);
        }

        private static void SendItem(HttpListenerResponse res, string page)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(page);
                res.ContentLength64 = buffer.Length;
                using (Stream o = res.OutputStream)
                {
                    o.Write(buffer, 0, buffer.Length);
                    o.Close();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException)
            {
            }
        }

        public static string ToFileSize(int source)
        {
            return ToFileSize(Convert.ToInt64(source));
        }

        public static string ToFileSize(long source)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(source);

            if (bytes >= Math.Pow(byteConversion, 3)) //GB Range
            {
                return string.Concat(Math.Round(bytes/Math.Pow(byteConversion, 3), 2), " GB");
            }
            if (bytes >= Math.Pow(byteConversion, 2)) //MB Range
            {
                return string.Concat(Math.Round(bytes/Math.Pow(byteConversion, 2), 2), " MB");
            }
            return bytes >= byteConversion
                       ? string.Concat(Math.Round(bytes/byteConversion, 2), " KB")
                       : string.Concat(bytes, " Bytes");
        }
    }
}