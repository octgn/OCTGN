namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Include
    {
        public Guid Id { get; set; }
        public Guid SetId { get; set; }
        public IDictionary<PropertyDef, object> Properties { get; set; }

    }
}