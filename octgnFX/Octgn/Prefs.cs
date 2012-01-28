using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn
{
    public class Prefs
    {
        private static string _hideLoginNotifications;

        public string HideLoginNotifications
        {
            get { return _hideLoginNotifications; }
            set 
            { 
                _hideLoginNotifications = value; 
                Registry.WriteValue("Options_HideLoginNotifications",value); 
            }
        }

        public Prefs()
        {
           string _hln = Registry.ReadValue("Options_HideLoginNotifications");
            _hideLoginNotifications = _hln == null || _hln == "false" ? "false" : "true";
        }

    }
}
