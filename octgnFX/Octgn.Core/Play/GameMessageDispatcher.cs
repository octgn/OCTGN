namespace Octgn.Core.Play
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Timers;
    using System.Windows.Media;

    public class GameMessageDispatcher : INotifyPropertyChanged
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly List<IGameMessage> messages;
        private Func<IGameMessage, IGameMessage> messageAction;

        public IList<IGameMessage> Messages
        {
            get
            {
                try
                {
					locker.EnterReadLock();
                    return messages.ToList();
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public GameMessageDispatcher()
        {
            this.messages = new List<IGameMessage>();
        }

        public void ProcessMessage(Func<IGameMessage, IGameMessage> messageActionParam)
        {
            this.messageAction = messageActionParam;
        }

        public List<T> MessageOfType<T>() where T : IGameMessage
        {
            return Messages.OfType<T>().ToList();
        }

        public void PlayerEvent(IPlayPlayer player, string message, params object[] args)
        {
            AddMessage(new PlayerEventMessage(player, message, args));
        }

        public void Chat(IPlayPlayer player, string message)
        {
            AddMessage(new ChatMessage(player, message));
        }

        public void Warning(string message, params object[] args)
        {
            AddMessage(new WarningMessage(message, args));
        }

        public void System(string message, params object[] args)
        {
            AddMessage(new SystemMessage(message, args));
        }

        public void Turn(IPlayPlayer turnPlayer, int turnNumber)
        {
            AddMessage(new TurnMessage(turnPlayer,turnNumber));
        }

        public void GameDebug(string message, params object[] args)
        {
            AddMessage(new DebugMessage(message, args));
        }

        public void Notify(string message, params object[] args)
        {
            AddMessage(new NotifyMessage(message, args));
        }

        public void NotifyBar(Color color, string message, params object[] args)
        {
            AddMessage(new NotifyBarMessage(color, message, args));
        }

        public void AddMessage(IGameMessage message)
        {
            try
            {
				locker.EnterWriteLock();
				if(messageAction != null)
					message = messageAction(message);
                messages.Add(message);
                OnPropertyChanged("Messages");
            }
            finally
            {
				locker.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                locker.EnterWriteLock();
				messages.Clear();
                OnPropertyChanged("Messages");
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class GameMessageDispatcherReader : IDisposable
    {
        private System.Timers.Timer _chatTimer;
        private readonly GameMessageDispatcher _dispatcher;
        private long lastId = -1;
        private Action<IGameMessage[]> onMessages;

        public GameMessageDispatcherReader(GameMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }


        public void Start(Action<IGameMessage[]> handler)
        {
            onMessages = handler;
            _chatTimer = new System.Timers.Timer(100);
            _chatTimer.Enabled = true;
            _chatTimer.Elapsed += OnTick;
        }

        public void Stop()
        {
            onMessages = null;
            _chatTimer.Enabled = false;
            _chatTimer.Elapsed -= OnTick;
            _chatTimer.Dispose();
            _chatTimer = null;
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                if (_chatTimer.Enabled == false) return;
                _chatTimer.Enabled = false;
            }

            try
            {
                var newMessages = _dispatcher.Messages.OrderBy(x => x.Id).Where(x => x.Id > lastId).ToArray();
                if (newMessages.Length == 0)
                {
                    return;
                }

                if (onMessages == null) return;

                lastId = newMessages.Last().Id;

				onMessages.Invoke(newMessages);

            }
            finally
            {
                if (_chatTimer != null)
                {
                    _chatTimer.Enabled = true;
                }
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }

    public abstract class GameMessage : IGameMessage
    {
        public bool IsMuted { get; private set; }
        public abstract bool CanMute { get; }
        public long Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IPlayPlayer From { get; private set; }
        public string Message { get; private set; }
        public object[] Arguments { get; private set; }

        private bool isClientMuted = false;

        private static long currentId = 0;
        private static readonly object cidLock = new object();

        public static Func<bool> MuteChecker { get; set; }

        static GameMessage()
        {
            MuteChecker = () => false;
        }

        protected GameMessage(IPlayPlayer from, string message, params object[] args)
        {
            lock (cidLock)
            {
                currentId++;
                Id = currentId;
            }
            isClientMuted = MuteChecker();
            IsMuted = CanMute && isClientMuted;
            Timestamp = DateTime.Now;
            From = from;
            Message = message;
            Arguments = args ?? new object[0];
        }
    }

    public class PlayerEventMessage : GameMessage
    {
        public PlayerEventMessage(IPlayPlayer @from, string message, params object[] args)
            : base(@from, message, args)
        {
        }

        public override bool CanMute{get{return true;}}
    }

    public class ChatMessage : GameMessage
    {
        public override bool CanMute { get { return false; } }
        public ChatMessage(IPlayPlayer @from, string message, params object[] args)
            : base(@from, message, args)
        {
        }
    }

    public class WarningMessage : GameMessage
    {
        public override bool CanMute { get { return false; } }
        public WarningMessage(string message, params object[] args)
			:base(BuiltInPlayer.Warning,message, args)
        {
            
        }
    }

    public class SystemMessage : GameMessage
    {
        public override bool CanMute { get { return false; } }
        public SystemMessage(string message, params object[] args)
			:base(BuiltInPlayer.System,message, args)
        {
            
        }
    }

    public class NotifyMessage : GameMessage
    {
        public override bool CanMute { get { return false; } }
        public NotifyMessage(string message, params object[] args)
            : base(BuiltInPlayer.Notify, message, args)
        {

        }
    }

    public class TurnMessage : GameMessage
    {
        public override bool CanMute { get { return false;} }
        public int TurnNumber { get; private set; }
        public IPlayPlayer TurnPlayer { get; set; }
        public TurnMessage(IPlayPlayer turnPlayer, int turnNum)
			:base(BuiltInPlayer.Turn,"Turn {0}: ", new object[]{turnNum})
        {
            TurnNumber = turnNum;
            TurnPlayer = turnPlayer;
        }
    }

    public class DebugMessage : GameMessage
    {
        public override bool CanMute { get { return false; } }
        public DebugMessage(string message, params object[] args)
			:base(BuiltInPlayer.Debug,message, args)
        {
            
        }
    }

    public class NotifyBarMessage : GameMessage
    {
        public override bool CanMute { get { return false; } }
        public Color MessageColor { get; private set; }
        public NotifyBarMessage(Color messageColor, string message, params object[] args)
			:base(BuiltInPlayer.NotifyBar,message, args)
        {
            MessageColor = messageColor;
        }
    }

    public interface IGameMessage
    {
        bool IsMuted { get; }
        bool CanMute { get; }
        long Id { get; }
        DateTime Timestamp { get; }
        IPlayPlayer From { get; }
        string Message { get; }
        object[] Arguments { get; }
    }

    public class BuiltInPlayer : IPlayPlayer
    {
        public byte Id { get; private set; }
        public string Name { get; private set; }
        public Color Color { get; private set; }
        public PlayerState State { get; private set; }

        private static readonly IPlayPlayer warningPlayer = new BuiltInPlayer
                                                            {
                                                                Color = Colors.Crimson,
                                                                Name = "Warning",
                                                                Id = 254,
                                                                State = PlayerState.Connected
                                                            };

        private static readonly IPlayPlayer systemPlayer = new BuiltInPlayer
                                                            {
                                                                Color = Colors.BlueViolet,
                                                                Name = "System",
                                                                Id = 253,
                                                                State = PlayerState.Connected
                                                            };

        private static readonly IPlayPlayer turnPlayer = new BuiltInPlayer
                   {
                       Color = Color.FromRgb(0x5A, 0x9A, 0xCF),
                       Name = "",
                       Id = 252,
                       State = PlayerState.Connected
                   };
        private static readonly IPlayPlayer debugPlayer = new BuiltInPlayer
                   {
                       Color = Colors.LightGray,
                       Name = "DEBUG",
                       Id = 250,
                       State = PlayerState.Connected
                   };
        private static readonly IPlayPlayer notifyPlayer = new BuiltInPlayer
                   {
                       Color = Colors.DimGray,
                       Name = "",
                       Id = 251,
                       State = PlayerState.Connected
                   };
        private static readonly IPlayPlayer notifyBarPlayer = new BuiltInPlayer
                   {
                       Color = Colors.Black,
                       Name = "",
                       Id = 251,
                       State = PlayerState.Connected
                   };

        public static IPlayPlayer Warning { get { return warningPlayer; } }
        public static IPlayPlayer System { get { return systemPlayer; } }
        public static IPlayPlayer Turn { get { return turnPlayer; } }
        public static IPlayPlayer Debug { get { return debugPlayer; } }
        public static IPlayPlayer Notify{ get { return notifyPlayer; } }
        public static IPlayPlayer NotifyBar{ get { return notifyBarPlayer; } }
    }
}