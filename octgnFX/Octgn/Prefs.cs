namespace Octgn
{
    public static class Prefs
    {
        private static string _hideLoginNotifications;

        static Prefs()
        {
            string hln = SimpleConfig.ReadValue("Options_HideLoginNotifications");
            _hideLoginNotifications = hln == null || hln == "false" ? "false" : "true";
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