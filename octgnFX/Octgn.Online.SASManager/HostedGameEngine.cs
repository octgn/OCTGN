namespace Octgn.Online.SASManagerService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Wrappers;

    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.Models;
    using Octgn.Online.Library.Net;

    using log4net;

    public class HostedGameEngine
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static HostedGameEngine GetById(Guid id)
        {
            var state = Data.DbContext.Get().Games.FirstOrDefault(x => x.Id == id);
            var all = Data.DbContext.Get().Games.ToList();
            if (state == null)
            {
                Log.ErrorFormat("Hosted Game State not found for ID {0}", id);
                if(System.Diagnostics.Debugger.IsAttached)
                    Debugger.Break();
            }
            return new HostedGameEngine(state);
        }
        public HostedGameState State { get; internal set; }
        public HostedGameEngine(IHostedGameState state)
        {
            State = (HostedGameState)state;
        }

        public HostedGameEngine Register()
        {
            this.SetStatus(EnumHostedGameStatus.BootRequested);
            return this;
        }

        public HostedGameEngine LaunchProcess()
        {
            this.SetStatus(EnumHostedGameStatus.Booting);
#if(DEBUG)
            var tempDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                @"..\..\..\Octgn.Online.StandAloneServer\bin\Debug\");
#else
            var tempDir = this.CloneSASToTemp();
#endif
            var fileName = Path.Combine(tempDir, "Octgn.Online.StandAloneServer.exe");

            var args = new List<String>();
            args.Add("-id=" + State.Id);
            args.Add("-name=" + State.Name);
            args.Add("-hostusername=" + State.HostUserName);
            args.Add("-gamename=" + State.GameName);
            args.Add("-gameid=" + State.GameId);
            args.Add("-gameversion=" + State.GameVersion);
            args.Add("-bind=" + State.HostUri.Host + ":" + State.HostUri.Port);
            if (State.HasPassword) args.Add("-password=" + State.Password);
#if(DEBUG)
            args.Add("-debug");
#endif

            var process = new ProcessStartInfo(fileName)
                              {
#if(!DEBUG)
                                  UseShellExecute = false,
                                  CreateNoWindow = true,
#endif
                                  WorkingDirectory = tempDir,
                                  Arguments = String.Join(" ", args)
                              };
            Process.Start(process);
            return this;
        }

        public HostedGameEngine SetStatus(EnumHostedGameStatus status)
        {
            State.Status = status;
            this.SaveState();
            return this;
        }

        internal void SaveState()
        {
            WriteConcernResult res = null;
            for (var i = 0; i < 3; i++)
            {
                res = Data.DbContext.Get().GameCollection.Save(State);
                if(res.Ok)return;
                
                Thread.Sleep(1000 * i);
            }
            if (res == null) Log.ErrorFormat("Could not save sate of game {0}", State.Id);
            else
                Log.ErrorFormat("Could not save state of game {0} in DB: {1}",State.Id,res.ErrorMessage ?? res.LastErrorMessage ?? "");
        }

        internal string CloneSASToTemp()
        {
            var cd = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;
            var sasDirectory = new DirectoryInfo(Path.Combine(cd.FullName, "sas"));
            var tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), State.Id.ToString()));
            if (Directory.Exists(tempDirectory.FullName)) Directory.CreateDirectory(tempDirectory.FullName);

            foreach (var d in sasDirectory.GetDirectories("*", SearchOption.AllDirectories))
            {
                var relDir = d.FullName.Replace(sasDirectory.FullName, "");
                var newDir = Path.Combine(tempDirectory.FullName, relDir);
                if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);
            }

            foreach (var f in sasDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var relFile = f.FullName.Replace(sasDirectory.FullName,"");
                var newFile = Path.Combine(tempDirectory.FullName, relFile);
                if (!File.Exists(newFile))
                {
                    f.CopyTo(newFile, true);
                }
            }
            return tempDirectory.FullName;
        }
    }
    public static class HostedGameEngineExtensionMethods
    {
        public static  HostedGameEngine Engine(this IHostedGameState state)
        {
            return new HostedGameEngine(state);
        }
    }
    
}