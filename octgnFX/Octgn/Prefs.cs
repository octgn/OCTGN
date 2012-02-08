namespace Octgn
{
    public static class Prefs
    {
        private static string _hideLoginNotifications;

        static Prefs()
        {
            string _hln = SimpleConfig.ReadValue("Options_HideLoginNotifications");
            _hideLoginNotifications = _hln == null || _hln == "false" ? "false" : "true";
        }

        public static string HideLoginNotifications
        {
            get { return _hideLoginNotifications; }
            set
            {
                _hideLoginNotifications = value;
                SimpleConfig.WriteValue("Options_HideLoginNotifications", value);
            }
        }
    }
}