namespace Octgn.Library.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class DirectoryInfoExtensionMethods
    {
        //public static List<DirectoryInfo> SplitFull(this DirectoryInfo path)
        //{
        //    if (path == null) throw new ArgumentNullException("path");
        //    var ret = new List<DirectoryInfo>();
        //    if (path.Parent != null) ret.AddRange(SplitFull(path.Parent));
        //    ret.Add(path);
        //    return ret;
        //}
        //public static List<string> Split(this DirectoryInfo path)
        //{
        //    if (path == null) throw new ArgumentNullException("path");
        //    var ret = new List<string>();
        //    if (path.Parent != null) ret.AddRange(Split(path.Parent));
        //    ret.Add(path.Name);
        //    return ret;
        //}

        public static string[] Split(this DirectoryInfo path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var ret = new string[200];
            var par = path.Parent;
            ret[0] = path.Name;
            var count = 1;
            while (par != null)
            {
                ret[count] = par.Name;
                count++;
                par = par.Parent;
            }
            Array.Resize(ref ret, count);
            Array.Reverse(ret);
            return ret;
        }

        public static DirectoryInfo[] SplitFull(this DirectoryInfo path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var ret = new DirectoryInfo[200];
            var par = path.Parent;
            ret[0] = path;
            var count = 1;
            while (par != null)
            {
                ret[count] = par;
                count++;
                par = par.Parent;
            }
            Array.Resize(ref ret, count);
            Array.Reverse(ret);
            return ret;
        }
    }
}