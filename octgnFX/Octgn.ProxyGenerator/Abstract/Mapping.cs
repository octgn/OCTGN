using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator.Abstract
{
    public class Mapping
    {
        public virtual string Name { get; set; }
        public virtual string MapTo { get; set; }

        public override bool Equals(object obj)
        {
            Mapping other = (Mapping)obj;
            if (this.Name == other.Name && this.MapTo == other.MapTo)
            {
                return true;
            }
            return base.Equals(obj);
        }
    }
}
