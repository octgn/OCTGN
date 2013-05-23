namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ICard
    {
        Guid Id { get; }
        Guid SetId { get; }
        string Name { get; }
        string ImageUri { get; }
        string Alternate { get; }
        IDictionary<string , CardPropertySet> Properties { get; } 
    }

    public class Card : ICard
    {
        public Guid Id { get; set; }

        public Guid SetId { get; set; }

        public string Name { get; set; }

        public string ImageUri { get; set; }

        public string Alternate { get; set; }

        public IDictionary<string , CardPropertySet> Properties { get; set; }
    }

    public class CardPropertySet : ICloneable
    {
        public string Type { get; set; }
        public IDictionary<PropertyDef, object> Properties { get; set; }

        public object Clone()
        {
            var ret = new CardPropertySet()
                          {
                              Type = this.Type.Clone() as string,
                              Properties =
                                  this.Properties.ToDictionary(
                                      x => x.Key.Clone() as PropertyDef, x => x.Value)
                          };
            return ret;
        }
    }
}