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

        public event EventHandler<Guid> JoinGame;

        private DateTime _lastActivityUpdate = DateTime.MinValue;
        private bool disposedValue;

        private Activity? _activity;

        private readonly long _clientId = 545092813155598336;

        private readonly Discord.Discord _discord;
        private readonly Timer _updateTimer;
        private readonly Discord.ActivityManager _activityManager;

        private static readonly DateTime _epoch = new DateTime(1970, 1, 1);

        public DiscordWrapper() {
            _discord = new Discord.Discord(_clientId, (UInt64)CreateFlags.NoRequireDiscord);
            _activityManager = _discord.GetActivityManager() ?? throw new InvalidOperationException($"Discord ActivityManager null");
            _activityManager.OnActivityJoin += ActivityManagerInstance_OnActivityJoin;
            _updateTimer = new Timer(UpdateDiscord, this, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        private void ActivityManagerInstance_OnActivityJoin(string secret) {
            try {
                var hostedGameId = new Guid(Convert.FromBase64String(secret));

                JoinGame?.Invoke(this, hostedGameId);
            } catch (Exception ex) {
                Error?.Invoke(this, ex);
            }
        }

        public void UpdateStatusNothing() {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;

            _activity = activity;
        }
        public void UpdateStatusMainWindow() {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;
            activity.Assets.LargeImage = "logolarge";
            activity.Details = "Just durdling around...";

            _activity = activity;
        }

        public void UpdateStatusInDeckEditor() {
            var activity = new Activity();
            activity.Type = ActivityType.Playing;
            activity.Details = "Building a Deck";
            activity.Assets.LargeImage = "logolarge";
            activity.Instance = true;

            _activity = activity;
        }

        public void UpdateStatusInGame(HostedGame game, bool isHost, bool isReplay, bool isSpectator, bool isPreGame, int playerCount) {
            var activity = new Activity();
            activity.Instance = true;
            activity.Details = game.GameName;
            activity.Assets.LargeImage = "logolarge";
            activity.Party.Id = game.Id.ToString();

            if (isReplay) {
                activity.State = "Watching a Replay";
                activity.Type = ActivityType.Watching;
            } else {
                if (isPreGame) {
                    activity.State = (isHost) ? "In Lobby (Host)" : "In Lobby";
                    activity.Type = ActivityType.Playing;
                }
                else if (isSpectator) {
                    activity.State = "Spectating";
                    activity.Type = ActivityType.Watching;
                }
                else {
                    activity.State = (isHost) ? "In Game (Host)" : "In Game";
                    activity.Type = ActivityType.Playing;
                }
                activity.Party.Size.CurrentSize = playerCount;
                activity.Party.Size.MaxSize = 8;
                activity.Secrets.Join = Convert.ToBase64String(game.Id.ToByteArray());
                activity.Timestamps.Start = (long)(game.DateCreated.UtcDateTime - _epoch).TotalSeconds;
            }
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
                }

                _discord?.Dispose();

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
