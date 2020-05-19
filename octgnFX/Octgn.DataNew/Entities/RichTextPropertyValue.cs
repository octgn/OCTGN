using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Octgn.DataNew.Entities
{
    public class RichTextPropertyValue: ICloneable
    {
        public RichSpan Value { get; set; }
                
        public object Clone()
        {
            var ret = new RichTextPropertyValue() { Value = this.Value };
            return ret;
        }

        public override string ToString()
        {
            return Value?.ToString();
        }
        
    }

    public enum RichSpanType
    {
        None,
        Color,
        Bold,
        Italic,
        Underline,
        Symbol
    }

    public class RichSpan
    {
        public RichSpanType Type { get; set; }
        public List<RichSpan> Items { get; set; }

        public RichSpan()
        {
            Items = new List<RichSpan>();
            Type = RichSpanType.None;
        }

        public override string ToString()
        {
            return string.Join("", Items.Select(x => x.ToString()));
        }
    }
    
    public class RichSymbol : RichSpan
    {
        public Symbol Attribute { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class RichColor : RichSpan
    {
        public string Attribute { get; set; }
    }


    public class RichText : RichSpan
    {
        public object Text { get; set; }
        public override string ToString()
        {
            return Text.ToString();
        }
    }
}