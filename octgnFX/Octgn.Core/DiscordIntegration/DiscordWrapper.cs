using Discord;
using Octgn.Online.Hosting;
using System;
using System.Threading;

namespace Octgn.Core.DiscordIntegration
{
    public class DiscordWrapper : ViewModelBase, IDisposable
    {
        public bool IsRunning {
            get => _isRunning;
            set => SetAndNotify(ref _isRunning, value);
        }

        private bool _isRunning;

        public event EventHandler<Exception> Error;

        public event EventHandler<HostedGame> JoinGame;
        public event EventHandler<HostedGame> SpectateGame;

        private DateTime _lastActivityUpdate = DateTime.MinValue;
        private bool disposedValue;

        private Activity? _activity;

        private readonly long _clientId = 545092813155598336;

        private readonly Discord.Discord _discord;
        private readonly Timer _updateTimer;
        private readonly Discord.ActivityManager _activityManager;

        private static readonly DateTime _epoch = new DateTime(1970, 1, 1);

        public DiscordWrapper() {
            _discord = new Discord.Discord(_clientId, (UInt64)CreateFlags.Default);
            _activityManager = _discord.GetActivityManager() ?? throw new InvalidOperationException($"Discord ActivityManager null");
            _activityManager.OnActivityJoin += ActivityManagerInstance_OnActivityJoin;
            _activityManager.OnActivitySpectate += ActivityManagerInstance_OnActivitySpectate;
            _updateTimer = new Timer(UpdateDiscord, this, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        private void ActivityManagerInstance_OnActivitySpectate(string secret) {
            try {
                var hostedGame = HostedGame.Deserialize(secret);

                SpectateGame?.Invoke(this, hostedGame);
            } catch (Exception ex) {
                Error?.Invoke(this, ex);
            }
        }

        private void ActivityManagerInstance_OnActivityJoin(string secret) {
            try {
                var hostedGame = HostedGame.Deserialize(secret);

                JoinGame?.Invoke(this, hostedGame);
            } catch (Exception ex) {
                Error?.Invoke(this, ex);
            }
        }

        public void UpdateStatusNothing() {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;

            _activity = activity;
        }

        public void UpdateStatusInDeckEditor() {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;
            activity.Details = "Building a Deck";

            _activity = activity;
        }

        public void UpdateStatusInGame(HostedGame game, bool isHost, bool isReplay, bool isSpectator, bool isPreGame, int playerCount) {
            var activity = new Activity();
            activity.Instance = true;
            activity.Type = ActivityType.Playing;
            activity.Details = game.GameName;
            activity.Assets.LargeImage = "bruco";
            activity.Party.Id = game.Id.ToString();

            String state;
            if (isReplay) {
                state = "Replaying";
                activity.Type = ActivityType.Watching;
            } else if (isPreGame) {
                activity.Party.Size.CurrentSize = playerCount;
                activity.Party.Size.MaxSize = 8;
                if (isHost) {
                    state = "In Lobby(Host)";
                } else {
                    state = "In Lobby";
                }

                activity.Secrets.Join = HostedGame.Serialize(game);

                activity.Timestamps.Start = (long)(game.DateCreated.UtcDateTime - _epoch).TotalSeconds;
            } else {
                activity.Party.Size.CurrentSize = playerCount;
                activity.Party.Size.MaxSize = 8;
                if (isHost) {
                    state = "In Game(Host)";
                } else if (isSpectator) {
                    state = "Spectating";
                    activity.Type = ActivityType.Watching;
                } else {
                    state = "In Game";
                }

                activity.Secrets.Spectate = HostedGame.Serialize(game);

                activity.Timestamps.Start = (long)(game.DateStarted.Value.UtcDateTime - _epoch).TotalSeconds;
            }
            activity.State = state;


            _activity = activity;
        }

        private void UpdateDiscord(object state) {
            try {
                _discord.RunCallbacks();
                IsRunning = true;
                UpdateActivity();
            } catch (ResultException ex) when (ex.Result == Result.NotRunning) {
                // Discord not running
                IsRunning = false;
            } catch (Exception ex) {
                Error?.Invoke(this, ex);
            } finally {
                _updateTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
            }
        }

        private void UpdateActivity() {
            var activity = _activity;

            if (activity == null) return;

            var nextUpdateActivity = _lastActivityUpdate.AddSeconds(30);

            if (DateTime.Now < nextUpdateActivity) return;

            _lastActivityUpdate = DateTime.Now;

            _discord.GetActivityManager().UpdateActivity(activity.Value, result => {
                if (result != Result.Ok) {
                    Error?.Invoke(this, new ResultException(result));
                }
            });
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _activityManager.OnActivityJoin -= ActivityManagerInstance_OnActivityJoin;
                    _activityManager.OnActivitySpectate -= ActivityManagerInstance_OnActivitySpectate;
                }

                _discord.Dispose();

                disposedValue = true;
            }
        }

        ~DiscordWrapper() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
