using System;

namespace Octgn.DataNew.Entities
{
    public class CardSize : ICloneable
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CornerRadius { get; set; }
        public string Back { get; set; }
        public string Front { get; set; }

        public CardSize()
        {

        }

        public object Clone()
        {
            var ret = new CardSize()
            {
				Name = Name.Clone() as string,
				Front = Front.Clone() as string,
				Back = Back.Clone() as string,
				CornerRadius = CornerRadius,
				Width = Width,
				Height = Height
            };
            return ret;
        }
    }
}