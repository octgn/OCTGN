namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class Section
    {
        public string Name { get; set; }
        public IList<MultiCard> Cards { get; set; }
    }
}