namespace Octgn.Data.Entities
{
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

    public class PropertyDef
    {
        public string Name { get; private set; }
        public PropertyType Type { get; private set; }
        public bool IgnoreText { get; private set; }
        public bool Hidden { get; private set; }
        public PropertyTextKind TextKind { get; private set; }
    }


}