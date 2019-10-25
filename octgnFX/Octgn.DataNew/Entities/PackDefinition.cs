namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class PackDefinition
    {
        public List<IPackItem> Items { get; set; } 
        public PackDefinition()
        {
            Items = new List<IPackItem>();
        }
    }
}