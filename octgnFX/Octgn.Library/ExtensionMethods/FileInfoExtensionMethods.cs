namespace Octgn.Library.ExtensionMethods
{
    using System;
    using System.IO;

    public static class FileInfoExtensionMethods
    {
        public static void MegaCopyTo(this FileInfo file, FileInfo newFile)
        {
            if(newFile.Exists)
                newFile.ClearReadonlyFlag();
            file.CopyTo(newFile.FullName, true);
        }
        public static void MegaCopyTo(this FileInfo file, string newFile)
        {
            var newfi = new FileInfo(newFile);
            file.MegaCopyTo(newfi);
        }
        public static void ClearReadonlyFlag(this FileInfo file)
        {
            if ((file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                file.Attributes = (FileAttributes)(Convert.ToInt32(file.Attributes) - Convert.ToInt32(FileAttributes.ReadOnly));
            }
        }
    }
}