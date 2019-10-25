namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class Pick : IPackItem
    {
        public int Quantity { get; set; }
        public List<PickProperty> Properties { get; set; }
    }
}
