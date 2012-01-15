// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Salient.SqlServer.Testing;

namespace CassiniDev.Testing.SqlServer
{
    /// <summary>
    /// Made a go at spinning the server up from this process but after dealing with 
    /// irratic behaviour regarding apartment state, platform concerns, unloaded app domains,
    /// and all the other issues that you can find that people struggle with I just decided
    /// to strictly format the console app's output and just spin up an external process. 
    /// Seems robust so far.
    /// </summary>
    public partial class CassiniSqlFixture : DatabaseFixture, IDisposable
    {
        private bool _disposed;
        private bool _hostAdded;
        private string _hostname;
        private StreamWriter _input;
        private IPAddress _ipAddress;
        private Thread _outputThread;
        private string _rootUrl;
        private Process _serverProcess;

        public CassiniSqlFixture(string dataSource, string initialCatalog) : base(dataSource, initialCatalog)
        {
        }

        public CassiniSqlFixture(string dataSource, string initialCatalog, string userId, string password) : base(dataSource, initialCatalog, userId, password)
        {
        }

        public CassiniSqlFixture(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// The root URL of the running web application
        /// </summary>
        public string RootUrl
        {
            get { return _rootUrl; }
        }

        /// <summary>
        /// Combine the RootUrl of the running web application with the relative url
        /// specified.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public virtual Uri NormalizeUri(string relativeUrl)
        {
            relativeUrl = relativeUrl.TrimStart(new[] {'/'});
            string rootUrl = _rootUrl;
            if (!rootUrl.EndsWith("/"))
            {
                rootUrl += "/";
            }
            return new Uri(rootUrl + relativeUrl);
        }

        /// <summary>
        /// <para>Finds first available port in range on specified IP address.</para>
        /// <para>To check a specific port set start and end to same value.</para>
        /// </summary>
        /// <param name="portRangeStart"></param>
        /// <param name="portRangeEnd"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If no port in range is available.</exception>
        public static ushort GetPort(ushort portRangeStart, ushort portRangeEnd, IPAddress ipAddress)
        {
            ushort port = ServiceFactory.Rules.GetAvailablePort(portRangeStart, portRangeEnd, ipAddress, true);

            if (port == 0)
            {
                throw new InvalidOperationException("Port is in use");
            }

            return port;
        }

        /// <summary>
        /// </summary>
        /// <param name="applicationPath">Physical path to application.</param>
        /// <param name="ipAddress">IP to listen on.</param>
        /// <param name="port">Port to listen on.</param>
        /// <param name="virtualPath">Optional. default value '/'</param>
        /// <param name="hostname">Optional unless addHostsEntry is true. In all cases is used to construct RootUrl.</param>
        /// <param name="addHostsEntry">If true, add hosts file entry. Requires read/write permissions to hosts file.</param>
        /// <param name="waitForPort">Length of time, in ms, to wait for a specific port before throwing an exception or exiting. 0 = don't wait.</param>
        /// <param name="timeOut">Length of time, in ms, to wait for a request before stopping the server. 0 = no timeout.</param>
        /// <para>If true attempts to add a temporary entry to windows hosts file comprised of ipAddress and hostname.</para>
        /// <para>The entry is removed when server is stopped.</para>
        /// <para>Throws UnauthorizedAccessException if process does not have write permissions to hosts file.</para>
        /// </param>
        public virtual void StartServer(string applicationPath, IPAddress ipAddress, ushort port, string virtualPath,
                                        string hostname, bool addHostsEntry, int waitForPort, int timeOut)
        {
            _hostAdded = addHostsEntry;
            _hostname = hostname;
            _ipAddress = ipAddress;

            // massage and validate arguments
            if (string.IsNullOrEmpty(virtualPath))
            {
                virtualPath = "/";
            }
            if (!virtualPath.StartsWith("/"))
            {
                virtualPath = "/" + virtualPath;
            }
            if (_serverProcess != null)
            {
                throw new InvalidOperationException("Server is running");
            }
            if (addHostsEntry)
            {
                if (string.IsNullOrEmpty(hostname))
                {
                    throw new InvalidOperationException("Hostname is missing");
                }

                ServiceFactory.Rules.AddHostEntry(_ipAddress.ToString(), _hostname);
            }

            // use the arg object to construct command line string


            string commandLine = (new CommandLineArguments
                                      {
                                          Port = port,
                                          Path = string.Format("\"{0}\"", Path.GetFullPath(applicationPath).Trim('\"').TrimEnd('\\')),
                                          HostName = hostname,
                                          IPAddress = ipAddress.ToString(),
                                          VirtualPath = string.Format("\"{0}\"",virtualPath),
                                          TimeOut = timeOut,
                                          WaitForPort = waitForPort,
                                          IPMode=IPMode.Specific,
                                          PortMode=PortMode.Specific 
                                      }).ToString();


            _serverProcess = new Process();
            _serverProcess.StartInfo = new ProcessStartInfo
                                           {
                                               UseShellExecute = false,
                                               ErrorDialog = false,
                                               CreateNoWindow = true,
                                               RedirectStandardOutput = true,
                                               RedirectStandardInput = true,
                                               FileName = "CassiniDev-console.exe",
                                               Arguments = commandLine,
                                               WorkingDirectory = Environment.CurrentDirectory
                                           };

            // we are going to monitor each line of the output until we get a start or error signal
            // and then just ignore the rest

            string line = null;

            _serverProcess.Start();

            _outputThread = new Thread(() =>
                                           {
                                               string l = _serverProcess.StandardOutput.ReadLine();
                                               while (l != null)
                                               {
                                                   if (l.StartsWith("started:") || l.StartsWith("error:"))
                                                   {
                                                       line = l;
                                                   }
                                                   l = _serverProcess.StandardOutput.ReadLine();
                                               }
                                           });
            _outputThread.Start();

            // use StandardInput to send the newline to stop the server when required
            _input = _serverProcess.StandardInput;

            // block until we get a signal
            while (line == null)
            {
                Thread.Sleep(10);
            }

            if (!line.StartsWith("started:"))
            {
                throw new Exception(string.Format("Could not start server: {0}", line));
            }

            // line is the root url
            _rootUrl = line.Substring(line.IndexOf(':') + 1);
        }

        /// <summary>
        /// <para>Stops the server, if running and removes hosts entry if added.</para>
        /// </summary>
        public virtual void StopServer()
        {
            StopServer(100);
        }

        /// <summary>
        /// <para>Stops the server, if running and removes hosts entry if added.</para>
        /// </summary>
        public virtual void StopServer(int delay)
        {
            Thread.Sleep(delay);
            if (_serverProcess != null)
            {
                try
                {
                    _input.WriteLine();
                    _serverProcess.WaitForExit(10000);
                    if (_hostAdded)
                    {
                        ServiceFactory.Rules.RemoveHostEntry(_ipAddress.ToString(), _hostname);
                    }
                    Thread.Sleep(10);
                }
                catch
                {
                }
                finally
                {
                    _serverProcess.Dispose();
                    _serverProcess = null;
                }
            }
        }

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_serverProcess != null)
                {
                    StopServer();
                }
            }
            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~CassiniSqlFixture()
        {
            Dispose();
        }

        #endregion
    }
}