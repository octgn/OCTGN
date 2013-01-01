using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Library
{
    using Octgn.Data;

    public class DataConfig : IDatabaseConfig 
    {
        public string DataPath
        {
            get
            {
                return Paths.DatabasePath;
            }
        }
    }
}
