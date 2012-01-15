using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;


namespace CORSHeaderPlugin
{
    public class Plugin
    {
        public NameValueCollection AddResponseHeaders()
        {
            return new NameValueCollection()
                       {
                           {"Access-Control-Allow-Origin","*"},
                           {"Access-Control-Allow-Methods","POST, GET, OPTIONS"},
                           {"Access-Control-Allow-Headers","X-Requested-With, Content-Type"},
                           {"Access-Control-Max-Age","1728000"},
                       };
        }
    }
}
