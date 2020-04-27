namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class OptionsList
    {
        public OptionsList()
        {
            Options = new List<Option>();
        }

        public IList<Option> Options { get; set; }
    }
}