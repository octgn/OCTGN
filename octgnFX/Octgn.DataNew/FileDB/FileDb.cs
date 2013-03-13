namespace Octgn.DataNew.FileDB
{
    public class FileDb
    {
        internal FileDbConfiguration Config { get; set; }
        public FileDb(FileDbConfiguration config)
        {
            Config = config;
        }
    }
}