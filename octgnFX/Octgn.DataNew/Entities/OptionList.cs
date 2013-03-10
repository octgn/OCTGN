namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class OptionList : IPackItem
    {
        public IEnumerable<Option> Options { get; set; } 
    }
}