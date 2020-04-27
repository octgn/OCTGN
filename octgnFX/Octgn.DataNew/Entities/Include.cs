namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Include
    {
        public Guid Id { get; set; }
        public Guid SetId { get; set; }
        public List<PickProperty> Properties { get; set; }

    }
}