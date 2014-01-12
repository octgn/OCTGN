namespace Octgn.Core.Play
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows.Media;

    public class GameMessageDispatcher : INotifyPropertyChanged
    {
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly List<IGameMessage> messages;

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

        public void GameAction(string message, params object[] args)
        {
            AddMessage(new GameActionMessage(message, args));
        }

        public void GameDebug(string message, params object[] args)
        {
            AddMessage(new DebugMessage(message, args));
        }

        public void AddMessage(IGameMessage message)
        {
            try
            {
				locker.EnterWriteLock();
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

    public class GameMessage : IGameMessage
    {
        public DateTime Timestamp { get; private set; }
        public IPlayPlayer From { get; private set; }
        public string Message { get; private set; }
        public object[] Arguments { get; private set; }

        public GameMessage(IPlayPlayer from, string message, params object[] args)
        {
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
    }

    public class ChatMessage : GameMessage
    {
        public ChatMessage(IPlayPlayer @from, string message, params object[] args)
            : base(@from, message, args)
        {
        }
    }

    public class WarningMessage : GameMessage
    {
        public WarningMessage(string message, params object[] args)
			:base(BuiltInPlayer.Warning,message, args)
        {
            
        }
    }

    public class SystemMessage : GameMessage
    {
        public SystemMessage(string message, params object[] args)
			:base(BuiltInPlayer.System,message, args)
        {
            
        }
    }

    public class GameActionMessage : GameMessage
    {
        public GameActionMessage(string message, params object[] args)
			:base(BuiltInPlayer.GameAction,message, args)
        {
            
        }
    }

    public class DebugMessage : GameMessage
    {
        public DebugMessage(string message, params object[] args)
			:base(BuiltInPlayer.Debug,message, args)
        {
            
        }
    }

    public interface IGameMessage
    {
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

        private static readonly IPlayPlayer gameActionPlayer = new BuiltInPlayer
                   {
                       Color = Colors.Bisque,
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

        public static IPlayPlayer Warning { get { return warningPlayer; } }
        public static IPlayPlayer System { get { return systemPlayer; } }
        public static IPlayPlayer GameAction { get { return gameActionPlayer; } }
        public static IPlayPlayer Debug { get { return debugPlayer; } }
    }
}