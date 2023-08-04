namespace Octgn.DataNew.Entities
{
    using System;

    public enum PropertyType : byte
    {
        String,
        Integer,
        RichText,
        GUID,
        Char
    };

    public enum PropertyTextKind : byte
    {
        FreeText,
        Enumeration,
        Tokens
    };

    public class PropertyDef : ICloneable
    {
        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public bool IgnoreText { get; set; }
        public bool Hidden { get; set; }
        public PropertyTextKind TextKind { get; set; }
        public string Delimiter { get; set; }

        public override string ToString()
        {
            return Name;
        }
        public object Clone()
        {
            var newprop = new PropertyDef()
            {
                Hidden = this.Hidden,
                Name = this.Name.Clone() as string,
                Type = this.Type,
                TextKind = this.TextKind,
                IgnoreText = this.IgnoreText,
                Delimiter = this.Delimiter.Clone() as string,
            };
            return newprop;
        }
    }

    public class NamePropertyDef : PropertyDef
    {
        public NamePropertyDef()
        {
            base.Name = "Name";
            base.Type = 0;
        }
    }

}