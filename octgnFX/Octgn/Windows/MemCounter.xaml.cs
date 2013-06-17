using System.Windows;

namespace Octgn.Windows
{
    using System;
    using System.ComponentModel;

    using Octgn.Annotations;

    /// <summary>
    /// Interaction logic for MemCounter.xaml
    /// </summary>
    public partial class MemCounter : Window, INotifyPropertyChanged
    {
        #region Singleton

        internal static MemCounter SingletonContext { get; set; }

        private static readonly object MemCounterSingletonLocker = new object();

        private int created;

        private int disposed;

        private int inUse;

        public static MemCounter Get()
        {
            lock (MemCounterSingletonLocker)
            {
                if (SingletonContext == null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            SingletonContext = new MemCounter();
#if(DEBUG)
                            SingletonContext.Show();
#endif
                        }));
                    return SingletonContext;
                }
                return SingletonContext;
            }
        }

        internal MemCounter()
        {
            this.InitializeComponent();
        }

        #endregion Singleton

        public int Created
        {
            get
            {
                return this.created;
            }
            set
            {
                if (value == this.created)
                {
                    return;
                }
                this.created = value;
                this.OnPropertyChanged("Created");
                this.OnPropertyChanged("NotDisposed");
            }
        }

        public int Disposed
        {
            get
            {
                return this.disposed;
            }
            set
            {
                if (value == this.disposed)
                {
                    return;
                }
                this.disposed = value;
                this.OnPropertyChanged("Disposed");
                this.OnPropertyChanged("NotDisposed");
            }
        }

        public int NotDisposed
        {
            get
            {
                return Created - Disposed;
            }
        }

        public int InUse
        {
            get
            {
                return this.inUse;
            }
            set
            {
                if (value == this.inUse)
                {
                    return;
                }
                this.inUse = value;
                this.OnPropertyChanged("InUse");
            }
        }

        public void AddCreated()
        {
#if(DEBUG)
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this. AddCreated));
                return;
            }
            Created++;
#endif
        }

        public void AddDisposed()
        {
#if(DEBUG)
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this. AddDisposed));
                return;
            }
            Disposed++;
#endif
        }

        public void SetInUse(int count)
        {
#if(DEBUG)
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(()=>this.SetInUse(count)));
                return;
            }

            InUse = count;
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
