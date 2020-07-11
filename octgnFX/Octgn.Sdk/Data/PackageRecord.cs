using Octgn.Sdk.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Octgn.Sdk.Data
{
    [DebuggerDisplay("PackageRecord({Id}@{Version})")]
    public class PackageRecord
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Path { get; set; }

        public string Website { get; set; }

        public string Icon { get; set; }

        public string OctgnVersion { get; set; }

        public string CombinedDependencies { get; set; }

        public IEnumerable<IDependency> Dependencies() {
            if (string.IsNullOrWhiteSpace(CombinedDependencies))
                yield break;

            var dependencies = CombinedDependencies.Split(';');

            foreach (var dependencyString in dependencies) {
                var parts = dependencyString.Split('@');

                if (parts.Length != 2) {
                    throw new InvalidOperationException($"Invalid dependency string {CombinedDependencies}");
                }

                yield return new DependencyRecord {
                    Id = parts[0],
                    Version = parts[1]
                };
            }
        }

        public void SetDependencies(IEnumerable<IDependency> dependencies) {
            var dstrings = dependencies.Select(d => $"{d.Id}@{d.Version}");
            CombinedDependencies = string.Join(";", dstrings);
        }
    }
}
