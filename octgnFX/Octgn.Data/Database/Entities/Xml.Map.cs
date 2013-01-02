namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    public class XmlMap : ClassMap<Xml>
    {
        public XmlMap()
        {
            this.Table("xml");
            this.Id(x => x.Id);
            this.Map(x => x.Xml_Link);
            this.Map(x => x.Gid);
            this.Map(x => x.Old_Xml).Nullable();
        }
    }
}
