namespace Octgn.Core
{
    using Octgn.Data;
    using Octgn.Library;

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
