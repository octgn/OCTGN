using System.Collections.Generic;

namespace Octgn.Sdk.Packaging
{
    public interface IInstalledPackageProvider : IPackageProvider
    {
        IEnumerable<IPackage> All();
    }
}
