namespace Octgn.Data.Database.Entities
{
    using FluentNHibernate.Mapping;

    using NHibernate.Type;

    public class CustomPropertyMap : ClassMap<CustomProperty>
    {
        public CustomPropertyMap()
        {
            this.Table("custom_properties");
            this.Id(x => x.Real_Id);
            this.Map(x => x.Id).CustomType<GuidType>().Unique();
            this.References<Entities.Card>(x => x.Card_Real_Id);
            this.Map(x => x.Game_Id);
            this.Map(x => x.Name);
            this.Map(x => x.Type);
            this.Map(x => x.VInt).Nullable();
            this.Map(x => x.VStr).Nullable();
        }
    }
}
