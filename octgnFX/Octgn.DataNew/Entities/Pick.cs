namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class Pick
    {
        public int Quantity { get; set; }
        public List<PickProperty> Properties { get; set; }
    }
}
