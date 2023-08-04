using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Octgn.DataNew.Entities
{
    public interface IRichText
    {
        List<IRichText> Items { get; set; }
        string ToLiteralString();
    }

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
        
        public string ToLiteralString()
        {
            return Value?.ToLiteralString();
        }
    }
    
    public class RichSpan : IRichText
    {
        public List<IRichText> Items { get; set; }

        public RichSpan()
        {
            Items = new List<IRichText>();
        }
        public override string ToString()
        {
            return string.Join("", Items.Select(x => x.ToString()));
        }
        public string ToLiteralString()
        {
            return string.Join("", Items.Select(x => x.ToLiteralString()));
        }
    }

    public class RichBold : IRichText
    {
        public List<IRichText> Items { get; set; }

        public RichBold()
        {
            Items = new List<IRichText>();
        }

        public override string ToString()
        {
            return string.Join("", Items.Select(x => x.ToString()));
        }
        public string ToLiteralString()
        {
            return "<b>" + string.Join("", Items.Select(x => x.ToLiteralString())) + "</b>";
        }
    }
    public class RichItalic : IRichText
    {
        public List<IRichText> Items { get; set; }

        public RichItalic()
        {
            Items = new List<IRichText>();
        }

        public override string ToString()
        {
            return string.Join("", Items.Select(x => x.ToString()));
        }
        public string ToLiteralString()
        {
            return "<i>" + string.Join("", Items.Select(x => x.ToLiteralString())) + "</i>";
        }
    }
    public class RichUnderline : IRichText
    {
        public List<IRichText> Items { get; set; }

        public RichUnderline()
        {
            Items = new List<IRichText>();
        }

        public override string ToString()
        {
            return string.Join("", Items.Select(x => x.ToString()));
        }
        public string ToLiteralString()
        {
            return "<u>" + string.Join("", Items.Select(x => x.ToLiteralString())) + "</u>";
        }
    }
    
    public class RichSymbol : IRichText
    {
        public List<IRichText> Items { get; set; }
        public Symbol Attribute { get; set; }
        public string Text { get; set; }

        public RichSymbol()
        {
            Items = new List<IRichText>();
        }

        public override string ToString()
        {
            return Text;
        }
        public string ToLiteralString()
        {
            return "<s value=\"" + Attribute.Id + "\">" + Text + "</s>";
        }
    }

    public class RichColor : IRichText
    {
        public List<IRichText> Items { get; set; }
        public string Attribute { get; set; }
        public RichColor()
        {
            Items = new List<IRichText>();
        }
        public override string ToString()
        {
            return string.Join("", Items.Select(x => x.ToString()));
        }
        public string ToLiteralString()
        {
            return "<c value=\"" + Attribute + "\">" + string.Join("", Items.Select(x => x.ToLiteralString())) + "</c>";
        }
    }


    public class RichText : IRichText
    {
        public List<IRichText> Items { get; set; }
        public string Text { get; set; }
        public RichText()
        {
            Items = new List<IRichText>();
        }
        public override string ToString()
        {
            return Text;
        }

        public string ToLiteralString()
        {
            return Text;
        }
    }
}