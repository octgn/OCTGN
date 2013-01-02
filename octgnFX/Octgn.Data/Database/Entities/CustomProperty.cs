using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Database.Entities
{
    public class CustomProperty
    {
        public virtual int Real_Id { get; set; }
        public virtual Guid Id { get; set; }
        public virtual int Card_Real_Id { get; set; }
        public virtual string Game_Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Type { get; set; }
        public virtual int VInt { get; set; }
        public virtual string VStr { get; set; }
    }
}
