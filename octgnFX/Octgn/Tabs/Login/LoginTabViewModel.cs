using GalaSoft.MvvmLight;
using log4net;
using Octgn.Core;
using Octgn.Extentions;
using Octgn.Site.Api;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Tabs.Login
{
    public class LoginTabViewModel : ViewModelBase, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private AutoResetEvent _xmppLoginEvent;
        private Skylabs.Lobby.LoginResults _xmppLoginResult = Skylabs.Lobby.LoginResults.Failure;

        private bool _isBusy;
        private string _username;
        private string _password;
        private string _errorString;
        private bool _loggedIn;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy) return;
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
                RaisePropertyChanged(() => NotBusy);
            }
        }

        public bool NotBusy { get { return !_isBusy; } }

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username == value) return;
                _username = value;
                RaisePropertyChanged(() => Username);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value) return;
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        public string ErrorString
        {
            get { return _errorString; }
            set
            {
                if (_errorString == value) return;
                _errorString = value;
                RaisePropertyChanged(() => ErrorString);
                RaisePropertyChanged(() => HasError);
            }
        }

        public bool HasError
        {
            get { return String.IsNullOrWhiteSpace(ErrorString) == false; }
        }

        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                if (_loggedIn == value) return;
                _loggedIn = value;
                RaisePropertyChanged(()=>LoggedIn);
            }
        }

        public LoginTabViewModel()
        {
            _xmppLoginEvent = new AutoResetEvent(false);
            Password = Prefs.Password != null ? Prefs.Password.Decrypt() : null;
            Username = Prefs.Username;
        }

        public void LoginAsync()
        {
            Task.Factory.StartNew(Login);
        }

        public void Login()
        {
            try
            {
                lock (this)
                {
                    if (IsBusy) return;
                    IsBusy = true;
                }
                ErrorString = "";

                var websiteLoginResult = LoginWithWebsite();
                if (websiteLoginResult?.Type != LoginResultType.Ok) return;

                //The rest of the application and the api uses Username as a key. To avoid changing the rest of the application,
                //     I use this to get the username back from the server instead of using the one the user typed in.
                //     This is just in case we logged in using the users email instead of their username.
                Username = websiteLoginResult.Username;

                if (!LoginWithOCTGNChat().Wait(Prefs.LoginTimeout)) return;
                if (Prefs.Username == null || Prefs.Username.Equals(Username, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    // Logging in with a new username, so clear admin flag
                    Prefs.IsAdmin = false;
                }
                Prefs.Username = Username;
                Prefs.Nickname = Username;
                Prefs.Password = Password.Encrypt();
                LoggedIn = true;
            }
            catch (Exception e)
            {
                Log.Error("Login", e);
                ErrorString = "An unknown error occured. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Site.Api.LoginResult LoginWithWebsite()
        {
            Log.Info("LoginWithWebsite");
            var client = new ApiClient();
            var ret = client.Login(Username, Password);
            if( ret == null ) return null;

            Log.Info("LoginWithWebsite=" + ret.Type + "-" + ret.Username);
            switch (ret.Type)
            {
                case Site.Api.LoginResultType.Ok:
                    break;
                case Site.Api.LoginResultType.EmailUnverified:
                    ErrorString = "Your e-mail hasn't been verified. Please check your e-mail. If you haven't received one, you can contact us as support@octgn.net for help.";
                    break;
                case Site.Api.LoginResultType.UnknownUsername:
                    ErrorString = "The username/e-mail you entered doesn't exist.";
                    break;
                case Site.Api.LoginResultType.PasswordWrong:
                    ErrorString = "The password you entered is incorrect.";
                    break;
                case Site.Api.LoginResultType.NotSubscribed:
                    ErrorString = "You are required to subscribe on our site in order to play online.";
                    break;
                case Site.Api.LoginResultType.NoEmailAssociated:
                    ErrorString = "You do not have an email associated with your account. Please visit your account page at OCTGN.net to associate an email address.";
                    break;
                default:
                    ErrorString = "An unknown error occured. Please try again.";
                    break;
            }
            return ret;
        }

        private async Task<bool> LoginWithOCTGNChat()
        {
            Log.Info(nameof(LoginWithOCTGNChat));

            var result = await Program.LobbyClient.Connect(Username, Password);
            switch (result)
            {
                case Chat.Communication.Messages.Login.LoginResultType.Ok:
                    return true;
                    break;
                case Chat.Communication.Messages.Login.LoginResultType.EmailUnverified:
                    ErrorString = "Your e-mail hasn't been verified. Please check your e-mail. If you haven't received one, you can contact us as support@octgn.net for help.";
                    break;
                case Chat.Communication.Messages.Login.LoginResultType.UnknownUsername:
                    ErrorString = "The username/e-mail you entered doesn't exist.";
                    break;
                case Chat.Communication.Messages.Login.LoginResultType.PasswordWrong:
                    ErrorString = "The password you entered is incorrect.";
                    break;
                case Chat.Communication.Messages.Login.LoginResultType.NotSubscribed:
                    ErrorString = "You are required to subscribe on our site in order to play online.";
                    break;
                case Chat.Communication.Messages.Login.LoginResultType.NoEmailAssociated:
                    ErrorString = "You do not have an email associated with your account. Please visit your account page at OCTGN.net to associate an email address.";
                    break;
                default:
                    ErrorString = "An unknown error occured. Please try again.";
                    break;
            }

            return false;
        }

        private void LobbyClient_OnLoginComplete(object sender, Skylabs.Lobby.LoginResults results)
        {
            Log.InfoFormat("Lobby Login Complete {0}", results);
            _xmppLoginResult = results;
            _xmppLoginEvent.Set();
        }

        public void Dispose()
        {
        }
    }
}
