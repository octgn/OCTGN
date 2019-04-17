using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Octgn.Core.Util
{
    public class SanitizeFilenames
    {
        public static string SanitizeFilename(string filename)
        {
            var forbidden = new char[] {'<', '>','?','|','/','\\',':','"', '*'};
            foreach (var c in forbidden)
            {
                filename = filename.Replace(c.ToString(), "_");
            }
            return filename;
        }
    }
}
