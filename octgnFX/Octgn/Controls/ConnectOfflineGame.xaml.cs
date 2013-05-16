namespace Octgn.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using Octgn.Core.DataManagers;
    using Octgn.Networking;
    using Octgn.ViewModels;

    using UserControl = System.Windows.Controls.UserControl;

    public partial class ConnectOfflineGame : UserControl,IDisposable
    {
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            var handler = this.OnClose;
            if (handler != null)
            {
                handler(sender, result);
            }
        }
        public static DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(String), typeof(ConnectOfflineGame));
        public bool HasErrors { get; private set; }
        private Decorator Placeholder;
        public string Error
        {
            get { return this.GetValue(ErrorProperty) as String; }
            private set { this.SetValue(ErrorProperty, value); }
        }

        public bool Successful { get; private set; }
        public DataNew.Entities.Game Game { get; private set; }

        public ObservableCollection<DataGameViewModel> Games { get; private set; }
        public ConnectOfflineGame()
        {
            InitializeComponent();
            Program.Dispatcher = WindowManager.Main.Dispatcher;
            Games = new ObservableCollection<DataGameViewModel>();
        }

        void RefreshInstalledGameList()
        {
            if (Games == null)
                Games = new ObservableCollection<DataGameViewModel>();
            var list = GameManager.Get().Games.Select(x => new DataGameViewModel(x)).ToList();
            Games.Clear();
            foreach (var l in list)
                Games.Add(l);
        }

        void Connect(DataGameViewModel game, string userhost, string userport)
        {
            Successful = false;
            IPAddress host = null;
            int port = -1;
            this.ValidateFields(game, userhost,userport,out host, out port);

            Program.Client = new Client(host,port);
            Program.Client.Connect();
            Successful = true;
        }

        void ConnectDone(Task task)
        {
            this.ProgressBar.Visibility = Visibility.Hidden;
            this.IsEnabled = true;
            if (task.IsFaulted || Successful == false)
            {
                if (task.Exception == null) Error = "Unknown error";
                else
                {
                    var valerror = task.Exception.InnerExceptions.OfType<ArgumentException>().FirstOrDefault();
                    if (valerror != null) this.Error = valerror.Message;
                    else if(task.Exception.InnerExceptions.Count > 0)
                    {
                        this.Error = "Could not connect: " + task.Exception.InnerExceptions.FirstOrDefault().Message;
                    }
                    else
                    {
                        this.Error = "Unknown Error";
                    }
                }
                Successful = false;
                return;
            }
            this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            this.Close(DialogResult.OK);
        }

        void ValidateFields(DataGameViewModel game, string host, string port, out IPAddress ip, out int conPort)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("You must enter a host name/ip");
            if (string.IsNullOrWhiteSpace(port)) throw new ArgumentException("You must enter a port number.");

            if(game == null)
                throw new ArgumentException("You must select a game");

            // validate host/ip
            try
            {
                ip = Dns.GetHostAddresses(host)
                    .First(x => x.AddressFamily == AddressFamily.InterNetwork);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new ArgumentException("Ip/Host name is invalid, or unreachable");
            }

            conPort = -1;

            if (!int.TryParse(port, out conPort))
                throw new ArgumentException("Port number is invalid");
            if (conPort <= 0 || conPort >= Int16.MaxValue)
                throw new ArgumentException("Port number is invalid");
        }

        #region Dialog
        public void Show(Decorator placeholder)
        {
            Placeholder = placeholder;
            RefreshInstalledGameList();
            placeholder.Child = this;
        }
        private void Close(DialogResult result)
        {
            Placeholder.Child = null;
            this.FireOnClose(this,result);
        }
        public void Close()
        {
            Close(DialogResult.Abort);
        }
        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private void ButtonConnectClick(object sender, RoutedEventArgs e)
        {
            Error = "";
            var strHost = TextBoxHostName.Text;
            var strPort = TextBoxPort.Text;
            var game = ComboBoxGame.SelectedItem as DataGameViewModel;
            var task = new Task(()=>this.Connect(game,strHost,strPort));
            this.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            task.ContinueWith(new Action<Task>((t) => this.Dispatcher.Invoke(new Action(() => this.ConnectDone(t)))));
            task.Start();
        }
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object, DialogResult>)d;
                }
            }
        }

        #endregion
    }
}
