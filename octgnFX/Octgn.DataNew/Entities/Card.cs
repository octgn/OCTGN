using System.Runtime.Serialization.Formatters;

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
        CardSize Size { get; }
        IDictionary<string , CardPropertySet> PropertySets { get; } 
    }

    public class Card : ICard
    {
        public Guid Id { get; set; }

        public Guid SetId { get; set; }

        public string Name { get; set; }

        public string ImageUri { get; set; }

        public string Alternate { get; set; }

        public CardSize Size { get; set; }

        [Obsolete("The Properties property will soon be deprecated. Use PropertySets instead.")]
        public IDictionary<string, CardPropertySet> Properties
        {
            get
            {
                var propSets = PropertySets.ToDictionary
                    (x => x.Key,
                    y =>
                    {
                        var pset = y.Value.Clone() as CardPropertySet;
                        var nameProp = new PropertyDef() { Name = "Name", Type = PropertyType.String };
                        pset.Properties[nameProp] = pset.Name;
                        return pset;
                    });
                return propSets;
            }
        }

        public IDictionary<string , CardPropertySet> PropertySets { get; set; }

        public Card(Guid id, Guid setId, string name, string imageuri, string alternate, CardSize size, IDictionary<string,CardPropertySet> properties )
        {
            Id = id;
            SetId = setId;
            Name = name.Clone() as string;
            ImageUri = imageuri.Clone() as string;
            Alternate = alternate.Clone() as string;
            Size = size;
            PropertySets = properties;
            this.PropertySets = this.CloneProperties();
        }

        public Card(ICard card)
			:this(card.Id,card.SetId,card.Name.Clone() as string, card.ImageUri.Clone() as string, card.Alternate.Clone() as string,card.Size,card.CloneProperties())
        {
            
        }
    }

    public class CardPropertySet : ICloneable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public CardSize Size { get; set; }
        public IDictionary<PropertyDef, object> Properties { get; set; }

        public object Clone()
        {
            var ret = new CardPropertySet()
                          {
                              Name = this.Name.Clone() as string,
                              Type = this.Type.Clone() as string,
                              Size = this.Size as CardSize,
                              Properties =
                                  this.Properties.ToDictionary(
                                      x => x.Key, x => x.Value)
                          };
            return ret;
        }
    }
}