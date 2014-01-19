using System.Text;
namespace Octgn.DataNew.Entities
{
    using System;

    public class Marker
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }

        public string ModelString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("('{0}','{1}')", Name, Id);
            return sb.ToString();
        }
    }
}