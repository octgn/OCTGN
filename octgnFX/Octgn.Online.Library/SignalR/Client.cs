namespace Octgn.Online.Library.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Microsoft.AspNet.SignalR.Client;
    using Microsoft.AspNet.SignalR.Client.Hubs;

    public abstract class Client : IDisposable
    {
        public string Host { get; protected set; }
        public string Hub { get; protected set; }
        internal HubConnection Connection;
        internal bool Stopped;
        internal bool Started;
        internal IHubProxy Proxy;

        public Client Setup<T>(string host, string hub)
        {
            Host = host;
            Hub = hub;
            Connection = new HubConnection(host,false);
            this.GenerateProxy<T>(hub);
            Connection.Closed += ConnectionOnClosed;
            Connection.Closed += ConnectionOnClosedInternal;
            Connection.Error += ConnectionOnError;
            Connection.Received += ConnectionOnReceived;
            Connection.Reconnected += ConnectionOnReconnected;
            Connection.Reconnecting += ConnectionOnReconnecting;
            Connection.StateChanged += ConnectionOnStateChanged;
            return this;
        }

        protected void Start()
        {
            if(Host == null || Hub == null)
                throw new InvalidOperationException("Must call .Setup before using.");
            if(Started)
                throw new InvalidOperationException("Can't call start if already started.");
            Started = true;
            Stopped = false;
            this.Connect();
        }

        protected void Stop()
        {
            if (Host == null || Hub == null)
                throw new InvalidOperationException("Must call .Setup before using.");
            Stopped = true;
            Started = false;
            Connection.Stop();
        }

        internal void Connect()
        {
            if (Stopped)
            {
                return;
            }

            Connection.Start().Wait(5000);
        }

        internal void GenerateProxy<T>(string hub)
        {
            Proxy = Connection.CreateHubProxy(hub);
            var t = typeof(T);
            var obj = Activator.CreateInstance<T>();
            foreach (var method in t.GetMethods())
            {
                var method1 = method;
                var sub = Proxy.Subscribe(method1.Name);
                sub.Data += tokens =>
                    {
                        var args = new List<Object>();
                        var curArg = 0;
                        foreach (var tok in tokens)
                        {
                            args.Add(tok.ToObject(method1.GetParameters()[curArg].ParameterType));
                            curArg++;
                        }
                        method1.Invoke(obj, args.ToArray());
                    };
            }
        }

        internal void OnProxyCall(MethodInfo method, dynamic args)
        {
            
        }

        #region Connection Events

        protected virtual void ConnectionOnStateChanged(StateChange obj){}

        protected virtual void ConnectionOnReconnecting() { }

        protected virtual void ConnectionOnReconnected() { }

        protected virtual void ConnectionOnReceived(string obj)
        {
        }

        protected virtual void ConnectionOnError(Exception obj) { }

        protected virtual void ConnectionOnClosed() { }

        internal void ConnectionOnClosedInternal()
        {
            if(Stopped == false)
                this.Connect();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Connection.Closed -= ConnectionOnClosed;
            Connection.Closed -= ConnectionOnClosedInternal;
            Connection.Error -= ConnectionOnError;
            Connection.Received -= ConnectionOnReceived;
            Connection.Reconnected -= ConnectionOnReconnected;
            Connection.Reconnecting -= ConnectionOnReconnecting;
            Connection.StateChanged -= ConnectionOnStateChanged;
            Connection.Stop();
        }
        #endregion 
    }
}
