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
        public static void ClearReadonlyFlag(this DirectoryInfo dir)
        {
            if ((dir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                dir.Attributes = (FileAttributes)(Convert.ToInt32(dir.Attributes) - Convert.ToInt32(FileAttributes.ReadOnly));
            }
            foreach (var f in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                f.ClearReadonlyFlag();
            }
        }

        public static void MoveDirectory(this DirectoryInfo source, string target)
        {
            var targetDirectory = new DirectoryInfo(target);
            source.MoveDirectory(targetDirectory);
        }

        public static void MoveDirectory(this DirectoryInfo source, DirectoryInfo target)
        {
            source.ClearReadonlyFlag();
            if (source.Root == target.Root)
            {
                source.MoveTo(target.FullName);
            }
            else
            {
                source.MoveContents(target);
                source.Delete(true);
            }
        }

        private static void MoveContents(this DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
            {
                target.Create();
            }
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }
            foreach (DirectoryInfo sourceSubdirectory in source.GetDirectories())
            {
                DirectoryInfo targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
                MoveContents(sourceSubdirectory, targetSubdirectory);
            }
        }
    }
}