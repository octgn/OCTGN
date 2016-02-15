namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Include
    {
        public Guid Id { get; set; }
        public Guid SetId { get; set; }
        public List<Tuple<string, string>> Properties { get; set; }

        public Include()
        {
            Properties = new List<Tuple<string, string>>();
        }
    }
}