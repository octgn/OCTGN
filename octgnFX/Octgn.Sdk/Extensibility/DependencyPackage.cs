using Octgn.Sdk.Data;
using System;

namespace Octgn.Sdk.Extensibility
{
    public class DependencyPackage : Package
    {
        public Package Parent { get; }

        public override string FullName {
            get {
                return Parent.FullName + "." + Record.Id;
            }
        }

        public DependencyPackage(PackageRecord packageRecord, Package parent) : base(packageRecord) {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }
    }
}
