using Discord;
using Octgn.Online.Hosting;
using System;
using System.Threading;

namespace Octgn.Core.DiscordIntegration
{
    public class DiscordWrapper : ViewModelBase, IDisposable
    {
        public bool IsRunning
        {
            get => _isRunning;
            set => SetAndNotify(ref _isRunning, value);
        }

        private bool _isRunning;

        public event EventHandler<Exception> Error;

        private DateTime _lastActivityUpdate = DateTime.MinValue;
        private bool disposedValue;

        private Activity? _activity;

        private readonly long _clientId = 545092813155598336;

        private readonly Discord.Discord _discord;
        private readonly Timer _updateTimer;

        private static readonly DateTime _epoch = new DateTime(1970, 1, 1);

        public DiscordWrapper()
        {
            _discord = new Discord.Discord(_clientId, (UInt64)Discord.CreateFlags.Default);
            _updateTimer = new Timer(UpdateDiscord, this, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        public void UpdateStatusNothing()
        {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;

            _activity = activity;
        }

        public void UpdateStatusInDeckEditor()
        {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;
            activity.Details = "Building a Deck";

            _activity = activity;
        }

        public void UpdateStatusInGame(string gameName, DateTimeOffset gameStartTime, bool isHost, bool isReplay, bool isSpectator, bool isPreGame, int playerCount)
        {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;
            activity.Details = gameName;
            activity.Assets.LargeImage = "bruco";

            String state;
            if (isReplay)
            {
                state = "Replaying";
                activity.Type = ActivityType.Watching;
            } else if (isPreGame)
            {
                activity.Party.Size.CurrentSize = playerCount;
                activity.Party.Size.MaxSize = 8;
                if (isHost)
                {
                    state = "In Lobby(Host)";
                } else
                {
                    state = "In Lobby";
                }
            } else
            {
                activity.Party.Size.CurrentSize = playerCount;
                activity.Party.Size.MaxSize = 8;
                if (isHost)
                {
                    state = "In Game(Host)";
                } else if(isSpectator)
                {
                    state = "Spectating";
                    activity.Type = ActivityType.Watching;
                }
                else
                {
                    state = "In Game";
                }
            }
            activity.State = state;

            activity.Timestamps.Start = (long)(gameStartTime.UtcDateTime - _epoch).TotalSeconds;

            _activity = activity;
        }

        private void UpdateDiscord(object state)
        {
            try
            {
                _discord.RunCallbacks();
                IsRunning = true;
                UpdateActivity();
            }
            catch (ResultException ex) when (ex.Result == Result.NotRunning)
            {
                // Discord not running
                IsRunning = false;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
            }
            finally
            {
                _updateTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
            }
        }

        private void UpdateActivity()
        {
            var activity = _activity;

            if (activity == null) return;

            var nextUpdateActivity = _lastActivityUpdate.AddSeconds(30);

            if (DateTime.Now < nextUpdateActivity) return;

            _lastActivityUpdate = DateTime.Now;

            _discord.GetActivityManager().UpdateActivity(activity.Value, result =>
            {
                if (result != Result.Ok)
                {
                    Error?.Invoke(this, new ResultException(result));
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                _discord.Dispose();

                disposedValue = true;
            }
        }

        ~DiscordWrapper()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
