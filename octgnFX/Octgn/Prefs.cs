using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn
{
    public static class Prefs
    {
        private static string _hideLoginNotifications;

        public static string HideLoginNotifications
        {
            get { return _hideLoginNotifications; }
            set
            {
                _hideLoginNotifications = value;
                Registry.WriteValue("Options_HideLoginNotifications", value);
            }
        }

        static Prefs()
        {
            string _hln = Registry.ReadValue("Options_HideLoginNotifications");
            _hideLoginNotifications = _hln == null || _hln == "false" ? "false" : "true";
        }

    }
}
