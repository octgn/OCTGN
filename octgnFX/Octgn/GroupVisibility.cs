using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Octgn
{
    [TypeConverter(typeof (GroupVisibilityConverter))]
    public enum GroupVisibility : byte
    {
        [XmlEnum("none")] Nobody,
        [XmlEnum("me")] Owner,
        [XmlEnum("all")] Everybody,
        [XmlEnum("undefined")] Undefined,
        Custom /* not used by definitions. In game indicates a list of players could see this card */
    };

    internal class GroupVisibilityConverter : EnumConverter
    {
        public GroupVisibilityConverter()
            : base(typeof (GroupVisibility))
        {
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value as string)
            {
                case "none":
                    return GroupVisibility.Nobody;
                case "me":
                    return GroupVisibility.Owner;
                case "all":
                    return GroupVisibility.Everybody;
                case "undefined":
                    return GroupVisibility.Undefined;
                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }
    }
}