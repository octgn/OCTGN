using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Windows
{
    /// <summary>
    /// Interaction logic for ReconnectingWindow.xaml
    /// </summary>
    public partial class ReconnectingWindow : Window
    {
        public bool Canceled { get; private set; }
        public bool Connected { get; private set; }
        public Timer ReconnectTimer;
        private agsXMPP.XmppConnectionState cState;
        public ReconnectingWindow()
        {
            InitializeComponent();
            Canceled = true;
            Program.LobbyClient.OnLoginComplete += LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
            Program.LobbyClient.OnStateChanged += LobbyClientOnOnStateChanged;
            ReconnectTimer = new Timer(TCall,null,0,10000);
            cState = XmppConnectionState.Disconnected;
            Connected = false;
        }
        private void TCall(object o)
        {
            if (Connected) return;
            switch(cState)
            {
                case XmppConnectionState.Disconnected:
                    Program.LobbyClient.BeginReconnect();
                    break;
                case XmppConnectionState.Connecting:
                    Program.LobbyClient.BeginReconnect();
                    break;
                case XmppConnectionState.Connected:
                    Program.LobbyClient.BeginReconnect();
                    break;
                case XmppConnectionState.Authenticating:
                    break;
                case XmppConnectionState.Authenticated:
                    break;
                case XmppConnectionState.Binding:
                    break;
                case XmppConnectionState.Binded:
                    break;
                case XmppConnectionState.StartSession:
                    break;
                case XmppConnectionState.StartCompression:
                    break;
                case XmppConnectionState.Compressed:
                    break;
                case XmppConnectionState.SessionStarted:
                    break;
                case XmppConnectionState.Securing:
                    break;
                case XmppConnectionState.Registering:
                    break;
                case XmppConnectionState.Registered:
                    break;
            }
        }
        private void LobbyClientOnOnStateChanged(object sender , string state)
        {
            Enum.TryParse(state , true , out cState);
            Dispatcher.Invoke(new Action(() =>
                                         {
                                             textBlock1.Text =  state;
                                         }));
        }

        private void LobbyClientOnOnDisconnect(object sender , EventArgs eventArgs)
        {
            //Program.LobbyClient.BeginReconnect();
        }

        private void LobbyClientOnOnLoginComplete(object sender , Client.LoginResults results)
        {
            switch (results)
            {
                case Client.LoginResults.Success:
                    Canceled = false;
                    ReconnectTimer.Dispose();
                    break;
                case Client.LoginResults.Failure:
                    Canceled = true;
                    ReconnectTimer.Dispose();
                    break;
            }
            Dispatcher.Invoke(new Action(() => 
            { 
                switch(results)
                {
                    case Client.LoginResults.Success:
                        if (Program.LobbyClient.DisconnectedBecauseConnectionReplaced) Canceled = false;
                        Connected = true;
                        this.Close();
                        break;
                    case Client.LoginResults.Failure:
                        this.Close();
                        break;
                }
            }));
        }

        public static ReconnectingWindow Reconnect()
        {
            var win = new ReconnectingWindow();
            win.ShowDialog();
            return win;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ReconnectTimer.Dispose();
            ReconnectTimer = null;
            Program.LobbyClient.OnLoginComplete -= LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnOnDisconnect;
            Program.LobbyClient.OnStateChanged -= LobbyClientOnOnStateChanged;
        }

        private void button1_Click(object sender, RoutedEventArgs e) 
        { 
            Canceled = true;
            Close();
        }
    }
}
