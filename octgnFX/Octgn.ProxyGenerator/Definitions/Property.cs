using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator.Definitions
{
    public class Property
    {
        private string format = "{}";

        public string Format
        {
            get { return format; }
            set { format = value; }
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
