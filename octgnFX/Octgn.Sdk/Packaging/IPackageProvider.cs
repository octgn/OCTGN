using System;
using System.Collections.Generic;

namespace Octgn.Sdk.Packaging
{
    public interface IPackageProvider
    {
        string Name { get; }

        IEnumerable<IPackage> Search(string query, int page = 0);

        IPackage Get(string id);

        IPackage Get(string id, Version version);
    }
}
