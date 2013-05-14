namespace Octgn.DataNew.Entities
{
    using System;

    public enum PropertyType : byte
    {
        String,
        Integer,
        GUID,
        Char
    };

    public enum PropertyTextKind : byte
    {
        FreeText,
        Enumeration,
        Tokens
    };

    public class PropertyDef : IEquatable<PropertyDef>,IComparable<PropertyDef>
    {
        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public bool IgnoreText { get; set; }
        public bool Hidden { get; set; }
        public bool IsUndefined { get; set; }
        public PropertyTextKind TextKind { get; set; }

        public bool Equals(PropertyDef other)
        {
            return Name == other.Name;
        }

        public int CompareTo(PropertyDef other)
        {
            return System.String.Compare(this.Name, other.Name, System.StringComparison.Ordinal);
        }
        public override bool Equals(object obj)
        {
            var other = obj as PropertyDef;
            if (other == null) return false;
            return other.Type == Type && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }


}