namespace Octgn.Library.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class DirectoryInfoExtensionMethods
    {
        public static List<DirectoryInfo> SplitFull(this DirectoryInfo path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var ret = new List<DirectoryInfo>();
            if (path.Parent != null) ret.AddRange(SplitFull(path.Parent));
            ret.Add(path);
            return ret;
        }
        public static List<string> Split(this DirectoryInfo path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var ret = new List<string>();
            if (path.Parent != null) ret.AddRange(Split(path.Parent));
            ret.Add(path.Name);
            return ret;
        }
    }
}