using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace Octgn.DataNew.Entities
{
    public class PropertyDefValue: ICloneable
    {
        public object Value { get; set; }
        
        public object Clone()
        {
            var ret = new PropertyDefValue() { Value = this.Value };
            return ret;
        }

        public override string ToString()
        {
            if (Value is XElement)
                return ((XElement)Value).Value;
            return Value.ToString();
        }
        
    }
}