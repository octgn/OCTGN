namespace Octgn.Data.Entities
{
    using System.Collections.Generic;

    public class OptionList : IPackItem
    {
        public IEnumerable<Option> Options { get; set; } 
    }
}