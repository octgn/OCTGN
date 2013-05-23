namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Pack
    {
        public Guid Id { get; set; }
        public Guid SetId { get; set; }
        public string Name { get; set; }
        public PackDefinition Definition { get; set; }
    }
}