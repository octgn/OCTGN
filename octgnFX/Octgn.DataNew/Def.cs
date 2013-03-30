namespace Octgn.DataNew
{
    using System;
    using System.ComponentModel;
    using System.Xml.Linq;

    // TODO: it would be nice to make this internal
    // This means bringing all the GameDef classes in this assembly as well.
    // While this is nice, too, it is a bit complicated to seperate them completely from the Game engine and model.
    public static class Defs
    {
        public static readonly XNamespace XmlnsOctgn = "";

        public static T Attr<T>(this XElement xml, string name)
        {
            XAttribute attr = xml.Attribute(XmlnsOctgn + name);
            if (attr == null) return default(T);
            // HACK: Strangely, .NET 3.5 SP1 doesn't support conversion from string to Version by its TypeConverter
            if (typeof (T) == typeof (Version)) return (T) (object) new Version(attr.Value);
            return (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromInvariantString(attr.Value);
        }

        public static T Attr<T>(this XElement xml, string name, T defaultValue)
        {
            XAttribute attr = xml.Attribute(XmlnsOctgn + name);
            if (attr == null) return defaultValue;
            return (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromInvariantString(attr.Value);
        }

        public static XElement Child(this XElement xml, string name)
        {
            return xml.Element(XmlnsOctgn + name);
        }
    }

    public class InvalidFormatException : Exception
    {
        public InvalidFormatException(string message)
            : base(message)
        {
        }
    }
}