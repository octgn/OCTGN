using System.IO;

namespace Octgn.Sdk.Packaging
{
    public interface IPackageFileReader
    {
        bool CanRead(string path);

        PackageFile ReadStream(string path, Stream stream);
    }
}
