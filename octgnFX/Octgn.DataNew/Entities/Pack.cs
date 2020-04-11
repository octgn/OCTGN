namespace Octgn.DataNew.Entities
{
    using System;
    using System.Collections.Generic;

    public class Pack
    {
        public Guid Id { get; set; }
        public Set Set { get; set; }
        public string Name { get; set; }
        public List<Include> Includes { get; set; }
        public List<object> Items { get; set; }

        public Pack()
        {
            Includes = new List<Include>();
            Items = new List<object>();
        }
    }

}