using System.Text;
namespace Octgn.DataNew.Entities
{
    using System;

    public class Marker : ICloneable
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

        public bool Equals(Marker other)
        {
            if (other == null) return false;
            if (other.Id != Id) return false;
            if (other.Name == null) return false;
            if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) == false) return false;
            return true;
        }

        public virtual object Clone()
        {
            var ret = new Marker();
            ret.Id = this.Id;
            ret.Name = this.Name.Clone() as string;
            ret.IconUri = IconUri;
            return ret;
        }
    }
}